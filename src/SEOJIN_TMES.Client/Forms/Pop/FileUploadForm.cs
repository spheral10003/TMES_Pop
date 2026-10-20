using DevExpress.XtraLayout.Customization;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Files;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    public partial class FileUploadForm : Form
    {
        private Point mousePoint;
        private IFileApiClient _fileApi;
        private string _category;
        private string _refKey;
        private long _maxBytes;

        /// <summary>전송 중에는 창을 닫지 못하게 한다 — 닫히면 Dispose된 창에 결과를 쓰게 된다.</summary>
        private bool _isUploading;

        private readonly List<FileUploadResponse> _uploadedFiles = new List<FileUploadResponse>();

        /// <summary>이번 창에서 서버에 올라간 파일들(서버 응답 그대로).</summary>
        public IReadOnlyList<FileUploadResponse> UploadedFiles => _uploadedFiles;

        public FileUploadForm(string key)
        {
            InitializeComponent();
            _refKey = key;
        }

        private void FileUploadForm_Load(object sender, EventArgs e)
        {
            Init();
            SetTexts();
            ControlEnabled();
        }

        private void Init()
        {
            _fileApi = Global.Instance.Settings.ApiClients.FileApiClient;
            _category = "";
            _refKey = "";
            _maxBytes = (long)Global.Instance.Settings.AppSet.UploadMaxMegaBytes * 1024 * 1024;
        }

        private void SetTexts()
        {
            lblTitle.Text = Language.Upload_Title;
            btnFileOpen.Text = Language.Upload_FileOpen;
            btnUpload.Text = Language.Upload_Upload;
            btnCancle.Text = Language.Common_Cancel;
        }

        private void ControlEnabled()
        {
            btnFileOpen.Enabled = !_isUploading;
            btnUpload.Enabled = !_isUploading && listFileName.Items.Count > 0;
            btnCancle.Enabled = !_isUploading;
            lblClose.Enabled = !_isUploading;
            listFileName.Enabled = !_isUploading;
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

        /// <summary>
        /// 고른 파일이 사진이면 미리보기를 그린다. 읽어 둔 바이트로 그리므로 파일을 다시 열지 않는다.
        /// Image.FromStream은 스트림이 살아 있어야 쓸 수 있어서, 복사본(new Bitmap)을 만든 뒤 스트림을 닫는다.
        /// </summary>
        private void ShowPreview(FileBinary file)
        {
            var old = picPreView.Image;
            picPreView.Image = null;
            old?.Dispose();

            if (file == null || !file.IsImage)
                return;

            try
            {
                using (var ms = new MemoryStream(file.Content))
                using (var original = Image.FromStream(ms))
                {
                    picPreView.Image = new Bitmap(original);
                }
            }
            catch (ArgumentException)
            {
                // 확장자는 이미지인데 내용이 깨진 파일 — 미리보기만 건너뛰고 업로드 여부는 서버 판단에 맡긴다.
                AppLog.Warn($"미리보기 실패(이미지 형식 아님) - {file.FileName}");
            }
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

            var total = listFileName.Items.Count;
            var done = 0;

            try
            {
                // 목록 맨 앞부터 하나씩 보내고, 성공한 파일은 목록에서 뺀다
                // → 중간에 실패해도 목록에는 "아직 안 올라간 파일"만 남는다.
                while (listFileName.Items.Count > 0)
                {
                    var file = (FileBinary)listFileName.Items[0];
                    lblTitle.Text = string.Format(Language.Upload_ProgressFormat, done + 1, total, file.FileName);

                    AppLog.Step($"파일 업로드 시작 - {file.FileName} ({file.Length:N0} byte, {file.ContentType})");
                    var result = await _fileApi.UploadAsync(file, _category, _refKey);
                    if (IsDisposed || Disposing) return;

                    AppLog.Step($"파일 업로드 완료 - {file.FileName}, FileId={result?.FileId}");
                    _uploadedFiles.Add(result);
                    listFileName.Items.RemoveAt(0);
                    done++;
                }

                // 전부 올라갔다 — 결과는 UploadedFiles로 넘기고 창을 자동으로 닫는다.
                _isUploading = false;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                AppLog.Error($"파일 업로드 실패 - {done}/{total} 완료 후 중단", ex);
                AppMessageBox.Error(this, ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : $"{Language.Upload_Failed} ({(int)ex.StatusCode})");
            }
            finally
            {
                if (!IsDisposed && !Disposing)
                {
                    _isUploading = false;
                    lblTitle.Text = Language.Upload_Title;
                    if (listFileName.Items.Count > 0 && listFileName.SelectedIndex < 0)
                        listFileName.SelectedIndex = 0;
                    ControlEnabled();
                }
            }
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
