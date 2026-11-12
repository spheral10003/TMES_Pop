using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Composition;

namespace SEOJIN_TMES.Client.Menu
{
    /// <summary>
    /// 3-Depth 트리 메뉴(TMES > 대분류 > 화면)의 노드 하나.
    /// TextAccessor를 Func로 두는 이유는 언어 전환 시 리소스를 다시 읽어 메뉴 문구를 즉시 갱신하기 위함이다.
    /// 리프 노드(FormFactory != null)만 실제 화면을 연다.
    /// </summary>
    public class MenuNode
    {
        public string Key { get; }
        public Func<string> TextAccessor { get; }
        public List<MenuNode> Children { get; } = new List<MenuNode>();
        public Func<Control> ScreenFactory { get; }

        /// <summary>
        /// 서버 메뉴 코드(POP01, POP02 …). 서버가 내려준 메뉴(<c>menuCd</c>)와 이 값으로 매칭해 화면 노출 여부를 정한다.
        /// 화면 노드 생성자에서만 받는다 — 화면과 코드를 한 줄에서 묶어 두어야 코드 누락 · 엇갈림이 눈에 띈다.
        /// Key는 탭 · 브레드크럼 식별용 내부 이름이라 서버 코드와 분리해 둔다. 분류 노드는 null이다.
        /// </summary>
        public string MenuCode { get; }

        /// <summary>화면을 여는 노드인지. 서버 메뉴와 매칭되는 대상은 이 노드들뿐이다.</summary>
        public bool IsScreen => ScreenFactory != null;

        /// <summary>메뉴 아이콘(AppIcons의 SVG 템플릿 문자열). 지정하지 않으면 점 아이콘이 사용된다.</summary>
        public string Icon { get; set; }

        /// <summary>
        /// 리프(화면) 항목 앞에 찍는 점(●)의 색. 화면마다 다른 색을 주면 좌측 메뉴에서 화면을 색으로도 구분할 수 있다.
        /// 지정하지 않으면 옅은 회색으로 그린다.
        /// </summary>
        public Color Accent { get; set; } = Color.FromArgb(178, 173, 224);

        public string Text => TextAccessor();

        /// <summary>화면 노드 — 서버 메뉴 코드와 화면 생성 함수를 함께 받는다.</summary>
        public MenuNode(string key, string menuCode, Func<string> textAccessor, Func<Control> screenFactory)
        {
            Key = key;
            MenuCode = menuCode;
            TextAccessor = textAccessor;
            ScreenFactory = screenFactory;
        }

        /// <summary>분류(폴더) 노드 — 화면이 없고 하위 노드만 묶는다.</summary>
        public MenuNode(string key, Func<string> textAccessor)
        {
            Key = key;
            TextAccessor = textAccessor;
        }

        public MenuNode AddChild(MenuNode child)
        {
            Children.Add(child);
            return this;
        }
    }
}
