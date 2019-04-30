using System;

namespace IECSC.TRANS
{
    public class RequestAndDownTask : IBiz
    {
        private CommonBiz commonBiz = null;
        
        public RequestAndDownTask()
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
                if (loc.plcStatus.StatusRequest != 1)
                {
                    loc.RequestTaskObjid = 0;
                    loc.RequestCmdObjid = 0;
                    loc.bizStatus = BizStatus.None;
                   
                    return;
                }
                if (string.IsNullOrEmpty(loc.plcStatus.PalletNo))
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[{loc.LocPlcNo}]未获取到PLC传递工装编号", locNo));
                    return;
                }
                if (loc.plcStatus.PalletQty<=0)
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[{loc.LocPlcNo}]叠盘机传递工装数量不能<=0", locNo));
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
                        loc.bizStatus = BizStatus.Bind;
                    }
                }
                if (loc.bizStatus==BizStatus.Bind)
                {
                    var result = commonBiz.SetBoundNoToDB(loc);
                    if (result)
                    {
                        loc.bizStatus = BizStatus.ReqTask;
                    }
                    else
                    {
                        return;
                    }
                }
                //获取任务
                if (loc.bizStatus == BizStatus.ReqTask)
                {
                    loc.TaskNo = commonBiz.RequstTask(loc);
                    if (loc.TaskNo > 0)
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
                    var result = commonBiz.RequstCmd(loc, loc.TaskNo);
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
                        loc.bizStatus = BizStatus.Update;
                    }
                    else
                    {
                        return;
                    }
                }
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
            catch (Exception ex)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"站台{locNo}请求处理失败：{ex.Message}", locNo));
            }
        }
    }
}