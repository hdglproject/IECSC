using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WCS.ExcuteJob
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //业务类
        private JobBiz _JobBiz = new JobBiz();
        //业务线程
        private Thread _Thread;
        //Jobs集合
        private IEnumerable<PsbJobStatus> _Jobs;
        //业务状态是否启用
        private bool IsBizEnable = true;
        //业务逻辑循环等待时长，毫秒
        private int ThreadExcuteInterval = 1000;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void Init()
        {
            _Jobs = _JobBiz.GetAllJob();

            this.dgMain.ItemsSource = _Jobs;

            //实例化并启动业务线程
            _Thread = new Thread(ThreadLogic);
            _Thread.IsBackground = true;
            _Thread.Start();
        }

        /// <summary>
        /// 线程逻辑
        /// </summary>
        private void ThreadLogic()
        {
            while (true)
            {
                if (IsBizEnable)
                {
                    foreach (var job in _Jobs)
                    {
                        ExcuteJob(job);
                    }
                }
                Thread.Sleep(ThreadExcuteInterval);
            }
        }

        /// <summary>
        /// 执行指定的作业
        /// </summary>
        /// <param name="job"></param>
        private void ExcuteJob(PsbJobStatus job)
        {
            //判断作业是否临时挂起
            if (job.IfSuspend)
            {
                return;
            }
            if(job.UsedFlag == 0)
            {
                return;
            }
            if (job.ExcuteStatus == 0 || job.ExcuteStatus == 2)
            {
                //判断是否达到执行时间间隔
                var excuteFinishDuration = Convert.ToInt64((DateTime.Now - job.FinishExcuteTime).TotalSeconds);
                if (excuteFinishDuration < job.Interval)
                {
                    return;
                }
                //达到执行时间间隔，执行作业
                job.StartExcuteTime = DateTime.Now;
                job.LastExcuteTime = DateTime.Now;
                job.ExcuteStatus = 1;
                job.JobStatus = 1;
                //Task.Run(() =>
                //{
                    //执行作业
                    string result = _JobBiz.ExcuteJob(job);
                    job.FinishExcuteTime = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        job.ExcuteStatus = 0;
                        job.JobStatus = 3;
                        job.ExcuteResult = result;
                        ShowInfo($"作业({job.JobNo}) 执行失败：{result}");
                        return;
                    }
                    //作业第一次执行不统计信息
                    if (job.IfFirstExcute)
                    {
                        job.ExcuteStatus = 2;
                        job.JobStatus = 2;
                        job.IfFirstExcute = false;
                        return;
                    }
                    //统计信息
                    var excuteDuration = Convert.ToInt32((job.FinishExcuteTime - job.StartExcuteTime).TotalMilliseconds);
                    if (job.ExcuteCount > 0)
                    {
                        job.TotalExcuteTime += excuteDuration;
                        job.ExcuteCount++;
                        job.AvgExcuteTime = job.TotalExcuteTime / job.ExcuteCount;

                        if (excuteDuration > job.MaxExcuteTime)
                        {
                            job.MaxExcuteTime = excuteDuration;
                        }
                        if (excuteDuration < job.MinExcuteTime)
                        {
                            job.MinExcuteTime = excuteDuration;
                        }
                    }
                    else
                    {
                        job.ExcuteCount = 1;
                        job.MaxExcuteTime = excuteDuration;
                        job.MinExcuteTime = excuteDuration;
                        job.AvgExcuteTime = excuteDuration;
                        job.TotalExcuteTime = excuteDuration;
                    }
                    //更新作业执行信息
                    result = _JobBiz.UpdateJobExcuteInfo(job);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        job.ExcuteStatus = 2;
                        job.JobStatus = 3;
                        job.ExcuteResult = result;
                        ShowInfo($"作业({job.JobNo}) 执行失败：{result}");
                        return;
                    }
                    job.ExcuteResult = "执行成功";
                    job.ExcuteStatus = 2;
                    job.JobStatus = 2;
                    ShowInfo($"作业({job.JobNo}) 执行成功，耗时(ms) {excuteDuration}");
                //});
            }
        }

        /// <summary>
        /// 显示信息
        /// </summary>
        /// <param name="message"></param>
        private void ShowInfo(string message)
        {
            this.Dispatcher.Invoke(() =>
                {
                    this.txtRecord.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff：") + message + Environment.NewLine);
                    this.txtRecord.ScrollToEnd();
                    if (this.txtRecord.Text.Length > 5000)
                    {
                        this.txtRecord.Clear();
                    }
                }
            );
        }

        #region 按钮事件
        private void btnSuspendAllJob_Click(object sender, RoutedEventArgs e)
        {
            foreach (var job in _Jobs)
            {
                job.IfSuspend = true;
                job.JobStatus = 4;
                ShowInfo($"作业({job.JobNo})暂停成功");
            }
        }

        private void btnContinueAllJob_Click(object sender, RoutedEventArgs e)
        {
            foreach (var job in _Jobs)
            {
                job.IfSuspend = false;
                job.JobStatus = 0;
                ShowInfo($"作业({job.JobNo})继续成功");
            }
        }

        private void btnSuspendJob_Click(object sender, RoutedEventArgs e)
        {
            var job = this.dgMain.SelectedItem as PsbJobStatus;
            if (job != null)
            {
                job.IfSuspend = true;
                job.JobStatus = 4;
                ShowInfo($"作业({job.JobNo})暂停成功");
            }

        }

        private void btnContinueJob_Click(object sender, RoutedEventArgs e)
        {
            var job = this.dgMain.SelectedItem as PsbJobStatus;
            if (job != null)
            {
                job.IfSuspend = false;
                job.JobStatus = 0;
                ShowInfo($"作业({job.JobNo})继续成功");
            }
        }
        #endregion;
    }
}
