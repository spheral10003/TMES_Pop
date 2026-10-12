using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SEOJIN_TMES.Client.Sockets
{
    public class NetTool
    {
        public string GetLocalIP
        {
            get
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                string LocalIP = string.Empty;
                for (int i = 0; i < host.AddressList.Length; i++)
                {
                    if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        LocalIP = host.AddressList[i].ToString();
                    }
                }
                return LocalIP;
            }
        }
    }
}
