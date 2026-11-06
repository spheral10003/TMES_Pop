using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using SEOJIN_TMES.Client.ApiClient;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Common.Logging;
using SEOJIN_TMES.Client.Common.Models;
using SEOJIN_TMES.Client.Core;
using SEOJIN_TMES.Client.Forms.Common;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Pop
{
    /// <summary>
    /// 페이지 공정 목록 위에 붙이는 <b>투입자재 검색 줄</b> — 투입자재 코드로
    /// 그 자재를 투입하는 페이지 공정만 추린다. &lt;작업 진입 확인&gt; · POP [공정 목록] 팝업이 같이 쓴다.
    ///
    /// - 비우고 검색하면 전체 목록으로 돌아간다.
    /// - 코드 일부만 넣어도 찾고 대소문자는 가리지 않는다.
    ///
    /// 공정 목록 응답에는 자재가 없어서 페이지마다 POP 공정 정보(투입자재 포함)를 받아 확인한다.
    /// 한 번 받은 페이지는 기억해 두어 검색어를 바꿔도 다시 묻지 않는다.
    /// 결과는 <see cref="SearchCompleted"/>로 넘기고, 목록을 어떻게 갈아 끼울지는 붙인 화면이 정한다.
    ///
    /// VS 디자이너 호환을 위해 매개변수 없는 생성자만 두고, 실제 내용은 Initialize()로 넣는다.
    /// </summary>
    public partial class MaterialSearchBar : XtraUserControl
    {
        /// <summary>
        /// 페이지별 공정 정보를 동시에 몇 건까지 받을지.
        /// 페이지 수만큼 요청이 나가므로 한꺼번에 몰리지 않게 막고, 하나씩 기다리지도 않게 한다.
        /// </summary>
        private const int LookupConcurrency = 4;

        private WorkOrderEntryDto _workOrder;
        private List<RoutingProcessDto> _pages = new List<RoutingProcessDto>();

        /// <summary>페이지(서브 라우팅 키)별 투입자재 목록 캐시.</summary>
        private readonly Dictionary<string, List<PopInputMaterialDto>> _pageMaterials =
            new Dictionary<string, List<PopInputMaterialDto>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>검색이 끝나 목록을 바꿔야 할 때. 조회 실패 · 취소면 오지 않는다.</summary>
        public event EventHandler<MaterialSearchResultEventArgs> SearchCompleted;

        public MaterialSearchBar()
        {
            InitializeComponent();
        }

        /// <param name="workOrder">공정 정보 조회 키(작업지시번호 · 시리얼)에 쓴다.</param>
        /// <param name="pages">검색 대상 페이지 공정 전체(순번 순). 결과도 이 순서를 따른다.</param>
        public void Initialize(WorkOrderEntryDto workOrder, List<RoutingProcessDto> pages)
        {
            _workOrder = workOrder;
            _pages = pages ?? new List<RoutingProcessDto>();

            lblMaterialCode.Text = "· " + Language.Col_MaterialCode;
            btnSearch.Text = Language.WorkEntryConfirm_MaterialSearch;

            ApplyTheme();
        }

        public void ApplyTheme()
        {
            BackColor = AppTheme.Card;
            AppTheme.BodyText(lblMaterialCode);
            AppTheme.CodeEditor(txtMaterialCode);
            AppTheme.StandardButton(btnSearch);
        }

        private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();

        /// <summary>입력칸에서 Enter(바코드 스캐너 포함)는 [검색]과 같다.</summary>
        private async void txtCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;
            await SearchAsync();
        }

        private async Task SearchAsync()
        {
            var materialCode = txtMaterialCode.Text?.Trim() ?? string.Empty;

            if (materialCode.Length == 0)
            {
                RaiseSearchCompleted(_pages, string.Empty);
                return;
            }

            var owner = FindForm();
            var title = owner?.Text;   // 안내창 제목은 붙인 팝업의 제목을 따른다
            using (var busy = BusyOverlay.Show((Control)owner ?? this, cancelable: true))
            {
                btnSearch.Enabled = false;
                try
                {
                    await LoadPageMaterialsAsync(busy?.Token ?? CancellationToken.None);
                    if (IsDisposed || Disposing) return;
                }
                catch (OperationCanceledException)
                {
                    return;   // 사용자가 [취소]를 눌렀다 — 받아 둔 페이지는 캐시에 남아 다음 검색이 빨라진다
                }
                catch (ApiException ex)
                {
                    if (IsDisposed || Disposing) return;
                    AppLog.Warn($"[자재 검색] 공정 정보 조회 실패 ({materialCode}): {ex.Message}");
                    AppMessageBox.Warn(owner, ex.Message, title);
                    return;
                }
                finally
                {
                    if (!IsDisposed && !Disposing)
                        btnSearch.Enabled = true;
                }
            }

            var matches = FindPages(materialCode);
            AppLog.Info($"[자재 검색] {materialCode}: {matches.Count}/{_pages.Count} 페이지");
            RaiseSearchCompleted(matches, materialCode);

            if (matches.Count == 0)
                AppMessageBox.Info(owner, Language.WorkEntryConfirm_NoMaterialMatch, title);
        }

        private void RaiseSearchCompleted(List<RoutingProcessDto> pages, string condition)
        {
            SearchCompleted?.Invoke(this, new MaterialSearchResultEventArgs(pages, condition));
        }

        /// <summary>검색어가 투입자재 코드에 들어 있는 페이지(순서 유지). 대소문자는 가리지 않는다.</summary>
        private List<RoutingProcessDto> FindPages(string materialCode)
        {
            var result = new List<RoutingProcessDto>();
            foreach (var page in _pages)
            {
                if (page == null || string.IsNullOrEmpty(page.SubRoutingReference))
                    continue;

                List<PopInputMaterialDto> materials;
                if (!_pageMaterials.TryGetValue(page.SubRoutingReference, out materials))
                    continue;

                foreach (var material in materials)
                {
                    if (material?.MaterialCode != null
                        && material.MaterialCode.IndexOf(materialCode, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        result.Add(page);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>아직 받지 않은 페이지의 투입자재를 받아 캐시에 채운다(동시 요청 수 제한).</summary>
        private async Task LoadPageMaterialsAsync(CancellationToken cancellationToken)
        {
            var apiClient = Global.Instance.Settings.ApiClients.PopApiClient;
            var workstationRef = Global.Instance.Settings.CurrentUser?.WorkstationInfo?.WorkstationRef;

            using (var gate = new SemaphoreSlim(LookupConcurrency))
            {
                var tasks = new List<Task>();
                var requested = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var page in _pages)
                {
                    if (page == null || string.IsNullOrEmpty(page.SubRoutingReference))
                        continue;
                    if (_pageMaterials.ContainsKey(page.SubRoutingReference) || !requested.Add(page.SubRoutingReference))
                        continue;

                    tasks.Add(LoadOnePageAsync(apiClient, BuildKey(page, workstationRef), gate, cancellationToken));
                }

                // 하나가 실패해도 나머지가 끝날 때까지 기다린 뒤 예외가 올라온다 — 그래서 gate를 먼저 치우지 않는다.
                await Task.WhenAll(tasks);
            }
        }

        /// <summary>한 페이지의 공정 정보를 받아 투입자재만 캐시에 남긴다.</summary>
        private async Task LoadOnePageAsync(IPopApiClient apiClient, PopProcessKey key, SemaphoreSlim gate, CancellationToken cancellationToken)
        {
            await gate.WaitAsync(cancellationToken);
            try
            {
                var screen = await apiClient.GetProcessInfoAsync(key, cancellationToken);
                // await 뒤에도 UI 스레드로 돌아오므로(동기화 컨텍스트) 사전을 잠그지 않아도 된다.
                _pageMaterials[key.SubRoutingRef] = screen?.Materials ?? new List<PopInputMaterialDto>();
            }
            finally
            {
                gate.Release();
            }
        }

        /// <summary>페이지 공정 조회 키 — POP 화면(PopScreen)이 만드는 키와 같은 값으로 채운다.</summary>
        private PopProcessKey BuildKey(RoutingProcessDto page, string workstationRef)
        {
            var key = new PopProcessKey();
            key.WorkOrderNo = _workOrder?.WorkOrderNo;
            key.SerialNo = string.IsNullOrEmpty(page.SerialNo) ? _workOrder?.SerialNo : page.SerialNo;
            key.RoutingGroup = page.RoutingGroup;
            key.RoutingGroupRef = page.RoutingGroupRef;
            key.RoutingOperation = page.RoutingName;
            key.PageOperation = page.PageName;
            key.SubRoutingRef = page.SubRoutingReference;
            key.CorpCode = page.CorpCode;
            key.WorkstationRef = workstationRef;
            return key;
        }
    }

    /// <summary><see cref="MaterialSearchBar.SearchCompleted"/> 결과.</summary>
    public class MaterialSearchResultEventArgs : EventArgs
    {
        public MaterialSearchResultEventArgs(List<RoutingProcessDto> pages, string condition)
        {
            Pages = pages;
            Condition = condition;
        }

        /// <summary>보여 줄 페이지 공정(검색 조건이 없으면 전체).</summary>
        public List<RoutingProcessDto> Pages { get; }

        /// <summary>목록 제목에 붙일 검색 조건 문구. 전체로 돌아갈 때는 빈 문자열.</summary>
        public string Condition { get; }
    }
}
