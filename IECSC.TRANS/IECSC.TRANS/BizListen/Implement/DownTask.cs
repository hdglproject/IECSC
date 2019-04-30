using System;

namespace IECSC.TRANS
{
    public class DownTask : IBiz
    {
        private CommonBiz commonBiz = null;
        
        public  DownTask()
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
                if(loc.plcStatus.StatusRequest != 1)
                {
                    loc.bizStatus = BizStatus.None;
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
                //写入任务已处理
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