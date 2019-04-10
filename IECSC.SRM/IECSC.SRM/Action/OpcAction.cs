using System;
using System.Linq;
using MSTL.LogAgent;
using MSTL.OpcClient;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace IECSC.SRM
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
                var readItems = BizHandle.Instance.readItems.Select(p => p.TagLongName).ToArray();
                var writeItems = BizHandle.Instance.writeItems.Select(p => p.TagLongName).ToArray();
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
                    //检查是否为堆垛机的读取项
                    var items = BizHandle.Instance.readItems.FirstOrDefault(p => p.TagLongName == item.TagLongName);
                    if (items == null)
                    {
                        continue;
                    }

                    #region 绑定读取值
                    switch (items.BusIdentity)
                    {
                        case "Read.DeviceId":
                            BizHandle.Instance.srm.plcStatus.DeviceId = (item.TagValue ?? 0).ToString();
                            break;
                        case "Read.HeartBeat":
                            BizHandle.Instance.srm.plcStatus.HeartBeat = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.OperateMode":
                            BizHandle.Instance.srm.plcStatus.OperateMode = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.MissionState":
                            BizHandle.Instance.srm.plcStatus.MissionState = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.MissionType":
                            BizHandle.Instance.srm.plcStatus.MissionType = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.MissionId":
                            BizHandle.Instance.srm.plcStatus.MissionId = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.PalletId":
                            BizHandle.Instance.srm.plcStatus.PalletNo = (item.TagValue ?? 0).ToString();
                            break;
                        case "Read.ActPosBay":
                            BizHandle.Instance.srm.plcStatus.ActPosBay = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActPosLevel":
                            BizHandle.Instance.srm.plcStatus.ActPosLevel = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActPosX":
                            BizHandle.Instance.srm.plcStatus.ActPosX = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActPosY":
                            BizHandle.Instance.srm.plcStatus.ActPosY = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActPosZ":
                            BizHandle.Instance.srm.plcStatus.ActPosZ = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActPosZDeep":
                            BizHandle.Instance.srm.plcStatus.ActPosZDeep = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActSpeedX":
                            BizHandle.Instance.srm.plcStatus.ActSpeedX = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActSpeedY":
                            BizHandle.Instance.srm.plcStatus.ActSpeedY = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActSpeedZ":
                            BizHandle.Instance.srm.plcStatus.ActSpeedZ = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.ActSpeedZDeep":
                            BizHandle.Instance.srm.plcStatus.ActSpeedZDeep = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.LoadStatus":
                            BizHandle.Instance.srm.plcStatus.LoadStatus = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.FaultNo":
                            BizHandle.Instance.srm.plcStatus.FaultNo = Convert.ToInt32(item.TagValue ?? 0);
                            break;
                        case "Read.NoFunction":
                            BizHandle.Instance.srm.plcStatus.NoFunction = Convert.ToInt32(item.TagValue ?? 0);
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
        /// 写入心跳信号
        /// </summary>
        public bool WriteHeartBeat(ref string errMsg)
        {
            try
            {
                var opcItem = BizHandle.Instance.writeItems.FirstOrDefault(p => p.BusIdentity.Equals("Write.HeartBeat"));
                if (opcItem != null)
                {
                    var kValue = new KeyValuePair<string, object>(opcItem.TagLongName, 1);
                    if (opcClient.WriteValue(McConfig.Instance.OpcGroupName, kValue, ref errMsg))
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
                    errMsg = "未找到OPC配置项";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }
        /// <summary>
        /// 写入顺序控制字
        /// </summary>
        public bool WriteSequenceNo(int sequenceNo, ref string errMsg)
        {
            try
            {
                var opcItem = BizHandle.Instance.writeItems.FirstOrDefault(p => p.BusIdentity.Equals("Write.SequenceNo"));
                if (opcItem != null)
                {
                    var kValue = new KeyValuePair<string, object>(opcItem.TagLongName, sequenceNo);
                    if (opcClient.WriteValue(McConfig.Instance.OpcGroupName, kValue, ref errMsg))
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
                    errMsg = "未找到OPC配置项";
                    return false;
                }
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
        public bool WriteTaskCmd(Srm srm, ref string errMsg)
        {
            try
            {
                var keyValues = new List<KeyValuePair<string, object>>();
                var tagLongName = string.Empty;
                foreach (var item in BizHandle.Instance.writeItems)
                {
                    #region 写入指令信息
                    switch(item.BusIdentity)
                    {
                        case "Write.HeartBeat":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, 1));
                            break;
                        case "Write.MissionCount":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, 1));
                            break;
                        case "Write.DeviceId":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.SrmName));
                            break;
                        case "Write.MissionType":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.MissionType));
                            break;
                        case "Write.MissionId":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.TaskNo));
                            break;
                        case "Write.PalletId":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.PalletNo));
                            break;
                        case "Write.EpArea":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.EpArea));
                            break;
                        case "Write.EpNo":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.EpNo));
                            break;
                        case "Write.FromRow":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.FromRow));
                            break;
                        case "Write.FromBay":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.FromBay));
                            break;
                        case "Write.FromLevel":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.FromLevel));
                            break;
                        case "Write.ApArea":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.ApArea));
                            break;
                        case "Write.ApNo":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.ApNo));
                            break;
                        case "Write.ToRow":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.ToRow));
                            break;
                        case "Write.ToBay":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.ToBay));
                            break;
                        case "Write.ToLevel":
                            keyValues.Add(new KeyValuePair<string, object>(item.TagLongName, srm.taskCmd.ToLevel));
                            break;
                        case "Write.SequenceNo":
                            tagLongName = item.TagLongName;
                            break;
                    }
                    #endregion 
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
