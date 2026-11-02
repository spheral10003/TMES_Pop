using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace SEOJIN_TMES.Client.Common.Logging
{
    /// <summary>
    /// 이 PC의 네트워크 상태(사용 중인 어댑터의 IPv4 · 게이트웨이, Wi-Fi SSID · BSSID · 신호)를 한 줄로 만든다.
    ///
    /// 현장에서 "재부팅 후 장비가 응답하지 않다가 SSID를 바꾸면 된다"는 증상이 있었는데(2026-11-02),
    /// 로그에 PC 쪽 IP · SSID가 없어 바뀐 것이 IP인지 AP인지 가릴 수 없었다. 그래서 설비 연결 시작 시점과
    /// 네트워크가 바뀔 때마다 이 값을 남긴다. 진단용이라 어떤 실패도 밖으로 던지지 않는다.
    /// </summary>
    public static class NetworkSnapshot
    {
        private const int NetshTimeoutMs = 3000;

        /// <summary>예: <c>Wi-Fi 172.16.151.81/22 GW 172.16.148.1 | SSID Texon_Office_5G BSSID cc:2d:d2:8c:36:30 신호 91%</c></summary>
        public static string Describe()
        {
            var parts = new List<string> { DescribeAdapters() };

            var wifi = DescribeWifi();
            if (!string.IsNullOrEmpty(wifi))
                parts.Add(wifi);

            return string.Join(" | ", parts);
        }

        private static string DescribeAdapters()
        {
            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up
                                && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .Select(DescribeAdapter)
                    .Where(text => text != null)
                    .ToList();

                return adapters.Count == 0 ? "사용 중인 네트워크 없음" : string.Join(", ", adapters);
            }
            catch (Exception ex)
            {
                return $"네트워크 조회 실패({ex.GetType().Name})";
            }
        }

        private static string DescribeAdapter(NetworkInterface adapter)
        {
            var properties = adapter.GetIPProperties();
            var addresses = properties.UnicastAddresses
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => $"{a.Address}/{a.PrefixLength}")
                .ToList();
            if (addresses.Count == 0)
                return null;

            var gateway = properties.GatewayAddresses
                .Select(g => g.Address)
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            return $"{adapter.Name} {string.Join(",", addresses)} GW {gateway?.ToString() ?? "-"}";
        }

        /// <summary>
        /// <c>netsh wlan show interfaces</c>에서 SSID · BSSID · 신호만 뽑는다(관리자 권한 불필요).
        /// 출력이 OS 언어를 따르는데 SSID · BSSID는 어느 언어든 그대로 나오고, 신호는 "신호"/"Signal" 둘 다 받는다.
        /// Wi-Fi가 없거나 끊겨 있으면 빈 문자열.
        /// </summary>
        private static string DescribeWifi()
        {
            try
            {
                var psi = new ProcessStartInfo("netsh.exe", "wlan show interfaces")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                };

                string output;
                using (var process = Process.Start(psi))
                {
                    // 출력을 먼저 읽어야 버퍼가 차서 netsh가 멈추는 일이 없다.
                    var read = process.StandardOutput.ReadToEndAsync();
                    if (!process.WaitForExit(NetshTimeoutMs))
                        return "Wi-Fi 조회 시간 초과";
                    output = read.Result;
                }

                var ssid = Match(output, @"^\s*SSID\s*:\s*(.+)$");
                if (ssid == null)
                    return string.Empty;

                var bssid = Match(output, @"^\s*(?:AP\s+)?BSSID\s*:\s*(.+)$");
                var signal = Match(output, @"^\s*(?:신호|Signal)\s*:\s*(.+)$");
                return $"SSID {ssid} BSSID {bssid ?? "-"} 신호 {signal ?? "-"}";
            }
            catch (Exception ex)
            {
                return $"Wi-Fi 조회 실패({ex.GetType().Name})";
            }
        }

        private static string Match(string text, string pattern)
        {
            var match = Regex.Match(text, pattern, RegexOptions.Multiline);
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }
    }
}
