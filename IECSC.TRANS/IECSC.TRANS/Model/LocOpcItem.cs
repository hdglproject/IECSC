using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSC.TRANS
{
    public class LocOpcItem
    {
        /// <summary>
        /// 站台编号
        /// </summary>
        public string LocNo { get; set; }
        /// <summary>
        /// 站台PLC编号
        /// </summary>
        public string LocPlcNo { get; set; }
        /// <summary>
        /// 测点长名
        /// </summary>
        public string TagLongName { get; set; }
        /// <summary>
        /// 业务唯一标示
        /// </summary>
        public string BusIdentity { get; set; }
    }
}
