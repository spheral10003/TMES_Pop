using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Files;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Attachment
{
    /// <summary>
    /// MEIS &gt; 작업관리 &gt; 첨부파일 조회.
    ///
    /// 흐름:
    ///  1) 등록일자 범위(필수) · 시리얼번호 · 작업지시번호로 [조회] → 좌측 [목록]
    ///  2) 목록에서 한 건을 클릭하면 → 우측 [파일 상세목록]에 그 작업지시의 첨부가 바로 표시된다(자동 조회)
    ///  3) 우측에서 [다운로드] · [삭제] · [업로드]로 첨부를 다룬다. 업로드 · 삭제 뒤에는 서버 목록을 다시 받는다.
    ///
    /// FileUploadForm과 같은 API(IFileApiClient)를 쓰되 미리보기는 없다.
    /// 그리드를 제외한 컨트롤은 .NET 기본 컨트롤이고, 색은 Designer에 직접 지정했다(테마 전환 대상 아님).
    /// </summary>
    public partial class AttachmentSearchScreen : UserControl
    {
        private Setting _appSet;
        private IFileApiClient _fileApi;
        private long _maxBytes;

        private readonly WorkOrderQuery _query = new WorkOrderQuery();

        /// <summary>우측에 첨부를 띄우고 있는 작업지시. 늦게 도착한 응답이 다른 지시의 첨부를 덮지 않게 비교할 때 쓴다.</summary>
        private WorkOrderEntryDto _currentOrder;

        /// <summary>조회 · 업로드 · 삭제 · 다운로드 중에는 버튼을 막는다 — 중복 요청이 서버 상태를 꼬이게 한다.</summary>
        private bool _isBusy;

        public AttachmentSearchScreen()
        {
            InitializeComponent();
        }

        public void Init()
        {
            _appSet = Global.Instance.Settings;
            _fileApi = _appSet.ApiClients.FileApiClient;
            _maxBytes = (long)_appSet.AppSet.UploadMaxMegaBytes * 1024 * 1024;

            ApplyTexts();

            // 등록일자 기본값: 최근 일주일.
            dtpFrom.Value = DateTime.Today.AddDays(-6);
            dtpTo.Value = DateTime.Today;

            ControlEnabled();
        }

        /// <summary>
        /// 화면 문구를 현재 UI 문화권(로그인 · 메인 화면에서 고른 언어)으로 입힌다.
        /// 메인 화면에서 언어를 바꾸면 MainForm이 열린 탭을 닫았다 다시 열어 Init()이 다시 불리므로 여기서 한 번만 읽으면 된다.
        /// </summary>
        private void ApplyTexts()
        {
            lblRegDate.Text = "· " + Language.Attach_RegDate + "(*)";
            lblSerialNo.Text = "· " + Language.Col_SerialNo;
            lblWorkOrderNo.Text = "· " + Language.Col_WorkOrderNo;
            btnSearch.Text = Language.Common_Search;

            lblOrderTitle.Text = "■ " + Language.Attach_ListTitle;
            lblDetailTitle.Text = "■ " + Language.Attach_DetailTitle;
            lblFileTitle.Text = "■ " + Language.Attach_FileListTitle;
            lblAttach.Text = "• " + Language.Attach_Files;
            btnDownload.Text = Language.Attach_Download;
            btnDelete.Text = Language.Attach_Delete;
            btnUpload.Text = Language.Attach_Upload;

            colOrderNo.Caption = Language.Col_No;
            colWorkOrderNo.Caption = Language.Col_WorkOrderNo;
            colSerialNo.Caption = Language.Col_SerialNo;
            colAttachmentCount.Caption = Language.Col_AttachmentCount;
            colCreatedDate.Caption = Language.Col_CreatedDate;

            colFileNo.Caption = Language.Col_No;
            colFileName.Caption = Language.Col_FileName;
            colFileType.Caption = Language.Col_FileType;
            colFileSize.Caption = Language.Col_FileSize;
            colCreatedBy.Caption = Language.Col_CreatedBy;
            colCreatedAt.Caption = Language.Col_CreatedAt;

            ArrangeByTextWidth();
        }

        /// <summary>
        /// 문구 길이에 맞춰 조회 조건 · 첨부 버튼 줄을 다시 배치한다.
        /// Designer 위치는 한국어 기준이라, 영어 · 베트남어처럼 긴 문구는 그대로 두면 라벨이 입력칸을 덮는다.
        /// </summary>
        private void ArrangeByTextWidth()
        {
            const int labelGap = 8;    // 라벨 → 입력칸
            const int groupGap = 24;   // 입력칸 → 다음 라벨

            int x = lblRegDate.Left;
            x = PlaceAfter(lblRegDate, dtpFrom, x, labelGap);
            lblDateTilde.Left = x + 4;
            dtpTo.Left = lblDateTilde.Left + lblDateTilde.PreferredWidth + 4;
            x = dtpTo.Right + groupGap;
            x = PlaceAfter(lblSerialNo, txtSerialNo, x, labelGap) + groupGap;
            PlaceAfter(lblWorkOrderNo, txtWorkOrderNo, x, labelGap);

            // 버튼은 문구가 넘치지 않게 넓힌다.
            int left = Math.Max(110, lblAttach.Left + lblAttach.PreferredWidth + 16);
            foreach (var button in new[] { btnDownload, btnDelete, btnUpload })
            {
                button.Width = Math.Max(76, TextRenderer.MeasureText(button.Text, button.Font).Width + 20);
                button.Left = left;
                left = button.Right + 6;
            }
        }

        /// <summary>라벨을 x에 두고 입력칸을 그 오른쪽에 붙인다. 입력칸의 오른쪽 끝을 돌려준다.</summary>
        private int PlaceAfter(Label label, Control input, int x, int gap)
        {
            label.Left = x;
            input.Left = x + label.PreferredWidth + gap;
            return input.Right;
        }

        private void ControlEnabled()
        {
            bool hasOrder = _currentOrder != null;
            bool hasSelection = gridViewFiles.SelectedRowsCount > 0;

            btnSearch.Enabled = !_isBusy;
            btnUpload.Enabled = !_isBusy && hasOrder;
            btnDownload.Enabled = !_isBusy && hasOrder && hasSelection;
            btnDelete.Enabled = !_isBusy && hasOrder && hasSelection;
        }

        #region 조회

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            await SearchAsync();
        }

        /// <summary>바코드 스캐너처럼 Enter로 끝나는 입력을 그대로 조회로 연결한다.</summary>
        private async void Condition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || _isBusy)
                return;

            e.SuppressKeyPress = true;
            await SearchAsync();
        }

        /// <summary>작업지시 목록을 조회한다. 등록일자는 필수이며, 시작이 종료보다 늦으면 안내 후 중단한다.</summary>
        private async Task SearchAsync()
        {
            var from = dtpFrom.Value.Date;
            var to = dtpTo.Value.Date;
            if (from > to)
            {
                AppMessageBox.Warn(this, Language.WorkStatus_DateReversed, Language.Attach_Title);
                dtpFrom.Focus();
                return;
            }

            _query.CorpCode = _appSet.CurrentUser?.Server;
            _query.OrderDateFrom = from;
            _query.OrderDateTo = to;
            _query.WorkOrderNo = txtWorkOrderNo.Text?.Trim();
            _query.SerialNo = txtSerialNo.Text?.Trim();

            _isBusy = true;
            ControlEnabled();

            using (var busy = BusyOverlay.Show(this, cancelable: true))
            {
                try
                {
                    var result = await _appSet.ApiClients.WorkStatusApiClient.SearchWorkOrdersAsync(_query, busy?.Token ?? default)
                                 ?? new List<WorkOrderEntryDto>();
                    if (IsDisposed || Disposing) return;   // 조회 중 탭을 닫았다

                    // 새 결과이므로 이전 지시의 첨부는 비운다(같은 지시라도 다시 읽는다).
                    ClearFiles();
                    gridOrders.DataSource = result;

                    if (result.Count == 0)
                    {
                        AppMessageBox.Info(this, Language.WorkEntry_NoResult, Language.Attach_Title);
                        return;
                    }

                    // 이미 0번 행에 포커스가 있으면 FocusedRowChanged가 안 걸리므로 직접 한 번 부른다.
                    gridViewOrders.FocusedRowHandle = 0;
                    await LoadFilesAsync(gridViewOrders.GetFocusedRow() as WorkOrderEntryDto);
                }
                catch (OperationCanceledException)
                {
                    // 사용자가 [취소]를 눌렀다 — 오류가 아니므로 조용히 끝낸다.
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;

                    AppLog.Error("첨부파일 조회 화면 - 작업지시 조회 실패", ex);
                    AppMessageBox.Warn(this, ex.Message, Language.Attach_Title);
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                    {
                        _isBusy = false;
                        ControlEnabled();
                    }
                }
            }
        }

        /// <summary>작업지시를 클릭(또는 키보드로 이동)할 때마다 우측 파일 상세목록을 그 지시의 첨부로 바꾼다.</summary>
        private async void gridViewOrders_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            await LoadFilesAsync(gridViewOrders.GetRow(e.FocusedRowHandle) as WorkOrderEntryDto);
        }

        /// <summary>
        /// 선택한 작업지시의 첨부 목록을 서버에서 다시 읽는다. 업로드 · 삭제 뒤에도 서버 기준으로 다시 그린다.
        /// 같은 지시를 이미 띄우고 있으면 건너뛴다(조회 직후 포커스 이벤트와 직접 호출이 겹치는 경우).
        /// </summary>
        private async Task LoadFilesAsync(WorkOrderEntryDto order, bool force = false)
        {
            if (order == null)
            {
                _currentOrder = null;
                ClearFiles();
                return;
            }

            if (!force && ReferenceEquals(_currentOrder, order))
                return;

            _currentOrder = order;
            try
            {
                var result = await _fileApi.GetAttachmentsAsync(KeyOf(order));
                if (IsDisposed || Disposing) return;

                // 받는 사이 다른 지시를 골랐으면 늦게 온 응답은 버린다.
                if (!ReferenceEquals(_currentOrder, order))
                    return;

                BindFiles((result?.Attachments ?? new List<WorkOrderAttachmentDto>()).Select(ToFileResponse).ToList());
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                ClearFiles();
                AppLog.Error($"첨부 조회 실패 - {order.WorkOrderNo}", ex);
                AppMessageBox.Error(this, ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : $"{Language.Upload_Failed} ({(int)ex.StatusCode})", Language.Attach_Title);
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                    ControlEnabled();
            }
        }

        /// <summary>첨부 목록 응답(WorkOrderAttachmentDto)을 이 화면의 표시 단위(FileUploadResponse)로 옮겨 담는다.</summary>
        private FileUploadResponse ToFileResponse(WorkOrderAttachmentDto item)
        {
            var file = new FileUploadResponse();
            file.AttachRef = item.AttachRef;
            file.FileName = item.FileName;
            file.FileExtension = item.FileExtension;
            file.FileSize = item.FileSize;
            file.CreatedBy = item.CreatedBy;
            file.CreatedAt = item.CreatedAt;
            return file;
        }

        private PopProcessKey KeyOf(WorkOrderEntryDto order)
        {
            var key = new PopProcessKey();
            key.CorpCode = string.IsNullOrEmpty(order.CorpCode) ? _appSet.CurrentUser?.Server : order.CorpCode;
            key.WorkOrderNo = order.WorkOrderNo;
            key.SerialNo = order.SerialNo;
            return key;
        }

        /// <summary>받은 첨부 목록을 파일 상세목록 그리드에 바로 붙인다.</summary>
        private void BindFiles(List<FileUploadResponse> files)
        {
            gridFiles.DataSource = files ?? new List<FileUploadResponse>();
            gridViewFiles.ClearSelection();
            ControlEnabled();
        }

        private void ClearFiles()
        {
            BindFiles(new List<FileUploadResponse>());
        }

        /// <summary>그리드에서 고른 행이 바뀌면 [다운로드] · [삭제] 사용 여부를 다시 정한다.</summary>
        private void gridViewFiles_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            ControlEnabled();
        }

        /// <summary>다운로드 · 삭제 대상 — 파일 상세목록에서 고른 행.</summary>
        private List<FileUploadResponse> SelectedFiles()
        {
            return gridViewFiles.GetSelectedRows()
                .Where(handle => handle >= 0)
                .Select(handle => gridViewFiles.GetRow(handle) as FileUploadResponse)
                .Where(file => file != null)
                .ToList();
        }

        #endregion

        #region 다운로드 · 삭제 · 업로드

        /// <summary>
        /// 고른 첨부를 받아 로컬에 저장한다. 한 건이면 저장 위치·이름을 묻고, 여러 건이면 폴더만 묻는다.
        /// 받은 내용(AttachmentFile.Content)을 그대로 쓴다 — 변환하지 않는다.
        /// </summary>
        private async void btnDownload_Click(object sender, EventArgs e)
        {
            var order = _currentOrder;
            var targets = SelectedFiles();
            if (order == null)
            {
                AppMessageBox.Warn(this, Language.Attach_SelectOrder, Language.Attach_Title);
                return;
            }
            if (targets.Count == 0)
            {
                AppMessageBox.Warn(this, Language.Attach_SelectFile, Language.Attach_Title);
                return;
            }

            var savePaths = AskSavePaths(targets);
            if (savePaths == null)
                return;

            _isBusy = true;
            ControlEnabled();

            var key = KeyOf(order);
            var failed = new List<string>();
            using (BusyOverlay.Show(this))
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    var target = targets[i];
                    try
                    {
                        var file = await _fileApi.DownloadAttachmentAsync(key, target.AttachRef);
                        if (IsDisposed || Disposing) return;

                        File.WriteAllBytes(savePaths[i], file?.Content ?? new byte[0]);
                        AppLog.Step($"첨부 다운로드 - {order.WorkOrderNo}, {target.DisplayName} → {savePaths[i]}");
                    }
                    catch (ApiException ex)
                    {
                        if (IsDisposed || Disposing) return;
                        AppLog.Error($"첨부 다운로드 실패 - {target.DisplayName}", ex);
                        failed.Add(target.DisplayName);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
                    {
                        AppLog.Error($"첨부 저장 실패 - {savePaths[i]}", ex);
                        failed.Add(target.DisplayName);
                    }
                }
            }

            if (IsDisposed || Disposing) return;
            _isBusy = false;
            ControlEnabled();

            if (failed.Count > 0)
                AppMessageBox.Error(this, Language.Attach_DownloadFailed + "\n\n" + string.Join("\n", failed), Language.Attach_Title);
            else
                AppMessageBox.Success(this, Language.Attach_DownloadDone, Language.Attach_Title);
        }

        /// <summary>저장 경로를 대상 순서대로 돌려준다. 사용자가 취소하면 null.</summary>
        private List<string> AskSavePaths(List<FileUploadResponse> targets)
        {
            if (targets.Count == 1)
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.FileName = targets[0].DisplayName;
                    dialog.OverwritePrompt = true;
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return null;
                    return new List<string> { dialog.FileName };
                }
            }

            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return null;

                // 여러 건은 덮어쓸지 하나하나 묻지 않고, 같은 이름이 있으면 "이름 (1).확장자"로 비켜 저장한다.
                var paths = new List<string>();
                foreach (var target in targets)
                    paths.Add(UniquePath(dialog.SelectedPath, target.DisplayName, paths));
                return paths;
            }
        }

        private string UniquePath(string folder, string fileName, List<string> reserved)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var path = Path.Combine(folder, fileName);
            for (int n = 1; File.Exists(path) || reserved.Contains(path, StringComparer.OrdinalIgnoreCase); n++)
                path = Path.Combine(folder, $"{name} ({n}){ext}");
            return path;
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            var order = _currentOrder;
            var targets = SelectedFiles();
            if (order == null || targets.Count == 0)
                return;

            if (!AppMessageBox.Confirm(this, string.Format(Language.Upload_DeleteConfirmFormat, targets.Count), Language.Attach_Title))
                return;

            _isBusy = true;
            ControlEnabled();

            try
            {
                AppLog.Step($"첨부 삭제 시작 - {order.WorkOrderNo}, atchRef={string.Join(",", targets.Select(t => t.AttachRef))}");
                await _fileApi.DeleteAttachmentsAsync(KeyOf(order), targets.Select(t => t.AttachRef));
                if (IsDisposed || Disposing) return;

                AppLog.Step($"첨부 삭제 완료 - {order.WorkOrderNo}, {targets.Count}건");
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                AppLog.Error($"첨부 삭제 실패 - {order.WorkOrderNo}", ex);
                AppMessageBox.Error(this, ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : $"{Language.Upload_DeleteFailed} ({(int)ex.StatusCode})", Language.Attach_Title);
                return;
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    _isBusy = false;
                    ControlEnabled();
                }
            }

            // 삭제 결과는 화면에서 빼지 않고, 서버 목록을 다시 받아서 그린다.
            await LoadFilesAsync(order, force: true);
        }

        /// <summary>파일을 골라 바로 한 요청으로 올린다(FileUploadForm과 같은 UploadManyAsync). 미리보기는 없다.</summary>
        private async void btnUpload_Click(object sender, EventArgs e)
        {
            var order = _currentOrder;
            if (order == null)
            {
                AppMessageBox.Warn(this, Language.Attach_SelectOrder, Language.Attach_Title);
                return;
            }

            string[] paths;
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = Language.Upload_SelectFile;
                dialog.Filter = Language.Attach_UploadFilter;
                dialog.Multiselect = true;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                paths = dialog.FileNames;
            }

            var files = new List<FileBinary>();
            var failed = new List<string>();
            foreach (var path in paths)
            {
                if (TryLoad(path, out var file, out var reason))
                    files.Add(file);
                else
                    failed.Add($"{Path.GetFileName(path)} : {reason}");
            }

            if (failed.Count > 0)
                AppMessageBox.Warn(this, Language.Upload_ReadFailed + "\n\n" + string.Join("\n", failed), Language.Attach_Title);
            if (files.Count == 0)
                return;

            _isBusy = true;
            ControlEnabled();

            using (BusyOverlay.Show(this))
            {
                try
                {
                    AppLog.Step($"파일 일괄 업로드 시작 - {order.WorkOrderNo}, {files.Count}건 ({files.Sum(f => f.Length):N0} byte)");
                    List<FileUploadResponse> uploaded = await _fileApi.UploadManyAsync(files, KeyOf(order));
                    if (IsDisposed || Disposing) return;

                    AppLog.Step($"파일 일괄 업로드 완료 - {order.WorkOrderNo}, {uploaded?.Count ?? files.Count}건");
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;

                    // 한 요청이라 실패하면 아무것도 안 올라간 것으로 본다.
                    AppLog.Error($"파일 일괄 업로드 실패 - {order.WorkOrderNo}, {files.Count}건", ex);
                    AppMessageBox.Error(this, ex.IsTransportFailure
                        ? Language.Upload_ConnectionError
                        : $"{Language.Upload_Failed} ({(int)ex.StatusCode})", Language.Attach_Title);
                    return;
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                    {
                        _isBusy = false;
                        ControlEnabled();
                    }
                }
            }

            // 올라간 결과도 서버 목록을 다시 받아 그린다(작성자 · 작성일시는 서버 값이어야 한다).
            await LoadFilesAsync(order, force: true);
        }

        /// <summary>파일 하나를 바이너리로 읽는다. 실패 사유는 로그와 안내 창에 함께 쓴다.</summary>
        private bool TryLoad(string path, out FileBinary file, out string reason)
        {
            try
            {
                file = FileBinary.Load(path, _maxBytes);
                reason = null;
                return true;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                // InvalidDataException(크기 초과/빈 파일) · FileNotFoundException도 IOException 계열이다.
                AppLog.Warn($"업로드 파일 읽기 실패 - {path}: {ex.Message}");
                file = null;
                reason = ex.Message;
                return false;
            }
        }

        #endregion

        #region 그리드 표시

        /// <summary>No. 열 — 정렬 후에도 화면에 보이는 순서대로 1부터 매긴다.</summary>
        private void gridView_RowNumberDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == null || e.Column.FieldName != "RowNo" || e.ListSourceRowIndex < 0)
                return;

            var view = (GridView)sender;
            int handle = view.GetRowHandle(e.ListSourceRowIndex);
            e.DisplayText = (view.GetVisibleIndex(handle) + 1).ToString();
        }

        /// <summary>파일 상세목록 — No. / 첨부유형(이미지·문서) / 파일크기(KB·MB) 표시 문구.</summary>
        private void gridViewFiles_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column == colFileNo)
            {
                gridView_RowNumberDisplayText(sender, e);
            }
            else if (e.Column == colFileType && e.Value is bool isImage)
            {
                e.DisplayText = isImage ? Language.Attach_FileTypeImage : Language.Attach_FileTypeDocument;
            }
            else if (e.Column == colFileSize && e.Value is long size)
            {
                e.DisplayText = FormatSize(size);
            }
        }

        private string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024)
                return $"{bytes / 1024d / 1024d:N1} MB";
            if (bytes >= 1024)
                return $"{bytes / 1024d:N1} KB";
            return $"{bytes:N0} B";
        }

        /// <summary>행이 없을 때 시안처럼 가운데에 안내 문구를 그린다.</summary>
        private void gridView_CustomDrawEmptyForeground(object sender, CustomDrawEventArgs e)
        {
            var view = (GridView)sender;
            if (view.RowCount > 0)
                return;

            using (var format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                e.Cache.DrawString(Language.Common_NoData, view.Appearance.Empty.GetFont(), Brushes.DarkGray, e.Bounds, format);
            }
            e.Handled = true;
        }

        #endregion
    }
}
