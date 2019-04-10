using System;
using System.Configuration;
using MSTL.LogAgent;

namespace IECSC.TRANS
{
    public class McConfig
    {
        private ILog log
        {
            get
            {
                return Log.Store[this.GetType().FullName];
            }
        }

        private static McConfig _instance = null;
        public static McConfig Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                lock (typeof(McConfig))
                {
                    if (_instance == null)
                    {
                        _instance = new McConfig();
                    }
                    return _instance;
                }
            }
        }
        private McConfig()
        {
            try
            {
                OpcServerName = getConfig("OpcServerName");
                OpcGroupName = getConfig("OpcGroupName");
                OpcServerIp = getConfig("OpcServerIP");
                LocArea = getConfig("LocArea");
                LocIp = getConfig("LocIp");
                DbIp = getConfig("DbIp");
                DbConnect = getConfig("DbConnect");
            }
            catch (Exception ex)
            {
                log.Error($"获取配置文件时出错{ex.ToString()}");
            }
        }
        /// <summary>
        /// OPC名称
        /// </summary>
        public string OpcServerName { get; set; }
        /// <summary>
        /// OPC组名称
        /// </summary>
        public string OpcGroupName { get; set; }
        /// <summary>
        /// OPC服务器地址
        /// </summary>
        public string OpcServerIp { get; set; }
        /// <summary>
        /// 线体区域
        /// </summary>
        public string LocArea { get; set; }
        /// <summary>
        /// 站台区域IP
        /// </summary>
        public string LocIp { get; set; }
        /// <summary>
        /// 数据库IP
        /// </summary>
        public string DbIp { get; set; }
        /// <summary>
        /// 数据库连接
        /// </summary>
        public string DbConnect { get; set; }
        /// <summary>
        /// 读配置文档
        /// </summary>
        public string getConfig(string key)
        {
            var values = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrWhiteSpace(values))
            {
                return values;
            }
            else
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// 写配置文档
        /// </summary>
        public void setConfig(string key, int value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                {
                    config.AppSettings.Settings[key].Value = value.ToString();
                }
                else
                {
                    config.AppSettings.Settings.Add(key, value.ToString());
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                log.Error($"写入Key[{key}]Value[{value}]时异常：{ex.ToString()}");
            }
        }
    }
}
