using System;
using System.Configuration;
using MSTL.LogAgent;

namespace CreateAlarmFIle
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
                DbConnect = getConfig("DbConnect");
            }
            catch (Exception ex)
            {
                log.Error($"获取配置文件时出错{ex.ToString()}");
            }
        }

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
    }
}
