using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using SEOJIN_TMES.Client.Common.Languages;
using SEOJIN_TMES.Client.Theme;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>가상 키보드의 자판 배열.</summary>
    public enum VirtualKeyboardMode
    {
        /// <summary>숫자 키패드(0~9 · 소수점 · 부호) — 숫자 형식 셀.</summary>
        Numeric,

        /// <summary>문자 키보드(한글 두벌식 / 영문 QWERTY · 숫자) — 문자열 셀.</summary>
        Text
    }

    /// <summary>
    /// &lt;가상 키보드&gt; 팝업 — 물리 키보드가 없는 터치 POP에서 그리드 셀 값을 넣을 때 연다(<c>PopScreen.ShowVirtualKeyboard</c>).
    ///
    /// 화면 자판을 눌러 입력값 상자(txtValue)를 채우고 [확인]이면 <see cref="Value"/>로 돌려준다.
    /// 값이 맞는지(숫자인지 등)는 이 창이 판단하지 않는다 — 셀 편집기의 검사(ValidatingEditor)를 그대로 타도록
    /// 호출하는 쪽이 받은 값을 편집기에 넣고 닫는다. 그래야 직접 타이핑한 값과 같은 규칙 · 같은 오류 문구로 걸러진다.
    ///
    /// 문자 키보드는 [한/영]으로 한글 두벌식과 영문을 바꾼다. 화면 자판은 IME를 거치지 않으므로
    /// 자모를 음절로 묶는 일은 <see cref="HangulComposer"/>가 하고, 조합 중인 글자는 입력 상자의 커서 앞 한 칸을 차지한다.
    ///
    /// 자판 버튼은 포커스를 받지 않는다(<see cref="KeyButton"/>) — 누를 때마다 입력 상자의 커서 · 선택 영역이 그대로 남아,
    /// 커서 위치에 끼워 넣거나 선택한 글자를 바꿔 쓸 수 있다. 물리 키보드 · IME 입력도 입력 상자에 그대로 들어간다.
    /// </summary>
    public partial class VirtualKeyboardForm : Form
    {
        private const string BackspaceKey = "⌫";
        private const string ClearKey = "C";
        private const string ShiftKey = "⇧";
        private const string SpaceKey = "Space";
        private const string LanguageKey = "한/영";

        /// <summary>자판 버튼 사이 간격(px).</summary>
        private const int KeyGap = 6;

        private const string EnglishLetters = "qwertyuiopasdfghjklzxcvbnm";
        private const string KoreanLetters = "ㅂㅈㄷㄱㅅㅛㅕㅑㅐㅔㅁㄴㅇㄹㅎㅗㅓㅏㅣㅋㅌㅊㅍㅠㅜㅡ";

        /// <summary>두벌식에서 Shift로 바뀌는 자리(쌍자음 · ㅒ · ㅖ). 나머지 자리는 Shift를 눌러도 같은 자모다.</summary>
        private const string EnglishShiftLetters = "qwertop";
        private const string KoreanShiftLetters = "ㅃㅉㄸㄲㅆㅒㅖ";

        private readonly Size _numericClientSize = new Size(420, 560);
        private readonly Size _textClientSize = new Size(1000, 480);

        private readonly Color _accent = AppTheme.ButtonBack;

        /// <summary>자판 한 줄 = (버튼, 폭 비율) 목록. 한 줄의 비율 합으로 줄 폭을 나눈다.</summary>
        private readonly List<List<KeyValuePair<KeyButton, float>>> _rows = new List<List<KeyValuePair<KeyButton, float>>>();

        private readonly List<KeyButton> _letterKeys = new List<KeyButton>();
        private readonly List<Font> _fonts = new List<Font>();
        private readonly HangulComposer _composer = new HangulComposer();

        private bool _shift;
        private bool _korean;

        /// <summary>조합 중인 글자가 입력 상자에서 차지하는 위치 · 길이(조합 중이 아니면 길이 0).</summary>
        private int _compositionStart;
        private int _compositionLength;

        /// <summary>[확인] 시 입력 상자의 내용.</summary>
        public string Value { get; private set; }

        public VirtualKeyboardForm()
        {
            InitializeComponent();
        }

        /// <summary>제목 · 시작 값 · 자판 배열을 넣는다. 시작 값은 전체 선택해 두어 첫 키가 기존 값을 바꿔 쓰게 한다.</summary>
        /// <param name="title">어떤 칸을 고치는지 — 보통 컬럼 머리글.</param>
        public void Initialize(string title, string initialValue, VirtualKeyboardMode mode)
        {
            Text = title;
            lblTitle.Text = title;
            btnOK.Text = Language.Common_Confirm;
            btnCancel.Text = Language.Common_Cancel;

            txtValue.Text = initialValue ?? string.Empty;
            txtValue.SelectAll();

            // 한국어 화면이면 한글로 시작한다 — 비고는 대부분 한글로 적는다.
            _korean = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ko";

            ClientSize = mode == VirtualKeyboardMode.Numeric ? _numericClientSize : _textClientSize;
            if (mode == VirtualKeyboardMode.Numeric)
                BuildNumericKeys();
            else
                BuildTextKeys();

            ApplyTheme();
            UpdateLetterKeys();
            DialogChrome.Apply(this, pnlTitle, lblClose);
            LayoutKeys();
        }

        private void ApplyTheme()
        {
            AppTheme.TitleBar(pnlTitle, lblTitle, _accent);
            AppTheme.TitleBarClose(lblClose, _accent);
            AppTheme.StandardButton(btnCancel, keepFont: true);
            AppTheme.StandardButton(btnOK, keepFont: true);
            pnlBody.BackColor = AppTheme.Surface;
            pnlKeys.BackColor = AppTheme.Surface;
            pnlFooter.BackColor = AppTheme.Surface;

            var buttonFont = TrackFont(AppTheme.ResizedFont(AppTheme.BodyBoldFont, 12F));
            btnOK.Font = buttonFont;
            btnCancel.Font = buttonFont;

            txtValue.BackColor = AppTheme.Card;
            txtValue.ForeColor = AppTheme.TextPrimary;
            txtValue.Font = TrackFont(AppTheme.ResizedFont(AppTheme.BodyFont, 20F));

            var keyFont = TrackFont(AppTheme.ResizedFont(AppTheme.BodyBoldFont, 16F));
            foreach (var row in _rows)
            {
                foreach (var key in row)
                {
                    AppTheme.DialogFlatButton(key.Key, _accent);
                    key.Key.Font = keyFont;
                }
            }
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

        // ─────────────────────────────────────────────────────────────────────────
        //  자판 구성
        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 숫자 키패드. 소수점 키는 현재 문화권의 소수 구분자를 넣는다 —
        /// 셀 검사가 CurrentCulture로 숫자를 읽으므로(베트남어는 ',') 자판도 같은 기호를 써야 거절되지 않는다.
        /// </summary>
        private void BuildNumericKeys()
        {
            var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            AddRow(Key("7"), Key("8"), Key("9"), Key(BackspaceKey));
            AddRow(Key("4"), Key("5"), Key("6"), Key(ClearKey));
            AddRow(Key("1"), Key("2"), Key("3"), Key("-"));
            AddRow(Key("0", 2f), Key(decimalSeparator, 2f));
        }

        /// <summary>문자 키보드 — 자리는 QWERTY 하나로 두고, 글자 키의 표시 · 입력만 한글/영문 · Shift에 따라 바꾼다.</summary>
        private void BuildTextKeys()
        {
            AddRow(Keys("1234567890", Key(BackspaceKey, 1.5f)));
            AddRow(Keys("qwertyuiop", Key("-")));
            AddRow(Keys("asdfghjkl", Key("."), Key(",")));
            AddRow(Keys("zxcvbnm", Key("/"), Key("_"), Key("("), Key(")"), Key(ShiftKey, 1.5f)));
            AddRow(Key(ClearKey, 1.5f), Key(LanguageKey, 1.5f), Key(SpaceKey, 7f), Key(":"), Key("+"));
        }

        /// <summary>글자 한 개씩 키를 만들어 뒤에 덧붙일 키와 함께 한 줄 목록으로 돌려준다.</summary>
        private KeyValuePair<KeyButton, float>[] Keys(string letters, params KeyValuePair<KeyButton, float>[] tail)
        {
            var keys = new List<KeyValuePair<KeyButton, float>>();
            foreach (var letter in letters)
            {
                var key = Key(letter.ToString());
                if (char.IsLetter(letter))
                    _letterKeys.Add(key.Key);
                keys.Add(key);
            }

            keys.AddRange(tail);
            return keys.ToArray();
        }

        private KeyValuePair<KeyButton, float> Key(string text, float weight = 1f)
        {
            var button = new KeyButton();
            button.Text = text;
            button.Tag = text;
            button.Click += Key_Click;
            pnlKeys.Controls.Add(button);
            return new KeyValuePair<KeyButton, float>(button, weight);
        }

        private void AddRow(params KeyValuePair<KeyButton, float>[] keys)
        {
            _rows.Add(new List<KeyValuePair<KeyButton, float>>(keys));
        }

        private void pnlKeys_Resize(object sender, EventArgs e) => LayoutKeys();

        /// <summary>자판 영역을 줄 수만큼 세로로 나누고, 각 줄은 키의 폭 비율대로 가로를 나눈다.</summary>
        private void LayoutKeys()
        {
            if (_rows.Count == 0)
                return;

            var area = pnlKeys.DisplayRectangle;
            var rowHeight = (area.Height - (KeyGap * (_rows.Count - 1))) / (float)_rows.Count;

            for (var r = 0; r < _rows.Count; r++)
            {
                var row = _rows[r];
                var totalWeight = 0f;
                foreach (var key in row)
                    totalWeight += key.Value;

                var unit = (area.Width - (KeyGap * (row.Count - 1))) / totalWeight;
                var top = area.Top + (int)Math.Round(r * (rowHeight + KeyGap));
                var x = (float)area.Left;

                foreach (var key in row)
                {
                    var width = unit * key.Value;
                    key.Key.Bounds = new Rectangle((int)Math.Round(x), top, (int)Math.Round(width), (int)Math.Round(rowHeight));
                    x += width + KeyGap;
                }
            }
        }

        /// <summary>글자 키 표시를 지금 상태(한글/영문 · Shift)에 맞춘다.</summary>
        private void UpdateLetterKeys()
        {
            foreach (var key in _letterKeys)
                key.Text = LetterOf((string)key.Tag).ToString();
        }

        /// <summary>QWERTY 자리(영문 소문자)가 지금 상태에서 넣는 글자.</summary>
        private char LetterOf(string position)
        {
            var letter = position[0];

            if (!_korean)
                return _shift ? char.ToUpperInvariant(letter) : letter;

            var shiftIndex = EnglishShiftLetters.IndexOf(letter);
            if (_shift && shiftIndex >= 0)
                return KoreanShiftLetters[shiftIndex];

            var index = EnglishLetters.IndexOf(letter);
            return index >= 0 ? KoreanLetters[index] : letter;
        }

        // ─────────────────────────────────────────────────────────────────────────
        //  입력
        // ─────────────────────────────────────────────────────────────────────────

        private void Key_Click(object sender, EventArgs e)
        {
            var button = (KeyButton)sender;
            var key = (string)button.Tag;

            switch (key)
            {
                case BackspaceKey:
                    Backspace();
                    break;
                case ClearKey:
                    EndComposition();
                    txtValue.Clear();
                    break;
                case ShiftKey:
                    // Shift는 조합을 끊지 않는다 — 쌍자음(ㄲ 등)이 조합 중인 음절에 받침으로 들어갈 수 있어야 한다.
                    _shift = !_shift;
                    UpdateLetterKeys();
                    break;
                case LanguageKey:
                    EndComposition();
                    _korean = !_korean;
                    UpdateLetterKeys();
                    break;
                case SpaceKey:
                    Insert(" ");
                    break;
                default:
                    if (_letterKeys.Contains(button))
                        InputLetter(LetterOf(key));
                    else
                        Insert(key);
                    break;
            }

            txtValue.Focus();
        }

        /// <summary>글자 키 입력 — 한글 자모면 조합기로, 아니면 그대로 넣는다. Shift는 한 글자만 유지된다.</summary>
        private void InputLetter(char letter)
        {
            if (_composer.IsJamo(letter))
                InputJamo(letter);
            else
                Insert(letter.ToString());

            if (_shift)
            {
                _shift = false;
                UpdateLetterKeys();
            }
        }

        /// <summary>
        /// 자모를 조합기에 넣고, 조합 중인 글자 자리를 (확정된 글자 + 새 조합 글자)로 바꿔 쓴다.
        /// 조합을 새로 시작할 때는 선택 영역을 먼저 지운다 — 시작 값을 전체 선택해 두었으므로 첫 글자가 기존 값을 대신한다.
        /// </summary>
        private void InputJamo(char jamo)
        {
            if (_compositionLength == 0)
            {
                if (IsFull(1))
                    return;

                txtValue.SelectedText = string.Empty;
                _compositionStart = txtValue.SelectionStart;
            }

            var composing = _composer.Input(jamo, out var committed);
            if (IsFull(committed.Length + composing.Length - _compositionLength))
            {
                // 칸이 가득 차 새 음절을 받을 수 없다 — 지금까지의 글자만 남기고 조합을 끝낸다.
                EndComposition();
                return;
            }

            ReplaceComposition(committed, composing);
        }

        private void ReplaceComposition(string committed, string composing)
        {
            txtValue.Select(_compositionStart, _compositionLength);
            txtValue.SelectedText = committed + composing;

            _compositionStart += committed.Length;
            _compositionLength = composing.Length;
            txtValue.Select(_compositionStart + _compositionLength, 0);

            if (_compositionLength == 0)
                _composer.Reset();
        }

        /// <summary>조합을 끝낸다. 조합 중이던 글자는 이미 입력 상자에 있으므로 그대로 확정된다.</summary>
        private void EndComposition()
        {
            _composer.Reset();
            _compositionLength = 0;
        }

        /// <summary>글자를 <paramref name="added"/>개 더 넣으면 입력 상자의 최대 길이를 넘는지.</summary>
        private bool IsFull(int added)
        {
            return txtValue.MaxLength > 0 && txtValue.TextLength - txtValue.SelectionLength + added > txtValue.MaxLength;
        }

        /// <summary>커서 위치에 끼워 넣는다(선택 영역이 있으면 바꿔 쓴다). 조합 중이던 글자는 확정한다.</summary>
        private void Insert(string text)
        {
            EndComposition();
            if (IsFull(text.Length))
                return;

            txtValue.SelectedText = text;
        }

        /// <summary>조합 중이면 조합을 한 단계 되돌리고(닭 → 달 → 다 → ㄷ), 아니면 커서 앞 글자를 지운다.</summary>
        private void Backspace()
        {
            if (_compositionLength > 0 && _composer.Backspace())
            {
                ReplaceComposition(string.Empty, _composer.Composing);
                return;
            }

            EndComposition();

            if (txtValue.SelectionLength > 0)
            {
                txtValue.SelectedText = string.Empty;
                return;
            }

            var start = txtValue.SelectionStart;
            if (start == 0)
                return;

            txtValue.Text = txtValue.Text.Remove(start - 1, 1);
            txtValue.SelectionStart = start - 1;
        }

        /// <summary>작업자가 입력 상자를 직접 눌러 커서를 옮기거나 물리 키보드로 치면 조합을 끝낸다(조합 위치가 어긋나므로).</summary>
        private void txtValue_MouseDown(object sender, MouseEventArgs e) => EndComposition();

        private void txtValue_KeyDown(object sender, KeyEventArgs e) => EndComposition();

        private void VirtualKeyboardForm_Shown(object sender, EventArgs e)
        {
            txtValue.Focus();
            txtValue.SelectAll();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            EndComposition();
            Value = txtValue.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lblClose_MouseEnter(object sender, EventArgs e) =>
            lblClose.BackColor = AppTheme.TitleBarCloseHover(_accent);

        private void lblClose_MouseLeave(object sender, EventArgs e) =>
            lblClose.BackColor = _accent;

        /// <summary>AppTheme의 글꼴 속성은 부를 때마다 새로 만들므로, 이 창에서 만든 것은 닫을 때 함께 정리한다.</summary>
        private Font TrackFont(Font font)
        {
            _fonts.Add(font);
            return font;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            foreach (var font in _fonts)
                font.Dispose();
            _fonts.Clear();
        }

        /// <summary>
        /// 포커스를 가져가지 않는 자판 버튼. 일반 Button은 누르는 순간 포커스를 가져가
        /// 입력 상자의 커서 위치를 잃고, Enter가 [확인] 대신 그 버튼을 눌러 버린다.
        /// </summary>
        private sealed class KeyButton : Button
        {
            public KeyButton()
            {
                SetStyle(ControlStyles.Selectable, false);
                TabStop = false;
            }
        }
    }
}
