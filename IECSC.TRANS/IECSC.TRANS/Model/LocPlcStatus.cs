using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSC.TRANS
{
    public class LocPlcStatus
    {
        /// <summary>
        /// 任务号
        /// </summary>
        public long TaskNo { get; set; }
        /// <summary>
        /// 托盘号
        /// </summary>
        public string PalletNo { get; set; }
        /// <summary>
        /// 源地址
        /// </summary>
        public string Sloc => this.SlocArea + this.SlocNo;
        /// <summary>
        /// 源地址区域符号
        /// </summary>
        public string SlocArea { get; set; }
        /// <summary>
        /// 源地址设备Id
        /// </summary>
        public string SlocNo { get; set; }
        /// <summary>
        /// 源地址
        /// </summary>
        public string Eloc => this.ElocArea + this.ElocNo;
        /// <summary>
        /// 目的地址区域符号
        /// </summary>
        public string ElocArea { get; set; }
        /// <summary>
        /// 目的地址设备ID
        /// </summary>
        public string ElocNo { get; set; }
        /// <summary>
        /// 自动 标识
        /// </summary>
        public int StatusAuto { get; set; }
        /// <summary>
        /// 故障 标识
        /// </summary>
        public int StatusFault { get; set; }
        /// <summary>
        /// 有载 标识
        /// </summary>
        public int StatusLoading { get; set; }
        /// <summary>
        /// 请求任务 标识
        /// </summary>
        public int StatusRequest { get; set; }
        /// <summary>
        /// 空闲可放货 标识
        /// </summary>
        public int StatusFree { get; set; }
        /// <summary>
        /// 有货需取货 标识
        /// </summary>
        public int StatusToLoad { get; set; }

    }
}
