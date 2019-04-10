using Dapper;
using DapperExtensions;
using MSTL.DbClient;
using MSTL.LogAgent;
using System;
using System.Data;
using System.Text;

namespace IECSC.ALARM
{
    public partial class DbAction
    {
        /// <summary>
        /// 数据库操作类
        /// </summary>
        private IDatabase Db = null;
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
        private static DbAction _instance = null;
        public static DbAction Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(DbAction))
                    {
                        if (_instance == null)
                        {
                            _instance = new DbAction();
                        }
                    }
                }
                return _instance;
            }
        }
        public DbAction()
        {
            var errMsg = string.Empty;
            ConnDb(ref errMsg);
        }
        #endregion

        public bool ConnDb(ref string errMsg)
        {
            try
            {
                this.Db = DbHelper.GetDb(McConfig.Instance.DbConnect, DbHelper.DataBaseType.SqlServer, ref errMsg);
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                log.Error($"[异常]执行DbAction()建立数据库连接失败:{ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// 获取数据库时间
        /// </summary>
        public bool GetDbTime()
        {
            try
            {
                var dt = Db.Connection.QueryTable("SELECT GETDATE()");
                if (dt == null || dt.Rows.Count == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                var Ip = McConfig.Instance.DbIp;
                if (Tools.Instance.PingNetAddress(Ip))
                {
                    var errMsg = string.Empty;
                    ConnDb(ref errMsg);
                }
                else
                {
                    log.Error($"[异常]执行GetDbTime()获取服务器时间失败:{ex.ToString()}");
                }
                return false;
            }
        }

        /// <summary>
        /// 获取报警项信息
        /// </summary>
        public DataTable GetAlarmOpcItems()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append("select t.loc_plc_no,t.area_no,t.taggroup+t1.tagname tagLongName,t1.discrip,t1.tagindex,t1.tagname");
                sb.Append(" from psb_opc_alarm_group t");
                sb.Append(" left join psb_opc_alarm_items t1 on t1.kind = t.kind");
                sb.Append(" where t.isenable = 1");
                sb.Append(" and t1.isenable = 1");
                sb.Append(" and t.area_no = @AreaNo");
                sb.Append(" order by t.loc_plc_no, t1.tagindex");
                var param = new DynamicParameters();
                param.Add("AreaNo", McConfig.Instance.LocArea);
                return Db.Connection.QueryTable(sb.ToString(), param);
            }
            catch (Exception ex)
            {
                log.Error($"初始化报警信息异常：{ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// 获取记录OBJID
        /// </summary>
        public int GetObjid(ref string errMsg)
        {
            try
            {
                return Db.Connection.ExecuteScalar<int>("select next value for seq_z40_loc_err_log");
            }
            catch (Exception ex)
            {
                log.Error($"获取OBJID失败：{ex.ToString()}");
                errMsg = ex.Message;
                return -1;
            }
        }
        
        /// <summary>
        /// 保存报警
        /// </summary>
        public bool SaveAlarmData(AlarmInfo alarmInfo, ref string errMsg)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" insert into z40_loc_err_log");
                sb.Append(" (objid, loc_plc_no, err_desc, err_begin_time)");
                sb.Append(" values");
                sb.Append(" (@objid, @loc_plc_no, @err_desc, getdate())");
                var param = new DynamicParameters();
                param.Add("objid", alarmInfo.Objid);
                param.Add("loc_plc_no", alarmInfo.LocPlcNo);
                param.Add("err_desc", alarmInfo.Discrip);
                var result = Db.Connection.Execute(sb.ToString(), param);
                if(result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error($"保存报警信息失败：{ex.ToString()}");
                errMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 更新报警已处理
        /// </summary>
        public bool UpdateAlarmData(AlarmInfo alarmInfo, ref string errMsg)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" update z40_loc_err_log");
                sb.Append(" set err_end_time = getdate(),");
                sb.Append(" err_seconds = datediff(SECOND, ERR_BEGIN_TIME, getdate())");
                sb.Append(" where objid = @objid");
                var param = new DynamicParameters();
                param.Add("objid", alarmInfo.Objid);
                var result = Db.Connection.Execute(sb.ToString(), param);
                if (result > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error($"更新报警已处理失败：{ex.ToString()}");
                errMsg = ex.Message;
                return false;
            }
        }
    }
}
