using System;
using System.Collections.Generic;
using MSTL.LogAgent;

namespace IECSC.SRM
{
    public class BizHandle
    {
        /// <summary>
        /// OPC读取项
        /// </summary>
        public List<SrmOpcItem> readItems = null;
        /// <summary>
        /// OPC写入项
        /// </summary>
        public List<SrmOpcItem> writeItems = null;
        /// <summary>
        /// SRM信息
        /// </summary>
        public Srm srm = null;
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
        private BizHandle()
        {
            readItems = new List<SrmOpcItem>();
            writeItems = new List<SrmOpcItem>();
            srm = new Srm();
        }
        #endregion

        public bool BizListen()
        {
            //检验设备状态 0：手动 1：自动 -1:PLC断线
            if (srm.plcStatus.OperateMode <= 0)
            {
                return false;
            }
            //校验心跳信号是否正常
            if ((DateTime.Now - srm.LastPlcHeartBeatTime).TotalSeconds > 10)
            {
                if (srm.LastPlcHeartBeat == srm.plcStatus.HeartBeat)
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[异常]未检测到心跳信号"));
                    return false;
                }
                else
                {
                    srm.LastPlcHeartBeat = srm.plcStatus.HeartBeat;
                    srm.LastPlcHeartBeatTime = DateTime.Now;
                }
            }
            var errMsg = string.Empty;
            //写入心跳信号
            if (!OpcAction.Instance.WriteHeartBeat(ref errMsg))
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[异常]无法写入心跳信号"));
                return false;
            }
            //业务处理
            Start();
            return true;
        }
        /// <summary>
        /// 业务逻辑
        /// </summary>
        private void Start()
        {
            if(srm.bizStatus == BizStatus.None)
            {
                BizStatusNone();
            }
            if (srm.bizStatus == BizStatus.Down)
            {
                BizStatusDown();
            }
            if (srm.bizStatus == BizStatus.Ready)
            {
                BizStatusReady();
            }
            if (srm.bizStatus == BizStatus.Exec)
            {
                BizStatusExec();
            }
            if (srm.bizStatus == BizStatus.Error)
            {
                BizStatusError();
            }
            if (srm.bizStatus == BizStatus.ErrorDbDeal)
            {
                BizStatusErrorDbDeal();
            }
            if (srm.bizStatus == BizStatus.ErrorPlcDeal)
            {
                BizStatusErrorPlcDeal();
            }
            if (srm.bizStatus == BizStatus.End)
            {
                BizStatusEnd();
            }
            if (srm.bizStatus == BizStatus.Reset)
            {
               BizStatusReset();
            }
        }

        /// <summary>
        /// 初始阶段：任务状态为0查询指令、任务状态为1或2为系统重启需恢复状态
        /// </summary>
        private void BizStatusNone()
        {
            try
            {
                var errMsg = string.Empty;
                //查找已下发指令，恢复状态
                if (srm.plcStatus.MissionState == 1 || srm.plcStatus.MissionState == 2)
                {
                    srm.taskCmd = DbAction.Instance.GetSrmForksCmd(srm.plcStatus.MissionId, ref errMsg);
                    if (srm.taskCmd != null)
                    {
                        srm.bizStatus = BizStatus.Exec;
                        srm.SetSrmTaskInfo(srm.taskCmd);
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[None]业务步骤跳转至执行等待阶段"));
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[None]获取已下发指令失败{errMsg}"));
                    }
                    return;
                }
                //检验设备是否故障
                if(srm.plcStatus.FaultNo > 0)
                {
                    if(srm.plcStatus.FaultNo != 68)
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[None]设备故障：{srm[srm.plcStatus.FaultNo.ToString()]}"));
                        return;
                    }
                }
                //防止异常状态下接收到上位机传递指令
                if(srm.plcStatus.MissionState != 0)
                {
                    return;
                }
                //获取未下发指令
                srm.taskCmd = DbAction.Instance.LoadCmdToSrmFork(ref errMsg);
                if (srm.taskCmd != null)
                {
                    srm.SetSrmTaskInfo(srm.taskCmd);
                    srm.bizStatus = BizStatus.Down;
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[None]查找到任务{srm.taskCmd.TaskNo},业务步骤跳转至任务下发阶段"));
                }
                else
                {
                    if(!string.IsNullOrEmpty(errMsg))
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[None]查找任务信息失败{errMsg}"));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[None]执行 BizStatusNone() 查找任务信息失败{ex.Message}"));
                log.Error($"[None]执行 BizStatusNone() 查找任务信息失败-{ex.ToString()}");
            }
        }

        /// <summary>
        /// 指令下传
        /// </summary>
        private void BizStatusDown()
        {
            try
            {
                var errMsg = string.Empty;
                //写入指令
                var result = OpcAction.Instance.WriteTaskCmd(srm, ref errMsg);
                if(result)
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Down]成功写入任务信息{srm.taskCmd.TaskNo}"));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Down]写入任务信息{srm.taskCmd.TaskNo}失败,原因{errMsg}"));
                    return;
                }
                //写入起效信号
                result = OpcAction.Instance.WriteSequenceNo(1, ref errMsg);
                if(result)
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Down]成功写入任务起效信号1"));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Down]写入任务起效信号失败,原因{errMsg}"));
                    return;
                }
                //更新指令步骤
                result = DbAction.Instance.UpdateCmdStep(srm.taskCmd.ObjId, "02");
                if (result)
                {
                    srm.taskCmd.CmdStep = "02";
                    srm.SetSrmTaskInfo(srm.taskCmd);
                    srm.bizStatus = BizStatus.Ready;
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Down]更新指令步骤为执行成功,业务步骤跳转至等待反馈阶段"));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Down]更新指令步骤失败,原因{errMsg}"));
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Down]执行 BizStatusDown() 下发指令失败{ex.Message}"));
                log.Error($"[Down]执行 BizStatusDown() 下发指令失败{ex.ToString()}");
            }
        }

        /// <summary>
        /// 等待下位机执行任务
        /// </summary>
        private void BizStatusReady()
        {
            if (srm.plcStatus.MissionState == 1)
            {
                srm.bizStatus = BizStatus.Exec;
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[Ready]获取到PLC反馈信号,业务步骤跳转至执行监控阶段"));
            }
        }

        /// <summary>
        /// 指令执行 
        /// </summary>
        private void BizStatusExec()
        {
            try
            {
                //故障检测
                if (srm.plcStatus.FaultNo > 0)
                {
                    srm.bizStatus = BizStatus.Error;
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Exec]设备故障:{srm[srm.plcStatus.FaultNo.ToString()]},业务步骤跳转至异常处理阶段"));
                    return;
                }
                //执行完成监控
                if(srm.plcStatus.MissionState == 2)
                {
                    //获取指令结束请求OBJID
                    if (srm.RequestFinishObjid <= 0)
                    {
                        srm.RequestFinishObjid = DbAction.Instance.GetObjidForCmdFinish();
                    }
                    if(srm.RequestFinishObjid <= 0)
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[Exec]PLC反馈任务完成,但生成指令结束参数表OBJID失败"));
                        return;
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Exec]PLC反馈任务完成,生成OBJID={srm.RequestFinishObjid}结束指令"));
                    }
                    //传入参数，结束指令
                    var errMsg = string.Empty;
                    var result = DbAction.Instance.RequestFinishTaskCmd(srm.RequestFinishObjid, srm.taskCmd.ObjId, srm.taskCmd.ElocNo, 1, ref errMsg);
                    if(result)
                    {
                        srm.ClearSrmTaskInfo();
                        srm.bizStatus = BizStatus.End;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Exec]成功结束指令{srm.taskCmd.ObjId},跳转业务步骤至信号复位阶段"));
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Exec]结束指令{srm.taskCmd.ObjId}失败,原因{errMsg}"));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Exec]执行 BizStatusExec() 监控设备执行情况失败:{ex.Message}"));
                log.Error($"[Exec]执行 BizStatusExec() 监控设备执行情况失败:{ex.ToString()}");
            }
        }
        /// <summary>
        /// 指令结束
        /// </summary>
        public void BizStatusEnd()
        {
            try
            {
                var errMsg = string.Empty;
                if (srm.plcStatus.MissionState == 2)
                {
                    var result = OpcAction.Instance.WriteSequenceNo(2, ref errMsg);
                    if(result)
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[End]成功传递完成信号2至PLC"));
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[End]传递完成信号2至PLC失败,原因{errMsg}"));
                        return;
                    }
                }
                if(srm.plcStatus.MissionState == 0)
                {
                    var result = OpcAction.Instance.WriteSequenceNo(0, ref errMsg);
                    if (result)
                    {
                        srm.bizStatus = BizStatus.Reset;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[End]成功传递复位信号0至PLC,业务步骤跳转至复位阶段"));
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[End]传递复位信号0至PLC失败,原因{errMsg}"));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[End]执行 BizStatusEnd() 传递复位信号失败:{ex.Message}"));
                log.Error($"[End]执行 BizStatusEnd() 传递复位信号失败:{ex.ToString()}");
            }
        }
        /// <summary>
        /// 信号复位
        /// </summary>
        public void BizStatusReset()
        {
            if (srm.plcStatus.MissionState == 0)
            {
                //复位请求结束指令OBJID
                srm.RequestFinishObjid = 0;
                //初始化指令信息
                srm.taskCmd = new TaskCmd();
                srm.ClearSrmTaskInfo();
                //更新业务步骤
                srm.bizStatus = BizStatus.None;
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[Reset]成功初始化业务状态,本次任务结束" + Environment.NewLine + "--------------------------------------------------------------"));
            }
        }
        /// <summary>
        /// 堆垛机叉异常指令处理
        /// </summary>
        public void BizStatusError()
        {
            try
            {
                //若故障已被处理
                if(srm.plcStatus.FaultNo == 0)
                {
                    if(srm.plcStatus.MissionState == 0)
                    {
                        srm.bizStatus = BizStatus.Reset;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Error]异常状态下收到PLC复位信号,请确认并处理任务{srm.taskCmd.TaskNo}"));
                    }
                    else if(srm.plcStatus.MissionState == 2)
                    {
                        srm.bizStatus = BizStatus.Exec;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Error]异常状态下收到PLC任务完成信号,跳转业务步骤至执行监控阶段"));
                    }
                }
                //空出库
                else if(srm.plcStatus.FaultNo == 53)
                {
                    var result = DbAction.Instance.UpdateEquipErrStatus(srm.taskCmd.TaskNo, srm.plcStatus.FaultNo, 1);
                    if(result)
                    {
                        srm.bizStatus = BizStatus.ErrorDbDeal;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[Error]接收到PLC空出库报警信号,成功记录标记,等待人工确认"));
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[Error]接收到PLC空出库报警信号,但记录标记失败"));
                    }
                }
                //先入品
                else if (srm.plcStatus.FaultNo == 68)
                {
                    var result = DbAction.Instance.UpdateEquipErrStatus(srm.taskCmd.TaskNo, srm.plcStatus.FaultNo, 1);
                    if (result)
                    {
                        srm.bizStatus = BizStatus.ErrorDbDeal;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[Error]接收到PLC先入品报警信号,成功记录标记,等待人工确认"));
                    }
                    else
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[Error]接收到PLC先入品报警信号,但记录标记失败"));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[Error]执行 BizStatusError() 记录设备故障标记失败:{ex.Message}"));
                log.Error($"[Error]执行 BizStatusError() 记录设备故障标记失败:{ex.ToString()}");
            }
        }
        /// <summary>
        /// 异常处理
        /// </summary>
        public void BizStatusErrorDbDeal()
        {
            try
            {
                #region 若故障已被处理
                if (srm.plcStatus.FaultNo == 0)
                {
                    if (srm.plcStatus.MissionState == 0)
                    {
                        srm.bizStatus = BizStatus.Reset;
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]等待人工确认异常状态下收到PLC复位信号,请确认并处理任务{srm.taskCmd.TaskNo}"));
                    }
                    else if (srm.plcStatus.MissionState == 2)
                    {
                        var result = DbAction.Instance.UpdateEquipErrStatus(srm.taskCmd.TaskNo);
                        if (result)
                        {
                            srm.bizStatus = BizStatus.Exec;
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]等待人工确认异常状态收到PLC任务完成信号,跳转业务步骤至执行监控阶段"));
                        }
                        else
                        {
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]等待人工确认异常状态收到PLC任务完成信号,清理异常标记失败"));
                        }
                    }
                    return;
                }
                #endregion

                #region 空出库处理
                else if (srm.plcStatus.FaultNo == 53)
                {
                    var fipFlag = DbAction.Instance.GetFipFlag();
                    if (fipFlag == 2)
                    {
                        //获取指令结束请求OBJID
                        if (srm.RequestFinishObjid == 0)
                        {
                            srm.RequestFinishObjid = DbAction.Instance.GetObjidForCmdFinish();
                        }
                        if (srm.RequestFinishObjid == 0)
                        {
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[ErrorDbDeal]接收到空出库人工确认信号,但生成指令结束参数表OBJID失败"));
                            return;
                        }
                        else
                        {
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]接收到空出库人工确认信号,生成OBJID={srm.RequestFinishObjid}请求结束指令"));
                        }
                        //传入参数，结束指令 201为空出库标记
                        var errMsg = string.Empty;
                        var result = DbAction.Instance.RequestFinishTaskCmd(srm.RequestFinishObjid, srm.taskCmd.ObjId, srm.taskCmd.ElocNo, 201, ref errMsg);
                        if (result)
                        {
                            srm.ClearSrmTaskInfo();
                            srm.bizStatus = BizStatus.ErrorPlcDeal;
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]成功结束空出库指令{srm.taskCmd.ObjId}"));
                        }
                        else
                        {
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]结束空出库指令{srm.taskCmd.ObjId}失败,原因{errMsg}"));
                            return;
                        }
                    }
                }
                #endregion

                #region  先入品处理
                else if (srm.plcStatus.FaultNo == 68)
                {
                    var fipFlag = DbAction.Instance.GetFipFlag();
                    if (fipFlag == 2)
                    {
                        var errMsg = string.Empty;
                        var dealFlag = DbAction.Instance.ExecProcFirstInProduct(ref errMsg);
                        if (dealFlag == 3)
                        {
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData("[ErrorDbDeal]接收到先入品人工确认信号,成功处理指令"));
                        }
                        else
                        {
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]接收到先入品人工确认信号,但处理指令失败,原因{errMsg}"));
                            return;
                        }
                        var result = DbAction.Instance.UpdateEquipErrStatus(srm.taskCmd.TaskNo);
                        if (result)
                        {
                            srm.bizStatus = BizStatus.ErrorPlcDeal;
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]清理数据库异常处理标记成功"));
                        }
                        else
                        {
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorDbDeal]清理数据库异常处理标记失败"));
                            return;
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[异常]执行 BizStatusErrorDeal() 处理先入品/空出库报警失败:{ex.Message}"));
                log.Error($"[异常]执行 BizStatusErrorDeal() 处理先入品/空出库报警失败:{ex.Message}");
            }
        }
        /// <summary>
        /// 异常处理
        /// </summary>
        public void BizStatusErrorPlcDeal()
        {
            try
            {
                var errMsg = string.Empty;
                var result = OpcAction.Instance.WriteSequenceNo(2, ref errMsg);
                if (result)
                {
                    srm.bizStatus = BizStatus.Reset;
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorPlcDeal]成功传递报警复位信号2至PLC"));
                }
                else
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[ErrorPlcDeal]传递报警复位信号2至PLC失败"));
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[异常]执行 BizStatusErrorPlcDeal() 传递报警复位信号2至PLC失败:{ex.Message}"));
                log.Error($"[异常]执行 BizStatusErrorPlcDeal() 传递报警复位信号2至PLC失败:{ex.Message}");
            }
        }
    }
}
