using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using SEOJIN_TMES.Client.Common.Imaging;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Storage;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 그리드 행이 가리키는 공유 폴더의 그림을 미리보기 창(<see cref="ImagePreviewPopup"/>)에 띄운다. 두 가지 방식이 있다.
    ///  - 마우스 오버: 행 위에 잠깐(<see cref="HoverDelayMs"/>) 머물면 커서 옆에 띄우고, 벗어나면 내린다.
    ///  - 행 클릭: 클릭한 행 옆에 띄우고 <b>고정</b>한다. 고정된 동안은 마우스 오버 미리보기를 멈춘다 —
    ///    마우스를 움직일 때마다 고정한 그림이 다른 그림으로 바뀌면 클릭한 의미가 없다.
    ///    그림 없는 행 · 빈 영역 클릭, Esc, 스크롤, 목록 갱신, 그리드가 포커스를 잃거나 창이 비활성화되면 고정을 푼다.
    ///
    /// 어느 행이 어떤 그림인지는 화면이 정한다 — 생성자로 받은 <c>pathOf</c>가 행 객체에서 서버 경로(상대 경로)를 꺼내고,
    /// 그림이 아니거나 경로가 없으면 null/빈 값을 돌려주면 된다. 경로는 App.config의 DocumentShare.RootPath 뒤에 붙인다.
    ///
    /// 파일은 반드시 DocumentShareClient로 읽는다 — 공유 계정(UserName/Password) 접속이 그 안에서 이루어진다
    /// (PopScreen.LoadDrawing과 같은 이유).
    ///
    /// 한 번 읽은 그림은 캐시에 두고 다시 읽지 않는다. 읽지 못한 경로도 기억해 같은 실패를 반복하지 않는다.
    /// 읽는 사이 대상이 바뀌었으면(다른 행으로 옮겨 감 · 고정 해제) 늦게 온 그림은 띄우지 않는다.
    /// </summary>
    public sealed class GridImagePreview : IDisposable
    {
        /// <summary>행 위에 이만큼 머물러야 미리보기를 띄운다(ms).</summary>
        private const int HoverDelayMs = 350;

        /// <summary>캐시에 들고 있을 그림 수. 넘으면 비우고 다시 쌓는다 — 원본 크기 그림이라 무한히 들고 있으면 메모리가 커진다.</summary>
        private const int MaxCachedImages = 30;

        private readonly GridView _view;
        private readonly Func<object, string> _pathOf;
        private readonly DocumentShareClient _docShare;
        private readonly string _rootPath;
        private readonly ByteToImage _imgCtrl = new ByteToImage();
        private readonly Dictionary<string, Image> _cache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _failed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Timer _hoverTimer = new Timer();

        private ImagePreviewPopup _popup;

        /// <summary>지금 마우스가 올라가 있는 행의 그림 전체 경로. 미리보기 대상이 아니면 null.</summary>
        private string _hoverPath;

        /// <summary>클릭으로 고정한 행의 그림 전체 경로. 고정하지 않았으면 null.</summary>
        private string _pinnedPath;

        /// <summary>클릭으로 고정한 행 — 창을 그 행 옆에 둘 때 행 위치를 다시 구한다.</summary>
        private int _pinnedRowHandle;

        /// <summary>그리드를 담은 창. 비활성화(다른 프로그램으로 전환)되면 고정 미리보기를 내린다. 처음 클릭할 때 찾는다.</summary>
        private Form _hostForm;

        private bool _enabled;
        private bool _disposed;

        /// <param name="view">마우스를 지켜볼 그리드 뷰.</param>
        /// <param name="pathOf">행 객체 → 서버가 준 그림 경로(RootPath 기준 상대 경로). 미리보기 대상이 아니면 null.</param>
        /// <param name="docShare">공유 폴더 읽기 클라이언트(계정 접속 포함).</param>
        /// <param name="rootPath">App.config의 DocumentShare.RootPath. 비어 있으면 미리보기를 쓰지 않는다.</param>
        public GridImagePreview(GridView view, Func<object, string> pathOf, DocumentShareClient docShare, string rootPath)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _pathOf = pathOf ?? throw new ArgumentNullException(nameof(pathOf));
            _docShare = docShare;
            _rootPath = rootPath;

            // 공유 폴더가 설정되지 않은 현장은 미리보기 없이 목록만 쓴다(안내 창은 띄우지 않는다 — 마우스만 올렸을 뿐이다).
            _enabled = _docShare != null && !string.IsNullOrWhiteSpace(_rootPath);
            if (!_enabled)
                return;

            _hoverTimer.Interval = HoverDelayMs;
            _hoverTimer.Tick += HoverTimer_Tick;

            _view.MouseMove += View_MouseMove;
            _view.MouseLeave += View_MouseLeave;
            _view.MouseWheel += View_MouseWheel;
            _view.MouseDown += View_MouseDown;
            _view.RowClick += View_RowClick;
            _view.KeyDown += View_KeyDown;
            _view.TopRowChanged += View_TopRowChanged;
            _view.DataSourceChanged += View_DataSourceChanged;

            var grid = _view.GridControl;
            if (grid != null)
            {
                grid.Leave += Grid_Leave;
                grid.VisibleChanged += Grid_VisibleChanged;
            }
        }

        private bool IsPinned => _pinnedPath != null;

        // ────────────────────────────── 마우스 오버 ──────────────────────────────

        private void View_MouseMove(object sender, MouseEventArgs e)
        {
            // 클릭으로 고정한 동안은 마우스 오버로 그림을 바꾸지 않는다.
            if (IsPinned)
                return;

            GridHitInfo hit = _view.CalcHitInfo(e.Location);
            string path = hit.InRow ? PathOfRow(hit.RowHandle) : null;

            if (path == null)
            {
                CancelHover();
                return;
            }

            // 같은 행(같은 그림) 안에서 움직이면 창만 따라 옮긴다.
            if (string.Equals(path, _hoverPath, StringComparison.OrdinalIgnoreCase))
            {
                if (_popup != null && _popup.Visible)
                    _popup.MoveNear(Cursor.Position);
                return;
            }

            // 다른 그림으로 옮겨 왔다 — 이전 미리보기는 내리고 잠깐 머무는지 다시 본다.
            _hoverPath = path;
            HidePopup();
            _hoverTimer.Stop();
            _hoverTimer.Start();
        }

        /// <summary>그리드를 벗어나면 마우스 오버 미리보기는 내린다. 고정한 미리보기는 그대로 둔다.</summary>
        private void View_MouseLeave(object sender, EventArgs e)
        {
            if (!IsPinned)
                CancelHover();
        }

        private async void HoverTimer_Tick(object sender, EventArgs e)
        {
            _hoverTimer.Stop();
            await ShowPreviewAsync(_hoverPath, pinned: false);
        }

        // ────────────────────────────── 클릭 고정 ──────────────────────────────

        /// <summary>
        /// 누르는 순간 마우스 오버 미리보기는 거둔다(곧이어 RowClick이 고정 미리보기를 띄운다).
        /// 행이 아닌 곳(헤더 · 빈 영역)을 누르면 고정도 푼다 — 행을 누른 경우는 RowClick이 판단한다.
        /// </summary>
        private void View_MouseDown(object sender, MouseEventArgs e)
        {
            CancelHover();

            if (!_view.CalcHitInfo(e.Location).InRow)
                Unpin();
        }

        /// <summary>행을 클릭하면 그 행의 그림을 행 옆에 고정해 띄운다. 그림이 없는 행이면 고정을 푼다.</summary>
        private async void View_RowClick(object sender, RowClickEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            string path = PathOfRow(e.RowHandle);
            if (path == null)
            {
                Unpin();
                return;
            }

            WatchHostForm();
            _pinnedPath = path;
            _pinnedRowHandle = e.RowHandle;
            await ShowPreviewAsync(path, pinned: true);
        }

        private void View_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && IsPinned)
            {
                Unpin();
                e.Handled = true;
            }
        }

        /// <summary>스크롤하면 행이 움직여 창이 엉뚱한 행 옆에 남는다 — 내린다(휠 · 스크롤바 모두).</summary>
        private void View_TopRowChanged(object sender, EventArgs e) => CancelAll();

        private void View_MouseWheel(object sender, MouseEventArgs e) => CancelAll();

        /// <summary>목록을 새로 받으면 고정했던 행이 사라졌을 수 있다.</summary>
        private void View_DataSourceChanged(object sender, EventArgs e) => CancelAll();

        /// <summary>다른 컨트롤로 포커스가 옮겨 가면(다른 그리드 · 입력칸 클릭) 고정을 푼다.</summary>
        private void Grid_Leave(object sender, EventArgs e) => CancelAll();

        /// <summary>탭을 옮기는 등으로 그리드가 숨으면 떠 있는 창도 내린다(TopMost라 혼자 남는다).</summary>
        private void Grid_VisibleChanged(object sender, EventArgs e)
        {
            if (_view.GridControl == null || !_view.GridControl.Visible)
                CancelAll();
        }

        /// <summary>다른 프로그램으로 전환하면 TopMost 창이 그 위에 남지 않게 내린다.</summary>
        private void HostForm_Deactivate(object sender, EventArgs e) => CancelAll();

        /// <summary>창을 옮기면 행 위치가 바뀐다 — 고정 창이 따라가지 않으므로 내린다.</summary>
        private void HostForm_Move(object sender, EventArgs e) => CancelAll();

        /// <summary>
        /// 그리드를 담은 창을 처음 클릭할 때 찾아 이벤트를 건다. 탭 화면(UserControl)은 생성 시점에 아직
        /// 창에 붙어 있지 않을 수 있어 생성자에서 찾지 않는다.
        /// </summary>
        private void WatchHostForm()
        {
            var form = _view.GridControl?.FindForm();
            if (form == null || ReferenceEquals(form, _hostForm))
                return;

            UnwatchHostForm();
            _hostForm = form;
            _hostForm.Deactivate += HostForm_Deactivate;
            _hostForm.Move += HostForm_Move;
        }

        private void UnwatchHostForm()
        {
            if (_hostForm == null)
                return;

            _hostForm.Deactivate -= HostForm_Deactivate;
            _hostForm.Move -= HostForm_Move;
            _hostForm = null;
        }

        /// <summary>행의 화면 좌표. 행이 화면에 보이지 않으면(스크롤로 밀려남) null.</summary>
        private Rectangle? RowBoundsOnScreen(int rowHandle)
        {
            var grid = _view.GridControl;
            var info = _view.GetViewInfo() as GridViewInfo;
            GridRowInfo row = info?.GetGridRowInfo(rowHandle);
            if (grid == null || row == null || row.Bounds.IsEmpty)
                return null;

            return grid.RectangleToScreen(row.Bounds);
        }

        // ────────────────────────────── 읽기 · 표시 ──────────────────────────────

        private async Task ShowPreviewAsync(string path, bool pinned)
        {
            if (path == null || _failed.Contains(path))
            {
                if (pinned) HidePopup();   // 고정하려던 그림을 못 읽으면 이전 그림을 남기지 않는다
                return;
            }

            if (!_cache.TryGetValue(path, out Image image))
            {
                image = await LoadImageAsync(path);
                if (_disposed)
                    return;
                if (image == null)
                {
                    if (pinned && string.Equals(path, _pinnedPath, StringComparison.OrdinalIgnoreCase))
                        HidePopup();
                    return;
                }

                AddToCache(path, image);
            }

            if (_view.GridControl == null || _view.GridControl.IsDisposed)
                return;

            // 읽는 사이 대상이 바뀌었으면 띄우지 않는다.
            if (pinned)
            {
                if (!string.Equals(path, _pinnedPath, StringComparison.OrdinalIgnoreCase))
                    return;
            }
            else
            {
                if (IsPinned || !string.Equals(path, _hoverPath, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            if (_popup == null || _popup.IsDisposed)
                _popup = new ImagePreviewPopup();

            if (pinned)
            {
                Rectangle? row = RowBoundsOnScreen(_pinnedRowHandle);
                if (row.HasValue)
                    _popup.ShowImageBeside(image, row.Value);
                else
                    _popup.ShowImage(image, Cursor.Position);
            }
            else
            {
                _popup.ShowImage(image, Cursor.Position);
            }
        }

        /// <summary>공유 폴더에서 읽어 Image로 바꾼다. 실패하면 로그만 남기고 null(같은 경로는 다시 시도하지 않는다).</summary>
        private async Task<Image> LoadImageAsync(string path)
        {
            try
            {
                byte[] raw = await _docShare.ReadAllBytesAsync(path);
                if (_disposed) return null;

                Image image = Decode(raw);
                if (image == null)
                {
                    AppLog.Warn($"[미리보기] 이미지 데이터를 찾을 수 없음(손상되었거나 이미지가 아닌 파일) - {path}");
                    _failed.Add(path);
                }
                return image;
            }
            catch (Exception ex) when (ex is DocumentShareException || ex is ArgumentException || ex is OutOfMemoryException)
            {
                // ArgumentException / OutOfMemoryException: Image.FromStream이 그림이 아닌 데이터를 만났을 때 던진다.
                AppLog.Warn($"[미리보기] 그림 읽기 실패 - {path} : {ex.Message}");
                if (!_disposed) _failed.Add(path);
                return null;
            }
        }

        /// <summary>도면과 같은 규칙으로 해석한다 — Base64 텍스트면 풀고, 앞에 OLE 헤더 등이 붙어 있으면 잘라 낸다.</summary>
        private Image Decode(byte[] raw)
        {
            if (raw == null || raw.Length == 0)
                return null;

            if (_imgCtrl.TryDecodeBase64(raw, out byte[] decoded))
                raw = decoded;

            int offset = _imgCtrl.FindImageOffset(raw);
            if (offset < 0)
                return null;

            if (offset > 0)
            {
                byte[] trimmed = new byte[raw.Length - offset];
                Buffer.BlockCopy(raw, offset, trimmed, 0, trimmed.Length);
                raw = trimmed;
            }

            return _imgCtrl.BytesToImage(raw);
        }

        private void AddToCache(string path, Image image)
        {
            if (_cache.Count >= MaxCachedImages)
                ClearCache();
            _cache[path] = image;
        }

        private void ClearCache()
        {
            HidePopup();   // 떠 있는 그림도 캐시 소유라 먼저 내린다
            foreach (var image in _cache.Values)
                image.Dispose();
            _cache.Clear();
        }

        /// <summary>행의 그림 전체 경로. 그림이 아닌 행(그룹 행 포함)이면 null.</summary>
        private string PathOfRow(int rowHandle)
        {
            return ToFullPath(_pathOf(_view.GetRow(rowHandle)));
        }

        /// <summary>
        /// 서버 경로를 실제 UNC 경로로 만든다. 서버 경로는 "/GUM/..."처럼 '/'로 시작하므로
        /// DocumentShareClient.Resolve에 그대로 넘기면 루트 경로로 판정된다 — 여기서 RootPath와 합쳐 넘긴다(PopScreen.LoadDrawing과 같음).
        /// </summary>
        private string ToFullPath(string serverPath)
        {
            if (string.IsNullOrWhiteSpace(serverPath))
                return null;

            string normalized = serverPath.Trim().Replace('/', '\\');
            if (normalized.StartsWith(@"\\", StringComparison.Ordinal))
                return normalized;   // 서버가 전체 UNC 경로를 준 경우

            return Path.Combine(_rootPath, normalized.TrimStart('\\'));
        }

        // ────────────────────────────── 정리 ──────────────────────────────

        /// <summary>마우스 오버 미리보기만 거둔다. 고정한 미리보기는 건드리지 않는다.</summary>
        private void CancelHover()
        {
            _hoverPath = null;
            _hoverTimer.Stop();
            if (!IsPinned)
                HidePopup();
        }

        /// <summary>고정을 풀고 창을 내린다.</summary>
        private void Unpin()
        {
            if (!IsPinned)
                return;

            _pinnedPath = null;
            HidePopup();
        }

        private void CancelAll()
        {
            _pinnedPath = null;
            CancelHover();
            HidePopup();
        }

        private void HidePopup()
        {
            if (_popup != null && !_popup.IsDisposed)
                _popup.HidePreview();
        }

        /// <summary>이벤트를 끊고 미리보기 창 · 캐시 그림을 해제한다. 화면이 닫힐 때 호출한다.</summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_enabled)
            {
                _view.MouseMove -= View_MouseMove;
                _view.MouseLeave -= View_MouseLeave;
                _view.MouseWheel -= View_MouseWheel;
                _view.MouseDown -= View_MouseDown;
                _view.RowClick -= View_RowClick;
                _view.KeyDown -= View_KeyDown;
                _view.TopRowChanged -= View_TopRowChanged;
                _view.DataSourceChanged -= View_DataSourceChanged;
                _hoverTimer.Tick -= HoverTimer_Tick;

                var grid = _view.GridControl;
                if (grid != null)
                {
                    grid.Leave -= Grid_Leave;
                    grid.VisibleChanged -= Grid_VisibleChanged;
                }
                UnwatchHostForm();
            }
            _hoverTimer.Stop();
            _hoverTimer.Dispose();

            ClearCache();
            _popup?.Dispose();
            _popup = null;
        }
    }
}
