using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// AlarmControl.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmControl : UserControl
    {
        //控件单击事件
        public event Action<string> Click;

        public AlarmControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 站台编号
        /// </summary>
        public string LocPlcNo
        {
            get
            {
                return this.btnLocPlcNo.Content.ToString();
            }
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.btnLocPlcNo.Content = value;
                });
            }
        }

        /// <summary>
        /// 设置报警内容
        /// </summary>
        public void SetAlarmInfo(string[] alarmInfo)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.spAlarm.Children.Clear();
                if(alarmInfo.Length > 0)
                {
                    this.btnLocPlcNo.Background = CustomSolidBrush.Red;
                }
                else
                {
                    this.btnLocPlcNo.Background = CustomSolidBrush.LightBlue;
                }
                for (int i = 0; i < alarmInfo.Length; i++)
                {
                    var lb = new Label();
                    lb.Content = alarmInfo[i];
                    this.spAlarm.Children.Add(lb);
                }
            });
        }

        /// <summary>
        /// 单击事件
        /// </summary>
        private void btnLocPlcNo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Click?.Invoke(this.LocPlcNo);
        }
    }
}
