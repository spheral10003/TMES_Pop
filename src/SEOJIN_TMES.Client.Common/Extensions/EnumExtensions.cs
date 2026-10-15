using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Common.Extensions
{
    /// <summary>enum 항목의 [Description] 문구를 꺼낸다 — 콤보 표시 등 여러 화면에서 공용으로 쓴다.</summary>
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            // Description이 없는 항목은 이름이라도 보여 콤보에 빈 줄이 생기지 않게 한다.
            return attr?.Description ?? value.ToString();
        }
    }
}
