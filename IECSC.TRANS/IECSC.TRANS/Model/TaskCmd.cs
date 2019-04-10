
using System;
using System.ComponentModel;
using System.Text;

namespace IECSC.TRANS
{ 
    public class TaskCmd 
    {
        /// <summary>
        /// 指令号
        /// </summary>
        public int ObjId { get; set; }
        /// <summary>
        /// 当前任务号
        /// </summary>
        public int TaskNo { get; set; }
        /// <summary>
        /// 指令类型
        /// </summary>
        public string CmdType { get; set; }
        /// <summary>
        /// 指令步骤
        /// </summary>
        public string CmdStep { get; set; }
        /// <summary>
        /// 起始地址类型
        /// </summary>
        public string SlocType { get; set; }
        /// <summary>
        /// 起始地址
        /// </summary>
        public string SlocNo { get; set; }
        /// <summary>
        /// 起始PLC地址
        /// </summary>
        public string SlocPlcNo { get; set; }
        /// <summary>
        /// 目的地址类型
        /// </summary>
        public string ElocType { get; set; }
        /// <summary>
        /// 结束地址
        /// </summary>
        public string ElocNo { get; set; }
        /// <summary>
        /// 结束PLC地址
        /// </summary>
        public string ElocPlcNo { get; set; }
        /// <summary>
        /// RFID号码
        /// </summary>
        public string PalletNo { get; set; }
        /// <summary>
        /// 起始站台区域
        /// </summary>
        public int SlocArea
        {
            get
            {
                return (int)Encoding.ASCII.GetBytes(SlocPlcNo.Substring(0, 1))[0];
            }
        }
        /// <summary>
        /// 起始站台编号
        /// </summary>
        public string SlocCode
        {
            get
            {
                return SlocPlcNo.Substring(1);
            }
        }
        /// <summary>
        /// 目的站台区域
        /// </summary>
        public int ElocArea
        {
            get
            {
                return (int)Encoding.ASCII.GetBytes(ElocPlcNo.Substring(0, 1))[0];
            }
        }
        /// <summary>
        /// 目的站台编号
        /// </summary>
        public string ElocCode
        {
            get
            {
                return ElocPlcNo.Substring(1);
            }
        }
    }
}
