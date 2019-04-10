using Dapper;
using DapperExtensions;
using MSTL.DbClient;
using MSTL.LogAgent;
using System;
using System.Data;
using System.Text;

namespace CreateAlarmFIle
{
    public class DbAction
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
                sb.Append("select t.loc_plc_no + '.' + t1.tagname TagName");
                sb.Append(" ,'DB61.' + 'WORD' + CASE WHEN T1.TAGINDEX > 16 THEN convert(varchar,(T.START_LOACTION + 4) * 2) ELSE convert(varchar,(T.START_LOACTION + 3) * 2) END+ '.' + CASE WHEN T1.TAGINDEX > 16 THEN convert(varchar,T1.TAGINDEX - 17) ELSE convert(varchar,T1.TAGINDEX - 1) END Address");
                sb.Append(" , 'Boolean' DataType");
                sb.Append(" , 1 RespectDataType");
                sb.Append(" , 'RO' ClientAccess");
                sb.Append(" , '100' ScanRate");
                sb.Append(" , '' Scaling");
                sb.Append(" , '' RawLow");
                sb.Append(" , '' RawHigh");
                sb.Append(" , '' ScaledLow");
                sb.Append(" , '' ScaledHigh");
                sb.Append(" , '' ScaledDataType");
                sb.Append(" , '' ClampLow");
                sb.Append(" , '' ClampHigh");
                sb.Append(" , '' EngUnits");
                sb.Append(" , t1.discrip Description");
                sb.Append(" , '' NegateValue");
                sb.Append(" from psb_opc_alarm_group t");
                sb.Append(" left join psb_opc_alarm_items t1 on t1.kind = t.kind");
                sb.Append(" where t.isenable = 1");
                sb.Append(" and t1.isenable = 1");
                sb.Append(" order by t.loc_plc_no, t1.tagindex");
                return Db.Connection.QueryTable(sb.ToString());
            }
            catch (Exception ex)
            {
                log.Error($"初始化报警信息异常：{ex.ToString()}");
                return null;
            }
        }
    }
}
