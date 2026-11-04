using System.Collections.Generic;

namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>
    /// 두벌식 한글 조합기 — 가상 키보드(<see cref="VirtualKeyboardForm"/>)에서 자모를 하나씩 받아 음절로 묶는다.
    ///
    /// 화면 자판은 Windows IME를 거치지 않고 글자를 직접 넣기 때문에 "ㄱ","ㅏ"를 "가"로 묶는 일을 여기서 한다.
    /// 조합 중인 음절은 한 글자(<see cref="Composing"/>)이고, 다음 음절로 넘어가며 확정된 글자는 <c>committed</c>로 돌려준다.
    /// 예) ㄷ ㅏ ㄹ ㄱ ㅏ → "다" → "달" → "닭" → (ㅏ 입력 시 겹받침 ㄺ을 쪼개) committed "달", Composing "가".
    /// </summary>
    internal sealed class HangulComposer
    {
        private const string ChoTable = "ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎ";
        private const string JungTable = "ㅏㅐㅑㅒㅓㅔㅕㅖㅗㅘㅙㅚㅛㅜㅝㅞㅟㅠㅡㅢㅣ";

        /// <summary>종성 표 — 0번은 "받침 없음" 자리라 공백을 둔다(유니코드 음절 계산식의 인덱스와 맞추기 위함).</summary>
        private const string JongTable = " ㄱㄲㄳㄴㄵㄶㄷㄹㄺㄻㄼㄽㄾㄿㅀㅁㅂㅄㅅㅆㅇㅈㅊㅋㅌㅍㅎ";

        private const int SyllableBase = 0xAC00;
        private const char None = '\0';

        private readonly Dictionary<string, char> _jungCombine = new Dictionary<string, char>
        {
            { "ㅗㅏ", 'ㅘ' }, { "ㅗㅐ", 'ㅙ' }, { "ㅗㅣ", 'ㅚ' },
            { "ㅜㅓ", 'ㅝ' }, { "ㅜㅔ", 'ㅞ' }, { "ㅜㅣ", 'ㅟ' },
            { "ㅡㅣ", 'ㅢ' }
        };

        private readonly Dictionary<string, char> _jongCombine = new Dictionary<string, char>
        {
            { "ㄱㅅ", 'ㄳ' }, { "ㄴㅈ", 'ㄵ' }, { "ㄴㅎ", 'ㄶ' },
            { "ㄹㄱ", 'ㄺ' }, { "ㄹㅁ", 'ㄻ' }, { "ㄹㅂ", 'ㄼ' }, { "ㄹㅅ", 'ㄽ' },
            { "ㄹㅌ", 'ㄾ' }, { "ㄹㅍ", 'ㄿ' }, { "ㄹㅎ", 'ㅀ' },
            { "ㅂㅅ", 'ㅄ' }
        };

        /// <summary>겹모음 · 겹받침 → (앞, 뒤). 지우기와 "받침이 다음 음절 초성으로 넘어가기"에 쓴다.</summary>
        private readonly Dictionary<char, KeyValuePair<char, char>> _split = new Dictionary<char, KeyValuePair<char, char>>();

        private char _cho = None;
        private char _jung = None;
        private char _jong = None;

        public HangulComposer()
        {
            foreach (var pair in _jungCombine)
                _split[pair.Value] = new KeyValuePair<char, char>(pair.Key[0], pair.Key[1]);
            foreach (var pair in _jongCombine)
                _split[pair.Value] = new KeyValuePair<char, char>(pair.Key[0], pair.Key[1]);
        }

        public bool IsComposing => _cho != None || _jung != None;

        /// <summary>조합 중인 글자(없으면 빈 문자열).</summary>
        public string Composing
        {
            get
            {
                if (_cho != None && _jung != None)
                {
                    var jongIndex = _jong == None ? 0 : JongTable.IndexOf(_jong);
                    var code = SyllableBase + ((ChoTable.IndexOf(_cho) * JungTable.Length) + JungTable.IndexOf(_jung)) * JongTable.Length + jongIndex;
                    return ((char)code).ToString();
                }

                if (_cho != None)
                    return _cho.ToString();
                if (_jung != None)
                    return _jung.ToString();
                return string.Empty;
            }
        }

        /// <summary>한글 자모(호환용 자모 ㄱ~ㅣ)인지 — 가상 키보드가 조합기로 보낼지 판단할 때 쓴다.</summary>
        public bool IsJamo(char c) => IsConsonant(c) || IsVowel(c);

        /// <summary>자모 하나를 넣는다. 앞 음절이 끝났으면 그 글자를 <paramref name="committed"/>로 돌려준다.</summary>
        /// <returns>새로 조합 중인 글자(<see cref="Composing"/>).</returns>
        public string Input(char jamo, out string committed)
        {
            committed = string.Empty;

            if (IsVowel(jamo))
                InputVowel(jamo, ref committed);
            else if (IsConsonant(jamo))
                InputConsonant(jamo, ref committed);

            return Composing;
        }

        /// <summary>조합 중인 글자를 한 단계 되돌린다(겹받침 → 앞 받침 → 받침 없음 → …). 조합 중이 아니면 false.</summary>
        public bool Backspace()
        {
            if (!IsComposing)
                return false;

            if (_jong != None)
                _jong = _split.TryGetValue(_jong, out var jong) ? jong.Key : None;
            else if (_jung != None && _cho != None)
                _jung = _split.TryGetValue(_jung, out var jung) ? jung.Key : None;
            else if (_jung != None)
                _jung = _split.TryGetValue(_jung, out var jungOnly) ? jungOnly.Key : None;
            else
                _cho = None;

            return true;
        }

        /// <summary>조합을 끝낸다 — 조합 중이던 글자는 이미 입력 상자에 들어가 있으므로 상태만 비운다.</summary>
        public void Reset()
        {
            _cho = None;
            _jung = None;
            _jong = None;
        }

        private void InputConsonant(char consonant, ref string committed)
        {
            if (_cho != None && _jung != None)
            {
                // 받침 자리 — ㄸ · ㅃ · ㅉ처럼 받침이 될 수 없는 자음은 새 음절로 간다.
                if (_jong == None && JongTable.IndexOf(consonant) > 0)
                {
                    _jong = consonant;
                    return;
                }

                if (_jong != None && _jongCombine.TryGetValue(string.Concat(_jong, consonant), out var combined))
                {
                    _jong = combined;
                    return;
                }
            }

            committed = Composing;
            Reset();
            _cho = consonant;
        }

        private void InputVowel(char vowel, ref string committed)
        {
            if (_jong != None)
            {
                // 받침 뒤에 모음이 오면 받침(겹받침이면 뒤 자음)이 다음 음절의 초성이 된다: 닭+ㅏ → 달가.
                var keep = None;
                var move = _jong;
                if (_split.TryGetValue(_jong, out var parts))
                {
                    keep = parts.Key;
                    move = parts.Value;
                }

                _jong = keep;
                committed = Composing;
                Reset();
                _cho = move;
                _jung = vowel;
                return;
            }

            if (_jung != None)
            {
                if (_jungCombine.TryGetValue(string.Concat(_jung, vowel), out var combined))
                {
                    _jung = combined;
                    return;
                }

                committed = Composing;
                Reset();
                _jung = vowel;
                return;
            }

            _jung = vowel;
        }

        private static bool IsConsonant(char c) => ChoTable.IndexOf(c) >= 0;

        private static bool IsVowel(char c) => JungTable.IndexOf(c) >= 0;
    }
}
