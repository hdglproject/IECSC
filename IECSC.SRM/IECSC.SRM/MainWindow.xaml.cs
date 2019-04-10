using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows;

namespace IECSC.SRM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 历史信息
        /// </summary>
        private string historyInfo = string.Empty;
        /// <summary>
        /// 定义委托进行跨线程操作控件
        /// </summary>
        private delegate void FlushForm(string msg, InfoType infoType);

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
            var srmList = new List<Srm>();
            srmList.Add(BizHandle.Instance.srm);
            this.dgv.ItemsSource = srmList;
            //业务处理
            Thread thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();
            //PLC连接状态监控
            Thread thConn = new Thread(ConnStatus);
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
                if(DbAction.Instance.GetDbTime())
                {
                    ShowDbConnStatus("Y");
                }
                else
                {
                    ShowDbConnStatus("N");
                    return false;
                }
                if(DbAction.Instance.LoadSrmFault(ref errMsg))
                {
                    ShowExecLog("初始化堆垛机异常描述成功");
                }
                else
                {
                    ShowExecLog($"初始化堆垛机异常描述失败,原因{errMsg}");
                }
                if (DbAction.Instance.LoadOpcItems(ref errMsg))
                {
                    ShowExecLog("初始化堆垛机数据库配置成功");
                }
                else
                {
                    ShowExecLog($"初始化堆垛机数据库配置失败,原因{errMsg}");
                    return false;
                }
                if (DbAction.Instance.LoadSrmCmd(ref errMsg))
                {
                    ShowExecLog("初始化堆垛机指令信息成功");
                }
                return true;
            }
            catch(Exception ex)
            {
                ShowExecLog($"[异常]初始化堆垛机数据库配置失败,原因{ex.Message}");
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
                if (Tools.Instance.PingNetAddress(McConfig.Instance.SrmIp))
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
                    ShowExecLog("初始化OPC连接成功");
                }
                else
                {
                    ShowExecLog($"初始化OPC连接失败,原因{errMsg}");
                    return false;
                }
                if(OpcAction.Instance.AddOpcGroup(ref errMsg))
                {
                    ShowExecLog("初始化OPC组成功");
                }
                else
                {
                    ShowExecLog($"初始化OPC组失败,原因{errMsg}");
                    return false;
                }
                if (OpcAction.Instance.AddOpcItem(ref errMsg))
                {
                    ShowExecLog("初始化OPC项成功");
                }
                else
                {
                    ShowExecLog($"初始化OPC项失败,原因{errMsg}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowExecLog($"[异常]初始化OPC,[原因]{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 执行业务
        /// </summary>
        private void Run()
        {
            while (true)
            {
                if (DbAction.Instance.GetDbTime())
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("Y", InfoType.dbConn));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("N", InfoType.dbConn));
                    continue;
                }
                //记录设备状态
                DbAction.Instance.RecordPlcInfo(BizHandle.Instance.srm);
                //记录设备报警日志
                DbAction.Instance.RecordSrmFaultInfo(BizHandle.Instance.srm);
                //界面渲染
                ShowSrmDetailInfo(BizHandle.Instance.srm);
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
                if(Tools.Instance.PingNetAddress(McConfig.Instance.SrmIp))
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("Y", InfoType.plcConn));

                    if (BizHandle.Instance.srm.plcStatus.OperateMode == -1)
                    {
                        var errMsg = string.Empty;
                        OpcAction.Instance.opcClient = new MSTL.OpcClient.OpcClient();
                        if (!OpcAction.Instance.ConnectOpc(ref errMsg))
                        {
                            ShowExecLog($"重新初始化OPC连接失败,原因{errMsg}");
                            continue;
                        }
                        if (!OpcAction.Instance.AddOpcGroup(ref errMsg))
                        {
                            ShowExecLog($"重新初始化OPC组失败,原因{errMsg}");
                            continue;
                        }
                        if (!OpcAction.Instance.AddOpcItem(ref errMsg))
                        {
                            ShowExecLog($"重新初始化OPC项失败,原因{errMsg}");
                            continue;
                        }
                    }
                }
                else
                {
                    //记录断连时间
                    BizHandle.Instance.srm.plcStatus.OperateMode = -1;
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("N", InfoType.plcConn));
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
            var infoType = appData.InfoType;
            FormShow(msg, infoType);
        }

        private void FormShow(string msg, InfoType infoType)
        {
            this.Dispatcher.Invoke(()=>
            {
                switch(infoType)
                {
                    case InfoType.dbConn:
                        ShowDbConnStatus(msg);
                        break;
                    case InfoType.plcConn:
                        ShowPlcConnStatus(msg);
                        break;
                    case InfoType.logInfo:
                        ShowExecLog(msg);
                        break;
                }
            });
        }
        
        private void ShowExecLog(string msg)
        {
            if (txtSrmRecord.Text.Length > 10000)
            {
                this.txtSrmRecord.Clear();
            }
            if(msg.Equals(historyInfo))
            {
                return;
            }
            historyInfo = msg;
            this.txtSrmRecord.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  " + msg + Environment.NewLine);
        }

        private void ShowPlcConnStatus(string msg)
        {
            if(msg.Equals("Y"))
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
        /// 渲染堆垛机详情信息
        /// </summary>
        private void ShowSrmDetailInfo(Srm srm)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.txtRefTime.Text = DateTime.Now.ToString("HH:mm:ss.fff");

                this.txtDeviceId.Text = srm.plcStatus.DeviceId;
                this.txtHeartBeat.Text = srm.plcStatus.HeartBeat.ToString();
                this.txtOperateMode.Text = srm.plcStatus.OperateMode.ToString();
                switch(srm.plcStatus.OperateMode)
                {
                    case 1:
                        this.txtOperateMode.Background = CustomSolidBrush.LigtGreen;
                        break;
                    default:
                        this.txtOperateMode.Background = CustomSolidBrush.Red;
                        break;
                }
                this.txtMissionState.Text = srm.plcStatus.MissionState.ToString();
                switch (srm.plcStatus.MissionState)
                {
                    case 1:
                        this.txtMissionState.Background = CustomSolidBrush.LigtGreen;
                        break;
                    case 2:
                        this.txtMissionState.Background = CustomSolidBrush.LigtGreen;
                        break;
                    case 3:
                        this.txtMissionState.Background = CustomSolidBrush.Orange;
                        break;
                    default:
                        this.txtMissionState.Background = CustomSolidBrush.WhiteGray;
                        break;
                }
                this.txtMissionType.Text = srm.plcStatus.MissionType.ToString();
                this.txtMissionId.Text = srm.plcStatus.MissionId.ToString();
                this.txtPalletNo.Text = srm.plcStatus.PalletNo;
                this.txtActPosBay.Text = srm.plcStatus.ActPosBay.ToString();
                this.txtActPosLevel.Text = srm.plcStatus.ActPosLevel.ToString();
                this.txtActPosX.Text = srm.plcStatus.ActPosX.ToString();
                this.txtActPosY.Text = srm.plcStatus.ActPosY.ToString();
                this.txtActPosZ.Text = srm.plcStatus.ActPosZ.ToString();
                this.txtActPosZDeep.Text = srm.plcStatus.ActPosZDeep.ToString();
                this.txtActSpeedX.Text = srm.plcStatus.ActSpeedX.ToString();
                this.txtActSpeedY.Text = srm.plcStatus.ActSpeedY.ToString();
                this.txtActSpeedZ.Text = srm.plcStatus.ActSpeedZ.ToString();
                this.txtActSpeedZDeep.Text = srm.plcStatus.ActSpeedZDeep.ToString();
                this.txtLoadStatus.Text = srm.plcStatus.LoadStatus.ToString();
                this.txtFaultNo.Text = srm.plcStatus.FaultNo.ToString();
                switch (srm.plcStatus.FaultNo)
                {
                    case 0:
                        this.txtFaultNo.Background = CustomSolidBrush.LigtGreen;
                        this.txtFaultDesc.Text = "无";
                        break;
                    default:
                        this.txtFaultNo.Background = CustomSolidBrush.Red;
                        this.txtFaultDesc.Text = BizHandle.Instance.srm[srm.plcStatus.FaultNo.ToString()];
                        break;
                }
            });
        }

        /// <summary>
        /// 指令重发
        /// </summary>
        private void btnRefSend_Click(object sender, EventArgs e)
        {
            try
            {
                //var selectItem = (DataRowView)dgv.SelectedItem;
                var srm = BizHandle.Instance.srm;
                if (srm.taskCmd.ObjId == 0 || srm.taskCmd.TaskNo == 0)
                {
                    MessageBox.Show("未找到任务信息", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"确定要重发任务[{srm.taskCmd.TaskNo}]吗？", "重发", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    var result = DbAction.Instance.UpdateCmdStep(srm.taskCmd.ObjId, "00");
                    if (result)
                    {
                        BizHandle.Instance.srm.bizStatus = BizStatus.Reset;
                        MessageBox.Show($"已重发任务[{srm.taskCmd.TaskNo}]", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"重发任务[{srm.taskCmd.TaskNo}]失败", "错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("重发失败：" + ex.ToString(), "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 指令结束
        /// </summary>
        private void btnFinish_Click(object sender, EventArgs e)
        {
            try
            {
                var srm = BizHandle.Instance.srm;
                if (srm.taskCmd.ObjId == 0 || srm.taskCmd.TaskNo == 0)
                {
                    MessageBox.Show("未找到任务信息", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"确定要完成任务[{srm.taskCmd.TaskNo}]吗？", "强制完成", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    var errMsg = string.Empty;
                    var requestId = DbAction.Instance.GetObjidForCmdFinish();
                    var result = DbAction.Instance.RequestFinishTaskCmd(requestId, srm.taskCmd.ObjId, srm.taskCmd.ElocNo, 2, ref errMsg);
                    if (result)
                    {
                        BizHandle.Instance.srm.bizStatus = BizStatus.Reset;
                        MessageBox.Show($"已强制完成任务[{srm.taskCmd.TaskNo}]", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"强制完成任务[{srm.taskCmd.TaskNo}]失败：{errMsg}", "错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("强制完成失败：" + ex.ToString(), "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// 指令删除
        /// </summary>
        private void btnDel_Click(object sender, EventArgs e)
        {
            try
            {
                var srm = BizHandle.Instance.srm;
                if (srm.taskCmd.ObjId == 0 || srm.taskCmd.TaskNo == 0)
                {
                    MessageBox.Show("未找到任务信息", "提示", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    return;
                }
                if (MessageBox.Show($"确定要删除任务[{srm.taskCmd.TaskNo}]吗？", "删除", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    var errMsg = string.Empty;
                    var result = DbAction.Instance.DeleteTaskCmd(srm.taskCmd.TaskNo);
                    if (result)
                    {
                        BizHandle.Instance.srm.bizStatus = BizStatus.Reset;
                        MessageBox.Show($"已删除完成任务[{srm.taskCmd.TaskNo}]", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"强制删除任务[{srm.taskCmd.TaskNo}]失败：{errMsg}", "错误", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("强制删除失败：" + ex.ToString(), "异常", MessageBoxButton.YesNo, MessageBoxImage.Error);
            }
        }
    }
}
