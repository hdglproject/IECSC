using System;

namespace IECSC.TRANS
{
    public class RequestTask : IBiz
    {
        private CommonBiz commonBiz = null;
        
        public RequestTask()
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
                if(string.IsNullOrEmpty(loc.plcStatus.PalletNo))
                {
                    ShowFormData.Instance.ShowFormInfo(new ShowInfoData($"[{loc.LocPlcNo}]未获取到PLC传递工装编号", locNo));
                    return;
                }
              
                //获取指令
                if (loc.bizStatus == BizStatus.None)
                {
                    //检查是否已生成任务
                    var result = commonBiz.SelectTaskCmd(loc);
                    if (result)
                    {
                        return;
                    }
                    else
                    {
                        loc.bizStatus = BizStatus.ReqTask;
                    }
                }
                //请求任务
                if(loc.bizStatus == BizStatus.ReqTask)
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
                        loc.bizStatus = BizStatus.WriteDeal;
                    }
                    else
                    {
                        return;
                    }
                }
                //写入任务已处理信号
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