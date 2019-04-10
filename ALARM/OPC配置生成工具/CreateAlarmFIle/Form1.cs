using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CreateAlarmFIle
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Init();
        }

        /// <summary>
        /// 初始化dgv列
        /// </summary>
        private void Init()
        {
            var columns = new DataGridViewTextBoxColumn();
            columns.Name = "Tag Name";
            columns.HeaderText = "Tag Name";
            columns.DataPropertyName = "TagName";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Address";
            columns.HeaderText = "Address";
            columns.DataPropertyName = "Address";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Data Type";
            columns.HeaderText = "Data Type";
            columns.DataPropertyName = "DataType";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Respect Data Type";
            columns.HeaderText = "Respect Data Type";
            columns.DataPropertyName = "RespectDataType";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Client Access";
            columns.HeaderText = "Client Access";
            columns.DataPropertyName = "ClientAccess";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Scan Rate";
            columns.HeaderText = "Scan Rate";
            columns.DataPropertyName = "ScanRate";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Scaling";
            columns.HeaderText = "Scaling";
            columns.DataPropertyName = "Scaling";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Raw Law";
            columns.HeaderText = "Raw Low";
            columns.DataPropertyName = "RawLow";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Raw High";
            columns.HeaderText = "Raw High";
            columns.DataPropertyName = "RawHigh";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Scaled Low";
            columns.HeaderText = "Scaled Low";
            columns.DataPropertyName = "ScaledLow";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Scaled High";
            columns.HeaderText = "Scaled High";
            columns.DataPropertyName = "ScaledHigh";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Scaled Data Type";
            columns.HeaderText = "Scaled Data Type";
            columns.DataPropertyName = "ScaledDataType";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Clamp Low";
            columns.HeaderText = "Clamp Low";
            columns.DataPropertyName = "ClampLow";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Clamp High";
            columns.HeaderText = "Clamp High";
            columns.DataPropertyName = "ClampHigh";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Eng Units";
            columns.HeaderText = "Eng Units";
            columns.DataPropertyName = "EngUnits";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Description";
            columns.HeaderText = "Description";
            columns.DataPropertyName = "Description";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);

            columns = new DataGridViewTextBoxColumn();
            columns.Name = "Negate Value";
            columns.HeaderText = "Negate Value";
            columns.DataPropertyName = "NegateValue";
            columns.Width = 100;
            columns.ReadOnly = false;
            this.dgv.Columns.Add(columns);
        }

        private void dgv_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X, e.RowBounds.Location.Y, dgv.RowHeadersWidth - 4, e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(), dgv.RowHeadersDefaultCellStyle.Font, rectangle, dgv.RowHeadersDefaultCellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Right);
        }

        private void btnRef_Click(object sender, EventArgs e)
        {
            var dt = new DataTable();
            dt = DbAction.Instance.GetAlarmOpcItems();
            this.dgv.DataSource = dt;
         }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var dt = new DataTable();
                dt = DbAction.Instance.GetAlarmOpcItems();
                dt.Columns["TagName"].ColumnName = "Tag Name";
                dt.Columns["Address"].ColumnName = "Address";
                dt.Columns["DataType"].ColumnName = "Data Type";
                dt.Columns["RespectDataType"].ColumnName = "Respect Data Type";
                dt.Columns["ClientAccess"].ColumnName = "Client Access";
                dt.Columns["ScanRate"].ColumnName = "Scan Rate";
                dt.Columns["Scaling"].ColumnName = "Scaling";
                dt.Columns["RawLow"].ColumnName = "Raw Low";
                dt.Columns["RawHigh"].ColumnName = "Raw High";
                dt.Columns["ScaledLow"].ColumnName = "Scaled Low";
                dt.Columns["ScaledHigh"].ColumnName = "Scaled High";
                dt.Columns["ScaledDataType"].ColumnName = "Scaled Data Type";
                dt.Columns["ClampLow"].ColumnName = "Clamp Low";
                dt.Columns["ClampHigh"].ColumnName = "Clamp High";
                dt.Columns["EngUnits"].ColumnName = "Eng Units";
                dt.Columns["Description"].ColumnName = "Description";
                dt.Columns["NegateValue"].ColumnName = "Negate Value";
                CsvHelper.WriteCSV("Alarm.csv", dt);
                MessageBox.Show("导出CSV文件成功");
            }
            catch(Exception ex)
            {
                MessageBox.Show("导出CSV文件出错" + ex.Message);
            }
        }
    }
}
