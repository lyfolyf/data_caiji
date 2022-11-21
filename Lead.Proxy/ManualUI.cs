using Lead.Tool.CommonData_3D;
using Lead.Tool.Focal;
using Lead.Tool.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lead.Proxy
{

    public partial class ManualUI : Form
    {
        ProxyData _Proxy = null;
        public ManualUI(ProxyData proxy)
        {
            InitializeComponent();

            _Proxy = proxy;
        }

        private void ManualUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true ;
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<FSPoint[]>  X = new List<FSPoint[]>();
            List<FSPoint[]>   y = new List<FSPoint[]>();
            


            if (_Proxy._iFocal_横 != null)
            {
                X = _Proxy._iFocal_横.GetScanResult();
            }


            if (_Proxy._iFocal_竖 != null)
            {
                y = _Proxy._iFocal_竖.GetScanResult();
            }

            List<FSPoint[]> z = new List<FSPoint[]>();
            if (_Proxy._iLMI != null)
            {
                z = _Proxy._iLMI.GetScanResult();
            }

            string str = "Focal_横：" + X.Count + " Focal_竖：" + y.Count + " _iLMI：" + z.Count;
            Logger.ShowForm("Debug数据查询",FormMode.TipsForm,"以下未各个传感器的接收数据数目", str);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _Proxy.ServerMongo.InsertOneAsync<PartResult>(new PartResult() { CreateTime =DateTime.Now}, "S1_A","test");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _Proxy.InitTrigInfo();
            _Proxy.启动传感器采集(StationEnum.S1_B);
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            _Proxy.传感器切换参数();
        }
    }
}
