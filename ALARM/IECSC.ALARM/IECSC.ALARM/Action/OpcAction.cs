using System;
using System.Linq;
using MSTL.LogAgent;
using MSTL.OpcClient;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace IECSC.ALARM
{
    public class OpcAction
    {
        private OpcClient opcClient;
        /// <summary>
        /// OPC服务端 IP
        /// </summary>
        private string serverIp = McConfig.Instance.OpcServerIp;
        /// <summary>
        /// OPC服务端名称
        /// </summary>
        private string serverName = McConfig.Instance.OpcServerName;
        /// <summary>
        /// OPC 组名
        /// </summary>
        private string groupName = McConfig.Instance.OpcGroupName;

        /// <summary>
        /// 日志
        /// </summary>
        private ILog log
        {
            get
            {
                return Log.Store[this.GetType().FullName];
            }
        }

        #region 单例模式
        private static OpcAction _instance = null;
        public static OpcAction Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(OpcAction))
                    {
                        if (_instance == null)
                        {
                            _instance = new OpcAction();
                        }
                    }
                }
                return _instance;
            }
        }
        private OpcAction()
        {
        }
        #endregion

        /// <summary>
        /// 连接OPC
        /// </summary>
        public bool ConnectOpc(ref string errMsg)
        {
            try
            {
                opcClient = new OpcClient();
                var result = opcClient.ConnectOpcServer(serverIp, serverName, ref errMsg);
                if(result)
                {
                    opcClient.DataChanged += OpcClient_DataChanged;
                }
                return result;
            }
            catch(Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 添加组
        /// </summary>
        public bool AddOpcGroup(ref string errMsg)
        {
            try
            {
                return opcClient.AddOpcGroup(groupName, ref errMsg);
                
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 添加项
        /// </summary>
        public bool AddOpcItem(ref string errMsg)
        {
            try
            {
                var items = BizHandle.Instance.alarmInfos.Keys.ToArray();
                if (!opcClient.AddOpcItems(groupName, items, ref errMsg))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// OPCCLIENT数据改变事件处理方法
        /// </summary>
        private void OpcClient_DataChanged(MSTL.OpcClient.Model.DataChangedEventArgs e)
        {
            try
            {
                foreach (var item in e.Data)
                {
                    //判断PLC连接
                    if (item.Quality.Equals(Opc.Da.Quality.Bad))
                    {
                        continue;
                    }
                    if(item.TagLongName == null)
                    {
                        continue;
                    }
                    if(!BizHandle.Instance.alarmInfos.ContainsKey(item.TagLongName))
                    {
                        continue;
                    }
                    BizHandle.Instance.alarmInfos[item.TagLongName].TagValue = Convert.ToInt32(item.TagValue ?? 0);
                }
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行OpcClient_DataChanged(MSTL.OpcClient.Model.DataChangedEventArgs e)读取OPC信息失败:{ex.ToString()}");
            }
        }
        
    }
}
