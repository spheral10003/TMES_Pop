using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Localization;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Localization;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 그리드에서 <b>컬럼을 지우는(숨기는) 조작만</b> 막는다. 정렬 · 이동 · 그룹 · 필터 등 나머지는 그대로 둔다.
    ///
    /// 머리글을 그리드 밖으로 끌어 놓거나 우클릭 메뉴의 "이 열 숨기기"를 누르면 컬럼이 사라지는데,
    /// 현장 작업자는 컬럼 선택 창으로 되살리는 방법을 모르고 화면을 다시 열어야만 돌아왔다.
    /// 그래서 끌어서 숨기기를 끄고, 우클릭 메뉴에서 숨기기 · 컬럼 선택 창 항목을 뺀다.
    ///
    /// <see cref="MeisGridView"/>는 생성될 때 스스로 적용한다.
    /// 일반 GridView · TreeList를 쓰는 화면은 초기화에서 직접 부른다.
    /// (POP 화면은 이보다 강하게 정렬만 남긴다 — PopScreen.LockGridCustomization.)
    /// </summary>
    public static class GridColumnGuard
    {
        public static void Apply(GridView view)
        {
            view.OptionsCustomization.AllowQuickHideColumns = false;
            view.PopupMenuShowing -= GridView_PopupMenuShowing;   // 두 번 불려도 한 번만 붙게
            view.PopupMenuShowing += GridView_PopupMenuShowing;
        }

        public static void Apply(TreeList tree)
        {
            tree.OptionsCustomization.AllowQuickHideColumns = false;
            tree.PopupMenuShowing -= TreeList_PopupMenuShowing;
            tree.PopupMenuShowing += TreeList_PopupMenuShowing;
        }

        private static void GridView_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.MenuType != GridMenuType.Column || e.Menu == null)
                return;

            foreach (DXMenuItem item in e.Menu.Items)
            {
                if (Equals(item.Tag, GridStringId.MenuColumnRemoveColumn)
                    || Equals(item.Tag, GridStringId.MenuColumnColumnCustomization))
                    item.Visible = false;
            }
        }

        private static void TreeList_PopupMenuShowing(object sender, DevExpress.XtraTreeList.PopupMenuShowingEventArgs e)
        {
            // 컬럼 선택 창 항목은 머리글 메뉴에만 있다 — 메뉴 종류는 따로 가리지 않는다.
            if (e.Menu == null)
                return;

            foreach (DXMenuItem item in e.Menu.Items)
            {
                if (Equals(item.Tag, TreeListStringId.MenuColumnColumnCustomization))
                    item.Visible = false;
            }
        }
    }
}
