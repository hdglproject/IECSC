
using System;
using System.Text;

namespace IECSC.SRM
{ 
    public class TaskCmd
    {
        /// <summary>
        /// ID序号
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
        /// 起始地址
        /// </summary>
        public string SlocNo { get; set; }
        /// <summary>
        /// 起始PLC地址
        /// </summary>
        public string SlocPlcNo { get; set; }
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
        public int EpArea
        {
            get
            {
                if (CmdType.Equals("I") || SlocPlcNo == "B1121")
                {
                    return (int)Encoding.ASCII.GetBytes(SlocPlcNo.Substring(0, 1))[0];
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 起始站台编号
        /// </summary>
        public int EpNo
        {
            get
            {
                if (CmdType.Equals("I")||SlocPlcNo=="B1121")
                {
                    return Convert.ToInt32(SlocPlcNo.Substring(1));
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 起始库位-行
        /// </summary>
        public int FromRow
        {
            get
            {
                if (CmdType.Equals("I"))
                {
                    return Convert.ToInt32(SlocPlcNo.Substring(0, 2));
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 起始库位-列
        /// </summary>
        public int FromBay
        {
            get
            {
                if (CmdType.Equals("I"))
                {
                    return Convert.ToInt32(SlocPlcNo.Substring(2, 2));
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 起始库位-排
        /// </summary>
        public int FromLevel
        {
            get
            {
                if (CmdType.Equals("I"))
                {
                    return Convert.ToInt32(SlocPlcNo.Substring(4, 2));
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 目的站台区域
        /// </summary>
        public int ApArea
        {
            get
            {
                if (CmdType.Equals("O"))
                {
                    return (int)Encoding.ASCII.GetBytes(ElocPlcNo.Substring(0, 1))[0];
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 目的站台编号
        /// </summary>
        public int ApNo
        {
            get
            {
                if (CmdType.Equals("O"))
                {
                    return Convert.ToInt32(ElocPlcNo.Substring(1));
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 目的库位-行
        /// </summary>
        public int ToRow
        {
            get
            {
                if (!CmdType.Equals("O"))
                {
                    return Convert.ToInt32(ElocPlcNo.Substring(0, 2));
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 目的库位-列
        /// </summary>
        public int ToBay
        {
            get
            {
                if (!CmdType.Equals("O"))
                {
                    return Convert.ToInt32(ElocPlcNo.Substring(2, 2));
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 目的库位-排
        /// </summary>
        public int ToLevel
        {
            get
            {
                if (!CmdType.Equals("O"))
                {
                    return Convert.ToInt32(ElocPlcNo.Substring(4, 2));
                }
                else
                {
                    return 0;
                }
            }
        }
        /// <summary>
        /// 目的库位-排
        /// </summary>
        public int MissionType
        {
            get
            {
                if (BizHandle.Instance.srm.plcStatus.LoadStatus == 1)
                {
                    return 5;
                }
                else
                {
                    return 1;
                }
            }
        }
    }
}
