using System;
using System.Collections.Generic;
using System.Linq;
using SEOJIN_TMES.Client.Equipment.Core;
using SEOJIN_TMES.Client.Equipment.Nutrunner;

namespace SEOJIN_TMES.Client.Equipment.Management
{
    /// <summary>
    /// <see cref="INutrunnerManager"/> 기본 구현체.
    ///
    /// 장비 한 대당 <see cref="NutrunnerDevice"/> + <see cref="NutrunnerSequence"/> 한 쌍을 만들어
    /// ID(int index)를 키로 보관하고, 각 장비의 이벤트를 ID가 붙은 하나의 이벤트로 모아서 다시 발생시킨다.
    /// </summary>
    public sealed class NutrunnerManager : INutrunnerManager
    {
        private readonly Dictionary<int, INutrunnerSequence> _sequences = new();
        private readonly object _sync = new();
        private bool _disposed;

        public event EventHandler<EquipmentConnectionEventArgs>? ConnectionChanged;
        public event EventHandler<NutrunnerRegisterEventArgs>? RegisterDataReceived;
        public event EventHandler<NutrunnerFasteningEventArgs>? FasteningReceived;
        public event EventHandler<AlarmEventArgs>? AlarmOccurred;

        public int Count
        {
            get { lock (_sync) { return _sequences.Count; } }
        }

        public IReadOnlyList<int> Ids
        {
            get { lock (_sync) { return _sequences.Keys.OrderBy(x => x).ToArray(); } }
        }

        public INutrunner? this[int id] => TryGet(id, out INutrunner nutrunner) ? nutrunner : null;

        public INutrunner? this[string ip] => TryGet(ip, out INutrunner nutrunner) ? nutrunner : null;
        public INutrunner Add(NutrunnerInfo info)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var device = new NutrunnerDevice(info);
            var sequence = new NutrunnerSequence(device);

            // 딕셔너리에 넣기 전에 구독을 끝내야 Start() 직후의 첫 이벤트를 놓치지 않는다.
            device.Connected += OnConnectionChanged;
            device.Disconnected += OnConnectionChanged;
            device.RegisterDataReceived += OnRegisterDataReceived;
            device.FasteningReceived += OnFasteningReceived;
            sequence.AlarmOccurred += OnAlarmOccurred;

            lock (_sync)
            {
                if (_sequences.ContainsKey(info.Id))
                {
                    Detach(sequence);
                    sequence.Dispose();
                    throw new ArgumentException($"이미 등록된 장비 ID 입니다. (ID={info.Id})", nameof(info));
                }

                _sequences.Add(info.Id, sequence);
            }

            return device;
        }

        public bool Remove(int id)
        {
            INutrunnerSequence? sequence;

            lock (_sync)
            {
                if (_sequences.TryGetValue(id, out sequence))
                {
                    _sequences.Remove(id);
                }
            }

            if (sequence is null)
            {
                return false;
            }

            Detach(sequence);
            sequence.Dispose();
            return true;
        }

        public bool TryGet(int id, out INutrunner nutrunner)
        {
            lock (_sync)
            {
                if (_sequences.TryGetValue(id, out INutrunnerSequence? sequence))
                {
                    nutrunner = sequence.Nutrunner;
                    return true;
                }
            }

            nutrunner = null!;
            return false;
        }

        public bool TryGet(string ip, out INutrunner nutrunner)
        {
            lock (_sync)
            {
                var sequence = _sequences.Values.FirstOrDefault(x => x.Nutrunner.Info.Ip == ip);
                if (sequence is not null)
                {
                    nutrunner = sequence.Nutrunner;
                    return true;
                }
            }

            nutrunner = null!;
            return false;
        }

        /// <summary>
        /// IP 주소로 Nutrunner를 찾습니다.
        /// </summary>
        /// <param name="ip">찾을 IP 주소</param>
        /// <returns>찾은 Nutrunner, 없으면 null</returns>
        public INutrunner? FindByIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                return null;
            }

            lock (_sync)
            {
                return _sequences.Values
                    .FirstOrDefault(x => x.Nutrunner.Info.Ip == ip)
                    ?.Nutrunner;
            }
        }

        /// <summary>
        /// IP 주소로 Nutrunner의 ID를 찾습니다.
        /// </summary>
        /// <param name="ip">찾을 IP 주소</param>
        /// <returns>찾은 ID, 없으면 -1</returns>
        public int FindIdByIp(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                return -1;
            }

            lock (_sync)
            {
                var kvp = _sequences.FirstOrDefault(x => x.Value.Nutrunner.Info.Ip == ip);
                return kvp.Key != 0 ? kvp.Key : -1;
            }
        }

        public void Start(int id) => Find(id)?.Start();

        public void StopAll()
        {
            foreach (INutrunnerSequence sequence in Snapshot())
            {
                sequence.Stop();
                sequence.Nutrunner.Disconnect();
            }
        }

        public void StartAll()
        {
            foreach (INutrunnerSequence sequence in Snapshot())
            {
                sequence.Start();
            }
        }

        public void Stop(int id)
        {
            INutrunnerSequence? sequence = Find(id);
            if (sequence is null)
            {
                return;
            }

            sequence.Stop();
            sequence.Nutrunner.Disconnect();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (INutrunnerSequence sequence in Snapshot())
            {
                Detach(sequence);
                sequence.Dispose();
            }

            lock (_sync)
            {
                _sequences.Clear();
            }
        }

        private INutrunnerSequence? Find(int id)
        {
            lock (_sync)
            {
                return _sequences.TryGetValue(id, out INutrunnerSequence? sequence) ? sequence : null;
            }
        }

        private INutrunnerSequence[] Snapshot()
        {
            lock (_sync)
            {
                return _sequences.Values.ToArray();
            }
        }

        private void Detach(INutrunnerSequence sequence)
        {
            INutrunner device = sequence.Nutrunner;

            device.Connected -= OnConnectionChanged;
            device.Disconnected -= OnConnectionChanged;
            device.RegisterDataReceived -= OnRegisterDataReceived;
            device.FasteningReceived -= OnFasteningReceived;
            sequence.AlarmOccurred -= OnAlarmOccurred;
        }

        private void OnConnectionChanged(object? sender, EquipmentConnectionEventArgs e)
            => ConnectionChanged?.Invoke(this, e);

        private void OnRegisterDataReceived(object? sender, NutrunnerRegisterEventArgs e)
            => RegisterDataReceived?.Invoke(this, e);

        private void OnFasteningReceived(object? sender, NutrunnerFasteningEventArgs e)
            => FasteningReceived?.Invoke(this, e);

        private void OnAlarmOccurred(object? sender, AlarmEventArgs e)
            => AlarmOccurred?.Invoke(this, e);
    }
}
