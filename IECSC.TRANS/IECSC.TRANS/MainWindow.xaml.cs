using IECSC.TRANS.CustomControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace IECSC.TRANS
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 定义委托进行跨线程操作控件
        /// </summary>
        private delegate void FlushForm(string msg, string locNo, InfoType infoType);
        /// <summary>
        /// 站台列列表
        /// </summary>
        private Dictionary<string, LocControl> locControlDic = new Dictionary<string, LocControl>();
        /// <summary>
        /// 限定站台
        /// </summary>
        private string LimitLocNo = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            //页面显示
            ShowFormData.Instance.OnAppDtoData += ShowInfo;
            //登陆时间
            this.lbTime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //初始化
            var errMsg = string.Empty;
            if (!InitDb(ref errMsg))
            {
                return;
            }
            if (!InitOpc(ref errMsg))
            {
                return;
            }
            //站台初始化
            InitLocControl();
            //业务处理
            var thBiz = new Thread(Run);
            thBiz.IsBackground = true;
            thBiz.Start();
            //PLC连接状态监控
            var thConn = new Thread(ConnStatus);
            thConn.IsBackground = true;
            thConn.Start();
        }

        /// <summary>
        /// 初始化数据库配置信息
        /// </summary>
        private bool InitDb(ref string errMsg)
        {
            try
            {
                if (DbAction.Instance.GetDbTime())
                {
                    ShowDbConnStatus("Y");
                }
                else
                {
                    ShowDbConnStatus("N");
                    return false;
                }
                if (DbAction.Instance.LoadOpcItems(ref errMsg))
                {
                    ShowExecLog("初始化站台数据库配置成功", string.Empty);
                }
                else
                {
                    ShowExecLog($"初始化站台数据库配置失败,原因{errMsg}", string.Empty);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowExecLog($"[异常]初始化数据库,[原因]{ex.Message}", string.Empty);
                return false;
            }
        }

        /// <summary>
        /// 初始化OPC信息
        /// </summary>
        private bool InitOpc(ref string errMsg)
        {
            try
            {
                if (Tools.Instance.PingNetAddress(McConfig.Instance.LocIp))
                {
                    ShowPlcConnStatus("Y");
                }
                else
                {
                    ShowPlcConnStatus("N");
                    return false;
                }
                if (OpcAction.Instance.ConnectOpc(ref errMsg))
                {
                    ShowExecLog("初始化OPC连接成功", string.Empty);
                }
                else
                {
                    ShowExecLog($"初始化OPC连接失败,原因{errMsg}", string.Empty);
                    return false;
                }
                if (OpcAction.Instance.AddOpcGroup(ref errMsg))
                {
                    ShowExecLog("初始化OPC组成功", string.Empty);
                }
                else
                {
                    ShowExecLog($"初始化OPC组失败,原因{errMsg}", string.Empty);
                    return false;
                }
                if (OpcAction.Instance.AddOpcItem(ref errMsg))
                {
                    ShowExecLog("初始化OPC项成功", string.Empty);
                }
                else
                {
                    ShowExecLog($"初始化OPC项失败,原因{errMsg}", string.Empty);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowExecLog($"[异常]初始化OPC,[原因]{ex.Message}", string.Empty);
                return false;
            }
        }

        /// <summary>
        /// 初始化站台信息
        /// </summary>
        private void InitLocControl()
        {
            foreach (var loc in BizHandle.Instance.locDic.Values.OrderBy(p => p.LocPlcNo))
            {
                var locControl = new LocControl();
                locControl.LocNo = loc.LocNo;
                locControl.LocPlcNo = loc.LocPlcNo;
                locControl.Width = 180;
                locControl.Height = 180;
                locControl.Margin = new Thickness(3);
                locControl.Click += LocControl_Click;
                this.locControlDic.Add(loc.LocNo, locControl);
                this.GridLocList.Children.Add(locControl);
            }
            ShowTaskCmd();
        }

        /// <summary>
        /// 单机站台状态控件事件
        /// </summary>
        private void LocControl_Click(string locNo)
        {
            if (locNo.Equals(LimitLocNo))
            {
                return;
            }
            var loc = BizHandle.Instance.locDic[locNo];
            this.gbExecLog.Header = $"{loc.LocPlcNo}运行日志";
            this.gbTaskCmd.Header = $"{loc.LocPlcNo}指令信息";
            this.txtLocRecord.Text = loc.ExecLog;
            this.dgv.ItemsSource = loc.TaskList;
            LimitLocNo = locNo;
        }

        /// <summary>
        /// 执行业务
        /// </summary>
        private void Run()
        {
            while (true)
            {
                //数据库连接验证
                if (DbAction.Instance.GetDbTime())
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("Y", string.Empty, InfoType.dbConn));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("N", string.Empty, InfoType.dbConn));
                    continue;
                }
                //根据业务步骤执行相关处理
                BizHandle.Instance.BizListen();

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// PLC连接状态监控
        /// </summary>
        private void ConnStatus()
        {
            while (true)
            {
                if (Tools.Instance.PingNetAddress(McConfig.Instance.LocIp))
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("Y", string.Empty, InfoType.plcConn));
                }
                else
                {
                    //记录断连时间
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("N", string.Empty, InfoType.plcConn));
                }

                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// 显示界面信息
        /// </summary>
        public void ShowInfo(object sender, AppDataEventArgs e)
        {
            var appData = e.AppData;
            var msg = appData.StringInfo;
            var locNo = appData.LocNo;
            var infoType = appData.InfoType;
            FormShow(msg, locNo, infoType);
        }

        private void FormShow(string msg, string locNo, InfoType infoType)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch (infoType)
                {
                    case InfoType.dbConn:
                        ShowDbConnStatus(msg);
                        break;
                    case InfoType.plcConn:
                        ShowPlcConnStatus(msg);
                        break;
                    case InfoType.logInfo:
                        ShowExecLog(msg, locNo);
                        break;
                    case InfoType.locStatus:
                        ShowLocStatus(locNo);
                        break;
                    case InfoType.taskCmd:
                        ShowTaskCmd();
                        ShowExecLog(msg, locNo);
                        break;
                }
            });
        }

        private void ShowExecLog(string msg, string locNo)
        {
            if (txtLocRecord.Text.Length > 10000)
            {
                this.txtLocRecord.Clear();
            }
            //如果日志不归属于任何站台，直接输出即可
            if (string.IsNullOrEmpty(locNo))
            {
                this.txtLocRecord.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  " + msg + Environment.NewLine);
                return;
            }
            if (msg.Equals(BizHandle.Instance.locDic[locNo].LastExecLog))
            {
                return;
            }
            BizHandle.Instance.locDic[locNo].ExecLog = msg;
            //如果未指定哪个站台,直接输出日志
            if (string.IsNullOrEmpty(LimitLocNo))
            {
                this.txtLocRecord.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  " + msg + Environment.NewLine);
                return;
            }
            //如果指定站台，则只显示指定站台日志
            if (locNo.Equals(LimitLocNo))
            {
                this.txtLocRecord.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  " + msg + Environment.NewLine);
            }
        }

        private void ShowPlcConnStatus(string msg)
        {
            if (msg.Equals("Y"))
            {
                this.recPlcConnStatus.Fill = CustomSolidBrush.Green;
            }
            else
            {
                this.recPlcConnStatus.Fill = CustomSolidBrush.Red;
            }
        }

        private void ShowDbConnStatus(string msg)
        {
            if (msg.Equals("Y"))
            {
                this.recDbConnStatus.Fill = CustomSolidBrush.Green;
            }
            else
            {
                this.recDbConnStatus.Fill = CustomSolidBrush.Red;
            }
        }

        /// <summary>
        /// 站台状态刷新
        /// </summary>
        private void ShowLocStatus(string locNo)
        {
            var loc = BizHandle.Instance.locDic[locNo];
            locControlDic[locNo].LocType = loc.LocTypeDesc;
            locControlDic[locNo].TaskNo = loc.plcStatus.TaskNo.ToString();
            locControlDic[locNo].PalletNo = loc.plcStatus.PalletNo;
            locControlDic[locNo].SlocNo = loc.plcStatus.Sloc;
            locControlDic[locNo].ElocNo = loc.plcStatus.Eloc;
            locControlDic[locNo].SetAuto(loc.plcStatus.StatusAuto);
            locControlDic[locNo].SetFault(loc.plcStatus.StatusFault);
            locControlDic[locNo].SetLoading(loc.plcStatus.StatusLoading);
            locControlDic[locNo].SetRequest(loc.plcStatus.StatusRequest);
            locControlDic[locNo].SetFree(loc.plcStatus.StatusFree);
            locControlDic[locNo].SetToLoad(loc.plcStatus.StatusToLoad);
        }

        /// <summary>
        /// 指令信息刷新
        /// </summary>
        private void ShowTaskCmd()
        {
            var errMsg = string.Empty;
            if (string.IsNullOrEmpty(LimitLocNo))
            {
                var taskList = new List<TaskCmd>();
                foreach (var loc in BizHandle.Instance.locDic.Values)
                {
                    loc.TaskList = DbAction.Instance.LoadTaskCmd(loc.LocNo, ref errMsg);
                    if (loc.TaskList != null)
                    {
                        taskList.AddRange(loc.TaskList);
                    }
                }
                this.dgv.ItemsSource = taskList;
            }
            else
            {
                var loc = BizHandle.Instance.locDic[LimitLocNo];
                loc.TaskList = DbAction.Instance.LoadTaskCmd(LimitLocNo, ref errMsg);
                this.dgv.ItemsSource = loc.TaskList;
            }
        }

        #region 功能按键
        /// <summary>
        /// 任务重发
        /// </summary>
        private void btnRefSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgv.SelectedItem == null)
                {
                    MessageBox.Show($"请选择数据行", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                var selectItem = (TaskCmd)dgv.SelectedItem;
                var cmdId = selectItem.ObjId;
                var locNo = selectItem.SlocNo;
                if (cmdId == 0)
                {
                    MessageBox.Show($"未找到{cmdId}指令信息", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"确定要重发指令[{cmdId}]吗？", "重发", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    var result = DbAction.Instance.UpdateCmdStep(cmdId, "00");
                    if (result)
                    {
                        BizHandle.Instance.locDic[locNo].bizStatus = BizStatus.None;
                        ShowTaskCmd();
                        MessageBox.Show($"已重发指令[{cmdId}]", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"重发指令[{cmdId}]失败", "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("重发失败：" + ex.ToString(), "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 强制完成
        /// </summary>
        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgv.SelectedItem == null)
                {
                    MessageBox.Show($"请选择数据行", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                var selectItem = (TaskCmd)dgv.SelectedItem;
                var taskNo = selectItem.TaskNo;
                var locNo = selectItem.SlocNo;
                var ElocNo = selectItem.ElocNo;
                if (taskNo == 0)
                {
                    MessageBox.Show($"未找到{taskNo}任务信息", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"确定要完成任务[{taskNo}]吗？", "强制完成", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    var errMsg = string.Empty;
                    var requestId = DbAction.Instance.GetObjidForCmdFinish();
                    var result = DbAction.Instance.RequestFinishTaskCmd(requestId, taskNo, ElocNo, 2, ref errMsg);
                    if (result)
                    {
                        BizHandle.Instance.locDic[locNo].RequestFinishObjid = 0;
                        BizHandle.Instance.locDic[locNo].bizStatus = BizStatus.None;
                        ShowTaskCmd();
                        MessageBox.Show($"已强制完成任务[{taskNo}]", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"强制完成任务[{taskNo}]失败：{errMsg}", "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("强制完成失败：" + ex.ToString(), "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 强制删除
        /// </summary>
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgv.SelectedItem == null)
                {
                    MessageBox.Show($"请选择数据行", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                var selectItem = (TaskCmd)dgv.SelectedItem;
                var cmdId = selectItem.ObjId;
                var locNo = selectItem.SlocNo;
                var taskNo = selectItem.TaskNo;
                if (cmdId == 0)
                {
                    MessageBox.Show($"未找到{cmdId}指令信息", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"确定要删除指令[{cmdId}]吗？", "删除", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    var errMsg = string.Empty;
                    var result = DbAction.Instance.DeleteTaskCmd(taskNo, ref errMsg);
                    if (result)
                    {
                        ShowTaskCmd();
                        MessageBox.Show($"已删除指令[{cmdId}]", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"强制删除指令[{cmdId}]失败：{errMsg}", "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("强制删除失败：" + ex.ToString(), "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 强制删除
        /// </summary>
        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            this.gbExecLog.Header = "运行日志";
            this.gbTaskCmd.Header = "指令信息";
            this.LimitLocNo = string.Empty;
            ShowTaskCmd();
        }
        #endregion
    }
}
