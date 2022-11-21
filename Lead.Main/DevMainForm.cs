using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Lead.Process.Manager;
using Lead.Tool.Login;
using Lead.Proxy;
using Lead.Tool.ProjectPath;
using Lead.Tool.Log;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;
using Lead.Tool.INI;
using System.IO;

namespace Lead.Main
{
    public partial class DevMainForm : DockContent
    {
        private ProxyData _ProxyData = ProxyData.GetInstance();//数据集
        private ProcessManager _ProcessUI = new ProcessManager();//任务集
        private Form _ManualUI = null;
        public DevMainForm()
        {
            InitializeComponent();
            this.MaximumSize = new Size(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);

            var x = Logger.ReadlUI;
            x.Dock = DockStyle.Fill;
            this.panel3.Controls.Add(x);

            var y = _ProcessUI.ProcessStateUI;
            y.Dock = DockStyle.Fill;
            this.panel4.Controls.Add(y);

            var z = _ProxyData.ToolConfigUI.StateMangerUI;
            z.Dock = DockStyle.Fill;
            this.panel5.Controls.Add(z);

            var hh = _ProxyData.测量结果UI;
            hh.Dock = DockStyle.Fill;
            this.panel测量结果.Controls.Add(hh);
        }

        private void DevMainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _ProxyData.FormOpen();

                //_ProcessUI.ProcessList["SafetyDor"].Init();
                //_ProcessUI.ProcessList["SafetyDor"].Start();
                //_ProcessUI.ProcessList["SafetyEMG"].Init();
                //_ProcessUI.ProcessList["SafetyEMG"].Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("系统加载失败"+ex.Message);
            }

        }

        private void DevMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _ProxyData.FormClose();
            System.Environment.Exit(0);
            Application.Exit();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_ProxyData.LoginUI == null)
            {
                buttonTool.Visible = false;
                buttonProcess.Visible = false;
                btnLogin.Visible = false;
                btnConfig.Visible = false;
                buttonDebug.Visible = false;

                buttonReset.Visible = false;
            }
            else
            {
                if (_ProxyData.LoginUI.IsLogined && _ProxyData.LoginUI._level == Level.管理员)
                {
                    buttonTool.Visible = true;
                    buttonProcess.Visible = true;
                    btnLogin.Visible = true;
                    btnConfig.Visible = true;
                    buttonDebug.Visible = true;
                    labelLogin.Text = "管理员";

                    buttonReset.Visible = true;
                }
                else if (_ProxyData.LoginUI.IsLogined && _ProxyData.LoginUI._level == Level.工艺员)
                {
                    buttonTool.Visible = false;
                    buttonProcess.Visible = true;
                    btnLogin.Visible = true;
                    btnConfig.Visible = true;
                    buttonDebug.Visible = false;
                    labelLogin.Text = "工艺员";
                    if (_ProxyData.ToolConfigUI != null)
                    {
                        _ProxyData.ToolConfigUI.Hide();
                    }

                    buttonReset.Visible = true;
                }
                else if (_ProxyData.LoginUI.IsLogined && _ProxyData.LoginUI._level == Level.普通用户)
                {
                    buttonTool.Visible = false;
                    buttonProcess.Visible = false;
                    btnLogin.Visible = true;
                    btnConfig.Visible = true;
                    buttonDebug.Visible = false;
                    labelLogin.Text = "普通用户";

                    buttonReset.Visible = true;
                }
                else
                {
                    buttonTool.Visible = false;
                    buttonProcess.Visible = false;
                    btnLogin.Visible = true;
                    btnConfig.Visible = false;
                    buttonDebug.Visible = false;
                    labelLogin.Text = "未登陆";
                    if (_ProxyData.ToolConfigUI != null)
                    {
                        _ProxyData.ToolConfigUI.Hide();
                    }
                    if (_ProcessUI != null)
                    {
                        _ProcessUI.Hide();
                    }
                    if (_ProxyData.ProjectConfigUI != null)
                    {
                        _ProxyData.ProjectConfigUI.Hide();
                    }

                    buttonReset.Visible = false;
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            labelTime.Text = System.DateTime.Now.ToString();

            labelVersion.Text = "Version："+Assembly.GetExecutingAssembly().GetName().Version.ToString();

            switch (_ProxyData.State)
            {
                case ProjectSate.NA:
                    {
                        buttonStart.Enabled = false;
                        buttonStop.Enabled = false;
                        buttonManual.Enabled = false;
                        buttonReset.Enabled = true;
                        this.labelState.Text  = "当前状态：首次启动，未初始化";
                        this.labelState.ForeColor = Color.White;
                    }
                    break;
                case ProjectSate.Init:
                    {
                        buttonStart.Enabled = true;
                        buttonStop.Enabled = false;
                        buttonManual.Enabled = true;
                        buttonReset.Enabled = true;
                        this.labelState.Text = "当前状态：已初始化";
                        this.labelState.ForeColor = Color.Green;
                    }
                    break;
                case ProjectSate.Auto_Start:
                    {
                        buttonStart.Enabled = false;
                        buttonStop.Enabled = true;
                        buttonManual.Enabled = false;
                        buttonReset.Enabled = true;
                        this.labelState.Text = "当前状态：自动运行";
                        this.labelState.ForeColor = this.labelState.ForeColor==Color.Green? Color.White: Color.Green;
                    }
                    break;
                case ProjectSate.Auto_Stop:
                    {
                        buttonStart.Enabled = true;
                        buttonStop.Enabled = false;
                        buttonManual.Enabled = false;
                        buttonReset.Enabled = true;
                        this.labelState.Text = "当前状态：暂停运行";
                        this.labelState.ForeColor = Color.Red;
                    }
                    break;
                case ProjectSate.Manual:
                    {
                        buttonStart.Enabled = false;
                        buttonStop.Enabled = false;
                        buttonManual.Enabled = false;
                        buttonReset.Enabled = true;
                        this.labelState.Text = "当前状态：手动";
                        this.labelState.ForeColor = Color.Yellow;
                    }
                    break;
                default:
                    break;
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (_ProxyData.GetIsReset())
            {
                Reset();

                if (_ProxyData.State == ProjectSate.Auto_Start)
                {
                    _ProxyData.SetReseted(true);
                }
                else
                {
                    _ProxyData.SetReseted(false);
                }
            }
        }

        string fenBuShiPath = @"D:\3D与运动控制交互\Ex_Online.ini";
        private bool IsOverTime(string Key)
        {
            try
            {
                DateTime FF = DateTime.Now.AddHours(-1);
                var re = INIhelp.GetValue(fenBuShiPath, "Config", Key);
                DateTime.TryParse(re, out FF);

                //小于3分钟
                if ((DateTime.Now - FF).TotalSeconds > 30)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }
        private void timer4_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(fenBuShiPath))
                {
                    FileStream fs = File.Create(fenBuShiPath);
                    fs.Close();
                }

                Dictionary<string, Label> flush = new Dictionary<string, Label>();

                flush.Add("192.168.100.101-A", labFenBuShi1A);
                flush.Add("192.168.100.101-B", labFenBuShi1B);
                flush.Add("192.168.100.102-A", labFenBuShi2A);
                flush.Add("192.168.100.102-B", labFenBuShi2B);
                flush.Add("192.168.100.103-A", labFenBuShi3A);
                flush.Add("192.168.100.103-B", labFenBuShi3B);
                flush.Add("192.168.100.104-A", labFenBuShi4A);
                flush.Add("192.168.100.104-B", labFenBuShi4B);

                foreach (var item in flush)
                {
                    if (IsOverTime(item.Key))
                    {
                        item.Value.BackColor = Color.Red;
                    }
                    else
                    {
                        item.Value.BackColor = Color.Green;
                    }
                }
            }
            catch
            {
                ;
            }
        }


        #region Docker按钮
        private void buttonMinWindows_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了最小化按钮");
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了权限管理按钮");
            _ProxyData.LoginUI.StartPosition = FormStartPosition.CenterScreen;
            _ProxyData.LoginUI.Activate();
            _ProxyData.LoginUI.ShowDialog();
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了参数管理按钮");
            if (_ProxyData.ProjectConfigUI == null)
            {
                throw new Exception("项目配置文件管理模块未生成管理UI");
            }
            _ProxyData.ProjectConfigUI.StartPosition = FormStartPosition.CenterScreen;
            _ProxyData.ProjectConfigUI.Activate();
            _ProxyData.ProjectConfigUI.Show();
        }

        private void buttonTool_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了工具管理按钮");
            _ProxyData.ToolConfigUI.StartPosition = FormStartPosition.CenterScreen;
            _ProxyData.ToolConfigUI.Activate();
            _ProxyData.ToolConfigUI.Show();
        }

        private void buttonProcess_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了任务管理按钮");
            _ProcessUI.StartPosition = FormStartPosition.CenterScreen;
            _ProcessUI.Activate();
            _ProcessUI.Show();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了退出程序按钮");
            var re = MessageBox.Show("是否退出程序", "提示", MessageBoxButtons.OKCancel);

            if (re == DialogResult.OK)
            {
                Logger.Info("您点击了退出程序-确定按钮");
                this.panel3.Controls.Clear();
                this.Close();
                _ProxyData.ProjectConfigUI.Close();
            }
            else
            {
                Logger.Info("您点击了退出程序-取消按钮");
                ;
            }
        }
        #endregion


        #region ☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆提示信息
        private void btnClose_MouseEnter(object sender, EventArgs e)
        {
            //toolTip1.Show("退出程序", this.buttonProcess);
        }

        private void buttonProcess_MouseEnter(object sender, EventArgs e)
        {
            //toolTip1.Show("任务管理", this.buttonProcess);
        }

        private void btnLogin_MouseEnter(object sender, EventArgs e)
        {
            //toolTip1.Show("权限管理", this.buttonProcess);
        }

        private void buttonTool_MouseEnter(object sender, EventArgs e)
        {
            //toolTip1.Show("工具管理", this.buttonProcess);
        }

        private void btnConfig_MouseEnter(object sender, EventArgs e)
        {
            //toolTip1.Show("参数管理", this.buttonProcess);
        }

        private void buttonProcess_MouseLeave(object sender, EventArgs e)
        {
            //toolTip1.Hide(this.buttonProcess);
        }
        #endregion


        #region 侧边栏按钮

        private void Reset()
        {
            try
            {
                Logger.ShowForm("Proxy初始化", FormMode.ProcessBar, "数据采集软件正在复位，请耐心等待");

                _ProxyData.State = ProjectSate.NA;

                foreach (var item in _ProcessUI.ProcessList)
                {
                    item.Value.Terminate();
                    Logger.OK(item.Key + "任务 终止成功（界面发起）");
                }

                _ProxyData.Init();
                Logger.Info("公共数据区域复位成功（界面发起）");

                foreach (var item in _ProcessUI.ProcessList)
                {
                    item.Value.Init();
                    Logger.OK(item.Key + "任务 初始化成功（界面发起）");
                    item.Value.StartStep = 0;
                }

                _ProxyData.State = ProjectSate.Init;

                foreach (var item in _ProcessUI.ProcessList)
                {
                    item.Value.Start();
                }
                _ProxyData.State = ProjectSate.Auto_Start;

                Logger.OK("系统复位成功（界面发起）");

                Logger.CloseForm("Proxy初始化");
                Logger.ShowForm("系统复位成功", FormMode.TickTipsForm, "3D数据采集端-系统复位成功", "", 3);
            }
            catch (Exception ex)
            {
                Logger.CloseForm("Proxy初始化");
                Logger.Error("3D数据采集端-系统复位失败:" + ex.Message);
                Logger.ShowForm("系统复位失败", FormMode.TickTipsForm, "3D数据采集端-系统复位失败:" + ex.Message);
            }

        }
        private void buttonReset_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了复位按钮");
            Action ac = () =>
            {
                Reset();
            };

            ac.BeginInvoke(new AsyncCallback((ar) =>
            {
                if (_ProxyData.State == ProjectSate.Auto_Start)
                {
                    Logger.OK("系统复位成功（界面发起）");
                }
                else
                {
                    Logger.OK("系统复位失败（界面发起）");
                }
            }), null);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Action ac = () =>
            {


            };
            ac.BeginInvoke(new AsyncCallback((ar) =>
            {
                if (_ProxyData.State == ProjectSate.Auto_Start)
                {
                    Logger.OK("系统启动成功（界面发起）");
                }
                else
                {
                    Logger.OK("系统启动失败（界面发起）");
                }
            }), null);

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            Action ac = () =>
            {
                foreach (var item in _ProcessUI.ProcessList)
                {
                    item.Value.Terminate();
                    Logger.OK(item.Key + "任务 暂停成功（界面发起）");
                }

                _ProxyData.State = ProjectSate.Auto_Stop;
            };
            ac.BeginInvoke(new AsyncCallback((ar) =>
            {
                if (_ProxyData.State == ProjectSate.Auto_Stop)
                {
                    Logger.OK("系统停止成功（界面发起）");
                }
                else
                {
                    Logger.OK("系统停止失败（界面发起）");
                }
            }), null);
        }

        private void buttonManual_Click(object sender, EventArgs e)
        {
            Action ac = () =>
            {
                foreach (var item in _ProcessUI.ProcessList)
                {
                    item.Value.Terminate();
                    Logger.Info(item.Key + "任务 终止成功（界面发起）");
                }
                _ProxyData.State = ProjectSate.Manual;
            };
            ac.BeginInvoke(new AsyncCallback((ar) =>
            {
                if (_ProxyData.State == ProjectSate.Manual)
                {
                    Logger.Info("系统进入手动模式成功（界面发起）");
                }
                else
                {
                    Logger.Error("系统进入手动模式失败（界面发起）");
                }
            }), null);
        }

        private void buttonMachineCheck_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了待机按钮");
            string file = PathManager.ConfigPath + @"\Bin\DeviceMaintainance.exe";
            System.Diagnostics.Process m_Process = System.Diagnostics.Process.Start(file);
            if (m_Process != null)
            {
                m_Process.WaitForExit();
            }
        }

        private void buttonDebug_Click(object sender, EventArgs e)
        {
            Logger.Info("您点击了调试按钮");
            if (null == _ManualUI)
            {
                _ManualUI = _ProxyData.ManualUI;
            }

            _ManualUI.StartPosition = FormStartPosition.CenterScreen;
            _ManualUI.Activate();
            _ManualUI.Show();
        }

        #endregion

        #region 12/20 Victor新增
        private void labelVersion_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string VersionPath = Application.StartupPath + @"\UpdateLog\UpdateLog.log";
                System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo();
                ps.FileName = VersionPath;
                System.Diagnostics.Process.Start(ps);
            }
            catch { }
        }
        #endregion
    }
}
