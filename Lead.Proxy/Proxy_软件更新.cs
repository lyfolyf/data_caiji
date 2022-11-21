using Lead.Tool.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Proxy
{
    public partial class ProxyData
    {
        public void 软件更新()
        {
            List<DelayTime> DaleyList = new List<DelayTime>();
            List<FuncEnable> FuncList = new List<FuncEnable>();
            List<DataReport> DataReportList = new List<DataReport>();


            #region ProjectManger-功能开关
            FuncList.Add(new FuncEnable() { Name = "是否启动不同产品切换Focal参数", Enable = false, Tips = "切换产品时，是否切换Focal参数" });
            FuncList.Add(new FuncEnable() { Name = "是否检索re", Enable = false, Tips = "是否检索re" });
            FuncList.Add(new FuncEnable() { Name = "检索数据是否保存", Enable = false, Tips = "是否保存检索数据" });
            FuncList.Add(new FuncEnable() { Name = "测试或", Enable = false, Tips = "" });
            FuncList.Add(new FuncEnable() { Name = "测试极差", Enable = false, Tips = "" });
            FuncList.Add(new FuncEnable() { Name = "二维码防呆", Enable = false, Tips = "开启防呆；不开启不进行防呆；默认不开启" });
            FuncList.Add(new FuncEnable() { Name = "Focal_竖Y轴是否取反", Enable = false, Tips = "开启时，Y轴乘以-1" });
            
            #endregion

            #region 数据导出
            DataReportList.Add(new DataReport() { Name = "设备名称", Value = "3D玻璃尺寸检测设备" });
            DataReportList.Add(new DataReport() { Name = "设备编号", Value = "LD20200514-24" });
            DataReportList.Add(new DataReport() { Name = "测试项目", Value = "N" });
            DataReportList.Add(new DataReport() { Name = "测试版本", Value = "2.2020.1023.22631"});
            DataReportList.Add(new DataReport() { Name = "产品名称", Value = "LAN" });
            DataReportList.Add(new DataReport() { Name = "部件", Value = "电池盖" });
            DataReportList.Add(new DataReport() { Name = "模式", Value = "量产" });
            DataReportList.Add(new DataReport() { Name = "Remark1", Value = "" });
            DataReportList.Add(new DataReport() { Name = "Remark2", Value = "" });
            #endregion

            #region 功能开关
            foreach (var aim in FuncList)
            {
                bool IsFined = false;
                foreach (var item in _ProjectConfig.Func)
                {
                    if (item.Name == aim.Name)
                    {
                        //item.Tips = aim.Tips;
                        IsFined = true;
                        break;
                    }
                }

                if (!IsFined)
                {
                    try
                    {
                        _ProjectConfig.Func.Add(aim);
                        Logger.OK("本机台未在 功能参数配置 中找到：" + aim.Name + ";先已自动更新：" + aim.Name + "=" + aim.Enable);
                    }
                    catch
                    { }
                }
            }
            #endregion

            #region 超时时间
            foreach (var aim in DaleyList)
            {
                bool IsFined = false;
                foreach (var item in _ProjectConfig.Delay)
                {
                    if (item.Name == aim.Name)
                    {
                        //item.Tips = aim.Tips;
                        IsFined = true;
                        break;
                    }
                }

                if (!IsFined)
                {
                    try
                    {
                        _ProjectConfig.Delay.Add(aim);
                        System.Windows.Forms.MessageBox.Show("本机台未在 延时参数配置 中找到：" + aim.Name + ";先已自动更新：" + aim.Name + "=" + aim.Time);
                    }
                    catch
                    {

                        throw;
                    }
                }
            }
            #endregion

            #region 数据导出
            foreach (var aim in DataReportList)
            {
                bool isFined = false;
                foreach (var item in _ProjectConfig.listDataReportManage)
                {
                    if(item.Name==aim.Name)
                    {
                        //item.Value = aim.Value;
                        isFined = true;
                        break;
                    }
                }

                if(!isFined)
                {
                    try
                    {
                        _ProjectConfig.listDataReportManage.Add(aim);
                        System.Windows.Forms.MessageBox.Show("本机台未在 数据导出配置 中找到：" + aim.Name + ";先已自动更新：" + aim.Name + "=" + aim.Value);
                    }
                    catch
                    {

                        throw;
                    }
                }
            }
            #endregion

        }
    }
}
