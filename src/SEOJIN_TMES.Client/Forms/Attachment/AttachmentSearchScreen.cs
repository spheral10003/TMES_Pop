using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Files;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Theme;
using SEOJIN_TMES.Client.Forms.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SEOJIN_TMES.Client.Forms.Attachment
{
    /// <summary>
    /// TMES &gt; 작업관리 &gt; 첨부파일 조회.
    ///
    /// 흐름:
    ///  1) 등록일자 범위 · 시리얼번호 · 작업지시번호로 [조회] → 좌측 [목록]
    ///     등록일자는 체크한 칸만 조건으로 보낸다. 날짜를 하나도 체크하지 않으면 시리얼번호 · 작업지시번호 중 하나는 필수다.
    ///     "첨부파일" 열은 목록 API가 내려준 건수(WorkOrderEntryDto.AttachmentCount)로 시작하고,
    ///     그 지시의 첨부를 받으면 첨부 API의 건수(WorkOrderAttachmentDto.AttachmentCount)로 바꿔 보여 준다.
    ///     "수정자" 열은 최종 수정자(updatedBy)다.
    ///  2) 목록에서 한 건을 클릭하면 → 우측 [파일 상세목록]에 그 작업지시의 첨부가 바로 표시된다(자동 조회)
    ///  3) 우측에서 체크한 파일을 [다운로드] · [삭제]하고, [업로드]로 올린다. 업로드 · 삭제 뒤에는 서버 목록을 다시 받는다.
    ///     체크 칸은 GridView의 CheckBoxRowSelect라 머리글 체크를 누르면 전체 체크 / 다시 누르면 전체 해제된다.
    ///
    /// FileUploadForm과 같은 API(IFileApiClient)를 쓴다. 파일 상세목록 맨 뒤 [미리보기] 칸(이미지 첨부만 사진 아이콘)에
    /// 마우스를 올리면 커서 옆에, 그 칸을 클릭하면 그 행 옆에 공유 폴더(DocumentShare.RootPath + attachFilePath)의 그림을 미리보기로 띄운다(<see cref="GridImagePreview"/>).
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

        /// <summary>파일 상세목록의 이미지 첨부 미리보기(마우스 오버 · 행 클릭). 탭이 닫힐 때(Disposed) 함께 해제한다.</summary>
        private GridImagePreview _preview;

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
            ApplyTheme();

            // 일반 GridView라 MeisGridView처럼 스스로 적용되지 않는다 — 컬럼 지우기만 막는다.
            GridColumnGuard.Apply(gridViewOrders);
            GridColumnGuard.Apply(gridViewFiles);

            // 등록일자 기본값: 최근 일주일.
            dtpFrom.Value = DateTime.Today.AddDays(-6);
            dtpTo.Value = DateTime.Today;

            // 언어를 바꾸면 Init()이 다시 불리므로 이전 미리보기를 먼저 정리한다(이벤트가 두 번 걸리지 않게).
            _preview?.Dispose();
            _preview = new GridImagePreview(gridViewFiles, PreviewPathOf, _appSet.DocShare, _appSet.AppSet.DocumentShare.RootPath);
            Disposed -= AttachmentSearchScreen_Disposed;
            Disposed += AttachmentSearchScreen_Disposed;

            ControlEnabled();
        }

        /// <summary>
        /// 미리보기할 그림 경로 — 올라간(업로드된) 첨부 중 이미지 파일만. 문서(PDF 등)는 미리보기하지 않는다.
        /// 이미지 여부는 파일 유형(확장자, FileUploadResponse.IsImage)으로 판단한다.
        /// </summary>
        private string PreviewPathOf(object row)
        {
            var file = row as FileUploadResponse;
            if (file == null || !file.IsImage)
                return null;
            return file.AttachFilePath;
        }

        private void AttachmentSearchScreen_Disposed(object sender, EventArgs e)
        {
            _preview?.Dispose();
            _preview = null;
        }

        /// <summary>버튼 색 — 앱 공통 버튼 색(글꼴은 Designer 값 유지). 그리드 구분선은 다른 목록과 같은 색.</summary>
        private void ApplyTheme()
        {
            foreach (var button in new[] { btnSearch, btnUpload, btnDelete, btnDownload })
                AppTheme.StandardButton(button, keepFont: true);

            AppTheme.GridLines(gridViewOrders);
            AppTheme.GridLines(gridViewFiles);
        }

        /// <summary>
        /// 화면 문구를 현재 UI 문화권(로그인 · 메인 화면에서 고른 언어)으로 입힌다.
        /// 메인 화면에서 언어를 바꾸면 MainForm이 열린 탭을 닫았다 다시 열어 Init()이 다시 불리므로 여기서 한 번만 읽으면 된다.
        /// </summary>
        private void ApplyTexts()
        {
            lblRegDate.Text = "· " + Language.Attach_RegDate;
            lblSerialNo.Text = "· " + Language.Col_SerialNo;
            lblWorkOrderNo.Text = "· " + Language.Col_WorkOrderNo;
            btnSearch.Text = Language.Common_Search;

            lblOrderTitle.Text = "■ " + Language.Attach_ListTitle;
            lblFileTitle.Text = "■ " + Language.Attach_FileListTitle;
            btnDownload.Text = Language.Attach_Download;
            btnDelete.Text = Language.Attach_Delete;
            btnUpload.Text = Language.Attach_Upload;

            colOrderNo.Caption = Language.Col_No;
            colWorkOrderNo.Caption = Language.Col_WorkOrderNo;
            colSerialNo.Caption = Language.Col_SerialNo;
            colAttachmentCount.Caption = Language.Col_AttachmentCount;
            colCreatedDate.Caption = Language.Col_CreatedDate;
            colUpdatedBy.Caption = Language.Col_UpdatedBy;

            colFileNo.Caption = Language.Col_No;
            colFileName.Caption = Language.Col_FileName;
            colFileType.Caption = Language.Col_FileType;
            colFileSize.Caption = Language.Col_FileSize;
            colCreatedBy.Caption = Language.Col_CreatedBy;
            colCreatedAt.Caption = Language.Col_CreatedAt;

            ArrangeByTextWidth();
        }

        /// <summary>
        /// 문구 길이에 맞춰 조회 조건과 첨부 버튼(다운로드 · 삭제 · 업로드)을 다시 배치한다.
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

            // 버튼은 문구가 넘치지 않게 넓히고, 파일 상세목록 제목 줄의 오른쪽 끝부터 [업로드] → [삭제] → [다운로드] 순으로 채운다.
            // Designer에서 Top | Right로 고정해 두었으므로 여기서 한 번 맞추면 창 크기가 바뀌어도 오른쪽 끝에 붙어 있다.
            int right = btnUpload.Right;
            foreach (var button in new[] { btnUpload, btnDelete, btnDownload })
            {
                button.Width = Math.Max(76, TextRenderer.MeasureText(button.Text, button.Font).Width + 20);
                button.Left = right - button.Width;
                right = button.Left - 6;
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

        /// <summary>
        /// 작업지시 목록을 조회한다. 조회 조건(<see cref="IsSearchConditionValid"/>)이 맞지 않으면 안내만 하고 API를 부르지 않는다.
        /// 등록일자는 체크한 칸만 보내고, 체크하지 않은 칸은 조건에서 뺀다(null).
        /// </summary>
        private async Task SearchAsync()
        {
            DateTime? from = dtpFrom.Checked ? dtpFrom.Value.Date : (DateTime?)null;
            DateTime? to = dtpTo.Checked ? dtpTo.Value.Date : (DateTime?)null;
            var serialNo = txtSerialNo.Text?.Trim();
            var workOrderNo = txtWorkOrderNo.Text?.Trim();

            if (!IsSearchConditionValid(from, to, serialNo, workOrderNo))
                return;

            _query.CorpCode = _appSet.CurrentUser?.Server;
            _query.OrderDateFrom = from;
            _query.OrderDateTo = to;
            _query.WorkOrderNo = workOrderNo;
            _query.SerialNo = serialNo;

            _isBusy = true;
            ControlEnabled();

            try
            {
                var result = await _appSet.ApiClients.WorkStatusApiClient.SearchWorkOrdersAsync(_query)
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

        /// <summary>
        /// 조회 조건을 확인한다. 맞지 않으면 사유를 알리고 해당 입력칸으로 포커스를 옮긴 뒤 false.
        ///  - 등록일자를 하나도 체크하지 않았으면 시리얼번호 · 작업지시번호 중 하나는 있어야 한다
        ///    (조건 없이 전체를 조회하면 결과가 지나치게 많아진다).
        ///  - 등록일자가 있으면 시리얼번호 · 작업지시번호는 비워도 된다.
        ///  - 시작 · 종료를 모두 체크했으면 시작이 종료보다 늦으면 안 된다.
        /// </summary>
        private bool IsSearchConditionValid(DateTime? from, DateTime? to, string serialNo, string workOrderNo)
        {
            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                AppMessageBox.Warn(this, Language.WorkStatus_DateReversed, Language.Attach_Title);
                dtpFrom.Focus();
                return false;
            }

            bool hasDate = from.HasValue || to.HasValue;
            if (!hasDate && string.IsNullOrEmpty(serialNo) && string.IsNullOrEmpty(workOrderNo))
            {
                AppMessageBox.Warn(this, Language.Attach_ConditionRequired, Language.Attach_Title);
                txtSerialNo.Focus();
                return false;
            }

            return true;
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

                var attachments = result?.Attachments ?? new List<WorkOrderAttachmentDto>();
                BindFiles(attachments.Select(ToFileResponse).ToList());
                ApplyAttachmentCount(order, attachments);
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                ClearFiles();
                AppLog.Error($"첨부 조회 실패 - {order.WorkOrderNo}", ex);
                AppMessageBox.Error(this, ex.WithLead(ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : Language.Upload_Failed), Language.Attach_Title);
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
            file.AttachFilePath = item.AttachFilePath;
            file.CreatedBy = item.CreatedBy;
            file.UpdatedBy = item.UpdatedBy;
            file.CreatedAt = item.CreatedAt;
            return file;
        }

        /// <summary>
        /// 목록의 "첨부파일" 열을 첨부 API가 준 건수(attachmentCount)로 바꾼다 — 업로드 · 삭제 뒤에도 목록 건수가 맞도록.
        /// 직접 세지 않고 서버 값만 쓴다. 첨부가 하나도 없으면 건수를 실어 줄 행이 없으므로 0으로 둔다.
        /// </summary>
        private void ApplyAttachmentCount(WorkOrderEntryDto order, List<WorkOrderAttachmentDto> attachments)
        {
            if (attachments.Count == 0)
            {
                order.AttachmentCount = 0;
            }
            else if (int.TryParse(attachments[0].AttachmentCount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count))
            {
                order.AttachmentCount = count;
            }
            else
            {
                return;   // 서버가 건수를 주지 않았으면 목록 API 값을 그대로 둔다
            }

            int handle = gridViewOrders.FindRow(order);
            if (handle != GridControl.InvalidRowHandle)
                gridViewOrders.RefreshRow(handle);
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

        /// <summary>체크한 행이 바뀌면(머리글 전체 체크 · 해제 포함) [다운로드] · [삭제] 사용 여부를 다시 정한다.</summary>
        private void gridViewFiles_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {
            ControlEnabled();
        }

        /// <summary>
        /// 다운로드 · 삭제 대상 — 파일 상세목록에서 체크한 행.
        /// CheckBoxRowSelect 모드에서는 체크 상태가 곧 선택 상태라 GetSelectedRows()가 체크한 행을 돌려준다.
        /// </summary>
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

            // 삭제는 되돌릴 수 없으므로 체크한 건수를 보여 주고 확인을 받은 뒤에만 지운다.
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
                AppMessageBox.Error(this, ex.WithLead(ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : Language.Upload_DeleteFailed), Language.Attach_Title);
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

            try
            {
                AppLog.Step($"파일 일괄 업로드 시작 - {order.WorkOrderNo}, {files.Count}건 ({files.Sum(f => f.Length):N0} byte)");
                List<FileUploadResponse> uploaded;

                // 큰 문서는 몇 분씩 걸리므로 보낸 양을 막대로 보여 준다. 되돌릴 수 없는 처리라 [취소]는 두지 않는다.
                // Progress는 여기(UI 스레드)에서 만들어야 보고가 UI 스레드로 넘어온다.
                using (var busy = BusyOverlay.Show(this))
                {
                    var progress = new Progress<TransferProgress>(p => busy?.SetProgress(p.Percent,
                        $"{Language.Upload_ProgressFormat} {p.Percent}% ({FormatSize(p.BytesSent)} / {FormatSize(p.TotalBytes)})"));

                    uploaded = await _fileApi.UploadManyAsync(files, KeyOf(order), progress);
                }
                if (IsDisposed || Disposing) return;

                AppLog.Step($"파일 일괄 업로드 완료 - {order.WorkOrderNo}, {uploaded?.Count ?? files.Count}건");
            }
            catch (ApiException ex)
            {
                if (IsDisposed || Disposing) return;

                // 한 요청이라 실패하면 아무것도 안 올라간 것으로 본다.
                AppLog.Error($"파일 일괄 업로드 실패 - {order.WorkOrderNo}, {files.Count}건", ex);
                AppMessageBox.Error(this, ex.WithLead(ex.IsTransportFailure
                    ? Language.Upload_ConnectionError
                    : Language.Upload_Failed), Language.Attach_Title);
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
