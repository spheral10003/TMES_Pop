using System.ComponentModel;

namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>Nutrunner 전용 레지스터 주소.</summary>
    public enum NutrunnerRegister : ushort
    {
        [Description("토크 설정")]
        Torque = 2,

        [Description("스크류 수량")]
        ScrewCount = 247,

        [Description("오토 락")]
        AutoLock = 283,

        [Description("드라이버 락")]
        DriverLock = 4001,

        [Description("프리셋 변경")]
        PresetChange = 4004,

        [Description("이벤트 데이터 사용")]
        EventDataEnable = 4015,

        [Description("이벤트 데이터 수신 확인")]
        EventAck = 4016,

        [Description("Alive 확인용 상태 영역")]
        Status = 3200
    }

    /// <summary>모델 코드.</summary>
    public enum ModelTypes
    {
        // 한타스 모델 코드 / Hantas model codes
        [Description("MD")] Md = 1,
        [Description("AD")] Ad = 2,
        [Description("BM")] Bm = 10,
        [Description("MDT")] Mdt = 15,
        [Description("BMT")] Bmt = 19,
        [Description("BPT")] Bpt = 20,
        [Description("MDT40")] Mdt40 = 27,
        [Description("BMT40")] Bmt40 = 29,
        [Description("ET")] Et = 30,
        [Description("BT")] Bt = 32,

        // Mountz 모델 코드 / Mountz model codes
        [Description("EC")] Ec = 1001,
        [Description("EP")] Ep = 1010,
        [Description("ECT")] Ect = 1015,
        [Description("EPT")] Ept = 1019,
        [Description("ECT40")] Ect40 = 1027,
        [Description("EPT40")] Ept40 = 1029
    }

    /// <summary>펌웨어 세대 구분.</summary>
    public enum GenerationTypes
    {
        [Description("1세대 / Gen.1")] GenRev1 = 0,
        [Description("1세대 AD용 / Gen.1 for AD")] GenRev1Ad = 1,
        [Description("1세대+ / Gen.1+")] GenRev1Plus = 3000,
        [Description("2세대 / Gen.2")] GenRev2 = 4000
    }

    /// <summary>토크 단위.</summary>
    public enum UnitTypes
    {
        [Description("kgf.cm")] KgfCm,
        [Description("kgf.m")] KgfM,
        [Description("N.m")] Nm,
        [Description("N.cm")] NCm,
        [Description("lbf.in")] LbfIn,
        [Description("ozf.in")] OzfIn,
        [Description("lbf.ft")] LbfFt
    }

    /// <summary>체결 이벤트 종류.</summary>
    public enum EventTypes
    {
        [Description("Etc.")] Etc,
        [Description("Fastening OK")] FastenOk,
        [Description("Fastening NG")] FastenNg,
        [Description("F / L")] Fl,
        [Description("Preset change")] PresetChange,
        [Description("Alarm reset")] AlarmReset,
        [Description("Error")] Error,
        [Description("Barcode")] Barcode,
        [Description("Screw cancel")] ScrewCancel,
        [Description("Screw count reset")] ScrewCountReset
    }
}
