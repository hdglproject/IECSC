/**************************************************************
 * 功能描述：此类用于将相关信息显示到界面上
 * 开发时间：2017-22-16
 * 版本编号：v1.0000
 *************************************************************/
using System;
using System.Collections.Generic;

namespace IECSC.TRANS
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
        public ShowInfoData(string stringInfo, string locNo, InfoType infoType = InfoType.logInfo)
        {
            StringInfo = stringInfo;
            InfoType = infoType;
            LocNo = locNo;
        }

        /// <summary>
        /// 显示在界面的文字信息
        /// </summary>
        public string StringInfo { get; set; }
        /// <summary>
        /// 站台编号
        /// </summary>
        public string LocNo { get; set; }
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
        logInfo = 0,
        /// <summary>
        /// Plc连接
        /// </summary>
        plcConn = 1,
        /// <summary>
        /// 数据库连接
        /// </summary>
        dbConn = 2,
        /// <summary>
        /// 设备状态
        /// </summary>
        locStatus = 3,
        /// <summary>
        /// 指令信息
        /// </summary>
        taskCmd = 4
    }
}
