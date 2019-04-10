using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace IECSC.TRANS
{
    public class Tools
    {
        #region 单例模式
        private static Tools _instance = null;
        public static Tools Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(Tools))
                    {
                        if (_instance == null)
                        {
                            _instance = new Tools();
                        }
                    }
                }
                return _instance;
            }
        }
        public Tools()
        {
        }
        #endregion

        /// <summary>
        /// Ping IP
        /// </summary>
        public bool PingNetAddress(string Ip)
        {
            bool flage;
            Ping ping = new Ping();
            try
            {
                PingReply pr = ping.Send(Ip, 3000);
                if (pr != null && pr.Status == IPStatus.TimedOut)
                {
                    return false;

                }
                if (pr != null && pr.Status == IPStatus.Success)
                {
                    flage = true;
                }
                else
                {
                    flage = false;
                }
            }
            catch
            {
                flage = false;
            }
            return flage;
        }
    }
}
