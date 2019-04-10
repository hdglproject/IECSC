using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSC.SRM
{
    public class Srm : INotifyPropertyChanged
    {
        /// <summary>
        /// 堆垛机编号
        /// </summary>
        public string SrmNo { get; set; } = McConfig.Instance.SrmNo;
        /// <summary>
        /// 堆垛机名称
        /// </summary>
        public string SrmName = string.Empty;
        /// <summary>
        /// 堆垛机业务状态
        /// </summary>
        public BizStatus bizStatus = new BizStatus();
        /// <summary>
        /// PLC状态信息
        /// </summary>
        public SrmPlcStatus plcStatus = new SrmPlcStatus();
        /// <summary>
        /// 堆垛机故障信息索引器
        /// </summary>
        private Hashtable FaultInfo = new Hashtable();
        public string this[string faultNo]
        {
            get
            {
                return FaultInfo[faultNo].ToString();
            }
            set
            {
                if (FaultInfo == null)
                {
                    FaultInfo = new Hashtable();
                }
                FaultInfo.Add(faultNo, value);
            }
        }
        /// <summary>
        /// 最近一次SRM的心跳
        /// </summary>
        public int LastPlcHeartBeat { get; set; } = -1;
        /// <summary>
        /// 最近一次SRM的心跳记录时间
        /// </summary>
        public DateTime LastPlcHeartBeatTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 任务信息
        /// </summary>
        public TaskCmd taskCmd = new TaskCmd();
        /// <summary>
        /// 请求任务结束参数表OBJID
        /// </summary>
        public int RequestFinishObjid { get; set; } = 0;
        /// <summary>
        /// 下位机联机状态变更标记 0:已停机 1:已联机
        /// </summary>
        public int RecordLastOperateMode { get; set; } = 0;
        /// <summary>
        /// 记录PLC报警日志的OBJID
        /// </summary>
        public int RecordLastLogObjid { get; set; } = 0;
        /// <summary>
        /// 已记录的PLC报警
        /// </summary>
        public int RecordLastFaultNo { get; set; } = 0;

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 属性更改通知事件
        /// </summary>
        /// <param name="info"></param>
        public void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private int objid;
        /// <summary>
        /// 指令号
        /// </summary>
        public int Objid
        {
            get { return this.objid; }
            set
            {
                this.objid = value;
                NotifyPropertyChanged(nameof(Objid));
            }
        }
        private int taskNo;
        /// <summary>
        /// 任务号
        /// </summary>
        public int TaskNo
        {
            get { return this.taskNo; }
            set
            {
                this.taskNo = value;
                NotifyPropertyChanged(nameof(TaskNo));
            }
        }
        private string cmdStep;
        /// <summary>
        /// 指令步骤
        /// </summary>
        public string CmdStep
        {
            get { return this.cmdStep; }
            set
            {
                this.cmdStep = value;
                NotifyPropertyChanged(nameof(CmdStep));
            }
        }
        private string taskType;
        /// <summary>
        /// 任务类型
        /// </summary>
        public string TaskType
        {
            get { return this.taskType; }
            set
            {
                this.taskType = value;
                NotifyPropertyChanged(nameof(TaskType));
            }
        }
        private string palletNo;
        /// <summary>
        /// 工装编号
        /// </summary>
        public string PalletNo
        {
            get { return this.palletNo; }
            set
            {
                this.palletNo = value;
                NotifyPropertyChanged(nameof(PalletNo));
            }
        }
        private string fromLoc;
        /// <summary>
        /// 任务起始位置 站台或库位
        /// </summary>
        public string FromLoc
        {
            get { return this.fromLoc; }
            set
            {
                this.fromLoc = value;
                NotifyPropertyChanged(nameof(FromLoc));
            }
        }
        private string toLoc;
        /// <summary>
        /// 任务结束位置 站台或库位
        /// </summary>
        public string ToLoc
        {
            get { return this.toLoc; }
            set
            {
                this.toLoc = value;
                NotifyPropertyChanged(nameof(ToLoc));
            }
        }

        /// <summary>
        /// 设置堆垛机任务信息
        /// </summary>
        public void SetSrmTaskInfo(TaskCmd cmd)
        {
            this.Objid = cmd.ObjId;
            this.TaskNo = cmd.TaskNo;
            switch(cmd.CmdStep)
            {
                case "00":
                    this.CmdStep = "等待下发";
                    break;
                case "02":
                    this.CmdStep = "执行";
                    break;
                case "04":
                    this.CmdStep = "完成";
                    break;
                default:
                    this.CmdStep = "异常";
                    break;
            }
            this.TaskType = cmd.CmdType;
            this.PalletNo = cmd.PalletNo;
            this.FromLoc = cmd.SlocPlcNo;
            this.ToLoc = cmd.ElocPlcNo;
        }
        /// <summary>
        /// 清除堆垛机任务信息
        /// </summary>
        public void ClearSrmTaskInfo()
        {
            this.Objid = 0;
            this.TaskNo = 0;
            this.CmdStep = string.Empty;
            this.TaskType = string.Empty;
            this.PalletNo = string.Empty;
            this.FromLoc = string.Empty;
            this.ToLoc = string.Empty;
        }
    }

    /// <summary>
    /// 业务状态
    /// </summary>
    public enum BizStatus
    {
        /// <summary>
        /// 初始状态
        /// </summary>
        None = 0,
        /// <summary>
        /// 已从数据库获取,等待下传
        /// </summary>
        Down = 1,
        /// <summary>
        /// 等待PLC任务状态变更为1
        /// </summary>
        Ready = 2,
        /// <summary>
        /// 已下传PLC,正在执行
        /// </summary>
        Exec = 3,
        /// <summary>
        /// 已接受任务完成信号,并已回传任务信号2
        /// </summary>
        End = 4,
        /// <summary>
        /// 已接受复位信号0,并已回传复位信号0
        /// </summary>
        Reset = 5,
        /// <summary>
        /// 异常
        /// </summary>
        Error = 6,
        /// <summary>
        /// 异常处理
        /// </summary>
        ErrorDbDeal = 7,
        /// <summary>
        /// 异常处理
        /// </summary>
        ErrorPlcDeal = 8
    }
}
