using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Composition;

namespace SEOJIN_TMES.Client.Menu
{
    /// <summary>
    /// 3-Depth 트리 메뉴(MEIS > 대분류 > 화면)의 노드 하나.
    /// TextAccessor를 Func로 두는 이유는 언어 전환 시 리소스를 다시 읽어 메뉴 문구를 즉시 갱신하기 위함이다.
    /// 리프 노드(FormFactory != null)만 실제 화면을 연다.
    /// </summary>
    public class MenuNode
    {
        public string Key { get; }
        public Func<string> TextAccessor { get; }
        public List<MenuNode> Children { get; } = new List<MenuNode>();
        public Func<Control> ScreenFactory { get; }

        /// <summary>메뉴 아이콘(AppIcons의 SVG 템플릿 문자열). 지정하지 않으면 점 아이콘이 사용된다.</summary>
        public string Icon { get; set; }

        /// <summary>
        /// 리프(화면) 항목 앞에 찍는 점(●)의 색. 화면마다 다른 색을 주면 좌측 메뉴에서 화면을 색으로도 구분할 수 있다.
        /// 지정하지 않으면 옅은 회색으로 그린다.
        /// </summary>
        public Color Accent { get; set; } = Color.FromArgb(178, 173, 224);

        public string Text => TextAccessor();

        public MenuNode(string key, Func<string> textAccessor, Func<Control> screenFactory = null)
        {
            Key = key;
            TextAccessor = textAccessor;
            ScreenFactory = screenFactory;
        }

        public MenuNode(string key, Func<string> textAccessor)
        {
            Key = key;
            TextAccessor = textAccessor;
            //ScreenFactory = screenFactory;
        }

        public MenuNode AddChild(MenuNode child)
        {
            Children.Add(child);
            return this;
        }
    }
}
