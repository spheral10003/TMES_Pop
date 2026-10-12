using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// init 전용 속성(init-only setter)과 record 를 컴파일할 때 컴파일러가 요구하는 표식 타입.
    /// .NET 5 이상에는 기본 제공되지만 .NET Framework 4.7.2 에는 없으므로 직접 선언한다.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
