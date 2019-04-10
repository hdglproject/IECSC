using Dapper;
using DapperExtensions;
using MSTL.DbClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WCS.ExcuteJob
{
    class JobBiz
    {
        //数据库访问类
        private IDatabase Db = null;

        private string orcleConnectionString = string.Empty;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JobBiz()
        {
            string errMsg = string.Empty;
            //读取数据库连接字符串
            orcleConnectionString = ConfigurationManager.ConnectionStrings["SqlConnect"].ConnectionString;
            //初始化数据库
            this.Db = DbHelper.GetDb(orcleConnectionString, DbHelper.DataBaseType.SqlServer, ref errMsg);
        }

        /// <summary>
        /// 获取所有的后台Job
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PsbJobStatus> GetAllJob()
        {
            try
            {
                var jobs = Db.Connection.Query<PsbJobStatus>("select t.* from psb_job_status t where t.usedflag = 1");
                return jobs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 执行作业
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public string ExcuteJob(PsbJobStatus job)
        {
            try
            {
                Db.Connection.Execute(job.SpName, commandType: CommandType.StoredProcedure);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        /// <summary>
        /// 更新作业的执行情况
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public string UpdateJobExcuteInfo(PsbJobStatus job)
        {
            try
            {
                string sql = @"update psb_job_status set 
                                        jobstatus = jobStatus,
                                        excuteresult = excuteresult,
                                        lastexcutetime = lastexcutetime,
                                        maxexcutetime = maxexcutetime,
                                        minexcutetime = minexcutetime,
                                        avgexcutetime = avgexcutetime,
                                        excutecount = excutecount,
                                        totalexcutetime = totalexcutetime
                                  where jobno = jobno";
                DynamicParameters paras = new DynamicParameters();
                paras.Add("jobStatus",job.JobStatus);
                paras.Add("excuteresult", job.ExcuteResult);
                paras.Add("lastexcutetime", job.LastExcuteTime);
                paras.Add("maxexcutetime", job.MaxExcuteTime);
                paras.Add("minexcutetime", job.MinExcuteTime);
                paras.Add("avgexcutetime", job.AvgExcuteTime);
                paras.Add("excutecount", job.ExcuteCount);
                paras.Add("totalexcutetime", job.TotalExcuteTime);
                paras.Add("jobno", job.JobNo);
                Db.Connection.Execute(sql, paras);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
