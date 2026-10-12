using SEOJIN_TMES.Client.Composition;
using SEOJIN_TMES.Client.Theme;
using System;

namespace SEOJIN_TMES.Client.Core
{
    public class RuntimeState
    {
        public EventHandler EventChangedMode = null;
        public EventHandler EventChangedAuthorization = null;

        public string Version { get;}

        public enum MODE { READY, AUTO, ALARM, SIMULATION, CALIBRATION };

        private MODE _modePrev = MODE.READY;
        private MODE _mode = MODE.READY;

        public MODE Mode
        {
            get { return _mode; }
            set
            {
                _modePrev = _mode;
                _mode = value;

                if (_mode != _modePrev)
                {
                    if (EventChangedMode != null)
                    {
                        EventChangedMode(null, null);
                    }
                }
            }
        }

        public RuntimeState()
        {
            // 처리되지 않은 예외를 .NET 기본 오류 창/디버거 중단 대신 공통 안내 창으로 바꾼다.
            // (서버 미기동 상태에서 API 호출이 터져도 앱이 죽지 않고 계속 쓸 수 있게 하는 마지막 방어선)
            GlobalErrorHandler.Install();

            // 전역 스킨 + SVG 팔레트를 적용한다(자세한 색 체계는 Theme/AppTheme.cs).
            // XtraForm/XtraUserControl은 별도 설정 없이 UserLookAndFeel.Default를 그대로 따라가므로
            // 이 한 줄로 전체 화면 톤(에디터/그리드/스크롤바 등)이 남보라 계열로 맞춰진다.
            // 상단바의 달/해 버튼을 누르면 AppTheme.ApplySkin(true/false)로 다크 모드로 전환된다.
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.Skins.SkinManager.EnableFormSkins();
            AppTheme.ApplySkin(false);
        }

    }
}
