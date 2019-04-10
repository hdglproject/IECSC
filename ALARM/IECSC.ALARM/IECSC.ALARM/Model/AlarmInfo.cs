using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSC.ALARM
{
    public class AlarmInfo
    {
        /// <summary>
        /// 报警设备编号
        /// </summary>
        public string LocPlcNo { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// 序号
        /// </summary>
        public string TagIndex { get; set; }
        /// <summary>
        /// 报警描述
        /// </summary>
        public string Discrip { get; set; }
        /// <summary>
        /// 项长名
        /// </summary>
        public string TagLongName { get; set; }
        /// <summary>
        /// 报警PLC标记
        /// </summary>
        public int TagValue { get; set; } = 0;
        /// <summary>
        /// 报警处理标记
        /// </summary>
        public int AlarmMark { get; set; } = 0;
        /// <summary>
        /// 报警记录OBJID
        /// </summary>
        public int Objid { get; set; } 
    }
}
