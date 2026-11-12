namespace SEOJIN_TMES.Client.Menu
{
    /// <summary>
    /// 서버 메뉴 코드(menuCd). 사이드바에 보일 화면을 고를 때(<see cref="MenuCatalog"/>)와
    /// 화면별 버튼 권한을 받을 때(<c>Core.MenuPermission</c>) 같은 값을 쓴다 — 한쪽만 고쳐 어긋나지 않게 여기 한 곳에 둔다.
    /// </summary>
    public static class MenuCodes
    {
        /// <summary>작업관리 &gt; POP(작업지시 조회 → POP 화면).</summary>
        public const string Pop = "POP01";

        /// <summary>작업 현황 조회 &gt; 작업 조회 현황.</summary>
        public const string WorkStatusSearch = "POP02";

        /// <summary>작업통합 개요 &gt; 작업통합 현황 조회.</summary>
        public const string WorkIntegratedStatus = "POP03";

        /// <summary>작업관리 &gt; 첨부파일 관리. TODO: 가정값 — 서버 메뉴에 등록된 코드와 맞춰야 사이드바에 보이고 권한도 받는다.</summary>
        public const string AttachmentSearch = "POP04";
    }
}
