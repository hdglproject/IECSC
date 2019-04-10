using System;
using System.Collections.Generic;
using System.Data;
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

namespace IECSC.ALARM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 定义委托进行跨线程操作控件
        /// </summary>
        private delegate void FlushForm(string msg, InfoType infoType);
        /// <summary>
        /// 报警工位列表
        /// </summary>
        private Dictionary<string, AlarmControl> alarmControlDic = new Dictionary<string, AlarmControl>();

        public MainWindow()
        {
            InitializeComponent();
            //页面显示
            ShowFormData.Instance.OnAppDtoData += ShowInfo;
            //登陆时间
            this.lbTime.Content = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //初始化
            var errMsg = string.Empty;
            if (!BizHandle.Instance.InitDb())
            {
                return;
            }
            if (!BizHandle.Instance.InitOpc())
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
        /// 初始化站台信息
        /// </summary>
        private void InitLocControl()
        {
            foreach (var alarm in BizHandle.Instance.alarmInfos.Values)
            {
                if(alarmControlDic.Keys.Contains(alarm.LocPlcNo))
                {
                    continue;
                }
                var alarmControl = new AlarmControl();
                alarmControl.LocPlcNo = alarm.LocPlcNo;
                alarmControl.Width = 200;
                alarmControl.Height = 200;
                alarmControl.Margin = new Thickness(2);
                alarmControl.Click += AlarmControl_Click;
                this.alarmControlDic.Add(alarm.LocPlcNo, alarmControl);
                this.GridLocList.Children.Add(alarmControl);
            }
        }

        /// <summary>
        /// 单机站台状态控件事件
        /// </summary>
        private void AlarmControl_Click(string locNo)
        {
            
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
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("Y",InfoType.dbConn));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("N", InfoType.dbConn));
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
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("Y", InfoType.plcConn));
                }
                else
                {
                    //记录断连时间
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
                        ShowExecLog(msg);
                        break;
                    case InfoType.locStatus:
                        ShowLocStatus(msg);
                        break;
                }
            });
        }
        private void ShowExecLog(string msg)
        {
            if (txtLocRecord.Text.Length > 10000)
            {
                this.txtLocRecord.Clear();
            }
            this.txtLocRecord.AppendText(DateTime.Now.ToString("yyy-mm-dd HH:mm:ss.fff") + "  " + msg + Environment.NewLine);
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
            var alarmInfo = BizHandle.Instance.alarmInfos.Values.Where(p => p.LocPlcNo == locNo && p.TagValue == 1).Select(p => p.Discrip).ToArray();
            alarmControlDic[locNo].SetAlarmInfo(alarmInfo);
        }
    }
}
