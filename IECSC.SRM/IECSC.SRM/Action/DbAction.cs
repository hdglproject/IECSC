using Dapper;
using DapperExtensions;
using MSTL.DbClient;
using MSTL.LogAgent;
using System;
using System.Data;
using System.Text;

namespace IECSC.SRM
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
        /// 获取数据库时间
        /// </summary>
        public DataTable GetSrmFault()
        {
            try
            {
                return Db.Connection.QueryTable("SELECT * FROM PSB_CRN_ERR");
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetSrmFault()获取堆垛机故障描述失败:{ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// 获取堆垛机信息
        /// </summary>
        private DataTable GetCrnData()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" SELECT T.CRN_NO, T.CRN_NAME FROM PSB_CRN T");
                sb.Append($" WHERE T.CRN_NO = @SrmNo");
                var param = new DynamicParameters();
                param.Add("SrmNo", McConfig.Instance.SrmNo);
                return Db.Connection.QueryTable(sb.ToString(), param);
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetCrnData()查找堆垛机基础信息失败:{ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// 获取堆垛机读取项信息
        /// </summary>
        private DataTable GetReadItemsData()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" SELECT T.TAGCHANNELPREFIX+T.TAGGROUP+T1.BUSIDENTITY TAGLONGNAME,");
                sb.Append(" T.TAGCHANNELPREFIX, T.TAGGROUP, T1.TAGNAME, T1.TAGINDEX,");
                sb.Append(" T1.BUSIDENTITY, T1.KIND FROM PSB_OPC_CRN_GROUP T");
                sb.Append(" LEFT JOIN PSB_OPC_CRN_ITEMS T1 ON T1.TAGPREFIX = 'SINGLE' AND T1.KIND = 'R'");
                sb.Append($" WHERE T.EQUIPNO = @SrmNo");
                var param = new DynamicParameters();
                param.Add("SrmNo", McConfig.Instance.SrmNo);
                return Db.Connection.QueryTable(sb.ToString(), param);
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetReadItemsData()查找堆垛机配置读取项失败:{ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// 获取堆垛机写入项信息
        /// </summary>
        /// <returns></returns>
        private DataTable GetWriteItemsData()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" SELECT T.TAGCHANNELPREFIX+T.TAGGROUP+T1.BUSIDENTITY TAGLONGNAME,");
                sb.Append(" T.TAGCHANNELPREFIX, T.TAGGROUP, T1.TAGNAME, T1.TAGINDEX,");
                sb.Append(" T1.BUSIDENTITY, T1.KIND FROM PSB_OPC_CRN_GROUP T");
                sb.Append(" LEFT JOIN PSB_OPC_CRN_ITEMS T1 ON T1.TAGPREFIX = 'SINGLE' AND T1.KIND = 'W'");
                sb.Append($" WHERE T.EQUIPNO = @SrmNo");
                var param = new DynamicParameters();
                param.Add("SrmNo", McConfig.Instance.SrmNo);
                return Db.Connection.QueryTable(sb.ToString(), param);
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetWriteItemsData()查找堆垛机配置写入项失败:{ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// 获取堆垛机指令
        /// </summary>
        /// <returns></returns>
        private DataTable GetSrmCmd()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" SELECT * FROM WBS_TASK_CMD T");
                sb.Append(" WHERE T.TRANSFER_TYPE = 10");
                sb.Append($" AND T.TRANSFER_NO LIKE '{McConfig.Instance.SrmNo}%'");
                sb.Append(" ORDER BY T.OBJID");
                return Db.Connection.QueryTable(sb.ToString());
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetSrmCmd()获取初始化堆垛机指令:{ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// 获取堆垛机指令
        /// </summary>
        /// <returns></returns>
        private DataTable GetCmdNoExec()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" SELECT * FROM WBS_TASK_CMD T");
                sb.Append(" WHERE T.TRANSFER_TYPE = 10 AND T.CMD_STEP = '00'");
                sb.Append($" AND T.TRANSFER_NO LIKE '{McConfig.Instance.SrmNo}%'");
                sb.Append(" ORDER BY T.OBJID");
                return Db.Connection.QueryTable(sb.ToString());
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetCmdNoExec()获取未下发堆垛机指令:{ex.ToString()}");
                return null;
            }
        }
        /// <summary>
        /// 获取已下发堆垛机指令
        /// </summary>
        public DataTable GetCmdByTaskNo(int taskNo)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(" SELECT T.OBJID,T.TASK_NO,T.SLOC_TYPE,T.SLOC_NO,T.SLOC_PLC_NO,T.ELOC_TYPE,T.ELOC_NO,");
                sb.Append(" T.ELOC_PLC_NO,T.PALLET_NO,T.CMD_TYPE,T.CMD_STEP FROM WBS_TASK_CMD T");
                sb.Append(" WHERE T.TRANSFER_TYPE = 10");
                sb.Append($" AND T.TRANSFER_NO LIKE '{McConfig.Instance.SrmNo}%'");
                sb.Append($" AND T.TASK_NO = {taskNo.ToString()}");
                sb.Append(" AND T.CMD_STEP = '02'");
                sb.Append(" ORDER BY T.OBJID DESC");
                return Db.Connection.QueryTable(sb.ToString());
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetCmdByCrnNo({taskNo})获取已下发堆垛机指令:{ex.ToString()}");
                return null;
            }
        }

        /// <summary>
        /// 修改指令步骤
        /// </summary>
        public bool UpdateCmdStep(int cmdId, string cmdStep)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" UPDATE WBS_TASK_CMD SET ");
                sb.Append(" CMD_STEP = @CMD_STEP,");
                sb.Append(" EXCUTE_DATE = GETDATE()");
                sb.Append(" WHERE OBJID = @OBJID");
                var param = new DynamicParameters();
                param.Add("OBJID", cmdId);
                param.Add("CMD_STEP", cmdStep);
                Db.Connection.Execute(sb.ToString(), param);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行UpdateCmdStep({cmdId},{cmdStep})修改指令步骤失败:{ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// 获取报警记录ID
        /// </summary>
        /// <returns></returns>
        public int GetObjidForFault()
        {
            try
            {
                var objid = Db.Connection.ExecuteScalar<int>("SELECT NEXT VALUE FOR SEQ_Z40_WH_CRN_FORK_ERR_LOG");
                return objid;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetObjidForFault()生成报警记录ID失败:{ex.ToString()}");
                return 0;
            }
        }

        /// <summary>
        /// 记录报警信息
        /// </summary>
        /// <returns></returns>
        public bool RecordWarnLog(int objid,int errNo, string errDesc, int taskNo, int cmdId)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" INSERT INTO Z40_CRN_FORK_ERR_LOG");
                sb.Append(" (OBJID, CRN_FORK_NO, ERR_NO, ERR_DESC, ERR_BEGIN_TIME, TASK_NO, CMD_OBJID)");
                sb.Append(" VALUES");
                sb.Append(" (@OBJID, @CRN_FORK_NO, @ERR_NO, @ERR_DESC, GETDATE(), @TASK_NO, @CMD_OBJID)");
                var param = new DynamicParameters();
                param.Add("OBJID", objid);
                param.Add("CRN_FORK_NO", McConfig.Instance.SrmNo);
                param.Add("ERR_NO", errNo);
                param.Add("ERR_DESC", errDesc);
                param.Add("TASK_NO", taskNo);
                param.Add("CMD_OBJID", cmdId);
                Db.Connection.Execute(sb.ToString(), param);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行RecordWarnLog({objid})记录设备报警信息失败:{ex.ToString()}");
                return false;
            }
        }
        
        /// <summary>
        /// 更新报警已处理信息
        /// </summary>
        /// <returns></returns>
        public bool UpdateWarnLog(int objid)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" UPDATE Z40_CRN_FORK_ERR_LOG");
                sb.Append(" SET ERR_END_TIME = GETDATE(),");
                sb.Append(" ERR_SECONDS = DATEDIFF(SECOND,ERR_BEGIN_TIME,GETDATE())");
                sb.Append(" WHERE OBJID = @OBJID");
                var param = new DynamicParameters();
                param.Add("OBJID", objid);
                Db.Connection.Execute(sb.ToString(), param);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行UpdateWarnLog({objid})更新报警已处理信息失败:{ex.ToString()}");
                return false;
            }
        }

        #region 指令结束
        /// <summary>
        /// 请求结束指令指令结束
        /// </summary>
        public bool RequestFinishTaskCmd(int ObjId, int cmdId, string curLoc, int finishStatus, ref string errMsg)
        {
            try
            {
                //判断指令是否已完成
                var dt = Db.Connection.QueryTable($"SELECT * FROM WBS_TASK_CMD WHERE OBJID = {cmdId}");
                if (dt == null || dt.Rows.Count == 0)
                {
                    return true;
                }
                //插入参数表数据
                if (!InsertFinishData(ObjId, cmdId, curLoc, finishStatus))
                {
                    return false;
                }
                //调用存储过程请求结束
                if (!ExecProcCmdFinish(ObjId, ref errMsg))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行RequestFinishTaskCmd({ObjId},{cmdId},{curLoc},{finishStatus},ref string errMsg)请求结束指令失败:{ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// 保存数据至参数表
        /// </summary>
        public bool InsertFinishData(int ObjId, int cmdId, string curLoc, int finishStatus)
        {
            try
            {
                var dt = Db.Connection.QueryTable($"SELECT * FROM TPROC_0300_CMD_FINISH WHERE OBJID = {ObjId}");
                if (dt == null || dt.Rows.Count <= 0)
                {
                    var sb = new StringBuilder();
                    sb.Append(" INSERT INTO TPROC_0300_CMD_FINISH");
                    sb.Append(" (OBJID,CMD_OBJID,CURR_LOC_NO,FINISH_STATUS)");
                    sb.Append(" VALUES");
                    sb.Append(" (@OBJID,@CMD_OBJID,@CURR_LOC_NO,@FINISH_STATUS)");
                    var param = new DynamicParameters();
                    param.Add("OBJID", ObjId);
                    param.Add("CMD_OBJID", cmdId);
                    param.Add("CURR_LOC_NO", curLoc);
                    param.Add("FINISH_STATUS", finishStatus);
                    Db.Connection.Execute(sb.ToString(), param);
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"执行InsertFinishData({ObjId},{cmdId},{curLoc},{finishStatus})保存指令结束请求参数表{ObjId}失败:{ex.ToString()}");
                return false;
            }
        }

        public bool ExecProcCmdFinish(int objId, ref string errMsg)
        {
            try
            {
                var procName = "PROC_0300_CMD_FINISH";
                var param = new DynamicParameters();
                param.Add("I_PARAM_OBJID", objId);
                param.Add("O_ERR_CODE", 0, DbType.Int32, ParameterDirection.Output);
                param.Add("O_ERR_DESC", null, DbType.String, ParameterDirection.Output, size: 80);
                Db.Connection.Execute(procName, param, commandType: CommandType.StoredProcedure);
                errMsg = param.Get<string>("O_ERR_DESC");
                if (!string.IsNullOrEmpty(errMsg))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行ExecProcCmdFinish({objId},ref string errMsg)请求结束指令失败:{ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// 获取指令结束参数表主键ID
        /// </summary>
        /// <returns></returns>
        public int GetObjidForCmdFinish()
        {
            try
            {
                var objid = Db.Connection.ExecuteScalar<int>("SELECT NEXT VALUE FOR SEQ_TPROC_0300_CMD_FINISH");
                return objid;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetObjidForCmdFinish()获取指令结束参数表主键ID异常:{ex.Message}");
                return 0;
            }
        }
        #endregion

        #region 先入品、空出库
        /// <summary>
        /// 获取空出库人工确认标记
        /// </summary>
        public int GetFipFlag()
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" SELECT ISNULL(MIN(T.FIP_FLAG),0) FIP_FLAG FROM PEM_CRN_FORK_STATUS T");
                sb.Append($" WHERE T.CRN_FORK_NO LIKE '{McConfig.Instance.SrmNo}%'");
                var dt = Db.Connection.QueryTable(sb.ToString());
                if (dt == null || dt.Rows.Count <= 0)
                {
                    return 0;
                }
                return Convert.ToInt32(dt.Rows[0]["FIP_FLAG"]);
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行GetFipFlag()获取空出库/先入品人工确认标记失败:{ex.ToString()}");
                return 0;
            }
        }

        /// <summary>
        /// 更新设备故障异常
        /// </summary>
        public bool UpdateEquipErrStatus(int taskNo, int faultNo, int fipFlag)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" UPDATE PEM_CRN_FORK_STATUS");
                sb.Append(" SET TASK_NO = @TASKNO,");
                sb.Append(" FIP_FAULT_NO = @FAULTNO,");
                sb.Append(" FIP_FLAG = @FIP_FLAG,");
                sb.Append(" FIP_DATE = GETDATE()");
                sb.Append(" WHERE CRN_NO = @CRN_NO");
                var param = new DynamicParameters();
                param.Add("TASKNO", taskNo);
                param.Add("FAULTNO", faultNo);
                param.Add("FIP_FLAG", fipFlag);
                param.Add("CRN_NO", McConfig.Instance.SrmNo);
                Db.Connection.Execute(sb.ToString(), param);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行UpdateEquipErrStatus({taskNo},{faultNo},{fipFlag})更新空出库/先入品标记失败:{ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// 更新设备故障异常
        /// </summary>
        public bool UpdateEquipErrStatus(int taskNo)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" UPDATE PEM_CRN_FORK_STATUS");
                sb.Append(" SET TASK_NO = @TASKNO,");
                sb.Append(" FIP_FAULT_NO = 0,");
                sb.Append(" FIP_FLAG = 0,");
                sb.Append(" FIP_DATE = NULL,");
                sb.Append(" FIP_HANDLE_RESULT = NULL");
                sb.Append(" WHERE CRN_NO = @CRN_NO");
                var param = new DynamicParameters();
                param.Add("TASKNO", taskNo);
                param.Add("CRN_NO", McConfig.Instance.SrmNo);
                Db.Connection.Execute(sb.ToString(), param);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行UpdateEquipErrStatus({taskNo})清除空出库/先入品标记失败:{ex.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// 先入品处理
        /// </summary>
        public int ExecProcFirstInProduct(ref string errMsg)
        {
            try
            {
                var procName = "PROC_0400_FIRST_IN_PRODUCT";
                var param = new DynamicParameters();
                param.Add("I_EQUIP_NO", McConfig.Instance.SrmNo);
                param.Add("O_FIP_FLAG", 0, DbType.Int32, ParameterDirection.Output);
                param.Add("O_FIP_HANDLE_RESULT", null, DbType.String, ParameterDirection.Output, size: 80);
                Db.Connection.Execute(procName, param, commandType: CommandType.StoredProcedure);
                errMsg = param.Get<string>("O_FIP_HANDLE_RESULT");
                return param.Get<int>("O_FIP_FLAG");
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行ExecProcFirstInProduct(ref string errMsg)处理先入品故障失败:{ex.Message}");
                return -1;
            }
        }
        #endregion

        /// <summary>
        /// 任务删除
        /// </summary>
        public bool DeleteTaskCmd(int taskNo)
        {
            try
            {
                var procName = "PROC_WCS_CRNCMD_DEL";
                var param = new DynamicParameters();
                param.Add("I_TASK_NO", taskNo);
                Db.Connection.Execute(procName, param, commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行DeleteTaskCmd({taskNo})删除任务失败:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新堆垛机状态
        /// </summary>
        public bool RecordPlcInfo(Srm srm)
        {
            try
            {
                var sb = new StringBuilder();
                sb.Append(" UPDATE PEM_CRN_FORK_STATUS");
                sb.Append(" SET TASK_NO = @TASKNO,DEVICEID = @DEVICEID");
                sb.Append(" ,OPERATEMODE = @OPERATEMODE,MISSIONSTATE = @MISSIONSTATE,MISSIONTYPE = @MISSIONTYPE");
                sb.Append(" ,MISSIONID = @MISSIONID,PALLETID = @PALLETID,ACTPOSBAY = @ACTPOSBAY");
                sb.Append(" ,ACTPOSLEVEL = @ACTPOSLEVEL,ACTPOSX = @ACTPOSX,ACTPOSY = @ACTPOSY");
                sb.Append(" ,ACTPOSZ = @ACTPOSZ,ACTPOSZDEEP = @ACTPOSZDEEP,ACTSPEEDX = @ACTSPEEDX");
                sb.Append(" ,ACTSPEEDY = @ACTSPEEDY,ACTSPEEDZ = @ACTSPEEDZ,ACTSPEEDZDEEP = @ACTSPEEDZDEEP");
                sb.Append(" ,LOADSTATUS = @LOADSTATUS,FAULTNO = @FAULTNO");
                sb.Append(" WHERE CRN_NO =@SRMNO");
                var param = new DynamicParameters();
                param.Add("TASKNO", srm.plcStatus.MissionId);
                param.Add("DEVICEID", srm.plcStatus.DeviceId);
                param.Add("OPERATEMODE", srm.plcStatus.OperateMode);
                param.Add("MISSIONSTATE", srm.plcStatus.MissionState);
                param.Add("MISSIONTYPE", srm.plcStatus.MissionType);
                param.Add("MISSIONID", srm.plcStatus.MissionId);
                param.Add("PALLETID", srm.plcStatus.PalletNo);
                param.Add("ACTPOSBAY", srm.plcStatus.ActPosBay);
                param.Add("ACTPOSLEVEL", srm.plcStatus.ActPosLevel);
                param.Add("ACTPOSX", srm.plcStatus.ActPosX);
                param.Add("ACTPOSY", srm.plcStatus.ActPosY);
                param.Add("ACTPOSZ", srm.plcStatus.ActPosZ);
                param.Add("ACTPOSZDEEP", srm.plcStatus.ActPosZDeep);
                param.Add("ACTSPEEDX", srm.plcStatus.ActSpeedX);
                param.Add("ACTSPEEDY", srm.plcStatus.ActSpeedY);
                param.Add("ACTSPEEDZ", srm.plcStatus.ActSpeedZ);
                param.Add("ACTSPEEDZDEEP", srm.plcStatus.ActSpeedZDeep);
                param.Add("LOADSTATUS", srm.plcStatus.LoadStatus);
                param.Add("FAULTNO", srm.plcStatus.FaultNo);
                param.Add("SRMNO", McConfig.Instance.SrmNo);
                Db.Connection.Execute(sb.ToString(), param);
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行RecordPlcInfo()更新堆垛机状态失败:{ex.ToString()}");
                return false;
            }
        }
    }
}
