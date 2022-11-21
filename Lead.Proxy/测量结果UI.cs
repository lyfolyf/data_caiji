using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lead.Tool.MongoDB;
using Lead.Tool.Log;
using DataGridViewTools;

namespace Lead.Proxy
{
    public partial class 测量结果UI : UserControl
    {
        ConfigParam _Config = null;
        public 测量结果UI()
        {
            InitializeComponent();

            #region 测量结果UI显示
            DataGridViewToolsClass dg = new DataGridViewToolsClass();
            dg.NoShanSHuo(dataGridView1);
            if (this.dataGridView1.Columns.Count == 0)
            {
                for (int i = 0; i < 200; i++)
                {
                    this.dataGridView1.Columns.Add(i.ToString(), "");
                }
            }
            #endregion
        }

        public void AddRows(int HEAD,string[] HeadRow, string[] DataRow, Color[] color)
        {
            Action ac = () => {

                bool IsAllmatch = true;

                if (HeadRow.Length == HEAD || this.dataGridView1.Columns.Count == 0)
                {
                    IsAllmatch = false;
                }

                for (int i = 0; i < HeadRow.Length; i++)
                {
                    if (this.dataGridView1.Columns[i].HeaderText != HeadRow[i])
                    {
                        IsAllmatch = false;
                        break;
                    }
                }
                if (IsAllmatch == false)
                {
                    //1.更新列
                    for (int i = 0; i < HeadRow.Length; i++)
                    {
                        this.dataGridView1.Columns[i].HeaderText = HeadRow[i];
                    }
                }

                //if (this.dataGridView1.Rows.Count > 30)
                //{
                //    this.dataGridView1.Rows.RemoveAt(this.dataGridView1.Rows.Count - 1);
                //    this.dataGridView1.Rows.RemoveAt(this.dataGridView1.Rows.Count - 1);
                //}
                this.SuspendLayout();
                this.dataGridView1.Rows.Insert(0, DataRow);
                this.ResumeLayout();

                for (int i = 0; i < color.Length; i++)
                {
                    if (color[i] != Color.Black)
                    {
                        this.dataGridView1.Rows[0].Cells[i].Style.BackColor = color[i];
                    }
                }
               
            };

            this.BeginInvoke(ac,null);
        }
    }
}
