using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Lead.Process.Manager
{
    public partial class DebugUI : UserControl
    {
        private IProcess _Proxy = null;
        private Color Step = Color.Blue;

        public DebugUI(string Name,IProcess proxy)
        {
            InitializeComponent();
            _Proxy = proxy;
            labelName.Text = Name + ":";

            this.dataGridView1.Columns.Add("步骤", "步骤");
            this.dataGridView1.Columns.Add("指令", "指令");
            DataGridViewButtonColumn bu = new DataGridViewButtonColumn();
            bu.Name = "执行";
            bu.Text = "执行";
            bu.HeaderText = "执行";
            bu.DefaultCellStyle.NullValue = "执行";
            this.dataGridView1.Columns.Add(bu);
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            if (_Proxy.SingleStep != null)
            {
                int index = 1;
                foreach (var item in _Proxy.SingleStep)
                {
                    this.dataGridView1.Rows.Add(index++,item.Method.ToString());
                }
            }
        }

        private void buttonInit_Click(object sender, EventArgs e)
        {
            Action ac = () => {
                try
                {
                    _Proxy.Init();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("执行失败："+ex.Message);
                }
            };
            ac.BeginInvoke(new AsyncCallback((ar)=> { this.BeginInvoke(new Action(() => {MessageBox.Show("执行成功");})); }),null);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Action ac = () => {
                try
                {
                    _Proxy.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("执行失败：" + ex.Message);
                }
            };
            ac.BeginInvoke(new AsyncCallback((ar) => { this.BeginInvoke(new Action(() => { MessageBox.Show("执行成功"); })); }), null);

        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            Action ac = () => {
                try
                {
                    _Proxy.Pause();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("执行失败：" + ex.Message);
                }
            };
            ac.BeginInvoke(new AsyncCallback((ar) => { this.BeginInvoke(new Action(() => { MessageBox.Show("执行成功"); })); }), null);
        }

        private void buttonTerminate_Click(object sender, EventArgs e)
        {
            Action ac = () => {
                try
                {
                    _Proxy.Terminate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("执行失败：" + ex.Message);
                }
            };
            ac.BeginInvoke(new AsyncCallback((ar) => { this.BeginInvoke(new Action(() => { MessageBox.Show("执行成功"); })); }), null);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "执行"
               && dataGridView1.Columns[e.ColumnIndex].HeaderText == "执行"
               && e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                    Action ac = () => {
                    try
                    {

                            int index = dataGridView1.CurrentCell.RowIndex;
                            _Proxy.SingleStep[index]();
                            MessageBox.Show("执行成功");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("执行失败：" + ex.Message);
                    }
                };
                //ac.BeginInvoke(new AsyncCallback( (ar) => { this.BeginInvoke(new Action(() => { MessageBox.Show("执行完毕"); })); }), null);
                ac.BeginInvoke(null, null);
            }
        }

        public  void UpdateState(ProcessState State)
        {
            if (State == ProcessState.ProcessNA)
            {
                labelPause.ForeColor = Color.Gray;
                labelRunning.ForeColor = Color.Gray;
                labelTerminate.ForeColor = Color.Gray;
                labelInit.ForeColor = Color.Gray;
            }
            else if (State == ProcessState.ProcessInit)
            {
                labelPause.ForeColor = Color.Gray;
                labelRunning.ForeColor = Color.Gray;
                labelTerminate.ForeColor = Color.Gray;
                labelInit.ForeColor = Color.Green;
            }
            else if (State == ProcessState.ProcessPause)
            {
                labelPause.ForeColor = Color.Green;
                labelRunning.ForeColor = Color.Gray;
                labelTerminate.ForeColor = Color.Gray;
                labelInit.ForeColor = Color.Gray;
            }
            else if (State == ProcessState.ProcessRunning)
            {
                labelPause.ForeColor = Color.Gray;
                labelRunning.ForeColor = Color.Green;
                labelTerminate.ForeColor = Color.Gray;
                labelInit.ForeColor = Color.Gray;
            }
            else
            {
                labelPause.ForeColor = Color.Gray;
                labelRunning.ForeColor = Color.Gray;
                labelTerminate.ForeColor = Color.Red;
                labelInit.ForeColor = Color.Gray;
            }
        }

        public void UpdateStep(int i)
        {
            if (this.dataGridView1.Rows.Count > i)
            {
                for (int index = 0; index < dataGridView1.Rows.Count; index++)
                {
                    if (index != i)
                    {
                        this.dataGridView1.Rows[index].DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
                Step = Step == Color.Blue ? Color.Black : Color.Blue;
                this.dataGridView1.Rows[i].DefaultCellStyle.ForeColor = Step;
            }
        }
    }
}
