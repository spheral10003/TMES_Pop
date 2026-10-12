namespace SEOJIN_TMES.Client.Forms.Common
{
    /// <summary>공통 메시지 창의 성격. 강조 색(확인 버튼/제목)만 달라지고 레이아웃은 동일하다.</summary>
    public enum AppMessageKind
    {
        /// <summary>일반 안내.</summary>
        Info,

        /// <summary>확인/취소를 묻는 질문.</summary>
        Question,

        /// <summary>성공 안내.</summary>
        Success,

        /// <summary>주의(조회 실패 등 업무 흐름은 유지되는 경우).</summary>
        Warning,

        /// <summary>오류.</summary>
        Error
    }

    /// <summary>공통 메시지 창의 버튼 구성.</summary>
    public enum AppMessageButtons
    {
        /// <summary>확인 1개.</summary>
        Ok,

        /// <summary>확인 + 취소.</summary>
        OkCancel
    }
}
