using System;
using System.Collections.Generic;
using SEOJIN_TMES.Client.Equipment.Core;

namespace SEOJIN_TMES.Client.Equipment.Nutrunner
{
    /// <summary>Request 채널(Read/Write)의 응답 수신 이벤트.</summary>
    public sealed class NutrunnerRegisterEventArgs : EquipmentEventArgs
    {
        public NutrunnerRegisterEventArgs(int id, bool success, ushort startAddress, IReadOnlyList<ushort> data)
            : base(id)
        {
            Success = success;
            StartAddress = startAddress;
            Data = data ?? Array.Empty<ushort>();
        }

        public bool Success { get; }

        public ushort StartAddress { get; }

        public IReadOnlyList<ushort> Data { get; }
    }

    /// <summary>Event Transport 채널의 체결 결과 수신 이벤트.</summary>
    public sealed class NutrunnerFasteningEventArgs : EquipmentEventArgs
    {
        public NutrunnerFasteningEventArgs(int id, FasteningData data) : base(id) => Data = data;

        public FasteningData Data { get; }
    }
}
