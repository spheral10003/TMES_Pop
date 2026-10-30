using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SEOJIN_TMES.Client.Forms.ModbusTest
{
    /// <summary>
    /// 접속 테스트 대상 1행(설비 1대). 입력(IP · 포트)과 진행 결과(성공/실패 횟수)를 한 줄에 같이 둔다.
    /// 테스트 중 값이 바뀔 때마다 그리드가 바로 다시 그리도록 <see cref="INotifyPropertyChanged"/>를 구현한다.
    /// </summary>
    public class ModbusTestTarget : INotifyPropertyChanged
    {
        private string _host;
        private int _port = 502;
        private int _eventPort = 503;
        private int _completed;
        private int _total;
        private int _successCount;
        private int _failCount;
        private string _state;
        private string _lastResult;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Host
        {
            get => _host;
            set => Set(ref _host, value);
        }

        /// <summary>
        /// 표준 포트. 이벤트 포트가 스펙 기본 규칙(포트+1)대로 따라오던 중이면 함께 옮긴다 —
        /// 포트만 바꾸고 이벤트 포트를 깜빡해 엉뚱한 포트로 테스트하는 일을 막는다.
        /// </summary>
        public int Port
        {
            get => _port;
            set
            {
                if (_port == value)
                    return;

                var followsPort = _eventPort <= 0 || _eventPort == _port + 1;
                Set(ref _port, value);
                if (followsPort)
                    EventPort = value + 1;
            }
        }

        public int EventPort
        {
            get => _eventPort;
            set => Set(ref _eventPort, value);
        }

        /// <summary>진행 표시("3 / 10"). 숫자 두 개를 따로 두면 컬럼이 늘어나 한 칸에 묶는다.</summary>
        public string Progress => _total > 0 ? _completed + " / " + _total : string.Empty;

        public int SuccessCount
        {
            get => _successCount;
            private set => Set(ref _successCount, value);
        }

        public int FailCount
        {
            get => _failCount;
            private set => Set(ref _failCount, value);
        }

        public string State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        public string LastResult
        {
            get => _lastResult;
            set => Set(ref _lastResult, value);
        }

        /// <summary>새 테스트를 시작하기 전에 지난 결과를 비운다.</summary>
        public void Reset(int total, string state)
        {
            _completed = 0;
            _total = total;
            SuccessCount = 0;
            FailCount = 0;
            LastResult = null;
            State = state;
            OnPropertyChanged(nameof(Progress));
        }

        /// <summary>1회 결과를 센다.</summary>
        public void Record(bool success, string resultText)
        {
            _completed++;
            if (success)
                SuccessCount++;
            else
                FailCount++;

            LastResult = resultText;
            OnPropertyChanged(nameof(Progress));
        }

        /// <summary>로그·리포트에 쓰는 대상 표시("192.168.0.101:502/503").</summary>
        public string Endpoint => Host + ":" + Port + "/" + EventPort;

        private void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return;

            field = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
