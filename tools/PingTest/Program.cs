using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace PingTest
{
    /// <summary>
    /// 지정한 호스트에 주기적으로 ping을 보내 결과를 콘솔과 일자별 로그 파일에 남긴다.
    /// 현장에서 "가끔 끊긴다"는 증상을 시간대별로 확인하려는 용도라, 성공/실패 모두 한 줄씩 기록한다.
    /// </summary>
    internal static class Program
    {
        private const int DefaultIntervalMs = 1000;
        private const int DefaultTimeoutMs = 1000;
        private const int DefaultBufferSize = 32;

        private static volatile bool _stopRequested;

        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var exitCode = Run(args);

            // 더블클릭으로 띄운 경우 끝나자마자 창이 닫히면 요약·오류를 볼 수 없어 키 입력을 기다린다.
            if (args.Length == 0 && !Console.IsInputRedirected)
            {
                Console.WriteLine();
                Console.WriteLine("아무 키나 누르면 창을 닫습니다.");
                Console.ReadKey(true);
            }
            return exitCode;
        }

        private static int Run(string[] args)
        {
            Options options;
            try
            {
                options = Options.Parse(args);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("설정/인자 오류: " + ex.Message);
                Console.WriteLine();
                PrintUsage();
                return 1;
            }

            if (options.ShowHelp)
            {
                PrintUsage();
                return 0;
            }

            if (options.Hosts.Count == 0)
            {
                // PingTest.exe.config 의 Hosts 도 비어 있으면 대상만 물어본다.
                Console.Write("Ping 대상 (여러 개는 공백 구분): ");
                var line = Console.ReadLine() ?? string.Empty;
                options.Hosts.AddRange(line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries));
                if (options.Hosts.Count == 0)
                {
                    Console.WriteLine("대상이 없어 종료합니다.");
                    return 1;
                }
            }

            Console.CancelKeyPress += (s, e) =>
            {
                // 바로 죽이지 않고 루프를 빠져나가 통계를 남긴 뒤 끝낸다.
                e.Cancel = true;
                _stopRequested = true;
            };

            using (var log = new PingLog(options.LogDirectory))
            {
                var stats = new Dictionary<string, HostStats>();
                foreach (var host in options.Hosts)
                    stats[host] = new HostStats();

                log.WriteLine(string.Format(CultureInfo.InvariantCulture,
                    "# 시작 대상={0} 간격={1}ms 타임아웃={2}ms 크기={3}byte 횟수={4}",
                    string.Join(",", options.Hosts), options.IntervalMs, options.TimeoutMs, options.BufferSize,
                    options.Count == 0 ? "무한" : options.Count.ToString(CultureInfo.InvariantCulture)));
                Console.WriteLine("로그 파일 폴더: " + log.Directory);
                Console.WriteLine("Ctrl+C 로 종료합니다.");
                Console.WriteLine();
                Console.WriteLine(FormatRow("시간", "대상", "응답 IP", "상태", "시간(ms)", "TTL", "byte"));

                var buffer = new byte[options.BufferSize];
                for (var i = 0; i < buffer.Length; i++)
                    buffer[i] = (byte)('a' + i % 23);

                using (var ping = new Ping())
                {
                    var sent = 0;
                    while (!_stopRequested && (options.Count == 0 || sent < options.Count))
                    {
                        var roundStart = DateTime.Now;
                        foreach (var host in options.Hosts)
                        {
                            if (_stopRequested) break;
                            var result = Send(ping, host, buffer, options.TimeoutMs);
                            stats[host].Add(result);
                            Print(result);
                            log.WriteLine(result.ToLogLine());
                        }
                        sent++;

                        // 대상이 여러 개거나 타임아웃이 길어도 주기가 밀리지 않도록 남은 시간만 쉰다.
                        var remain = options.IntervalMs - (int)(DateTime.Now - roundStart).TotalMilliseconds;
                        while (remain > 0 && !_stopRequested && (options.Count == 0 || sent < options.Count))
                        {
                            var step = Math.Min(remain, 100);
                            Thread.Sleep(step);
                            remain -= step;
                        }
                    }
                }

                Console.WriteLine();
                foreach (var host in options.Hosts)
                {
                    var summary = stats[host].ToSummary(host);
                    Console.WriteLine(summary);
                    log.WriteLine("# " + summary);
                }
                log.WriteLine("# 종료");
            }

            return 0;
        }

        private static PingResult Send(Ping ping, string host, byte[] buffer, int timeoutMs)
        {
            var result = new PingResult { Time = DateTime.Now, Host = host };
            try
            {
                var reply = ping.Send(host, timeoutMs, buffer, new PingOptions(128, true));
                result.Status = ToKorean(reply.Status);
                result.Success = reply.Status == IPStatus.Success;
                result.Address = reply.Address == null ? string.Empty : reply.Address.ToString();
                if (result.Success)
                {
                    result.RoundtripMs = reply.RoundtripTime;
                    result.Ttl = reply.Options == null ? 0 : reply.Options.Ttl;
                    result.Bytes = reply.Buffer == null ? 0 : reply.Buffer.Length;
                }
            }
            catch (PingException ex)
            {
                // 이름 해석 실패 등은 PingException 안쪽에 실제 원인이 들어 있다.
                result.Status = "오류";
                result.Address = string.Empty;
                result.Error = (ex.InnerException ?? ex).Message;
            }
            return result;
        }

        private static void Print(PingResult r)
        {
            var color = Console.ForegroundColor;
            if (!r.Success)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(FormatRow(
                r.Time.ToString("[yyyy-MM-dd HH:mm:ss.fff]", CultureInfo.InvariantCulture),
                r.Host, r.Address, r.Status,
                r.Success ? r.RoundtripMs.ToString(CultureInfo.InvariantCulture) : "-",
                r.Success ? r.Ttl.ToString(CultureInfo.InvariantCulture) : "-",
                r.Success ? r.Bytes.ToString(CultureInfo.InvariantCulture) : "-")
                + (string.IsNullOrEmpty(r.Error) ? string.Empty : "  " + r.Error));
            Console.ForegroundColor = color;
        }

        // 콘솔 칸 너비. 음수는 왼쪽 정렬, 양수는 오른쪽 정렬(string.Format 의 {0,-30} 과 같은 뜻).
        // 머리글과 데이터 줄이 같은 값을 쓰도록 한곳에 둔다.
        private static readonly int[] ColumnWidths = { -30, -20, -16, -20, 10, 10, 10 };

        /// <summary>
        /// string.Format 의 칸 맞춤은 글자 수로 세는데, 콘솔에서 한글은 두 칸을 차지해서
        /// 한글 머리글·상태가 들어가면 뒤 칸이 밀린다. 그래서 화면에 보이는 폭으로 직접 채운다.
        /// </summary>
        private static string FormatRow(params string[] cells)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < cells.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                var text = cells[i] ?? string.Empty;
                var width = ColumnWidths[i];
                var pad = new string(' ', Math.Max(0, Math.Abs(width) - DisplayWidth(text)));
                if (width < 0) sb.Append(text).Append(pad);
                else sb.Append(pad).Append(text);
            }
            return sb.ToString();
        }

        private static int DisplayWidth(string text)
        {
            var width = 0;
            foreach (var c in text)
                width += IsWide(c) ? 2 : 1;
            return width;
        }

        private static bool IsWide(char c)
        {
            return (c >= 'ᄀ' && c <= 'ᅟ')    // 한글 자모(초성)
                || (c >= '⺀' && c <= '꓏')    // 한중일 기호·한자·호환 자모
                || (c >= '가' && c <= '힣')    // 한글 음절
                || (c >= '豈' && c <= '﫿')    // 한자 호환
                || (c >= '＀' && c <= '｠')    // 전각 문자
                || (c >= '￠' && c <= '￦');
        }

        /// <summary>
        /// IPStatus 영문 이름은 현장에서 알아보기 어려워 한글로 바꿔 콘솔·로그에 남긴다.
        /// 상태 칸(20칸)을 넘지 않도록 짧게 줄였다.
        /// </summary>
        private static string ToKorean(IPStatus status)
        {
            switch (status)
            {
                case IPStatus.Success: return "성공";
                case IPStatus.TimedOut: return "응답 없음(시간초과)";
                case IPStatus.DestinationHostUnreachable: return "호스트 도달 불가";
                case IPStatus.DestinationNetworkUnreachable: return "네트워크 도달 불가";
                case IPStatus.DestinationUnreachable: return "대상 도달 불가";
                case IPStatus.DestinationPortUnreachable: return "포트 도달 불가";
                case IPStatus.DestinationProhibited: return "대상 접근 금지";
                case IPStatus.BadDestination: return "잘못된 대상";
                case IPStatus.BadRoute: return "잘못된 경로";
                case IPStatus.TtlExpired: return "TTL 만료";
                case IPStatus.TtlReassemblyTimeExceeded: return "재조립 시간 초과";
                case IPStatus.TimeExceeded: return "전송 시간 초과";
                case IPStatus.PacketTooBig: return "패킷 크기 초과";
                case IPStatus.HardwareError: return "하드웨어 오류";
                case IPStatus.NoResources: return "리소스 부족";
                case IPStatus.SourceQuench: return "상대 혼잡";
                case IPStatus.BadOption: return "잘못된 옵션";
                case IPStatus.BadHeader: return "잘못된 헤더";
                case IPStatus.ParameterProblem: return "매개변수 오류";
                case IPStatus.IcmpError: return "ICMP 오류";
                case IPStatus.UnrecognizedNextHeader: return "알 수 없는 헤더";
                case IPStatus.DestinationScopeMismatch: return "주소 범위 불일치";
                default: return "알 수 없음(" + status + ")";
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("사용법: PingTest.exe <대상> [<대상> ...] [옵션]");
            Console.WriteLine();
            Console.WriteLine("  -i <ms>     전송 간격 (기본 " + DefaultIntervalMs + ")");
            Console.WriteLine("  -w <ms>     응답 대기 타임아웃 (기본 " + DefaultTimeoutMs + ")");
            Console.WriteLine("  -l <byte>   데이터 크기 (기본 " + DefaultBufferSize + ")");
            Console.WriteLine("  -n <횟수>   대상별 전송 횟수, 0이면 Ctrl+C 까지 (기본 0)");
            Console.WriteLine("  -o <폴더>   로그 폴더 (기본 실행파일 폴더\\logs)");
            Console.WriteLine();
            Console.WriteLine("예) PingTest.exe 172.16.151.70 172.16.151.71 -i 500");
            Console.WriteLine();
            Console.WriteLine("인자 없이 실행(더블클릭)하면 PingTest.exe.config 의 Hosts 등 설정값으로 바로 시작합니다.");
            Console.WriteLine("인자를 주면 같은 항목은 인자가 설정값보다 우선합니다.");
        }

        private sealed class Options
        {
            public readonly List<string> Hosts = new List<string>();
            public int IntervalMs = DefaultIntervalMs;
            public int TimeoutMs = DefaultTimeoutMs;
            public int BufferSize = DefaultBufferSize;
            public int Count;
            public string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            public bool ShowHelp;

            public static Options Parse(string[] args)
            {
                var o = FromConfig();
                var argHosts = new List<string>();
                for (var i = 0; i < args.Length; i++)
                {
                    var a = args[i];
                    switch (a.ToLowerInvariant())
                    {
                        case "-h":
                        case "-?":
                        case "/?":
                        case "--help":
                            o.ShowHelp = true;
                            break;
                        case "-i": o.IntervalMs = ReadInt(args, ref i, a, 1); break;
                        case "-w": o.TimeoutMs = ReadInt(args, ref i, a, 1); break;
                        case "-l": o.BufferSize = ReadInt(args, ref i, a, 0, 65500); break;
                        case "-n": o.Count = ReadInt(args, ref i, a, 0); break;
                        case "-o":
                            if (++i >= args.Length) throw new ArgumentException(a + " 뒤에 폴더가 필요합니다.");
                            o.LogDirectory = args[i];
                            break;
                        default:
                            if (a.StartsWith("-", StringComparison.Ordinal))
                                throw new ArgumentException("알 수 없는 옵션 " + a);
                            argHosts.Add(a);
                            break;
                    }
                }

                // 인자로 대상을 주면 설정 파일의 대상에 덧붙이지 않고 통째로 바꾼다 — 잠깐 다른 주소만 볼 때 쓰는 용도.
                if (argHosts.Count > 0)
                {
                    o.Hosts.Clear();
                    o.Hosts.AddRange(argHosts);
                }
                return o;
            }

            private static Options FromConfig()
            {
                var o = new Options();
                var s = ConfigurationManager.AppSettings;

                var hosts = s["Hosts"];
                if (!string.IsNullOrWhiteSpace(hosts))
                    o.Hosts.AddRange(hosts.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries));

                o.IntervalMs = ReadConfigInt(s["IntervalMs"], "IntervalMs", o.IntervalMs, 1);
                o.TimeoutMs = ReadConfigInt(s["TimeoutMs"], "TimeoutMs", o.TimeoutMs, 1);
                o.BufferSize = ReadConfigInt(s["BufferSize"], "BufferSize", o.BufferSize, 0, 65500);
                o.Count = ReadConfigInt(s["Count"], "Count", o.Count, 0);

                var logDir = s["LogDirectory"];
                if (!string.IsNullOrWhiteSpace(logDir))
                    o.LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logDir.Trim());
                return o;
            }

            private static int ReadConfigInt(string text, string key, int defaultValue, int min, int max = int.MaxValue)
            {
                if (string.IsNullOrWhiteSpace(text))
                    return defaultValue;
                int value;
                if (!int.TryParse(text.Trim(), out value) || value < min || value > max)
                    throw new ArgumentException("PingTest.exe.config 의 " + key + " 값(" + text + ")은 " + min + " 이상 " + (max == int.MaxValue ? "" : max + " 이하 ") + "정수여야 합니다.");
                return value;
            }

            private static int ReadInt(string[] args, ref int i, string name, int min, int max = int.MaxValue)
            {
                int value;
                if (++i >= args.Length || !int.TryParse(args[i], out value) || value < min || value > max)
                    throw new ArgumentException(name + " 값은 " + min + " 이상 " + (max == int.MaxValue ? "" : max + " 이하 ") + "정수여야 합니다.");
                return value;
            }
        }

        private sealed class PingResult
        {
            public DateTime Time;
            public string Host;
            public string Address;
            public string Status;
            public bool Success;
            public long RoundtripMs;
            public int Ttl;
            public int Bytes;
            public string Error;

            /// <summary>엑셀로 열어 시간대별로 걸러 보기 쉽도록 탭 구분으로 남긴다.</summary>
            public string ToLogLine()
            {
                return string.Join("\t",
                    Time.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                    Host,
                    Address,
                    Status,
                    Success ? RoundtripMs.ToString(CultureInfo.InvariantCulture) : "",
                    Success ? Ttl.ToString(CultureInfo.InvariantCulture) : "",
                    Success ? Bytes.ToString(CultureInfo.InvariantCulture) : "",
                    Error ?? "");
            }
        }

        private sealed class HostStats
        {
            private int _sent;
            private int _received;
            private long _min = long.MaxValue;
            private long _max;
            private long _sum;

            public void Add(PingResult r)
            {
                _sent++;
                if (!r.Success) return;
                _received++;
                _sum += r.RoundtripMs;
                if (r.RoundtripMs < _min) _min = r.RoundtripMs;
                if (r.RoundtripMs > _max) _max = r.RoundtripMs;
            }

            public string ToSummary(string host)
            {
                var lost = _sent - _received;
                var lossPct = _sent == 0 ? 0 : lost * 100.0 / _sent;
                var text = string.Format(CultureInfo.InvariantCulture,
                    "[{0}] 보냄={1} 받음={2} 손실={3} ({4:0.#}%)", host, _sent, _received, lost, lossPct);
                if (_received > 0)
                    text += string.Format(CultureInfo.InvariantCulture,
                        " 최소={0}ms 최대={1}ms 평균={2:0.#}ms", _min, _max, (double)_sum / _received);
                return text;
            }
        }

        /// <summary>
        /// 날짜가 바뀌면 새 파일로 넘어간다. 오래 띄워 두는 도구라 하나의 파일이 끝없이 커지지 않게 하려는 것.
        /// 매 줄 Flush 하는 이유: 창을 닫거나 PC가 꺼져도 직전까지의 기록이 남아야 끊김 시각을 확인할 수 있다.
        /// </summary>
        private sealed class PingLog : IDisposable
        {
            public readonly string Directory;
            private StreamWriter _writer;
            private DateTime _date;

            public PingLog(string directory)
            {
                Directory = Path.GetFullPath(directory);
                System.IO.Directory.CreateDirectory(Directory);
            }

            public void WriteLine(string line)
            {
                var today = DateTime.Today;
                if (_writer == null || today != _date)
                {
                    if (_writer != null) _writer.Dispose();
                    var path = Path.Combine(Directory, "ping_" + today.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".log");
                    var isNew = !File.Exists(path);
                    _writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite), new UTF8Encoding(true));
                    _date = today;
                    if (isNew)
                        _writer.WriteLine(string.Join("\t", "시간", "대상", "응답IP", "상태", "시간(ms)", "TTL", "byte", "오류"));
                }
                _writer.WriteLine(line);
                _writer.Flush();
            }

            public void Dispose()
            {
                if (_writer != null) _writer.Dispose();
                _writer = null;
            }
        }
    }
}
