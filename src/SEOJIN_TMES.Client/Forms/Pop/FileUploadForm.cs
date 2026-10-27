using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Files;
using SEOJIN_TMES.Client.Common.Imaging;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class FileUploadForm : Form
    {
        private Point mousePoint;
        private IFileApiClient _fileApi;
        private PopProcessKey _refKey;
        private long _maxBytes;
        private readonly ByteToImage _imgCtrl = new ByteToImage();

        /// <summary>전송 중에는 창을 닫지 못하게 한다 — 닫히면 Dispose된 창에 결과를 쓰게 된다.</summary>
        private bool _isUploading;

        private readonly List<FileUploadResponse> _uploadedFiles = new List<FileUploadResponse>();

        /// <summary>이번 창에서 서버에 올라간 파일들(서버 응답 그대로).</summary>
        public IReadOnlyList<FileUploadResponse> UploadedFiles => _uploadedFiles;

        public FileUploadForm(PopProcessKey key)
        {
            InitializeComponent();
            _refKey = key;
        }

        private async void FileUploadForm_Load(object sender, EventArgs e)
        {
            Init();
            SetTexts();
            ApplyTheme();
            ControlEnabled();
            await LoadServerFilesAsync();
        }

        private void Init()
        {
            _fileApi = Global.Instance.Settings.ApiClients.FileApiClient;
            _maxBytes = (long)Global.Instance.Settings.AppSet.UploadMaxMegaBytes * 1024 * 1024;
        }

        /// <summary>하단 버튼 색 — 앱 공통 버튼 색(글꼴은 Designer 값 유지).</summary>
        private void ApplyTheme()
        {
            foreach (var button in new[] { btnFileOpen, btnUpload, btnDelete, btnCancle })
                AppTheme.StandardButton(button, keepFont: true);
        }

        private void SetTexts()
        {
            lblTitle.Text = Language.Upload_Title;
            btnFileOpen.Text = Language.Upload_FileOpen;
            btnUpload.Text = Language.Upload_Upload;
            btnDelete.Text = Language.Upload_Delete;
            btnCancle.Text = Language.Common_Cancel;
        }

        private void ControlEnabled()
        {
            btnFileOpen.Enabled = !_isUploading;
            btnUpload.Enabled = !_isUploading && listFileName.Items.Count > 0;
            btnDelete.Enabled = !_isUploading && listServerFile.SelectedItems.Count > 0;
            btnCancle.Enabled = !_isUploading;
            lblClose.Enabled = !_isUploading;
            listFileName.Enabled = !_isUploading;
            listServerFile.Enabled = !_isUploading;
        }

        /// <summary>서버에 이미 붙어 있는 첨부를 다시 읽는다. 업로드 · 삭제 뒤에도 서버 기준으로 다시 그린다.</summary>
        private async Task LoadServerFilesAsync()
        {
            try
            {
                var list = await _fileApi.GetAttachmentsAsync(_refKey);
                if (IsDisposed || Disposing) return;

                listServerFile.BeginUpdate();
                listServerFile.Items.Clear();
                if (list != null)
                    listServerFile.Items.AddRange(list.Attachments.ToArray());
                listServerFile.EndUpdate();
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                AppLog.Error($"첨부 조회 실패 - {_refKey?.WorkOrderNo}", ex);
                AppMessageBox.Error(this, ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : $"{Language.Upload_Failed} ({(int)ex.StatusCode})");
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                    ControlEnabled();
            }
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

        /// <summary>미리보기를 비운다. 비트맵은 폼이 소유하므로 바꿀 때마다 이전 것을 정리한다.</summary>
        private void ClearPreview()
        {
            var old = picPreView.Image;
            picPreView.Image = null;
            old?.Dispose();
        }

        /// <summary>고른 로컬 파일이 사진이면 미리보기를 그린다. 읽어 둔 바이트로 그리므로 파일을 다시 열지 않는다.</summary>
        private void ShowPreview(FileBinary file)
        {
            ClearPreview();
            if (file == null || !file.IsImage)
                return;

            ShowPreviewBytes(file.Content, file.FileName);
        }

        /// <summary>
        /// 받은 바이트를 그림으로 바꿔 미리보기에 띄운다. PopScreen.LoadImageAsync와 같은 순서다:
        /// Base64 텍스트면 먼저 디코딩 → OLE 헤더 등 앞부분 제거 → Bitmap 복사본 생성.
        /// 이미지가 아니면 비워 둔다.
        /// </summary>
        private void ShowPreviewBytes(byte[] raw, string fileName)
        {
            ClearPreview();
            if (raw == null || raw.Length == 0)
                return;

            if (_imgCtrl.TryDecodeBase64(raw, out var decoded))
                raw = decoded;

            int offset = _imgCtrl.FindImageOffset(raw);
            if (offset < 0)
            {
                AppLog.Warn($"미리보기 실패(이미지 데이터 없음) - {fileName}");
                return;
            }

            if (offset > 0)
            {
                var trimmed = new byte[raw.Length - offset];
                Buffer.BlockCopy(raw, offset, trimmed, 0, trimmed.Length);
                raw = trimmed;
            }

            try
            {
                picPreView.Image = _imgCtrl.BytesToImage(raw);
            }
            catch (ArgumentException)
            {
                // 시그니처는 맞는데 내용이 깨진 경우 — 미리보기만 건너뛴다.
                AppLog.Warn($"미리보기 실패(이미지 형식 아님) - {fileName}");
            }
        }

        /// <summary>서버 첨부 한 건을 받아 미리보기에 띄운다. 이미지가 아니면 받지 않고 비워 둔다.</summary>
        private async Task ShowServerPreviewAsync(WorkOrderAttachmentDto item)
        {
            ClearPreview();
            if (item == null || !item.IsImage)
                return;

            try
            {
                var file = await _fileApi.DownloadAttachmentAsync(_refKey, item.AttachRef);
                if (IsDisposed || Disposing) return;

                // 받는 사이 다른 항목을 골랐으면 늦게 온 응답은 버린다 — 지금 선택과 다른 그림이 뜨지 않게.
                if (!ReferenceEquals(SingleSelectedServerFile(), item))
                    return;

                ShowPreviewBytes(file?.Content, file?.FileName ?? item.FileName);
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                // 미리보기는 보조 기능이라 안내 창 없이 로그만 남기고 비워 둔다.
                AppLog.Warn($"첨부 미리보기 다운로드 실패 - {item.FileName}: {ex.Message}");
            }
        }

        /// <summary>여러 개를 골랐으면(삭제용) 미리보기 대상이 없다.</summary>
        private WorkOrderAttachmentDto SingleSelectedServerFile()
        {
            return listServerFile.SelectedItems.Count == 1
                ? listServerFile.SelectedItem as WorkOrderAttachmentDto
                : null;
        }

        private void btnFileOpen_Click(object sender, EventArgs e)
        {
            string[] paths;
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = Language.Upload_SelectFile;
                dialog.Filter = "이미지|*.png;*.jpg;*.jpeg;*.bmp|PDF|*.pdf|모든 파일|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                paths = dialog.FileNames;
            }

            var failed = new List<string>();
            foreach (var path in paths)
            {
                if (TryLoad(path, out var file, out var reason))
                    listFileName.Items.Add(file);
                else
                    failed.Add($"{Path.GetFileName(path)} : {reason}");
            }

            // 마지막에 추가한 파일을 선택해 바로 미리보기가 뜨게 한다.
            if (listFileName.Items.Count > 0)
                listFileName.SelectedIndex = listFileName.Items.Count - 1;

            ControlEnabled();

            if (failed.Count > 0)
                AppMessageBox.Warn(this, Language.Upload_ReadFailed + "\n\n" + string.Join("\n", failed));
        }

        private void Close_Click(object sender, EventArgs e)
        {
            if (_isUploading)
                return;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void listFileName_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowPreview(listFileName.SelectedItem as FileBinary);
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            if (listFileName.Items.Count == 0)
                return;

            _isUploading = true;
            ControlEnabled();

            // 목록 전체를 한 요청에 담는다 — 성공/실패가 한 번에 정해지므로 하나씩 빼는 처리는 필요 없다.
            var files = listFileName.Items.Cast<FileBinary>().ToList();

            try
            {
                AppLog.Step($"파일 일괄 업로드 시작 - {_refKey.WorkOrderNo}, {files.Count}건 ({files.Sum(f => f.Length):N0} byte)");
                var result = await _fileApi.UploadManyAsync(files, _refKey);
                if (IsDisposed || Disposing) return;

                AppLog.Step($"파일 일괄 업로드 완료 - {_refKey.WorkOrderNo}, {files.Count}건");
                if (result != null)
                    _uploadedFiles.AddRange(result);
                listFileName.Items.Clear();
                ShowPreview(null);
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                // 한 요청이라 실패하면 아무것도 안 올라간 것으로 본다 — 목록을 그대로 두고 다시 누르면 된다.
                AppLog.Error($"파일 일괄 업로드 실패 - {_refKey.WorkOrderNo}, {files.Count}건", ex);
                AppMessageBox.Error(this, ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : $"{Language.Upload_Failed} ({(int)ex.StatusCode})");
                return;
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    _isUploading = false;
                    if (listFileName.Items.Count > 0 && listFileName.SelectedIndex < 0)
                        listFileName.SelectedIndex = 0;
                    ControlEnabled();
                }
            }

            // 창은 닫지 않고 서버 목록을 다시 받아, 올라간 파일을 바로 확인 · 삭제할 수 있게 한다.
            await LoadServerFilesAsync();
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            var targets = listServerFile.SelectedItems.Cast<WorkOrderAttachmentDto>().ToList();
            if (targets.Count == 0)
                return;

            if (!AppMessageBox.Confirm(this, string.Format(Language.Upload_DeleteConfirmFormat, targets.Count)))
                return;

            _isUploading = true;
            ControlEnabled();

            try
            {
                string atchRefs = string.Join(",", targets.Select(t => t.AttachRef));
                AppLog.Step($"첨부 삭제 시작 - {_refKey.WorkOrderNo}, atchRef={string.Join(",", targets.Select(t => t.AttachRef))}");
                await _fileApi.DeleteAttachmentsAsync(_refKey, targets.Select(t => t.AttachRef));
                //await _fileApi.DeleteAttachmentsAsync(_refKey, atchRefs);
                if (IsDisposed || Disposing) return;

                AppLog.Step($"첨부 삭제 완료 - {_refKey.WorkOrderNo}, {targets.Count}건");
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                AppLog.Error($"첨부 삭제 실패 - {_refKey.WorkOrderNo}", ex);
                AppMessageBox.Error(this, ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : $"{Language.Upload_DeleteFailed} ({(int)ex.StatusCode})");
                return;
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    _isUploading = false;
                    ControlEnabled();
                }
            }

            // 삭제 결과는 클라이언트가 목록에서 빼지 않고, 서버 목록을 다시 받아서 그린다.
            await LoadServerFilesAsync();
        }

        private async void listServerFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            ControlEnabled();
            await ShowServerPreviewAsync(SingleSelectedServerFile());
        }

        private void FileUploadForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isUploading)
                e.Cancel = true;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 미리보기 비트맵은 폼이 소유한다 — 닫을 때 같이 정리한다.
            picPreView.Image?.Dispose();
            picPreView.Image = null;
            base.OnFormClosed(e);
        }

        private void lblTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 현재 마우스 커서 포지션을 저장
                mousePoint = new Point(e.X, e.Y);
            }
        }

        private void lblTitle_MouseMove(object sender, MouseEventArgs e)
        {
            // 마우스 왼쪽 버튼이 눌린 상태로 움직일 때
            if (e.Button == MouseButtons.Left)
            {
                // 현재 폼의 위치를 마우스가 이동한 만큼 업데이트
                this.Location = new Point(this.Left - (mousePoint.X - e.X), this.Top - (mousePoint.Y - e.Y));
            }
        }
    }
}
