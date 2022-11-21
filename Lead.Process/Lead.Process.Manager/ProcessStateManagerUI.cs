using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lead.Process.Manager
{
    public partial class ProcessStateManagerUI : UserControl
    {
        private Dictionary<string, IProcess> ProcessList = null;
        private Dictionary<string, ProcessState> OldProcessList = new Dictionary<string, ProcessState>();

        public ProcessStateManagerUI(ref Dictionary<string, IProcess> _List)
        {
            InitializeComponent();
            ProcessList = _List;
            foreach (var item in ProcessList)
            {
                this.dataGridView1.Rows.Add(item.Key, item.Value.ThreadId);
                OldProcessList.Add(item.Key, item.Value.State);
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                foreach (var item in ProcessList)
                {
                    if (this.dataGridView1.Rows[i].Cells[0].Value.ToString() == item.Key)
                    {
                        OldProcessList[item.Key] = item.Value.State;
                        this.dataGridView1.Rows[i].Cells[1].Value = item.Value.ThreadId.ToString() == "-1" ? "无线程" : item.Value.ThreadId.ToString();
                        if (item.Value.SingleStep.Count > item.Value.StartStep)
                        {
                            this.dataGridView1.Rows[i].Cells[2].Value = item.Value.SingleStep[item.Value.StartStep].Method.Name;
                        }
                        this.dataGridView1.Rows[i].Cells[3].Style.BackColor = Color.White;
                        this.dataGridView1.Rows[i].Cells[4].Style.BackColor = Color.White;
                        this.dataGridView1.Rows[i].Cells[5].Style.BackColor = Color.White;
                        this.dataGridView1.Rows[i].Cells[6].Style.BackColor = Color.White;
                        this.dataGridView1.Rows[i].Cells[7].Style.BackColor = Color.White;

                        this.dataGridView1.Rows[i].Cells[3 + ((int)item.Value.State)].Style.BackColor =
                            item.Value.State == ProcessState.ProcessNA ? Color.Gray :
                            item.Value.State == ProcessState.ProcessInit ? Color.GreenYellow :
                            item.Value.State == ProcessState.ProcessRunning ? Color.Green :
                            item.Value.State == ProcessState.ProcessPause ? Color.Yellow : Color.Red;
                    }
                }
            }
        }
    }
}
