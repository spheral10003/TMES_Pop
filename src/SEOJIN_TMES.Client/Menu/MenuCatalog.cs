using SEOJIN_TMES.Client.Forms.ProductionStatus;
using SEOJIN_TMES.Client.Forms.WorkIntegratedStatus;
using SEOJIN_TMES.Client.Forms.WorkStatus;
using SEOJIN_TMES.Client.Forms.WorkOrderEntry;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Menu
{
    /// <summary>
    /// 요구된 3-Depth 트리 메뉴 구조를 정의한다.
    ///   MEIS
    ///    ├─ 작업관리 > POP
    ///    ├─ 작업 현황 조회 > 생산 현황조회 / 작업 조회 현황
    ///    └─ 작업통합 개요 > 작업통합 현황 조회
    /// 화면(4번째 depth의 실제 Form)이 늘어나면 이 파일에 노드만 추가하면 된다.
    /// 각 노드의 Icon은 사이드바에 표시되는 SVG 아이콘(AppIcons)이다.
    /// </summary>
    public static class MenuCatalog
    {
        public static MenuNode BuildRoot()
        {
            var root = new MenuNode("MEIS", () => Language.Menu_MEIS);
            root.Icon = AppIcons.Dashboard;

            var workManagement = new MenuNode("WorkManagement", () => Language.Menu_WorkManagement);
            workManagement.Icon = AppIcons.Clipboard;
            // POP 메뉴는 진입(도입) 화면을 먼저 연다 — 작업지시 조회 → [작업] → 작업 진입 확인 → [이동] 시 같은 탭이 POP 화면으로 바뀐다.
            var pop = new MenuNode("POP", () => Language.Menu_POP, () =>
            {
                var screen = new WorkOrderEntryScreen();
                screen.Init();
                return screen;
            });
            pop.Icon = AppIcons.Factory;
            pop.Accent = AppTheme.Emerald;
            workManagement.AddChild(pop);

            var workStatusInquiry = new MenuNode("WorkStatusInquiry", () => Language.Menu_WorkStatusInquiry);
            workStatusInquiry.Icon = AppIcons.Chart;
            var productionStatus = new MenuNode("ProductionStatusInquiry", () => Language.Menu_ProductionStatusInquiry, () =>
            {
                var screen = new ProductionStatusScreen();
                screen.Init();
                return screen;
            });
            productionStatus.Icon = AppIcons.Chart;
            productionStatus.Accent = AppTheme.Cyan;
            workStatusInquiry.AddChild(productionStatus);

            // 작업지시 조건 조회 → 선택한 지시의 공정·스팩별 진행 현황까지 한 화면에서 본다.
            var workStatusSearch = new MenuNode("WorkStatusSearch", () => Language.Menu_WorkStatusSearch, () =>
            {
                var screen = new WorkStatusScreen();
                screen.Init();
                return screen;
            });
            workStatusSearch.Icon = AppIcons.Clipboard;
            workStatusSearch.Accent = AppTheme.Cyan;
            workStatusInquiry.AddChild(workStatusSearch);

            var workIntegratedOverview = new MenuNode("WorkIntegratedOverview", () => Language.Menu_WorkIntegratedOverview);
            workIntegratedOverview.Icon = AppIcons.Layers;

            var workIntegratedStatus = new MenuNode("WorkIntegratedStatusInquiry", () => Language.Menu_WorkIntegratedStatusInquiry, () =>
            {
                var screen = new WorkIntegratedStatusScreen();
                screen.Init();
                return screen;
            });
            workIntegratedStatus.Icon = AppIcons.Layers;
            workIntegratedStatus.Accent = AppTheme.Amber;
            workIntegratedOverview.AddChild(workIntegratedStatus);

            root.AddChild(workManagement);
            root.AddChild(workStatusInquiry);
            root.AddChild(workIntegratedOverview);

            return root;
        }
    }
}
