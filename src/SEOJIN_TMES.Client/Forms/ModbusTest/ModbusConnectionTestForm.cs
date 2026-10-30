using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Columns;
using SEOJIN_TMES.Client.Common.Configuration;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Equipment.Diagnostics;
using SEOJIN_TMES.Client.Equipment.Management;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.ModbusTest
{
    /// <summary>
    /// &lt;Modbus TCP 접속 테스트&gt; 팝업 — 메인 화면 상단 바의 네트워크 아이콘으로 연다.
    ///
    /// 설비마다 Request 포트에 붙고 → Event 포트까지 붙으면 성공, 그 뒤 둘 다 끊는다. 이것을 반복 횟수만큼 되풀이해
    /// 성공/실패 횟수를 센다. 현장에서 "가끔 접속이 안 된다"는 설비를 재현·확인하려는 용도다.
    /// 여러 대를 넣으면 <b>설비끼리는 동시에</b> 돌고, 한 설비 안에서는 순서대로 반복한다.
    ///
    /// POP가 쓰는 운영 연결(<see cref="INutrunnerManager"/>)과는 별개의 연결을 연다 — POP가 붙어 있는 설비를
    /// 테스트하면 장비가 받는 동시 접속 수에 따라 실패로 나올 수 있다.
    ///
    /// 창틀은 <see cref="Pop.StopIssueForm"/>과 같다 — Windows 제목 줄 대신 pnlTitle + lblTitle + lblClose(X).
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class ModbusConnectionTestForm : XtraForm
    {
        /// <summary>접속 기록 창에 남기는 최대 글자 수. 넘으면 앞쪽 절반을 버린다(반복이 길면 메모가 한없이 커진다).</summary>
        private const int MaxLogLength = 200000;

        /// <summary>이 창의 강조 색(제목 띠 · 그리드 포커스).</summary>
        private readonly Color _accent = AppTheme.Teal;

        /// <summary>창을 줄일 수 있는 한계. 이보다 작으면 옵션 줄의 버튼이 겹친다.</summary>
        private readonly Size _minimumFormSize = new Size(900, 520);

        private readonly BindingList<ModbusTestTarget> _targets = new BindingList<ModbusTestTarget>();
        private AppSettings _appSet;
        private CancellationTokenSource _runCancel;

        public ModbusConnectionTestForm()
        {
            InitializeComponent();
        }

        /// <summary>문구 · 색 · 창틀을 입히고, 등록된 설비로 대상 목록을 채운다.</summary>
        public void Initialize()
        {
            _appSet = Global.Instance.Settings.AppSet;

            Text = Language.ModbusTest_Title;
            lblTitle.Text = Language.ModbusTest_Title;
            lblRepeat.Text = Language.ModbusTest_RepeatCount;
            lblTimeout.Text = Language.ModbusTest_TimeoutMs;
            lblIntervalInfo.Text = string.Format(Language.ModbusTest_IntervalInfo, _appSet.ModbusTestIntervalMs);

            // 접속 대기 시간은 App.config 값으로 시작하되 화면에서 바로 바꿔 시험한다 —
            // POP와 같은 값(ModbusConnectTimeoutMs)으로 재현해 보고, 늘리면 붙는지 비교하는 용도다.
            spinTimeout.Value = Math.Max(spinTimeout.Properties.MinValue, Math.Min(spinTimeout.Properties.MaxValue, _appSet.ModbusTestTimeoutMs));
            lblListTitle.Text = Language.ModbusTest_ListTitle;
            lblLogTitle.Text = Language.ModbusTest_LogTitle;
            btnLoadEquipments.Text = Language.ModbusTest_LoadEquipments;
            btnAddRow.Text = Language.ModbusTest_AddRow;
            btnRemoveRow.Text = Language.ModbusTest_RemoveRow;
            btnStart.Text = Language.ModbusTest_Start;
            btnStop.Text = Language.ModbusTest_Stop;
            btnClose.Text = Language.Common_Close;

            ApplyTheme();
            DialogChrome.Apply(this, pnlTitle, lblClose);
            DialogChrome.FitPopupSize(this);
            DialogChrome.EnableResizeGrip(pnlGrip);
            MinimumSize = _minimumFormSize;
            ConfigureGrid();

            // 대개 POP에 등록된 설비를 그대로 시험하므로 처음부터 채워 둔다(없으면 빈 행 하나).
            AddRegisteredEquipments();
            if (_targets.Count == 0)
                _targets.Add(new ModbusTestTarget());

            SetRunning(false);
            FormClosing += ModbusConnectionTestForm_FormClosing;
        }

        private void ApplyTheme()
        {
            AppTheme.TitleBar(pnlTitle, lblTitle, _accent);
            AppTheme.TitleBarClose(lblClose, _accent);
            foreach (var button in new[] { btnLoadEquipments, btnAddRow, btnRemoveRow, btnStart, btnStop, btnClose })
                AppTheme.StandardButton(button, keepFont: true);

            pnlBody.BackColor = AppTheme.Surface;
            pnlOptions.BackColor = AppTheme.Surface;
            pnlFooter.BackColor = AppTheme.Surface;
            pnlGrip.BackColor = AppTheme.Surface;

            foreach (var label in new[] { lblListTitle, lblLogTitle })
            {
                label.ForeColor = AppTheme.TextPrimary;
                label.Font = AppTheme.SectionFont;
            }

            foreach (var label in new[] { lblRepeat, lblTimeout })
            {
                label.ForeColor = AppTheme.TextPrimary;
                label.Font = AppTheme.BodyBoldFont;
            }
            lblIntervalInfo.ForeColor = AppTheme.TextMuted;
            lblIntervalInfo.Font = AppTheme.BodyFont;

            AppTheme.Editor(spinRepeat);
            AppTheme.Editor(spinTimeout);
            AppTheme.Grid(grid, gridView, _accent);
            AppTheme.Console(memoLog);
        }

        private void ConfigureGrid()
        {
            grid.DataSource = _targets;
            GridColumns.Apply(gridView, Language.Col_No,
                GridColumns.Left(nameof(ModbusTestTarget.Host), Language.ModbusTest_ColHost, 160),
                GridColumns.Text(nameof(ModbusTestTarget.Port), Language.ModbusTest_ColPort, 80),
                GridColumns.Text(nameof(ModbusTestTarget.EventPort), Language.ModbusTest_ColEventPort, 90),
                GridColumns.Text(nameof(ModbusTestTarget.Progress), Language.ModbusTest_ColProgress, 100),
                GridColumns.Number(nameof(ModbusTestTarget.SuccessCount), Language.ModbusTest_ColSuccess, 80),
                GridColumns.Number(nameof(ModbusTestTarget.FailCount), Language.ModbusTest_ColFail, 80),
                GridColumns.Text(nameof(ModbusTestTarget.State), Language.ModbusTest_ColState, 90),
                GridColumns.Left(nameof(ModbusTestTarget.LastResult), Language.ModbusTest_ColLastResult, 300));
            gridView.OptionsView.ColumnAutoWidth = true;

            // AppTheme.Grid는 모든 목록을 읽기 전용으로 만든다 — 이 화면만 IP · 포트를 그리드에서 바로 고친다.
            gridView.OptionsBehavior.Editable = true;
            foreach (GridColumn column in gridView.Columns)
                column.OptionsColumn.AllowEdit = IsInputColumn(column.FieldName);
        }

        private static bool IsInputColumn(string fieldName) =>
            fieldName == nameof(ModbusTestTarget.Host) ||
            fieldName == nameof(ModbusTestTarget.Port) ||
            fieldName == nameof(ModbusTestTarget.EventPort);

        /// <summary>POP 설비 매니저에 등록된 설비(서버에서 받은 장비 목록)를 대상으로 넣는다.</summary>
        private void AddRegisteredEquipments()
        {
            var manager = Global.Instance.Devices.Manager;
            foreach (var id in manager.Ids)
            {
                var info = manager[id]?.Info;
                if (info == null || string.IsNullOrWhiteSpace(info.Ip))
                    continue;

                var target = new ModbusTestTarget();
                target.Host = info.Ip;
                target.Port = info.Port;
                target.EventPort = info.ResolvedEventPort;
                _targets.Add(target);
            }
        }

        private void btnLoadEquipments_Click(object sender, EventArgs e)
        {
            gridView.CloseEditor();
            if (Global.Instance.Devices.Manager.Count == 0)
            {
                AppMessageBox.Warn(this, Language.ModbusTest_NoEquipments, Language.ModbusTest_Title);
                return;
            }

            _targets.Clear();
            AddRegisteredEquipments();
        }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            gridView.CloseEditor();
            _targets.Add(new ModbusTestTarget());
            gridView.FocusedRowHandle = gridView.GetRowHandle(_targets.Count - 1);
            gridView.FocusedColumn = gridView.Columns.ColumnByFieldName(nameof(ModbusTestTarget.Host));
            gridView.ShowEditor();
        }

        private void btnRemoveRow_Click(object sender, EventArgs e)
        {
            gridView.CloseEditor();
            if (gridView.GetFocusedRow() is ModbusTestTarget target)
                _targets.Remove(target);
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                await Run();
            }
            catch (Exception ex)
            {
                // async void라 여기서 새면 앱이 그대로 종료된다.
                AppLog.Error("[접속 테스트] 실행 중 예외", ex);
                if (IsDisposed || Disposing) return;
                SetRunning(false);
                AppMessageBox.Error(this, ex.Message, Language.ModbusTest_Title);
            }
        }

        private void btnStop_Click(object sender, EventArgs e) => _runCancel?.Cancel();

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>테스트 중 닫아도 남은 접속 시도를 바로 멈춘다.</summary>
        private void ModbusConnectionTestForm_FormClosing(object sender, FormClosingEventArgs e) => _runCancel?.Cancel();

        /// <summary>입력이 올바르면 전 대상을 동시에 돌리고, 끝나면(중지 포함) 성공/실패 횟수를 리포트한다.</summary>
        private async Task Run()
        {
            gridView.CloseEditor();
            gridView.UpdateCurrentRow();

            var targets = _targets.ToList();
            if (!ValidateTargets(targets))
                return;

            var repeat = (int)spinRepeat.Value;
            var timeoutMs = (int)spinTimeout.Value;
            var intervalMs = _appSet.ModbusTestIntervalMs;

            foreach (var target in targets)
                target.Reset(repeat, Language.ModbusTest_StateRunning);

            memoLog.Text = string.Empty;
            AppLog.Step($"[접속 테스트] 시작 - 대상 {targets.Count}대, 반복 {repeat}회, 대기 {timeoutMs}ms");

            _runCancel = new CancellationTokenSource();
            SetRunning(true);

            var token = _runCancel.Token;
            var stopped = false;
            try
            {
                await Task.WhenAll(targets.Select(target => RunTarget(target, repeat, timeoutMs, intervalMs, token)));
            }
            catch (OperationCanceledException)
            {
                stopped = true;
            }
            finally
            {
                _runCancel.Dispose();
                _runCancel = null;
            }

            if (IsDisposed || Disposing) return;   // 테스트 중 팝업을 닫았다

            SetRunning(false);
            foreach (var target in targets)
                target.State = stopped ? Language.ModbusTest_StateStopped : Language.ModbusTest_StateDone;

            ShowReport(targets, stopped);
        }

        /// <summary>설비 1대를 반복 횟수만큼 붙였다 끊는다. 실패해도 멈추지 않고 다음 회차로 넘어간다(실패 빈도를 보는 것이 목적).</summary>
        private async Task RunTarget(ModbusTestTarget target, int repeat, int timeoutMs, int intervalMs, CancellationToken token)
        {
            for (var i = 1; i <= repeat; i++)
            {
                token.ThrowIfCancellationRequested();

                var result = await ModbusConnectionProbe.Probe(target.Host, target.Port, target.EventPort, timeoutMs, token);
                if (IsDisposed || Disposing) return;

                var resultText = DescribeResult(result, timeoutMs);
                target.Record(result.Success, resultText);
                AppendLog($"{DateTime.Now:HH:mm:ss.fff}  {target.Endpoint}  #{i}  {resultText}");

                if (!result.Success)
                    AppLog.Warn($"[접속 테스트] {target.Endpoint} #{i} 실패 - {result.FailedChannel} :: {(result.TimedOut ? "시간 초과" : result.ErrorMessage)}");

                if (i < repeat && intervalMs > 0)
                    await Task.Delay(intervalMs, token);
            }

            AppLog.Info($"[접속 테스트] {target.Endpoint} 완료 - 성공 {target.SuccessCount} / 실패 {target.FailCount}");
        }

        private static string DescribeResult(ConnectionProbeResult result, int timeoutMs)
        {
            if (result.Success)
                return string.Format(Language.ModbusTest_ResultOk, result.ElapsedMs);

            var port = result.FailedChannel == ProbeChannel.Event
                ? Language.ModbusTest_ColEventPort
                : Language.ModbusTest_ColPort;
            var reason = result.TimedOut
                ? string.Format(Language.ModbusTest_Timeout, timeoutMs)
                : result.ErrorMessage;
            return string.Format(Language.ModbusTest_ResultFail, port, reason);
        }

        /// <summary>빈 행은 건너뛰지 않고 막는다 — 대상 수가 입력한 것과 다르게 리포트되면 헷갈린다.</summary>
        private bool ValidateTargets(List<ModbusTestTarget> targets)
        {
            if (targets.Count == 0)
            {
                AppMessageBox.Warn(this, Language.ModbusTest_NoTargets, Language.ModbusTest_Title);
                return false;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                target.Host = target.Host?.Trim();
                if (string.IsNullOrEmpty(target.Host) || !IsValidPort(target.Port) || !IsValidPort(target.EventPort))
                {
                    gridView.FocusedRowHandle = gridView.GetRowHandle(i);
                    AppMessageBox.Warn(this, string.Format(Language.ModbusTest_InvalidTarget, i + 1), Language.ModbusTest_Title);
                    return false;
                }
            }

            return true;
        }

        private static bool IsValidPort(int port) => port > 0 && port <= 65535;

        private void ShowReport(List<ModbusTestTarget> targets, bool stopped)
        {
            var report = new StringBuilder();
            if (stopped)
                report.AppendLine(Language.ModbusTest_ReportStopped).AppendLine();

            foreach (var target in targets)
            {
                report.AppendLine(string.Format(Language.ModbusTest_ReportLine,
                    target.Endpoint, target.SuccessCount, target.FailCount, target.SuccessCount + target.FailCount));
            }

            AppLog.Info($"[접속 테스트] {(stopped ? "중지" : "종료")}{Environment.NewLine}{report}");

            if (targets.Any(IsFailed))
                AppMessageBox.Warn(this, report.ToString().TrimEnd(), Language.ModbusTest_Title);
            else
                AppMessageBox.Success(this, report.ToString().TrimEnd(), Language.ModbusTest_Title);
        }

        private static bool IsFailed(ModbusTestTarget target) => target.FailCount > 0;

        private void AppendLog(string line)
        {
            var box = memoLog.MaskBox;
            if (box.TextLength > MaxLogLength)
                memoLog.Text = memoLog.Text.Substring(memoLog.Text.Length / 2);

            box.AppendText(line + Environment.NewLine);
        }

        /// <summary>테스트 중에는 대상 목록 · 반복 횟수를 잠근다(돌고 있는 대상의 IP가 바뀌면 결과가 엉킨다).</summary>
        private void SetRunning(bool running)
        {
            btnStart.Enabled = !running;
            btnStop.Enabled = running;
            btnLoadEquipments.Enabled = !running;
            btnAddRow.Enabled = !running;
            btnRemoveRow.Enabled = !running;
            spinRepeat.Enabled = !running;
            spinTimeout.Enabled = !running;
            gridView.OptionsBehavior.Editable = !running;
        }

        /// <summary>부모 창(없으면 화면) 가운데에 띄운다.</summary>
        public void CenterOn(IWin32Window owner)
        {
            var ownerForm = (owner as Control)?.FindForm();
            var bounds = ownerForm != null ? ownerForm.Bounds : Screen.PrimaryScreen.WorkingArea;

            Location = new Point(
                bounds.Left + ((bounds.Width - Width) / 2),
                bounds.Top + ((bounds.Height - Height) / 2));
        }

        private void lblClose_MouseEnter(object sender, EventArgs e) =>
            lblClose.BackColor = AppTheme.TitleBarCloseHover(_accent);

        private void lblClose_MouseLeave(object sender, EventArgs e) =>
            lblClose.BackColor = _accent;
    }
}
