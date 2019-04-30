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
                if (loc.plcStatus.StatusRequest != 1 && loc.plcStatus.StatusToLoad != 1)
                {
                    loc.RequestFinishObjid = 0;
                    loc.bizStatus = BizStatus.None;
                    return;
                }
                //接受到“请求上位机下发任务”信号，请求生成指令
                if (loc.plcStatus.StatusRequest == 1)
                {
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
                            ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[{loc.LocPlcNo}]站台不存在未下发的指令", locNo));
                            return;
                        }
                    }
                    //写入指令
                    if (loc.bizStatus == BizStatus.WriteTask)
                    {
                        var result = commonBiz.WriteTaskCmd(loc);
                        if (result)
                        {
                            loc.bizStatus = BizStatus.Update;
                        }
                        else
                        {
                            return;
                        }
                    }
                    //修改指令步骤为已下发
                    if (loc.bizStatus == BizStatus.Update)
                    {
                        var result = commonBiz.UpdateTaskCmd(loc);
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
                         commonBiz.WriteTaskDeal(loc);
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
                        commonBiz.WriteTaskDeal(loc);
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