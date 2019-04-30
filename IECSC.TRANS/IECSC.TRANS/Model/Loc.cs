using System;
using System.Collections.Generic;

namespace IECSC.TRANS
{
    public class Loc
    {
        /// <summary>
        /// 站台编号
        /// </summary>
        public string LocNo { get; set; }
        /// <summary>
        /// PLC编号
        /// </summary>
        public string LocPlcNo { get; set; }
        /// <summary>
        /// 站台类型编号
        /// </summary>
        public string LocTypeNo { get; set; }
        /// <summary>
        /// 站台类型描述
        /// </summary>
        public string LocTypeDesc { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string TaskType { get; set; }
        /// <summary>
        /// 业务状态
        /// </summary>
        public BizStatus bizStatus = BizStatus.None;
        /// <summary>
        /// PLC状态信息
        /// </summary>
        public LocPlcStatus plcStatus = new LocPlcStatus();
        /// <summary>
        /// 当前指令信息
        /// </summary>
        public TaskCmd taskCmd = new TaskCmd();
        /// <summary>
        /// 以当前站台为起点的所有指令信息
        /// 用于界面指令信息刷新
        /// </summary>
        public List<TaskCmd> TaskList = new List<TaskCmd>();
        /// <summary>
        /// 请求生成任务参数表OBJID
        /// </summary>
        public int RequestTaskObjid { get; set; } = 0;
        /// <summary>
        /// 请求生成指令参数表OBJID
        /// </summary>
        public int RequestCmdObjid { get; set; } = 0;
        /// <summary>
        /// 请求任务结束参数表OBJID
        /// </summary>
        public int RequestFinishObjid { get; set; } = 0;

        /// <summary>
        /// 请求生成绑定空托盘参数表OBJID
        /// </summary>
        public int RequestBindObjid { get; set; } = 0;

        private string execLog = string.Empty;
        /// <summary>
        /// 运行日志记录
        /// </summary>
        public string ExecLog
        {
            get
            {
                return execLog;
            }
            set
            {
                if(execLog.Length > 1000)
                {
                    execLog = string.Empty;
                }
                if(value.Equals(LastExecLog))
                {
                    return;
                }
                LastExecLog = value;
                execLog += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  " + LastExecLog + Environment.NewLine;
            }
        }
        /// <summary>
        /// 上次运行日志
        /// </summary>
        public string LastExecLog { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public int TaskNo { get; set; }
    }

    public enum BizStatus
    {
        /// <summary>
        /// 初始状态，检测信号
        /// </summary>
        None = 0,
        /// <summary>
        /// 叠盘机绑定空盘
        /// </summary>
        Bind =1,
        /// <summary>
        /// 请求任务
        /// </summary>
        ReqTask = 2,
        /// <summary>
        /// 请求指令
        /// </summary>
        ReqCmd = 3,
        /// <summary>
        /// 获取数据-指令
        /// </summary>
        Select = 4,
        /// <summary>
        /// 下传指令
        /// </summary>
        WriteTask = 5,
        /// <summary>
        /// 下传已处理信号
        /// </summary>
        WriteDeal = 6,
        /// <summary>
        /// 更新指令步骤
        /// </summary>
        Update = 7,
        /// <summary>
        /// 结束指令
        /// </summary>
        Finish = 8,
        /// <summary>
        /// 完成-传递任务已处理信号
        /// </summary>
        End = 9,
        /// <summary>
        /// 复位
        /// </summary>
        Reset = 10
    }
}
