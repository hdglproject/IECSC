using System;
using System.Collections.Generic;
using System.Data;

namespace IECSC.TRANS
{
    public partial class DbAction
    {
        /// <summary>
        /// 初始化站台信息
        /// </summary>
        public bool LoadOpcItems(ref string errMsg)
        {
            try
            {
                var dt = GetLocData();
                if (dt == null || dt.Rows.Count <= 0)
                {
                    errMsg = "未找到站台信息";
                    return false;
                }
                foreach (DataRow row in dt.Rows)
                {
                    var loc = new Loc();
                    loc.LocNo = row["LOC_NO"].ToString();
                    loc.LocPlcNo = row["LOC_PLC_NO"].ToString();
                    loc.LocTypeNo = row["LOC_TYPE_NO"].ToString();
                    loc.LocTypeDesc = row["LOC_TYPE_NAME"].ToString();
                    loc.TaskType = row["TYPEDESC"].ToString();
                    loc.TaskList = LoadTaskCmd(loc.LocNo, ref errMsg);//获取指令
                    BizHandle.Instance.locDic.Add(loc.LocNo, loc);
                }
                //初始化读取项信息
                var dtRead = GetReadItemsData();
                if (dtRead == null || dtRead.Rows.Count <= 0)
                {
                    errMsg = "未找到读取配置项信息";
                    return false;
                }
                foreach (DataRow row in dtRead.Rows)
                {
                    var opcItem = new LocOpcItem();
                    opcItem.LocNo = row["LOCNO"].ToString();
                    opcItem.LocPlcNo = row["LOCPLCNO"].ToString();
                    opcItem.TagLongName = row["TAGLONGNAME"].ToString();
                    opcItem.BusIdentity = row["BUSINESSIDENTITY"].ToString();
                    BizHandle.Instance.readItems.Add(opcItem.TagLongName, opcItem);
                }
                //初始化写入项信息
                var dtWrite = GetWriteItemsData();
                if (dtWrite == null || dtWrite.Rows.Count <= 0)
                {
                    errMsg = "未找到写入配置项信息";
                    return false;
                }
                foreach (DataRow row in dtWrite.Rows)
                {
                    var opcItem = new LocOpcItem();
                    opcItem.LocNo = row["LOCNO"].ToString();
                    opcItem.LocPlcNo = row["LOCPLCNO"].ToString();
                    opcItem.TagLongName = row["TAGLONGNAME"].ToString();
                    opcItem.BusIdentity = row["BUSINESSIDENTITY"].ToString();
                    BizHandle.Instance.writeItems.Add(opcItem.TagLongName, opcItem);
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
        /// 获取站台尚未完成的所有指令信息
        /// </summary>
        public TaskCmd LoadTaskCmd(string locNo, string cmdStep, ref string errMsg)
        {
            try
            {
                var dt = GetTaskCmd(locNo, cmdStep);
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
        /// 获取指令信息
        /// </summary>
        public List<TaskCmd> LoadTaskCmd(string locNo, ref string errMsg)
        {
            try
            {
                var taskList = new List<TaskCmd>();
                var dt = GetTaskCmd(locNo);
                if (dt == null || dt.Rows.Count <= 0)
                {
                    return null;
                }
                foreach (DataRow row in dt.Rows)
                {
                    var taskCmd = new TaskCmd();
                    taskCmd.ObjId = Convert.ToInt32(row["OBJID"].ToString());
                    taskCmd.TaskNo = Convert.ToInt32(row["TASK_NO"]);
                    taskCmd.SlocType = row["SLOC_TYPE"].ToString();
                    taskCmd.SlocNo = row["SLOC_NO"].ToString();
                    taskCmd.SlocPlcNo = row["SLOC_PLC_NO"].ToString();
                    taskCmd.ElocType = row["ELOC_TYPE"].ToString();
                    taskCmd.ElocNo = row["ELOC_NO"].ToString();
                    taskCmd.ElocPlcNo = row["ELOC_PLC_NO"].ToString();
                    taskCmd.PalletNo = row["PALLET_NO"].ToString();
                    taskCmd.CmdType = row["CMD_TYPE"].ToString();
                    taskCmd.CmdStep = row["CMD_STEP"].ToString();
                    taskList.Add(taskCmd);
                }
                return taskList;
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return null;
            }
        }
    }
}
