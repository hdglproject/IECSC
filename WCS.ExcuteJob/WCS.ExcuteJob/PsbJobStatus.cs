using DapperExtensions.Mapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS.ExcuteJob
{
    ///<summary>
    ///表PSB_JOB_STATUS的实体类
    ///</summary>
    public class PsbJobStatus : INotifyPropertyChanged
    {
        private long excuteCount = 0;
        private byte jobStatus = 0;
        private DateTime lastExcuteTime;
        private long maxExcuteTime = 0;
        private long minExcuteTime = 0;
        private long avgExcuteTime = 0;
        private long totalExcuteTime = 0;
        private string excuteResult = string.Empty;

        /// <summary>
        /// 作业编码
        /// </summary>
        public long ID { get; set; }
        /// <summary>
        /// 作业编码
        /// </summary>
        public string JobNo { get; set; }
        /// <summary>
        /// 作业名称
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// 间隔时间//秒钟
        /// </summary>
        public long Interval { get; set; }
        /// <summary>
        /// 0:无执行  1：执行中  2：执行成功  3：执行失败  4:暂停执行
        /// </summary>
        public byte JobStatus {
            get { return jobStatus; }
            set
            {
                this.jobStatus = value;
                NotifyPropertyChanged(nameof(JobStatus));
            }
        }
        /// <summary>
        /// 执行结果
        /// </summary>
        public string ExcuteResult {
            get { return excuteResult; }
            set
            {
                this.excuteResult = value;
                NotifyPropertyChanged(nameof(ExcuteResult));
            }
        }
        /// <summary>
        /// 使用标志  1：启用 0：停用
        /// </summary>
        public byte UsedFlag { get; set; }
        /// <summary>
        /// 最近一次执行时间
        /// </summary>
        public System.DateTime LastExcuteTime {
            get { return lastExcuteTime; }
            set
            {
                this.lastExcuteTime = value;
                NotifyPropertyChanged(nameof(LastExcuteTime));
            }
        }
        /// <summary>
        /// 最长执行时长(毫秒)
        /// </summary>
        public long MaxExcuteTime {
            get { return maxExcuteTime; }
            set
            {
                this.maxExcuteTime = value;
                NotifyPropertyChanged(nameof(MaxExcuteTime));
            }
        }
        /// <summary>
        /// 最短执行时长(毫秒)
        /// </summary>
        public long MinExcuteTime {
            get { return minExcuteTime; }
            set
            {
                this.minExcuteTime = value;
                NotifyPropertyChanged(nameof(MinExcuteTime));
            }
        }
        /// <summary>
        /// 平均执行时长(毫秒)
        /// </summary>
        public long AvgExcuteTime {
            get { return avgExcuteTime; }
            set
            {
                this.avgExcuteTime = value;
                NotifyPropertyChanged(nameof(AvgExcuteTime));
            }
        }
        /// <summary>
        /// 总执行次数
        /// </summary>
        public long ExcuteCount
        {
            get { return excuteCount; }
            set { this.excuteCount = value;
                NotifyPropertyChanged(nameof(ExcuteCount));
            }
        }
        /// <summary>
        /// 总执行时间
        /// </summary>
        public long TotalExcuteTime
        {
            get { return totalExcuteTime; }
            set
            {
                this.totalExcuteTime = value;
                NotifyPropertyChanged(nameof(TotalExcuteTime));
            }
        }
        /// <summary>
        /// 执行存储过程的名称
        /// </summary>
        public string SpName { get; set; }

        /// <summary>
        /// 开始执行时间
        /// </summary>
        public DateTime StartExcuteTime { get; set; }
        /// <summary>
        /// 结束执行时间
        /// </summary>
        public DateTime FinishExcuteTime { get; set; }
        /// <summary>
        /// 执行状态 0 未在执行，1 正在执行，2 执行完成
        /// </summary>
        public int ExcuteStatus { get; set; } = 0;
        /// <summary>
        /// 作业是否为首次执行
        /// </summary>
        public bool IfFirstExcute { get; set; } = true;
        /// <summary>
        /// 作业是否暂时挂起
        /// </summary>
        public bool IfSuspend { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }

    ///<summary>
    ///表PSB_JOB_STATUS的实体ID映射类
    ///</summary>
    public class PsbJobStatusOraMap : ClassMapper<PsbJobStatus>
    {
        public PsbJobStatusOraMap()
        {
            Table("PSB_JOB_STATUS");
            Map(x => x.ID).Key(KeyType.TriggerIdentity);

            Map(x => x.JobNo).Column("JOB_NO");
            Map(x => x.JobName).Column("JOB_NAME");
            Map(x => x.Interval).Column("INTERVAL");
            Map(x => x.JobStatus).Column("JOB_STATUS");
            Map(x => x.ExcuteResult).Column("EXCUTE_RESULT");
            Map(x => x.UsedFlag).Column("USED_FLAG");
            Map(x => x.LastExcuteTime).Column("LAST_EXCUTE_TIME");
            Map(x => x.MaxExcuteTime).Column("MAX_EXCUTE_TIME");
            Map(x => x.MinExcuteTime).Column("MIN_EXCUTE_TIME");
            Map(x => x.AvgExcuteTime).Column("AVG_EXCUTE_TIME");
            Map(x => x.TotalExcuteTime).Column("TOTAL_EXCUTE_TIME");
            Map(x => x.ExcuteCount).Column("EXCUTE_COUNT"); 
            Map(x => x.SpName).Column("SP_NAME");

            Map(x => x.StartExcuteTime).Ignore();
            Map(x => x.FinishExcuteTime).Ignore();
            Map(x => x.ExcuteStatus).Ignore();
            Map(x => x.IfFirstExcute).Ignore();
            Map(x => x.IfSuspend).Ignore();
            AutoMap();
        }
    }
}
