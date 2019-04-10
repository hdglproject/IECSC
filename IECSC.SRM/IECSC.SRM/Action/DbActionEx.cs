using System;
using System.Data;

namespace IECSC.SRM
{
    public partial class DbAction
    {
        /// <summary>
        /// 初始化报警信息
        /// </summary>
        /// <returns></returns>
        public bool LoadSrmFault(ref string errMsg)
        {
            try
            {
                var dt = GetSrmFault();
                if (dt == null || dt.Rows.Count <= 0)
                {
                    return false;
                }
                foreach(DataRow row in dt.Rows)
                {
                    BizHandle.Instance.srm[row["ERR_NO"].ToString()] = row["ERR_NAME"].ToString();
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
        /// 初始化堆垛机信息
        /// </summary>
        public bool LoadOpcItems(ref string errMsg)
        {
            try
            {
                var dt = GetCrnData();
                if (dt == null || dt.Rows.Count <= 0)
                {
                    errMsg = "未找到设备信息";
                    return false;
                }
                BizHandle.Instance.srm.SrmName = dt.Rows[0]["CRN_NAME"].ToString();
                
                //初始化读取项信息
                var dtRead = GetReadItemsData();
                if (dtRead == null || dtRead.Rows.Count <= 0)
                {
                    errMsg = "未找到读取配置项信息";
                    return false;
                }
                foreach (DataRow Row in dtRead.Rows)
                {
                    var opcItem = new SrmOpcItem(); 
                    opcItem.TagName = Row["TAGLONGNAME"].ToString();
                    opcItem.TagLongName = Row["TAGLONGNAME"].ToString();
                    opcItem.BusIdentity = Row["BUSIDENTITY"].ToString();
                    opcItem.TagIndex = Convert.ToInt32(Row["TAGINDEX"].ToString());
                    BizHandle.Instance.readItems.Add(opcItem);
                }

                //初始化写入项信息
                var dtWrite = GetWriteItemsData();
                if (dtWrite == null || dtWrite.Rows.Count <= 0)
                {
                    errMsg = "未找到写入配置项信息";
                    return false;
                }
                foreach (DataRow Row in dtWrite.Rows)
                {
                    var opcItem = new SrmOpcItem();
                    opcItem.TagName = Row["TAGLONGNAME"].ToString();
                    opcItem.TagLongName = Row["TAGLONGNAME"].ToString();
                    opcItem.BusIdentity = Row["BUSIDENTITY"].ToString();
                    opcItem.TagIndex = Convert.ToInt32(Row["TAGINDEX"].ToString());
                    BizHandle.Instance.writeItems.Add(opcItem);
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
        /// 初始化堆垛机指令
        /// </summary>
        /// <returns></returns>
        public bool LoadSrmCmd(ref string errMsg)
        {
            try
            {
                var dt = GetSrmCmd();
                if (dt == null || dt.Rows.Count <= 0)
                {
                    return false;
                }
                BizHandle.Instance.srm.taskCmd = new TaskCmd();
                BizHandle.Instance.srm.taskCmd.ObjId = Convert.ToInt32(dt.Rows[0]["OBJID"].ToString());
                BizHandle.Instance.srm.taskCmd.TaskNo = Convert.ToInt32(dt.Rows[0]["TASK_NO"]);
                BizHandle.Instance.srm.taskCmd.SlocNo = dt.Rows[0]["SLOC_NO"].ToString();
                BizHandle.Instance.srm.taskCmd.SlocPlcNo = dt.Rows[0]["SLOC_PLC_NO"].ToString();
                BizHandle.Instance.srm.taskCmd.ElocNo = dt.Rows[0]["ELOC_NO"].ToString();
                BizHandle.Instance.srm.taskCmd.ElocPlcNo = dt.Rows[0]["ELOC_PLC_NO"].ToString();
                BizHandle.Instance.srm.taskCmd.PalletNo = dt.Rows[0]["PALLET_NO"].ToString();
                BizHandle.Instance.srm.taskCmd.CmdType = dt.Rows[0]["CMD_TYPE"].ToString();
                BizHandle.Instance.srm.taskCmd.CmdStep = dt.Rows[0]["CMD_STEP"].ToString();
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取未下发指令
        /// </summary>
        public TaskCmd LoadCmdToSrmFork(ref string errMsg)
        {
            try
            {
                var dt = GetCmdNoExec();
                if (dt == null || dt.Rows.Count <= 0)
                {
                    return null;
                }
                var taskCmd = new TaskCmd();
                taskCmd.ObjId = Convert.ToInt32(dt.Rows[0]["OBJID"].ToString());
                taskCmd.TaskNo = Convert.ToInt32(dt.Rows[0]["TASK_NO"]);
                taskCmd.SlocNo = dt.Rows[0]["SLOC_NO"].ToString();
                taskCmd.SlocPlcNo = dt.Rows[0]["SLOC_PLC_NO"].ToString();
                taskCmd.ElocNo = dt.Rows[0]["ELOC_NO"].ToString();
                taskCmd.ElocPlcNo = dt.Rows[0]["ELOC_PLC_NO"].ToString();
                taskCmd.PalletNo = dt.Rows[0]["PALLET_NO"].ToString();
                taskCmd.CmdType = dt.Rows[0]["CMD_TYPE"].ToString();
                taskCmd.CmdStep = dt.Rows[0]["CMD_STEP"].ToString();
                return taskCmd;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 获取已下发堆垛机指令
        /// </summary>
        public TaskCmd GetSrmForksCmd(int taskNo, ref string errMsg)
        {
            try
            {
                var dt = GetCmdByTaskNo(taskNo);
                if (dt == null || dt.Rows.Count <= 0)
                {
                    return null;
                }
                var taskCmd = new TaskCmd();
                taskCmd.ObjId = Convert.ToInt32(dt.Rows[0]["OBJID"].ToString());
                taskCmd.TaskNo = Convert.ToInt32(dt.Rows[0]["TASK_NO"]);
                taskCmd.SlocNo = dt.Rows[0]["SLOC_NO"].ToString();
                taskCmd.SlocPlcNo = dt.Rows[0]["SLOC_PLC_NO"].ToString();
                taskCmd.ElocNo = dt.Rows[0]["ELOC_NO"].ToString();
                taskCmd.ElocPlcNo = dt.Rows[0]["ELOC_PLC_NO"].ToString();
                taskCmd.PalletNo = dt.Rows[0]["PALLET_NO"].ToString();
                taskCmd.CmdType = dt.Rows[0]["CMD_TYPE"].ToString();
                taskCmd.CmdStep = dt.Rows[0]["CMD_STEP"].ToString();
                return taskCmd;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// 记录设备报警信息
        /// </summary>
        /// <returns></returns>
        public bool RecordSrmFaultInfo(Srm srm)
        {
            try
            {
                #region 记录设备联机
                if (srm.plcStatus.OperateMode == 1 && srm.RecordLastOperateMode != 1)
                {
                    var objid = GetObjidForFault();
                    if(objid == 0)
                    {
                        return false;
                    }
                    var result = RecordWarnLog(objid, 0, "设备联机", 0, 0);
                    if(result)
                    {
                        srm.RecordLastOperateMode = 1;
                    }
                    else
                    {
                        return false;
                    }
                }
                #endregion

                #region 记录设备停机
                if (srm.plcStatus.OperateMode == 0 && srm.RecordLastOperateMode != 0)
                {
                    var objid = GetObjidForFault();
                    if (objid == 0)
                    {
                        return false;
                    }
                    var result = RecordWarnLog(objid, 0, "设备停机", 0, 0);
                    if (result)
                    {
                        srm.RecordLastOperateMode = 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                #endregion

                #region 记录PLC无法连接
                if (srm.plcStatus.OperateMode == -1 && srm.RecordLastOperateMode != -1)
                {
                    var objid = GetObjidForFault();
                    if (objid == 0)
                    {
                        return false;
                    }
                    var result = RecordWarnLog(objid, 0, $"ping {McConfig.Instance.SrmIp}超时", 0, 0);
                    if (result)
                    {
                        srm.RecordLastOperateMode = -1;
                    }
                    else
                    {
                        return false;
                    }
                }
                #endregion

                #region 记录设备故障
                if (srm.plcStatus.FaultNo > 0)
                {
                    if(srm.plcStatus.FaultNo != srm.RecordLastFaultNo)
                    {
                        var objid = GetObjidForFault();
                        if (objid == 0)
                        {
                            return false;
                        }
                        var result = RecordWarnLog(objid, srm.plcStatus.FaultNo, srm[srm.plcStatus.FaultNo.ToString()], srm.taskCmd.TaskNo, srm.taskCmd.ObjId);
                        if (result)
                        {
                            srm.RecordLastLogObjid = objid;
                            srm.RecordLastFaultNo = srm.plcStatus.FaultNo;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                #endregion

                #region 记录设备故障已处理
                if (srm.plcStatus.FaultNo == 0)
                {
                    if(srm.RecordLastLogObjid > 0)
                    {
                        var result = UpdateWarnLog(srm.RecordLastLogObjid);
                        if(result)
                        {
                            srm.RecordLastLogObjid = 0;
                            srm.RecordLastFaultNo = 0;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                log.Error($"[异常]执行RecordSrmFaultInfo()记录设备报警日志失败,原因{ex.ToString()}");
                return false;
            }
        }
    }
}
