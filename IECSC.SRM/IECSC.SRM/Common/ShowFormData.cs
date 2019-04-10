/**************************************************************
 * 功能描述：此类用于将相关信息显示到界面上
 * 开发时间：2017-22-16
 * 版本编号：v1.0000
 *************************************************************/
using System;

namespace IECSC.SRM
{
    public class ShowFormData
    {
        public delegate void AppDataEventHandler(object sender, AppDataEventArgs e);

        public event AppDataEventHandler OnAppDtoData;

        #region 单例实现
        private static ShowFormData _instance;
        public static ShowFormData Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(ShowFormData))
                    {
                        if (_instance == null)
                        {
                            _instance = new ShowFormData();
                        }
                    }
                }
                return _instance;
            }
        }
        private ShowFormData()
        {
        }
        #endregion

        public void ShowFormInfo(ShowInfoData data)
        {
            if (OnAppDtoData == null)
            {
                return;
            }
            var eventArgs = new AppDataEventArgs();
            eventArgs.AppData = data;
            OnAppDtoData(this, eventArgs);
        }

        /// <summary>
        /// 清理数据
        /// </summary>
        public void Reset()
        {
            _instance = null;
        }
    }

    public class AppDataEventArgs : EventArgs
    {
        public ShowInfoData AppData { get; set; }
    }
    /// <summary>
    /// 要显示在界面上的信息
    /// </summary>
    public class ShowInfoData
    {
        public ShowInfoData(string stringInfo, InfoType infoType = InfoType.logInfo)
        {
            StringInfo = stringInfo;
            InfoType = infoType;
        }

        /// <summary>
        /// 显示在界面的文字信息
        /// </summary>
        public string StringInfo { get; set; }
        /// <summary>
        /// 异常标记
        /// </summary>
        public int InfoSign { get; set; }

        /// <summary>
        /// 显示信息类型
        /// </summary>
        public InfoType InfoType { get; set; }
    }

    /// <summary>
    /// 显示信息枚举类
    /// </summary>
    public enum InfoType
    {
        /// <summary>
        /// 日志信息
        /// </summary>
        logInfo = 1,
        /// <summary>
        /// Plc连接
        /// </summary>
        plcConn = 2,
        /// <summary>
        /// 数据库连接
        /// </summary>
        dbConn = 3
    }
}
