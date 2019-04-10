using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSC.ALARM
{
    public class BizHandle
    {
        /// <summary>
        /// 报警信息
        /// </summary>
        public Dictionary<string, AlarmInfo> alarmInfos = null;

        #region 单例模式
        private static BizHandle _instance = null;
        public static BizHandle Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(BizHandle))
                    {
                        if (_instance == null)
                        {
                            _instance = new BizHandle();
                        }
                    }
                }
                return _instance;
            }
        }
        public BizHandle()
        {
            alarmInfos = new Dictionary<string, AlarmInfo>();
        }
        #endregion

        /// <summary>
        /// 初始化配置项信息
        /// </summary>
        public bool InitDb()
        {
            try
            {
                var dt = DbAction.Instance.GetAlarmOpcItems();
                if (dt == null || dt.Rows.Count == 0)
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("未获取到配置项信息"));
                    return false;
                }
                foreach (DataRow row in dt.Rows)
                {
                    var alarmInfo = new AlarmInfo();
                    alarmInfo.LocPlcNo = row["loc_plc_no"].ToString();
                    alarmInfo.TagName = row["tagname"].ToString();
                    alarmInfo.TagIndex = row["tagindex"].ToString();
                    alarmInfo.Discrip = row["discrip"].ToString();
                    alarmInfo.TagLongName = row["taglongname"].ToString();
                    alarmInfos.Add(alarmInfo.TagLongName, alarmInfo);
                }
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData("初始化数据库配置成功"));
                return true;
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[异常]初始化数据库配置,[原因]{ex.Message}"));
                return false;
            }
        }

        public bool InitOpc()
        {
            try
            {
                var errMsg = string.Empty;
                if (Tools.Instance.PingNetAddress(McConfig.Instance.LocIp))
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("Y", InfoType.plcConn));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("N", InfoType.plcConn));
                    return false;
                }
                if (OpcAction.Instance.ConnectOpc(ref errMsg))
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("初始化OPC连接成功"));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"初始化OPC连接失败,原因{errMsg}"));
                    return false;
                }
                if (OpcAction.Instance.AddOpcGroup(ref errMsg))
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("初始化OPC组成功"));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"初始化OPC组失败,原因{errMsg}"));
                    return false;
                }
                if (OpcAction.Instance.AddOpcItem(ref errMsg))
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("初始化OPC项成功"));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"初始化OPC项失败,原因{errMsg}"));
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[异常]初始化OPC,[原因]{ex.Message}"));
                return false;
            }
        }

        public void BizListen()
        {
            foreach(AlarmInfo alarmInfo in alarmInfos.Values)
            {
                if (alarmInfo.AlarmMark == alarmInfo.TagValue)
                {
                    continue;
                }
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData(alarmInfo.LocPlcNo, InfoType.locStatus));
                var errMsg = string.Empty;
                if (alarmInfo.TagValue == 1)
                {
                    alarmInfo.Objid = DbAction.Instance.GetObjid(ref errMsg);
                    if (alarmInfo.Objid <= 0)
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[运行]获取OBJID出错：{errMsg}"));
                        continue;
                    }
                    var result = DbAction.Instance.SaveAlarmData(alarmInfo, ref errMsg);
                    if (result)
                    {
                        alarmInfo.AlarmMark = alarmInfo.TagValue;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[报警]{alarmInfo.LocPlcNo}：{alarmInfo.Discrip}"));
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[运行]记录报警{alarmInfo.LocPlcNo}：{alarmInfo.Discrip}失败：{errMsg}"));
                        continue;
                    }
                }
                else
                {
                    if (alarmInfo.Objid <= 0)
                    {
                        alarmInfo.AlarmMark = alarmInfo.TagValue;
                        continue;
                    }
                    var result = DbAction.Instance.UpdateAlarmData(alarmInfo, ref errMsg);
                    if (result)
                    {
                        alarmInfo.AlarmMark = alarmInfo.TagValue;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[报警]{alarmInfo.LocPlcNo}：{alarmInfo.Discrip} 已处理"));
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[运行]更新报警{alarmInfo.LocPlcNo}：{alarmInfo.Discrip}：{alarmInfo.Objid}已处理失败：{errMsg}"));
                        continue;
                    }
                }
            }
        }
    }
}
