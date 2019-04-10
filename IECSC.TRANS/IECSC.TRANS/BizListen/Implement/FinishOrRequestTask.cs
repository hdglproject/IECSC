using System;

namespace IECSC.TRANS
{
    public class FinishOrRequestTask : IBiz
    {
        private CommonBiz commonBiz = null;
       
        public FinishOrRequestTask()
        {
            commonBiz = new CommonBiz();
        }

        public void HandleLoc (string locNo)
        {
            try
            {
                var loc = BizHandle.Instance.locDic[locNo];
                //更新站台状态
                DbAction.Instance.RecordPlcInfo(loc);
                if (loc.plcStatus.StatusAuto <= 0)
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[{loc.LocPlcNo}]非自动状态", locNo));
                    return;
                }
                if (loc.plcStatus.StatusFault > 0)
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[{loc.LocPlcNo}]站台故障", locNo));
                    return;
                }
                //接受到“请求上位机下发任务”信号，请求生成指令
                if (loc.plcStatus.StatusRequest == 1)
                {
                    if (string.IsNullOrEmpty(loc.plcStatus.PalletNo))
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[{loc.LocPlcNo}]未获取到PLC传递工装编号", locNo));
                        return;
                    }
                    //获取指令
                    if (loc.bizStatus == BizStatus.None)
                    {
                        var result = commonBiz.SelectTaskCmd(loc);
                        if (result)
                        {
                            loc.bizStatus = BizStatus.WriteTask;
                        }
                        else
                        {
                            loc.bizStatus = BizStatus.ReqTask;
                        }
                    }
                    //获取任务
                    var taskNo = 0;
                    if (loc.bizStatus == BizStatus.ReqTask)
                    {
                        taskNo = commonBiz.RequstTask(loc);
                        if (taskNo > 0)
                        {
                            loc.bizStatus = BizStatus.ReqCmd;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //请求指令
                    if (loc.bizStatus == BizStatus.ReqCmd)
                    {
                        var result = commonBiz.RequstCmd(loc, taskNo);
                        if (result)
                        {
                            loc.bizStatus = BizStatus.Select;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //查找指令
                    if (loc.bizStatus == BizStatus.Select)
                    {
                        var result = commonBiz.SelectTaskCmd(loc);
                        if (result)
                        {
                            loc.bizStatus = BizStatus.WriteTask;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //写入指令
                    if (loc.bizStatus == BizStatus.WriteTask)
                    {
                        var result = commonBiz.WriteTaskCmd(loc);
                        if (result)
                        {
                            loc.bizStatus = BizStatus.WriteDeal;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //写入已处理信号
                    if (loc.bizStatus == BizStatus.WriteDeal)
                    {
                        var result = commonBiz.WriteTaskDeal(loc);
                        if (result)
                        {
                            loc.bizStatus = BizStatus.Reset;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //复位
                    if (loc.bizStatus == BizStatus.Reset)
                    {
                        loc.RequestTaskObjid = 0;
                        loc.RequestCmdObjid = 0;
                        loc.plcStatus.StatusRequest = 0;
                        loc.bizStatus = BizStatus.None;
                    }
                }
                //接受到“站点有货需取货信号”，结束指令
                if (loc.plcStatus.StatusToLoad == 1)
                {
                    if (loc.plcStatus.TaskNo == 0)
                    {
                        ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[{loc.LocPlcNo}]PLC传递任务编号错误", locNo));
                        return;
                    }
                    //结束指令
                    if (loc.bizStatus == BizStatus.None)
                    {
                        var result = commonBiz.FinishCmd(loc, loc.plcStatus.TaskNo);
                        if (result)
                        {
                            loc.bizStatus = BizStatus.WriteDeal;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //写入已处理信号
                    if (loc.bizStatus == BizStatus.WriteDeal)
                    {
                        var result = commonBiz.WriteTaskDeal(loc);
                        if (result)
                        {
                            loc.bizStatus = BizStatus.Reset;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //复位
                    if (loc.bizStatus == BizStatus.Reset)
                    {
                        loc.RequestFinishObjid = 0;
                        loc.plcStatus.StatusToLoad = 0;
                        loc.bizStatus = BizStatus.None;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"站台{locNo}请求处理失败：{ex.Message}", locNo));
            }
        }
    }
}