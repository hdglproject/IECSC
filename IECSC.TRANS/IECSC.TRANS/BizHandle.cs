using MSTL.LogAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IECSC.TRANS
{
    public class BizHandle
    {
        /// <summary>
        /// OPC读取项
        /// </summary>
        public Dictionary<string, LocOpcItem> readItems = null;
        /// <summary>
        /// OPC写入项
        /// </summary>
        public Dictionary<string, LocOpcItem> writeItems = null;
        /// <summary>
        /// 站台信息
        /// </summary>
        public Dictionary<string,Loc> locDic = null;
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
            readItems = new Dictionary<string, LocOpcItem>();
            writeItems = new Dictionary<string, LocOpcItem>();
            locDic = new Dictionary<string, Loc>();
        }
        #endregion

        /// <summary>
        /// 业务处理入口
        /// </summary>
        public void BizListen()
        {
            IBiz biz = null;
            foreach (var loc in locDic.Values)
            {
                ShowFormData.Instance.ShowFormInfo(new ShowInfoData("更新状态", loc.LocNo, InfoType.locStatus));
                switch (loc.TaskType)
                {
                    case "RequestTask":
                        biz = new RequestTask();
                        break;
                    case "RequestAndDownTask":
                        biz = new RequestAndDownTask();
                        break;
                    case "FinishTask":
                        biz = new FinishTask();
                        break;
                    case "DownTask":
                        biz = new DownTask();
                        break;
                    case "DownOrFinishTask":
                        biz = new DownOrFinishTask();
                        break;
                    case "FinishAndDownTask":
                        biz = new FinishAndDownTask();
                        break;
                    case "FinishOrRequestTask":
                        biz = new FinishOrRequestTask();
                        break;
                    case "UpdateFreeFlag":
                        biz = new UpdateFreeFlag();
                        break;
                }
                biz?.HandleLoc(loc.LocNo);
            }
        }
    }
}
