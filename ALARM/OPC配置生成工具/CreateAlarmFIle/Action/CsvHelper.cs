using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateAlarmFIle
{
    public class CsvHelper
    {
        /// <summary>
        /// 写入CSV
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="dt">要写入的datatable</param>
        public static void WriteCSV(string fileName, DataTable dt)
        {
            string data = null;
            var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(fs, Encoding.UTF8);

            //写出列名称
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                data += dt.Columns[i].ColumnName.ToString();
                if (i < dt.Columns.Count - 1)
                {
                    data += ",";//中间用，隔开
                }
            }
            sw.WriteLine(data);
            //写出各行数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = null;
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    data += dt.Rows[i][j].ToString();
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";//中间用，隔开
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();
        }
    }
}
