using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IECSC.TRANS.CustomControl
{
    /// <summary>
    /// LocControl.xaml 的交互逻辑
    /// </summary>
    public partial class LocControl : UserControl
    {
        //控件单击事件
        public event Action<string> Click;

        public LocControl()
        {
            InitializeComponent();

            this.btnLocPlcNo.Content = string.Empty;
            this.tbTaskNo.Text = string.Empty;
            this.tbPalletNo.Text = string.Empty;
            this.tbSlocNo.Text = string.Empty;
            this.tbElocNo.Text = string.Empty;
            this.gbTitle.Header = string.Empty;
        }

        /// <summary>
        /// 站台类型
        /// </summary>
        public string LocType
        {
            get
            {
                return this.gbTitle.Header.ToString();
            }
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.gbTitle.Header = value;
                });
            }
        }

        /// <summary>
        /// 站台号
        /// </summary>
        public string LocNo { get; set; }

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
        /// 任务编号
        /// </summary>
        public string TaskNo
        {
            get
            {
                return this.tbTaskNo.Text;
            }
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.tbTaskNo.Text = value;
                });
            }
        }

        /// <summary>
        /// 工装编号
        /// </summary>
        public string PalletNo
        {
            get
            {
                return this.tbPalletNo.Text;
            }
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.tbPalletNo.Text = value;
                });
            }
        }

        /// <summary>
        /// 起始站台
        /// </summary>
        public string SlocNo
        {
            get
            {
                return this.tbSlocNo.Text;
            }
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.tbSlocNo.Text = value;
                });
            }
        }

        /// <summary>
        /// 设置自动状态标示
        /// </summary>
        public void SetAuto(int value)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (value == 1)
                {
                    this.epAuto.Fill = new SolidColorBrush(Colors.Lime);
                }
                else
                {
                    this.epAuto.Fill = new SolidColorBrush(Colors.Red);
                }
            });
        }

        /// <summary>
        /// 设置故障状态标示
        /// </summary>
        public void SetFault(int value)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (value == 1)
                {
                    this.epFault.Fill = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    this.epFault.Fill = new SolidColorBrush(Colors.Lime);
                }
            });
        }

        /// <summary>
        /// 设置有载状态标示
        /// </summary>
        public void SetLoading(int value)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (value == 1)
                {
                    this.epLoading.Fill = new SolidColorBrush(Colors.Lime);
                }
                else
                {
                    this.epLoading.Fill = new SolidColorBrush(Colors.White);
                }
            });
        }

        /// <summary>
        /// 设置请求任务状态标示
        /// </summary>
        public void SetRequest(int value)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (value == 1)
                {
                    this.epRequest.Fill = new SolidColorBrush(Colors.Lime);
                }
                else
                {
                    this.epRequest.Fill = new SolidColorBrush(Colors.White);
                }
            });
        }

        /// <summary>
        /// 设置空闲状态标示
        /// </summary>
        public void SetFree(int value)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (value == 1)
                {
                    this.epFree.Fill = new SolidColorBrush(Colors.Lime);
                }
                else
                {
                    this.epFree.Fill = new SolidColorBrush(Colors.White);
                }
            });
        }

        /// <summary>
        /// 设置取货状态标识
        /// </summary>
        public void SetToLoad(int value)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (value == 1)
                {
                    this.epToLoad.Fill = new SolidColorBrush(Colors.Lime);
                }
                else
                {
                    this.epToLoad.Fill = new SolidColorBrush(Colors.White);
                }
            });
        }

        /// <summary>
        /// 目的站台
        /// </summary>
        public string ElocNo
        {
            get
            {
                return this.tbElocNo.Text;
            }
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.tbElocNo.Text = value;
                });
            }
        }

        /// <summary>
        /// 单击事件
        /// </summary>
        private void btnLocPlcNo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Click?.Invoke(this.LocNo);
        }
    }
}
