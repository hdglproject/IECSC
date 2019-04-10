using System;
using System.Linq;
using MSTL.LogAgent;
using MSTL.OpcClient;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace IECSC.TRANS
{
    public class OpcAction
    {
        public OpcClient opcClient;
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
            opcClient = new OpcClient();
        }
        #endregion

        /// <summary>
        /// 连接OPC
        /// </summary>
        public bool ConnectOpc(ref string errMsg)
        {
            try
            {
                var serverIp = McConfig.Instance.OpcServerIp;
                var serverName = McConfig.Instance.OpcServerName;
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
                var groupName = McConfig.Instance.OpcGroupName;
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
                var groupName = McConfig.Instance.OpcGroupName;
                var readItems = BizHandle.Instance.readItems.Keys.ToArray();
                var writeItems = BizHandle.Instance.writeItems.Keys.ToArray();
                if (!opcClient.AddOpcItems(groupName, readItems, ref errMsg))
                {
                    return false;
                }
                if (!opcClient.AddOpcItems(groupName, writeItems, ref errMsg))
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
        /// <param name="e"></param>
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
                    //检查是否为站台的读取项
                    if(!BizHandle.Instance.readItems.Keys.Contains(item.TagLongName))
                    {
                        continue;
                    }
                    var items = BizHandle.Instance.readItems[item.TagLongName];
                    //获取站台号
                    var locNo = BizHandle.Instance.readItems[item.TagLongName].LocNo;
                    #region 绑定读取值
                    switch (items.BusIdentity)
                    {
                        case "Read.TaskNo":
                            BizHandle.Instance.locDic[locNo].plcStatus.TaskNo = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.PalletNo":
                            BizHandle.Instance.locDic[locNo].plcStatus.PalletNo = (item.TagValue ?? 0).ToString().Trim();
                            break;
                        case "Read.SlocArea":
                            BizHandle.Instance.locDic[locNo].plcStatus.SlocArea = (item.TagValue ?? 0).ToString().Trim();
                            break;
                        case "Read.SlocNo":
                            BizHandle.Instance.locDic[locNo].plcStatus.SlocNo = (item.TagValue ?? 0).ToString().Trim();
                            break;
                        case "Read.ElocArea":
                            BizHandle.Instance.locDic[locNo].plcStatus.ElocArea = (item.TagValue ?? 0).ToString().Trim();
                            break;
                        case "Read.ElocNo":
                            BizHandle.Instance.locDic[locNo].plcStatus.ElocNo = (item.TagValue ?? 0).ToString().Trim();
                            break;
                        case "Read.StatusAuto":
                            BizHandle.Instance.locDic[locNo].plcStatus.StatusAuto = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.StatusFault":
                            BizHandle.Instance.locDic[locNo].plcStatus.StatusFault = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.StatusLoading":
                            BizHandle.Instance.locDic[locNo].plcStatus.StatusLoading = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.StatusRequest":
                            BizHandle.Instance.locDic[locNo].plcStatus.StatusRequest = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.StatusFree":
                            BizHandle.Instance.locDic[locNo].plcStatus.StatusFree = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.StatusToLoad":
                            BizHandle.Instance.locDic[locNo].plcStatus.StatusToLoad = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行OpcClient_DataChanged(MSTL.OpcClient.Model.DataChangedEventArgs e)读取OPC信息失败:{ex.ToString()}");
            }
        }

        /// <summary>
        /// 写入顺序控制字
        /// </summary>
        public bool WriteTaskDeal(Loc loc, ref string errMsg)
        {
            try
            {
                foreach (var item in BizHandle.Instance.writeItems)
                {
                    if (item.Value.LocNo != loc.LocNo)
                    {
                        continue;
                    }
                    if(item.Value.BusIdentity.Equals("Write.TaskDeal"))
                    {
                        var kValue = new KeyValuePair<string, object>(item.Key, 1);
                        if (opcClient.WriteValue(McConfig.Instance.OpcGroupName, kValue, ref errMsg))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                errMsg = "未找到OPC“任务已处理标记”写入项";
                return false;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 写入指令
        /// </summary>
        public bool WriteTaskCmd(Loc loc, ref string errMsg)
        {
            try
            {
                if(loc.taskCmd.TaskNo == 0)
                {
                    errMsg = $"{loc.LocPlcNo}未找到可下发PLC的指令";
                    return false;
                }
                var keyValues = new List<KeyValuePair<string, object>>();
                foreach (var item in BizHandle.Instance.writeItems)
                {
                    if (item.Value.LocNo != loc.LocNo)
                    {
                        continue;
                    }
                    switch (item.Value.BusIdentity)
                    {
                        case "Write.TaskNo":
                            keyValues.Add(new KeyValuePair<string, object>(item.Key, loc.taskCmd.TaskNo));
                            break;
                        case "Write.PalletNo":
                            keyValues.Add(new KeyValuePair<string, object>(item.Key, loc.taskCmd.PalletNo ?? string.Empty));
                            break;
                        case "Write.SlocArea":
                            keyValues.Add(new KeyValuePair<string, object>(item.Key, loc.taskCmd.SlocArea));
                            break;
                        case "Write.SlocNo":
                            keyValues.Add(new KeyValuePair<string, object>(item.Key, loc.taskCmd.SlocCode));
                            break;
                        case "Write.ElocArea":
                            keyValues.Add(new KeyValuePair<string, object>(item.Key, loc.taskCmd.ElocArea));
                            break;
                        case "Write.ElocNo":
                            keyValues.Add(new KeyValuePair<string, object>(item.Key, loc.taskCmd.ElocCode));
                            break;
                    }
                }
                if (keyValues.Count > 0)
                {
                    if(opcClient.WriteValues(McConfig.Instance.OpcGroupName, keyValues.ToArray(), ref errMsg))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    errMsg = "未找到OPC任务信息写入项";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }
    }
}
