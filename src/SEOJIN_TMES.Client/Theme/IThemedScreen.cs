namespace SEOJIN_TMES.Client.Theme
{
    /// <summary>
    /// 테마(라이트/다크) 전환에 반응하는 화면.
    /// 상단바의 테마 전환 버튼을 누르면 MainForm이 열려 있는 모든 화면의 ApplyTheme()을 다시 호출한다.
    /// 각 화면은 자신의 컨트롤에 AppTheme.* 스타일을 다시 입히기만 하면 된다.
    /// </summary>
    public interface IThemedScreen
    {
        void ApplyTheme();
    }
}
