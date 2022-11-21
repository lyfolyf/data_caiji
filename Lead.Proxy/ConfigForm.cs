
using Lead.Tool.Focal;
using Lead.Tool.Log;
using Lead.Tool.ProjectPath;
using Lead.Tool.XML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Lead.Tool.Login;

namespace Lead.Proxy
{
    enum CopyList
    {
        节点参数 = 0,
        触发参数,
    }

    public delegate void UpdateOffsetUI();


    public partial class ConfigForm : Form
    {
        ConfigParam _Config = null;
        string _ConfigPathAll = "";
        ProxyData _Proxy = null;
        int _CurrentIndex = 0;
        int _CurrentIndex_点检 = 0;
        bool _isFirst = true;


        public ConfigForm(ProxyData proxy, string PathAll)
        {
            InitializeComponent();

            _Proxy = proxy;
            _Config = proxy.ProjectConfig;

            _ConfigPathAll = PathAll;

            #region ///当前产品
            this.comboBoxProdution.Text = _Config.ProdutionId.ToString();
            #endregion
            
            if (null == _Config.DataSaveParam)
            {
                _Config.DataSaveParam = new DataSave();
            }

            if (null == _Config.ProdutionList)
            {
                _Config.ProdutionList = new List<ProdutionFAI>();
            }

            if (null == _Config.Foacl_横)
            {
                _Config.Foacl_横 = new List<FocalConfigDiffProdution>();
            }
            for (int index = _Config.Foacl_横.Count; index < 30; index++)
            {
                _Config.Foacl_横.Add(new FocalConfigDiffProdution()
                {
                    Gain = 2,
                    Freq = 3000,
                    LedPulseWidth = 300,
                    PeakThreshold = 12,
                    PeakXFilter = 7,
                    FillGapXmax = 50,
                    MedianZFilterSize = 0,
                    AutoHeightZeroPosition = false,
                });
            }


            if (null == _Config.Foacl_竖)
            {
                _Config.Foacl_竖 = new List<Tool.Focal.FocalConfigDiffProdution>();
            }
            for (int index = _Config.Foacl_竖.Count; index < 30; index++)
            {
                _Config.Foacl_竖.Add(new FocalConfigDiffProdution()
                {
                    Gain = 2.2,
                    Freq = 3000,
                    LedPulseWidth = 300,
                    PeakThreshold = 10,
                    PeakXFilter = 7,
                    FillGapXmax = 0,
                    MedianZFilterSize = 0,
                    AutoHeightZeroPosition = false,
                });
            }
            if (null == _Config.list极差1)
            {
                _Config.list极差1 = new List<极差测量项>();
            }
            for (int index = _Config.list极差1.Count; index < 30; index++)
            {
                _Config.list极差1.Add(new 极差测量项()
                {
                    是否输出 = false,
                    测量项名称 = "极差1",
                    标准值 = 0,
                    极差上限 = 0.1,
                    极差下限 = 0,
                    是否判定 = false,
                    str = "EHK1LMax,EHK2LMax,EHK3LMax,EHK4LMax,EHK5LMax,EHK6LMax,EHK7LMax"
                });
            }
            if (null == _Config.list极差2)
            {
                _Config.list极差2 = new List<极差测量项>();
            }
            for (int index = _Config.list极差2.Count; index < 30; index++)
            {
                _Config.list极差2.Add(new 极差测量项()
                {
                    是否输出 = false,
                    测量项名称 = "极差2",
                    标准值 = 0,
                    极差上限 = 0.1,
                    极差下限 = 0,
                    是否判定 = false,
                    str = "EHK1RMax,EHK2RMax,EHK3RMax,EHK4RMax,EHK5RMax,EHK6RMax,EHK7RMax"
                });
            }

            for (int index = _Config.ProdutionList.Count; index < 30; index++)
            {
                _Config.ProdutionList.Add(new ProdutionFAI() { ProFAI = new List<FAI>(), Id = index });
            }
            for (int index = 0; index < _Config.ProdutionList.Count; index++)
            {
                _Config.ProdutionList[index].Id = index;
                //FAI
                for (int i = _Config.ProdutionList[index].ProFAI.Count; i < 4; i++)
                {
                    _Config.ProdutionList[index].ProFAI.Add(new FAI() { ListFAI = new BindingList<FAIjudge>() });
                }
                //点检
                for (int i = _Config.ProdutionList[index].点检.Count; i < 20; i++)
                {
                    _Config.ProdutionList[index].点检.Add(new FAI() { ListFAI = new BindingList<FAIjudge>() });
                }
            }
            for (int index = 0; index < _Config.ProdutionList.Count; index++)
            {
                for (int i = 0; i < _Config.ProdutionList[index].ProFAI.Count; i++)
                {
                    var STR = i == 0 ? PartEnum.S1_A_L.ToString() :
                        i == 1 ? PartEnum.S1_A_R.ToString() :
                        i == 2 ? PartEnum.S1_B_L.ToString() : PartEnum.S1_B_R.ToString();

                    _Config.ProdutionList[index].ProFAI[i].Part = STR;
                }
            }

            this.comboBox当前穴位.Items.Add(PartEnum.S1_A_L.ToString());
            this.comboBox当前穴位.Items.Add(PartEnum.S1_A_R.ToString());
            this.comboBox当前穴位.Items.Add(PartEnum.S1_B_L.ToString());
            this.comboBox当前穴位.Items.Add(PartEnum.S1_B_R.ToString());
            //点检
            for (int i = 0; i < _Config.ProdutionList[_Config.ProdutionId].点检.Count; i++)
            {
                this.comboBox点检产品.Items.Add(i);
            }

            this.propertyGrid1.SelectedObject = _Config.DataSaveParam;
            propertyGrid极差1.SelectedObject = _Config.list极差1[_Config.ProdutionId];
            propertyGrid极差2.SelectedObject = _Config.list极差2[_Config.ProdutionId];


            string[] HerdText = new string[] { };
            //开关设置
            _Config.Func = _Config.Func == null ? new BindingList<FuncEnable>() : _Config.Func;
            this.dataGridViewFuncEnable.DataSource = _Config.Func;
            HerdText = new string[] { "名称", "使能", "备注" };
            for (int i = 0; i < HerdText.Length; i++)
            {
                this.dataGridViewFuncEnable.Columns[i].HeaderText = HerdText[i];
            }
            this.dataGridViewFuncEnable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            //延时设置
            _Config.Delay = _Config.Delay == null ? new BindingList<DelayTime>() : _Config.Delay;
            this.dataGridViewOverTime.DataSource = _Config.Delay;
            HerdText = new string[] { "名称", "时长", "备注" };
            for (int i = 0; i < HerdText.Length; i++)
            {
                this.dataGridViewOverTime.Columns[i].HeaderText = HerdText[i];
            }
            this.dataGridViewOverTime.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            #region FAI-当前穴位-新增数据
            comboBoxSource_StationID.Items.Add(StationEnum.S1_A);
            comboBoxSource_StationID.Items.Add(StationEnum.S1_B);
            comboBoxAim_StationID.Items.Add(StationEnum.S1_A);
            comboBoxAim_StationID.Items.Add(StationEnum.S1_B);
            #endregion
            
            //产品设置
            for (int i = 0; i < _Config.ProdutionList.Count; i++)
            {
                comboBoxProdution.Items.Add(i);
            }
            HerdText = new string[] { };
            //产品列表            
            this.dataGridViewListProdution.DataSource = _Config.ProdutionList;
            HerdText = new string[] { "产品ID", "产品名称", "备注" };
            for (int i = 0; i < HerdText.Length; i++)
            {
                this.dataGridViewListProdution.Columns[i].HeaderText = HerdText[i];
            }
            this.dataGridViewListProdution.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            //hotfset
            foreach (var item in _Config.HotOffsetConfig)
            {
                if (!this.comboBox传感器选择.Items.Contains(item.Name))
                {
                    this.comboBox传感器选择.Items.Add(item.Name);
                }
            }
            this.comboBox工站选择.Items.Add(StationEnum.S1_A);
            this.comboBox工站选择.Items.Add(StationEnum.S1_B);
            this.comboBox工站调加.Items.Add(StationEnum.S1_A);
            this.comboBox工站调加.Items.Add(StationEnum.S1_B);


            #region 自动点检

            _Config.公差管理 = _Config.公差管理 ?? new BindingList<自动点检公差>();
            this.dgvGongCha.DataSource = _Config.公差管理;
            
            string []HerdTextGongCha = new string[] { };
            HerdTextGongCha = new string[] { "名称", "上公差", "下公差" };
            for (int i = 0; i < HerdTextGongCha.Length; i++)
            {
                this.dgvGongCha.Columns[i].HeaderText = HerdTextGongCha[i];
            }

            this.dgvGongCha.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView_自动真值列表.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView_自动真值详细表.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            #endregion


            #region 数据导出
            _Config.listDataReportManage = _Config.listDataReportManage == null ? new BindingList<DataReport>() : _Config.listDataReportManage;
            this.dgvDataExport.DataSource = _Config.listDataReportManage;
            HerdText = new string[] {"选项","值" };
            for (int i = 0; i < HerdText.Length; i++)
            {
                this.dgvDataExport.Columns[i].HeaderText = HerdText[i];
            }
            this.dgvDataExport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            #endregion

            //Fai
            richTextBox1.Text = _Config.FaiKey;
            richTextBox2.Text = _Config.FaiKey;

            UpdateStationID(_Config.ProdutionId);
        }


        private void ConfigForm_Load(object sender, EventArgs e)
        {
            this.tabControl1.Controls["FAI"].Visible = false;
        }


        private void ConfigForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            label产品名.Text = _Config.ProdutionList[_Config.ProdutionId].Name;

            foreach (var item in _Config.HotOffsetConfig)
            {
                if (!this.comboBox传感器选择.Items.Contains(item.Name))
                {
                    this.comboBox传感器选择.Items.Add(item.Name);
                }
            }

            #region 权限设置
            switch (_Proxy.LoginUI._level)
            {
                case Level.NA:
                case Level.普通用户:
                    for (int i = 0; i < 产品列表.Controls.Count; i++)
                    {
                        产品列表.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 功能开关.Controls.Count; i++)
                    {
                        功能开关.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 超时时间.Controls.Count; i++)
                    {
                        超时时间.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 数据相关.Controls.Count; i++)
                    {
                        数据相关.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 温漂.Controls.Count; i++)
                    {
                        温漂.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < FAI.Controls.Count; i++)
                    {
                        FAI.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 点检.Controls.Count; i++)
                    {
                        点检.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < Focal_横参数.Controls.Count; i++)
                    {
                        Focal_横参数.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < Focal_竖参数.Controls.Count; i++)
                    {
                        Focal_竖参数.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 极差测量项.Controls.Count; i++)
                    {
                        极差测量项.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 数据导出.Controls.Count; i++)
                    {
                        数据导出.Controls[i].Enabled = false;
                    }
                    break;
                case Level.工艺员:
                    for (int i = 0; i < 产品列表.Controls.Count; i++)
                    {
                        产品列表.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 功能开关.Controls.Count; i++)
                    {
                        功能开关.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 超时时间.Controls.Count; i++)
                    {
                        超时时间.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 数据相关.Controls.Count; i++)
                    {
                        数据相关.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 温漂.Controls.Count; i++)
                    {
                        温漂.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < FAI.Controls.Count; i++)
                    {
                        FAI.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 点检.Controls.Count; i++)
                    {
                        点检.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < Focal_横参数.Controls.Count; i++)
                    {
                        Focal_横参数.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < Focal_竖参数.Controls.Count; i++)
                    {
                        Focal_竖参数.Controls[i].Enabled = false;
                    }
                    for (int i = 0; i < 极差测量项.Controls.Count; i++)
                    {
                        极差测量项.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 数据导出.Controls.Count; i++)
                    {
                        数据导出.Controls[i].Enabled = true;
                    }
                    break;
                case Level.管理员:
                    for (int i = 0; i < 产品列表.Controls.Count; i++)
                    {
                        产品列表.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 功能开关.Controls.Count; i++)
                    {
                        功能开关.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 超时时间.Controls.Count; i++)
                    {
                        超时时间.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 数据相关.Controls.Count; i++)
                    {
                        数据相关.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 温漂.Controls.Count; i++)
                    {
                        温漂.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < FAI.Controls.Count; i++)
                    {
                        FAI.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 点检.Controls.Count; i++)
                    {
                        点检.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < Focal_横参数.Controls.Count; i++)
                    {
                        Focal_横参数.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < Focal_竖参数.Controls.Count; i++)
                    {
                        Focal_竖参数.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 极差测量项.Controls.Count; i++)
                    {
                        极差测量项.Controls[i].Enabled = true;
                    }
                    for (int i = 0; i < 数据导出.Controls.Count; i++)
                    {
                        数据导出.Controls[i].Enabled = true;
                    }
                    break;
                default:
                    break;
            }
            #endregion
        }


        #region ~~~~~~~~~~~~~~~~保存所有配置~~~~~~~~~~~~~~~~
        private void buttonSaveAll_Click(object sender, EventArgs e)
        {
            var Re = MessageBox.Show("是否保存", "提示", MessageBoxButtons.OKCancel);

            if (Re == DialogResult.OK)
            {
                XmlSerializerHelper.WriteXML(_Config, _ConfigPathAll, typeof(ConfigParam));
                Logger.ShowTickTipsForm("保存项目配置成功", "保存项目配置成功", 1);

                if ((DateTime.Now - _Config.LastSaveTime).TotalHours > 1)
                {
                    if (!Directory.Exists(@"D:\3D数据采集配置文件备份\"))
                    {
                        Directory.CreateDirectory(@"D:\3D数据采集配置文件备份\");
                    }
                    _Config.LastSaveTime = DateTime.Now;

                    var FileBack = @"D:\3D数据采集配置文件备份\" + DateTime.Now.ToString("yyyy-MM-hh_HH-mm-ss") + "_ProjectConfig.xml";
                    FileStream fs = new FileStream(FileBack, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    fs.Close();

                    XmlSerializerHelper.WriteXML(_Config, FileBack, typeof(ConfigParam));
                }
            }
        }
        #endregion


        #region ~~~~~~~~~~~~~~~~更换当前产品~~~~~~~~~~~~~~~~
        private void UpdateStationID(int Id)
        {

            this.dataGridViewFAI_1.DataSource = _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI;

            if (_isFirst)
            {
                var HerdText = new string[] { "名称", "测量结果", "标准值", "上公差", "下公差", "补偿系数", "补偿值", "是否判定", "判定结果" };
                for (int i = 0; i < HerdText.Length; i++)
                {
                    this.dataGridViewFAI_1.Columns[i].HeaderText = HerdText[i];
                }
                this.dataGridViewFAI_1.Columns["补偿系数"].Visible = false;
                this.dataGridViewFAI_1.Columns["补偿值"].Visible = false;
                this.dataGridViewFAI_1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            }

            this.dataGridView点检.DataSource = _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex].ListFAI;

            this.propertyGrid横.SelectedObject = _Config.Foacl_横[_Config.ProdutionId];
            this.propertyGrid竖.SelectedObject = _Config.Foacl_竖[_Config.ProdutionId];
            this.propertyGrid极差1.SelectedObject = _Config.list极差1[_Config.ProdutionId];
            this.propertyGrid极差2.SelectedObject = _Config.list极差2[_Config.ProdutionId];

            this.dataGridView_自动真值列表.DataSource = _Config.ProdutionList[_Config.ProdutionId].点检_自动;
            var HerdText111 = new string[] { "穴位", "产品ID" };
            for (int i = 0; i < HerdText111.Length; i++)
            {
                this.dataGridView_自动真值列表.Columns[i].HeaderText = HerdText111[i];
            }
            this.dataGridView_自动真值详细表.DataSource = null;
            this.labelID.Text = "";
            this.label穴位.Text = "";
        }

        private void comboBoxProdution_SelectedIndexChanged(object sender, EventArgs e)
        {
            _Config.ProdutionId = Convert.ToInt32(comboBoxProdution.Text);
            UpdateStationID(Convert.ToInt32(comboBoxProdution.Text));
        }
        #endregion

        #region ~~~~~~~~~~~~~~~~产品列表~~~~~~~~~~~~~~~~
        private void buttonCopyParam_Click(object sender, EventArgs e)
        {
            var Re = MessageBox.Show("是否确认复制参数？", "提示", MessageBoxButtons.OKCancel);
            if (Re == DialogResult.Cancel)
            {
                return;
            }

            if (comboBoxSource_ProdutinID.Text == "" ||
                comboBoxAim_ProdutinID.Text == "" ||
                comboBoxSource_StationID.Text == "" ||
                comboBoxAim_StationID.Text == "")
            {
                MessageBox.Show("请选择源和目标ID！！！");
                return;
            }

            int Source_ProdutionId = Convert.ToInt32(comboBoxSource_ProdutinID.Text.Trim());
            int Aim_ProdutionId = Convert.ToInt32(comboBoxAim_ProdutinID.Text.Trim());
            int Source_StationId = comboBoxSource_StationID.Text == StationEnum.S1_A.ToString() ? (int)StationEnum.S1_A :
                comboBoxSource_StationID.Text == StationEnum.S1_B.ToString() ? (int)StationEnum.S1_B : (int)StationEnum.S2;
            int Aim_StationId = comboBoxAim_StationID.Text == StationEnum.S1_A.ToString() ? (int)StationEnum.S1_A :
                comboBoxAim_StationID.Text == StationEnum.S1_B.ToString() ? (int)StationEnum.S1_B : (int)StationEnum.S2;

            bool IsStationPos = this.checkedListBox1.GetItemChecked((int)CopyList.节点参数);
            bool IsTrigParam = this.checkedListBox1.GetItemChecked((int)CopyList.触发参数);

            Logger.ShowTickTipsForm("复制项目配置成功", "复制项目配置成功", 2);
        }
        #endregion


        #region ~~~~~~~~~~~~~~~~功能开关~~~~~~~~~~~~~~~~
        private void buttonAddFunc_Click(object sender, EventArgs e)
        {
            if (_Config.Func.Count == 0)
            {
                _Config.Func.Insert(0, new FuncEnable());
                return;
            }
            _Config.Func.Insert(dataGridViewFuncEnable.CurrentCell.RowIndex + 1, new FuncEnable());
        }


        private void buttonDeleteFunc_Click(object sender, EventArgs e)
        {
            _Config.Func.RemoveAt(dataGridViewFuncEnable.CurrentCell.RowIndex);
        }


        private void button一键量产配置_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in _Config.Func)
                {
                    if (item.Name == "纵向保存")
                    {
                        item.Enable = false;
                    }
                }
                XmlSerializerHelper.WriteXML(_Config, _ConfigPathAll, typeof(ConfigParam));
                Logger.Info("一键生产配置修改成功,保存项目配置成功");
                Logger.ShowTickTipsForm("一键生产配置修改成功", "一键生产配置修改成功", 1);
            }
            catch (Exception ex)
            {
                Logger.ShowTickTipsForm("一键生产配置修改失败", "一键生产配置修改失败：" + ex.Message, 1);
            }
        }
        #endregion


        #region ~~~~~~~~~~~~~~~~超时时间~~~~~~~~~~~~~~~~
        private void buttonAddDelay_Click(object sender, EventArgs e)
        {
            if (_Config.Delay.Count == 0)
            {
                _Config.Delay.Insert(0, new DelayTime());
                return;
            }
            _Config.Delay.Insert(dataGridViewOverTime.CurrentCell.RowIndex, new DelayTime());
        }

        private void buttonDeleteDelay_Click(object sender, EventArgs e)
        {
            _Config.Delay.RemoveAt(dataGridViewOverTime.CurrentCell.RowIndex);
        }
        #endregion


        #region ~~~~~~~~~~~~~~~~温漂~~~~~~~~~~~~~~~~
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxSenserName.Text == "")
                {
                    throw new Exception("传感器名称不能为空！");
                }

                StationEnum station = comboBox工站调加.Text == StationEnum.S1_A.ToString() ? StationEnum.S1_A :
                    comboBox工站调加.Text == StationEnum.S1_B.ToString() ? StationEnum.S1_B : StationEnum.Min;
                if (station == StationEnum.Min)
                {
                    throw new Exception("请选择工站");
                }

                foreach (var item in _Config.HotOffsetConfig)
                {
                    if (item.Name == textBoxSenserName.Text && station == item.Station)
                    {
                        throw new Exception("已存在相同的传感器，请确定后再添加！");
                    }
                }

                _Config.HotOffsetConfig.Add(new HotOffsetType()
                {
                    Station = station,
                    Name = textBoxSenserName.Text,
                });

                if (!this.comboBox传感器选择.Items.Contains(textBoxSenserName.Text))
                {
                    this.comboBox传感器选择.Items.Add(textBoxSenserName.Text);
                }

                Logger.ShowTickTipsForm("添加温漂传感器", station.ToString() + ":" + textBoxSenserName.Text + "  添加成功!", 1);
            }
            catch (Exception EX)
            {
                MessageBox.Show("添加失败：" + EX.Message);
            }
        }

        private void comboBox工站选择_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var 温漂工站 = comboBox工站选择.Text == StationEnum.S1_A.ToString() ? StationEnum.S1_A :
                    comboBox工站选择.Text == StationEnum.S1_B.ToString() ? StationEnum.S1_B : StationEnum.Min;
                if (温漂工站 == StationEnum.Min)
                {
                    throw new Exception("请选择工站");
                }


                var 温漂传感器 = comboBox传感器选择.Text;

                foreach (var item in _Config.HotOffsetConfig)
                {
                    if (item.Name == 温漂传感器 && item.Station == 温漂工站)
                    {
                        this.propertyGrid2.SelectedObject = item;
                        return;
                    }
                }
            }
            catch (Exception EX)
            {
                MessageBox.Show("工站失败：" + EX.Message);
            }
        }

        private void comboBox传感器选择_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                StationEnum 温漂工站 = StationEnum.Min;
                string 温漂传感器 = "";

                温漂工站 = comboBox工站选择.Text == StationEnum.S1_A.ToString() ? StationEnum.S1_A :
                comboBox工站选择.Text == StationEnum.S1_B.ToString() ? StationEnum.S1_B : StationEnum.Min;
                if (温漂工站 == StationEnum.Min)
                {
                    throw new Exception("请选择工站");
                }

                温漂传感器 = comboBox传感器选择.Text;

                foreach (var item in _Config.HotOffsetConfig)
                {
                    if (item.Name == 温漂传感器 && item.Station == 温漂工站)
                    {
                        this.propertyGrid2.SelectedObject = item;
                        return;
                    }
                }

            }
            catch (Exception EX)
            {
                MessageBox.Show("工站失败：" + EX.Message);
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            _Config.FaiKey = richTextBox1.Text;
        }
        #endregion


        #region ~~~~~~~~~~~~~~~~FAI~~~~~~~~~~~~~~~~
        private void button导入Fai_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox当前穴位.Text == "")
                {
                    MessageBox.Show("未选择穴位");
                    return;
                }

                openFileDialog1.Filter = ".csv|*csv";
                var show = openFileDialog1.ShowDialog();
                if (show == DialogResult.Cancel)
                {
                    return;
                }

                int index = 0;

                FileStream fs = null;
                StreamReader sr = null;
                fs = new FileStream(openFileDialog1.FileName, System.IO.FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                sr = new StreamReader(fs, Encoding.UTF8);
                var re = sr.ReadLine();//忽略第一行

                BindingList<FAIjudge> temp = new BindingList<FAIjudge>();
                while (re != null)
                {
                    re = sr.ReadLine();
                    if (re != null)
                    {
                        index++;
                        var sp = re.Split(',');
                        temp.Add(new FAIjudge()
                        {
                            名称 = sp[0],
                            测量结果 = Convert.ToDouble(sp[1]),
                            标准值 = Convert.ToDouble(sp[2]),
                            上公差 = Convert.ToDouble(sp[3]),
                            下公差 = Convert.ToDouble(sp[4]),
                            补偿系数 = Convert.ToDouble(sp[5]),
                            补偿值 = Convert.ToDouble(sp[6]),
                            是否判定 = Convert.ToBoolean(sp[7]),
                            判定结果 = sp[8],
                        });
                    }
                }
                sr.Close();
                fs.Close();

                _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Clear();
                foreach (var item in temp)
                {
                    _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Add(new FAIjudge()
                    {
                        名称 = item.名称,
                        测量结果 = item.测量结果,
                        标准值 = item.标准值,
                        上公差 = item.上公差,
                        下公差 = item.下公差,
                        补偿系数 = item.补偿系数,
                        补偿值 = item.补偿值,
                        是否判定 = item.是否判定,
                        判定结果 = item.判定结果,
                    });
                }

                Logger.ShowTickTipsForm("FAI CSV导入成功", "点检CSV导入成功，共" + index, 1);
            }
            catch (Exception ex)
            {
                Logger.ShowTickTipsForm("FAI CSV导入失败", "点检CSV导入失败:" + ex.Message);
            }

        }


        private void button到处FAI_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox当前穴位.Text == "")
                {
                    MessageBox.Show("未选择穴位");
                    return;
                }

                saveFileDialog1.Filter = ".csv|*csv";
                var show = saveFileDialog1.ShowDialog();

                if (show == DialogResult.Cancel)
                {
                    return;
                }

                int index = 0;
                FileStream fs = null;
                StreamWriter sw = null;
                fs = new FileStream(saveFileDialog1.FileName, System.IO.FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                sw = new StreamWriter(fs, Encoding.Default);
                StringBuilder text0 = new StringBuilder();
                text0.Clear();
                text0.Append("名称" + "," + "测量结果" + "," + "标准值" + "," + "上公差" + "," + "下公差" + "," +
                    "基准值" + "," + "基准系数" + "," + "是否判定" + "," + "判定结果");
                sw.WriteLine(text0);
                for (int i = 0; i < _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Count; i++)
                {
                    var item = _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI[i];
                    text0.Clear();
                    text0.Append(item.名称 + "," + item.测量结果 + "," + item.标准值 + "," + item.上公差 + "," + item.下公差 + "," +
                        item.补偿值 + "," + item.补偿系数 + "," + item.是否判定 + "," + item.判定结果);
                    sw.WriteLine(text0);
                    index++;
                }
                sw.Close();
                fs.Close();

                Logger.ShowTickTipsForm("FAI CSV导出成功", "点检CSV导出成功，共" + index, 1);
            }
            catch (Exception ex)
            {
                Logger.ShowTickTipsForm("FAI CSV导出失败", "点检CSV导出失败:" + ex.Message);
            }
        }


        private void button增加Fai_Click(object sender, EventArgs e)
        {
            if (comboBox当前穴位.Text == "")
            {
                MessageBox.Show("未选择穴位");
                return;
            }
            if (_Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Count == 0)
            {
                _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Insert(0, new FAIjudge());
                return;
            }
            _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Insert(dataGridViewFAI_1.CurrentCell.RowIndex, new FAIjudge());
        }

        private void button删除Fai_Click(object sender, EventArgs e)
        {
            if (comboBox当前穴位.Text == "")
            {
                MessageBox.Show("未选择穴位");
                return;
            }
            _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.RemoveAt(dataGridViewFAI_1.CurrentCell.RowIndex);
        }

        private void button打开任务文件_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "";
            openFileDialog1.FileName = "LeiSaiConfig";
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBox任务文件夹.Text = openFileDialog1.FileName;
            }
        }

        public BindingList<FAIjudge> ReadTaskXml(string TaskPath)//读取任务默认参数
        {
            string Path = TaskPath;

            XmlDocument doc = new XmlDocument();
            doc.Load(Path);


            var Keys = _Config.FaiKey.Replace("\n", "").Split(';');

            XmlElement root = doc.DocumentElement;
            //XmlNodeList nodesList = root.SelectNodes("/TasksConfigure//primConfigSingle");
            XmlNodeList nodesList = root.SelectNodes("/ArrayOfPrimParBase//PrimParBase");
            BindingList<FAIjudge> _paiList = new BindingList<FAIjudge>();
            foreach (XmlNode node in nodesList)
            {
                foreach (XmlElement xElee in node)
                {
                    bool IsFined = false;
                    foreach (var item in Keys)
                    {
                        if (item != "")
                        {
                            if (xElee.Name == item)
                            {
                                IsFined = true;
                                break;
                            }
                        }
                    }
                    if (IsFined)
                    {
                        FAIjudge tempFai = new FAIjudge();
                        bool isOutput = false;
                        foreach (XmlElement xEleee in xElee)
                        {
                            //判断该Fai是否输出
                            if (xEleee.Name == "isOutput")
                            {
                                if (xEleee.InnerText == "1" || xEleee.InnerText.ToLower() == "true")
                                {
                                    isOutput = true;        //该节点有效
                                }
                            }
                        }
                        if (isOutput)
                        {
                            foreach (XmlElement xEleee in xElee)
                            {
                                if (xEleee.Name == "factor")
                                {
                                    tempFai.补偿系数 = Convert.ToDouble(xEleee.InnerText);
                                }
                                else if (xEleee.Name == "tolUp")
                                {
                                    tempFai.上公差 = Convert.ToDouble(xEleee.InnerText);
                                }
                                else if (xEleee.Name == "tolLow")
                                {
                                    tempFai.下公差 = Convert.ToDouble(xEleee.InnerText);
                                }
                                else if (xEleee.Name == "remark")
                                {
                                    tempFai.名称 = xEleee.InnerText;
                                }
                                else if (xEleee.Name == "offset")
                                {
                                    tempFai.补偿值 = Convert.ToDouble(xEleee.InnerText);
                                }
                                else if (xEleee.Name == "normal")
                                {
                                    tempFai.标准值 = Convert.ToDouble(xEleee.InnerText);
                                }
                            }
                            _paiList.Add(tempFai);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            return _paiList;
        }

        private void buttonParseTestReal_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox任务文件夹.Text == "")
                {
                    MessageBox.Show("未选择文件");
                    return;
                }
                if (comboBox当前穴位.Text == "")
                {
                    MessageBox.Show("未选择穴位");
                    return;
                }

                var x = ReadTaskXml(textBox任务文件夹.Text);

                _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Clear();

                foreach (var item in x)
                {
                    _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("解析失败:" + ex.Message);
            }
        }

        private void comboBox当前穴位_SelectedIndexChanged(object sender, EventArgs e)
        {
            _CurrentIndex = comboBox当前穴位.Text == PartEnum.S1_A_L.ToString() ? (int)PartEnum.S1_A_L :
                comboBox当前穴位.Text == PartEnum.S1_A_R.ToString() ? (int)PartEnum.S1_A_R :
                comboBox当前穴位.Text == PartEnum.S1_B_L.ToString() ? (int)PartEnum.S1_B_L : (int)PartEnum.S1_B_R;

            UpdateStationID(Convert.ToInt32(comboBoxProdution.Text));
        }

        private void button清空表格_Click(object sender, EventArgs e)
        {
            if (comboBox当前穴位.Text == "")
            {
                MessageBox.Show("未选择穴位");
                return;
            }
            _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI.Clear();
        }

        private void checkBox是否判定_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboBox当前穴位.Text == "")
                {
                    MessageBox.Show("未选择穴位");
                    return;
                }
                bool 是否判定 = checkBox是否判定.Checked;
                foreach (var item in _Config.ProdutionList[_Config.ProdutionId].ProFAI[_CurrentIndex].ListFAI)
                {
                    item.是否判定 = 是否判定;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("一键勾选失败");
            }
        }
        #endregion


        #region ~~~~~~~~~~~~~~~~手动点检~~~~~~~~~~~~~~~~
        private void comboBox点检产品_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _CurrentIndex_点检 = Convert.ToInt32(comboBox点检产品.Text);
                this.dataGridView点检.DataSource = _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI;
                this.dataGridView点检.Columns["补偿系数"].Visible = false;
                this.dataGridView点检.Columns["补偿值"].Visible = false;
                this.dataGridView点检.Columns["是否判定"].Visible = false;
                当前点检产品.Text = "当前点检产品\\" + _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Count;
            }
            catch (Exception ex)
            {
                MessageBox.Show("点检切换出错:" + ex.Message);
            }
        }

        private void button点检添加一行_Click(object sender, EventArgs e)
        {
            if (comboBox点检产品.Text == "")
            {
                MessageBox.Show("未选择穴位");
                return;
            }
            if (_Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Count == 0)
            {
                _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Insert(0, new FAIjudge());
                return;
            }
            _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Insert(dataGridView点检.CurrentCell.RowIndex, new FAIjudge());

        }

        private void button点检删除一行_Click(object sender, EventArgs e)
        {
            if (comboBox点检产品.Text == "")
            {
                MessageBox.Show("未选择穴位");
                return;
            }
            _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.RemoveAt(dataGridView点检.CurrentCell.RowIndex);
        }

        private void button点检清空表格_Click(object sender, EventArgs e)
        {
            if (comboBox点检产品.Text == "")
            {
                MessageBox.Show("未选择穴位");
                return;
            }
            _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Clear();
        }

        private void buttonParseFAI点检_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox点检产品.Text == "")
                {
                    MessageBox.Show("未选择穴位");
                    return;
                }
                DialogResult re = openFileDialog1.ShowDialog();

                if (re == DialogResult.OK)
                {
                    var x = ReadTaskXml(openFileDialog1.FileName);

                    _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Clear();

                    foreach (var item in x)
                    {
                        _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Add(item);
                    }

                    MessageBox.Show("解析成功!");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("解析失败:" + ex.Message);
            }
        }

        private void button点检CVS导入_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox点检产品.Text == "")
                {
                    MessageBox.Show("未选择穴位");
                    return;
                }
                openFileDialog1.Filter = ".csv|*csv";
                var show = openFileDialog1.ShowDialog();
                if (show == DialogResult.Cancel)
                {
                    return;
                }

                int index = 0;

                FileStream fs = null;
                StreamReader sr = null;
                fs = new FileStream(openFileDialog1.FileName, System.IO.FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                sr = new StreamReader(fs, Encoding.UTF8);
                var re = sr.ReadLine();//忽略第一行


                BindingList<FAIjudge> temp = new BindingList<FAIjudge>();
                while (re != null)
                {
                    re = sr.ReadLine();
                    if (re != null)
                    {
                        index++;
                        var sp = re.Split(',');
                        temp.Add(new FAIjudge()
                        {
                            名称 = sp[0],
                            测量结果 = Convert.ToDouble(sp[1]),
                            标准值 = Convert.ToDouble(sp[2]),
                            上公差 = Convert.ToDouble(sp[3]),
                            下公差 = Convert.ToDouble(sp[4]),
                            补偿系数 = Convert.ToDouble(sp[5]),
                            补偿值 = Convert.ToDouble(sp[6]),
                            是否判定 = Convert.ToBoolean(sp[7]),
                            判定结果 = sp[8],
                        });
                    }
                }
                sr.Close();
                fs.Close();


                _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Clear();
                foreach (var item in temp)
                {
                    _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Add(new FAIjudge()
                    {
                        名称 = item.名称,
                        测量结果 = item.测量结果,
                        标准值 = item.标准值,
                        上公差 = item.上公差,
                        下公差 = item.下公差,
                        补偿系数 = item.补偿系数,
                        补偿值 = item.补偿值,
                        是否判定 = item.是否判定,
                        判定结果 = item.判定结果,
                    });
                }

                Logger.ShowTickTipsForm("点检CSV导入成功", "点检CSV导入成功，共" + index, 1);
            }
            catch (Exception ex)
            {
                Logger.ShowTickTipsForm("点检CSV导入失败", "点检CSV导入失败:" + ex.Message);
            }

        }

        private void button点检CSV导出_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox点检产品.Text == "")
                {
                    MessageBox.Show("未选择穴位");
                    return;
                }

                saveFileDialog1.Filter = ".csv|*csv";
                var show = saveFileDialog1.ShowDialog();

                if (show == DialogResult.Cancel)
                {
                    return;
                }

                if (comboBox点检产品.Text == "" || comboBox点检产品.Text == null)
                {
                    MessageBox.Show("点检产品未选择");
                }

                int index = 0;
                FileStream fs = null;
                StreamWriter sw = null;
                fs = new FileStream(saveFileDialog1.FileName, System.IO.FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                sw = new StreamWriter(fs, Encoding.Default);
                StringBuilder text0 = new StringBuilder();
                text0.Clear();
                text0.Append("名称" + "," + "测量结果" + "," + "标准值" + "," + "上公差" + "," + "下公差" + "," +
                    "补偿值" + "," + "补偿系数" + "," + "是否判定" + "," + "判定结果");
                sw.WriteLine(text0);
                for (int i = 0; i < _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI.Count; i++)
                {
                    var item = _Config.ProdutionList[_Config.ProdutionId].点检[_CurrentIndex_点检].ListFAI[i];
                    text0.Clear();
                    text0.Append(item.名称 + "," + item.测量结果 + "," + item.标准值 + "," + item.上公差 + "," + item.下公差 + "," +
                        item.补偿值 + "," + item.补偿系数 + "," + item.是否判定 + "," + item.判定结果);
                    sw.WriteLine(text0);
                    index++;
                }
                sw.Close();
                fs.Close();

                Logger.ShowTickTipsForm("点检CSV导出成功", "点检CSV导出成功，共" + index, 1);
            }
            catch (Exception ex)
            {
                Logger.ShowTickTipsForm("点检CSV导出失败", "点检CSV导出失败:" + ex.Message);
            }

        }
        #endregion


        #region ~~~~~~~~~~~~~~~~自动点检~~~~~~~~~~~~~~~~
        Dictionary<string, FAI_点检数据分析> FAI_点检数据 = new Dictionary<string, FAI_点检数据分析>();
        private void button删除当前点检ID_自动_Click(object sender, EventArgs e)
        {
            try
            {
                var index = this.dataGridView_自动真值列表.CurrentCell.RowIndex;
                //this.dataGridView_自动真值列表.DataSource = _Config.ProdutionList[_Config.ProdutionId].点检_自动;
                foreach (var item in _Config.ProdutionList[_Config.ProdutionId].点检_自动)
                {
                    if (item.ID == _Config.ProdutionList[_Config.ProdutionId].点检_自动[index].ID
                        && item.Part == _Config.ProdutionList[_Config.ProdutionId].点检_自动[index].Part)
                    {
                        _Config.ProdutionList[_Config.ProdutionId].点检_自动.Remove(item);
                        break;
                    }
                }
                this.dataGridView_自动真值详细表.DataSource = null;
                this.labelID.Text = "";
                this.label穴位.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除当前ID错误:" + ex.Message);
            }
        }


        #region 插入真值test
        private void button1_Click_1(object sender, EventArgs e)
        {
            FAI ff = new FAI()
            {
                ID = "1",
                Part = "S1_A_L",
                ListFAI = new BindingList<FAIjudge>()
            };

            ff.ListFAI.Add(new FAIjudge()
            {
                名称 = "1",
                上公差 = 0.1,

            });
            ff.ListFAI.Add(new FAIjudge()
            {
                名称 = "2",
                上公差 = 0.2,

            });
            _Config.ProdutionList[_Config.ProdutionId].点检_自动.Add(ff);

            this.dataGridView_自动真值列表.DataSource = _Config.ProdutionList[_Config.ProdutionId].点检_自动;
        }
        #endregion


        private void dataGridView_自动真值列表_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var i = this.dataGridView_自动真值列表.CurrentCell.RowIndex;
                if (i < _Config.ProdutionList[_Config.ProdutionId].点检_自动.Count)
                {
                    this.labelID.Text = _Config.ProdutionList[_Config.ProdutionId].点检_自动[i].ID;
                    this.label穴位.Text = _Config.ProdutionList[_Config.ProdutionId].点检_自动[i].Part;
                    this.dataGridView_自动真值详细表.DataSource = _Config.ProdutionList[_Config.ProdutionId].点检_自动[i].ListFAI;
                    this.dataGridView_自动真值详细表.Columns["补偿系数"].Visible = false;
                    this.dataGridView_自动真值详细表.Columns["补偿值"].Visible = false;
                    this.dataGridView_自动真值详细表.Columns["上公差"].Visible = false;
                    this.dataGridView_自动真值详细表.Columns["下公差"].Visible = false;
                    this.dataGridView_自动真值详细表.Columns["判定结果"].Visible = false;
                    this.dataGridView_自动真值详细表.Columns["测量结果"].Visible = false;
                    this.dataGridView_自动真值详细表.Columns["是否判定"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("显示详细点检信息出错:" + ex.Message);
            }
        }


        private void button录入A侧点检真值_Click(object sender, EventArgs e)
        {
            try
            {
                var re = MessageBox.Show("是否录入A侧点检真值？", "提示", MessageBoxButtons.OKCancel);
                if (re == DialogResult.Cancel)
                {
                    return;
                }

                Dictionary<string, List<PartResult>> 点检真值记录表_All = new Dictionary<string, List<PartResult>>();
                Dictionary<string, PartResult> 点检真值记录表_Ave = new Dictionary<string, PartResult>();
                点检真值记录表_All.Clear();
                点检真值记录表_Ave.Clear();

                
                foreach (var item in _Proxy.点检真值记录表)
                {
                    if (item.Part != PartEnum.S1_A_L && item.Part != PartEnum.S1_A_R)
                    {
                        continue;
                    }
                    if (!点检真值记录表_All.ContainsKey(item.ID))
                    {
                        点检真值记录表_All.Add(item.ID, new List<PartResult>());
                    }
                    点检真值记录表_All[item.ID].Add(item);
                }


                //求平均值
                foreach (var item in 点检真值记录表_All)
                {
                    if (!点检真值记录表_Ave.ContainsKey(item.Key))
                    {
                        点检真值记录表_Ave.Add(item.Key, new PartResult() { FaiInfos = new List<FAIjudge>() });
                    }
                    foreach (var pro in item.Value)
                    {
                        点检真值记录表_Ave[item.Key].Part = pro.Part;
                        foreach (var fai in pro.FaiInfos)
                        {
                            var findFai = 点检真值记录表_Ave[item.Key].FaiInfos.Find((xxx) => xxx.名称 == fai.名称);
                            if (findFai == null)
                            {
                                点检真值记录表_Ave[item.Key].FaiInfos.Add(new FAIjudge() { 名称 = fai.名称 });
                            }

                            //累加
                            findFai = 点检真值记录表_Ave[item.Key].FaiInfos.Find((xxx) => xxx.名称 == fai.名称);
                            findFai.测量结果 += fai.测量结果;
                            findFai.补偿系数++;//当计数器使用
                        }
                    }
                }
                
                //更新标准值
                foreach (var item in 点检真值记录表_Ave)
                {
                    //1
                    var find = _Config.ProdutionList[_Config.ProdutionId].点检_自动.ToList().Find((xxx) => xxx.ID == item.Key && xxx.Part == item.Value.Part.ToString());
                    if (find == null)
                    {
                        _Config.ProdutionList[_Config.ProdutionId].点检_自动.Add(new Proxy.FAI()
                        {
                            ID = item.Key,
                            Part = item.Value.Part.ToString(),
                        });
                    }
                    find = _Config.ProdutionList[_Config.ProdutionId].点检_自动.ToList().Find((xxx) => xxx.ID == item.Key && xxx.Part == item.Value.Part.ToString());

                    //2
                    foreach (var fai in item.Value.FaiInfos)
                    {
                        var findFai = find.ListFAI.ToList().Find((xxx) => xxx.名称 == fai.名称);
                        if (findFai == null)
                        {
                            find.ListFAI.Add(new FAIjudge() { 名称 = fai.名称 });
                        }

                        findFai = find.ListFAI.ToList().Find((xxx) => xxx.名称 == fai.名称);

                        findFai.标准值 = Math.Round(fai.测量结果 / fai.补偿系数,4);
                    }
                }

                this.dataGridView_自动真值列表.DataSource = _Config.ProdutionList[_Config.ProdutionId].点检_自动;
                this.dataGridView_自动真值详细表.DataSource = null;
                this.labelID.Text = "";
                this.label穴位.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("录入失败:" + ex.Message);
            }
        }


        private void button录入B侧点检真值_Click(object sender, EventArgs e)
        {
            try
            {
                var re = MessageBox.Show("是否录入B侧点检真值？", "提示", MessageBoxButtons.OKCancel);
                if (re == DialogResult.Cancel)
                {
                    return;
                }

                Dictionary<string, List<PartResult>> 点检真值记录表_All = new Dictionary<string, List<PartResult>>();
                Dictionary<string, PartResult> 点检真值记录表_Ave = new Dictionary<string, PartResult>();


                foreach (var item in _Proxy.点检真值记录表)
                {
                    if (item.Part != PartEnum.S1_B_L && item.Part != PartEnum.S1_B_R)
                    {
                        continue;
                    }
                    if (!点检真值记录表_All.ContainsKey(item.ID))
                    {
                        点检真值记录表_All.Add(item.ID, new List<PartResult>());
                    }
                    点检真值记录表_All[item.ID].Add(item);
                }


                //求平均值
                foreach (var item in 点检真值记录表_All)
                {
                    if (!点检真值记录表_Ave.ContainsKey(item.Key))
                    {
                        点检真值记录表_Ave.Add(item.Key, new PartResult() { FaiInfos = new List<FAIjudge>() });
                    }
                    foreach (var pro in item.Value)
                    {
                        点检真值记录表_Ave[item.Key].Part = pro.Part;
                        foreach (var fai in pro.FaiInfos)
                        {
                            var findFai = 点检真值记录表_Ave[item.Key].FaiInfos.Find((xxx) => xxx.名称 == fai.名称);
                            if (findFai == null)
                            {
                                点检真值记录表_Ave[item.Key].FaiInfos.Add(new FAIjudge() { 名称 = fai.名称 });
                            }

                            //累加
                            findFai = 点检真值记录表_Ave[item.Key].FaiInfos.Find((xxx) => xxx.名称 == fai.名称);
                            findFai.测量结果 += fai.测量结果;
                            findFai.补偿系数++;//当计数器使用
                        }
                    }
                }

                //更新标准值
                foreach (var item in 点检真值记录表_Ave)
                {
                    //1
                    var find = _Config.ProdutionList[_Config.ProdutionId].点检_自动.ToList().Find((xxx) => xxx.ID == item.Key && xxx.Part == item.Value.Part.ToString());
                    if (find == null)
                    {
                        _Config.ProdutionList[_Config.ProdutionId].点检_自动.Add(new Proxy.FAI()
                        {
                            ID = item.Key,
                            Part = item.Value.Part.ToString(),
                        });
                    }
                    find = _Config.ProdutionList[_Config.ProdutionId].点检_自动.ToList().Find((xxx) => xxx.ID == item.Key && xxx.Part == item.Value.Part.ToString());

                    //2
                    foreach (var fai in item.Value.FaiInfos)
                    {
                        var findFai = find.ListFAI.ToList().Find((xxx) => xxx.名称 == fai.名称);
                        if (findFai == null)
                        {
                            find.ListFAI.Add(new FAIjudge() { 名称 = fai.名称 });
                        }

                        findFai = find.ListFAI.ToList().Find((xxx) => xxx.名称 == fai.名称);

                        findFai.标准值 = Math.Round(fai.测量结果 / fai.补偿系数, 4);
                    }
                }

                this.dataGridView_自动真值列表.DataSource = _Config.ProdutionList[_Config.ProdutionId].点检_自动;
                this.dataGridView_自动真值详细表.DataSource = null;
                this.labelID.Text = "";
                this.label穴位.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("录入失败:" + ex.Message);
            }
        }

        private void btnGongChaDaoRu_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog2.Filter = ".csv|*csv";
                var show = openFileDialog2.ShowDialog();
                if (show == DialogResult.Cancel)
                {
                    return;
                }

                int index = 0;

                FileStream fs = null;
                StreamReader sr = null;
                fs = new FileStream(openFileDialog2.FileName, System.IO.FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                sr = new StreamReader(fs, Encoding.Default);
                var re = sr.ReadLine();//忽略第一行
                int index1 = -1;
                int index2 = -1;
                int index3 = -1;
                if (re != null)
                {
                    List<string> sp = re.Split(',').ToList<string>();
                    index1 = sp.FindIndex((x) =>  x == "名称");
                    index2 = sp.FindIndex((x) =>  x == "上公差");
                    index3 = sp.FindIndex((x) =>  x == "下公差");

                    if (index1 == -1 || index2 == -1 || index3 == -1)
                    {
                        throw new Exception(@"导入csv文件中第一行不包含“名称”或“上公差”、“下公差”！");
                    }
                }
                else
                {
                    throw new Exception("导入csv文件中第一行不包含数据！");
                }

                    BindingList<自动点检公差> temp = new BindingList<自动点检公差>();
                while (re != null)
                {
                    re = sr.ReadLine();
                    if (re != null)
                    {
                        index++;
                        var sp = re.Split(',');
                        if (sp.Count() < index1 || sp.Count() < index2 || sp.Count() < index3)
                        {
                            throw new Exception($"导入csv文件中第{index}不包含足够的数据！");
                        }
                        temp.Add(new 自动点检公差()
                        {
                            名称 = sp[index1],
                            上公差 = Convert.ToDouble(sp[index2]),
                            下公差 = Convert.ToDouble(sp[index3]),
                        });
                    }
                }
                sr.Close();
                fs.Close();

                _Config.公差管理.Clear();
                foreach (var item in temp)
                {
                    _Config.公差管理.Add(new 自动点检公差()
                    {
                        名称 = item.名称,
                        上公差 = item.上公差,
                        下公差 = item.下公差,
                    });
                }

                Logger.ShowTickTipsForm("自动点检公差 CSV导入成功", "点检CSV导入成功，共" + index, 1);
            }
            catch (Exception ex)
            {
                Logger.ShowTickTipsForm("自动点检公差 CSV导入失败", "点检CSV导入失败:" + ex.Message);
            }
        }

        private void btnGongChaDaoChu_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog2.Filter = ".csv|*csv";
                var show = saveFileDialog2.ShowDialog();

                if (show == DialogResult.Cancel)
                {
                    return;
                }

                int index = 0;
                FileStream fs = null;
                StreamWriter sw = null;
                fs = new FileStream(saveFileDialog2.FileName, System.IO.FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                sw = new StreamWriter(fs, Encoding.Default);
                StringBuilder text0 = new StringBuilder();
                text0.Clear();
                text0.Append("名称" + "," + "上公差" + "," + "下公差");
                sw.WriteLine(text0);
                for (int i = 0; i < _Config.公差管理.Count; i++)
                {
                    var item = _Config.公差管理[i];
                    text0.Clear();
                    text0.Append(item.名称 + "," + item.上公差 + "," + item.下公差);
                    sw.WriteLine(text0);
                    index++;
                }
                sw.Close();
                fs.Close();

                Logger.ShowTickTipsForm("自动点检公差 CSV导出成功", "点检CSV导出成功，共" + index, 1);
            }
            catch (Exception ex)
            {
                Logger.ShowTickTipsForm("自动点检公差 CSV导出失败", "点检CSV导出失败:" + ex.Message);
            }
        }
        #endregion

        #region ~~~~~~~~~~~~~~~~数据导出~~~~~~~~~~~~~~~~
        private void btnDataReportAddData_Click(object sender, EventArgs e)
        {
            if(_Config.listDataReportManage.Count == 0)
            {
                _Config.listDataReportManage.Insert(0, new DataReport());
                return;
            }
            _Config.listDataReportManage.Insert(dgvDataExport.CurrentCell.RowIndex, new DataReport());
        }

        private void btnDataReportAddDatabtnDataReportDeleteData_Click(object sender, EventArgs e)
        {
            _Config.listDataReportManage.RemoveAt(this.dgvDataExport.CurrentCell.RowIndex);
        }
        #endregion
    }
}
