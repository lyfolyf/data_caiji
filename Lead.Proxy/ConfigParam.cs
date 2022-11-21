using Lead.Tool.Focal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lead.Proxy
{
    #region ProjectManager-点检_自动-公差管理
    /// <summary>
    /// ProjectManager-点检_自动-公差管理
    /// </summary>
    public class 自动点检公差
    {
        /// <summary>
        /// 点检_自动-公差管理-名称
        /// </summary>
        public string 名称 { set; get; }
        /// <summary>
        /// 点检_自动-公差管理-上公差
        /// </summary>
        public double 上公差 { set; get; } = 0.1;
        /// <summary>
        /// 点检_自动-公差管理-下公差
        /// </summary>
        public double 下公差 { set; get; } = -0.1;
    }
    #endregion


    #region ProjectManager-数据导出
    /// <summary>
    /// ProjectManager-数据导出
    /// </summary>
    public class DataReport
    {
        #region Debug
        ///// <summary>
        ///// 数据导出-设备名称
        ///// </summary>
        //public string EquipmentName { get; set; }
        ///// <summary>
        ///// 数据导出-设备编号
        ///// </summary>
        //public string EquipmentID { get; set; }
        ///// <summary>
        ///// 数据导出-测试项目
        ///// </summary>
        //public string TestProject { get; set; }
        ///// <summary>
        ///// 数据导出-产品名称
        ///// </summary>
        //public string ProductName { get; set; }
        ///// <summary>
        ///// 数据导出-部件
        ///// </summary>
        //public string Unit { get; set; }
        ///// <summary>
        ///// 数据导出-模式
        ///// </summary>
        //public string Mode { get; set; }
        ///// <summary>
        ///// 数据导出-备用1
        ///// </summary>
        //public string Remark1 { get; set; }
        ///// <summary>
        ///// 数据导出-备用2
        ///// </summary>
        //public string Remark2 { get; set; }
        #endregion
        /// <summary>
        /// 数据导出-选项
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 数据导出-值
        /// </summary>
        public string Value { get; set; }
    }
    #endregion


    public class FuncEnable
    {
        public string Name { get; set; }
        public bool Enable { get; set; }
        public string Tips { get; set; }
    }

    public class DelayTime
    {
        public string Name { get; set; }
        public int Time { get; set; }
        public string Tips { get; set; }
    }

    public class DataSave
    {
        [Category("点云数据"), DisplayName("分布式点云远端读取寻址地址"), Description(@"默认 \\192.168.100.107\\Z:")]
        public string WcfAddress { get; set; } = "";
        [Category("点云数据"), DisplayName("分布式点云本地存储寻址地址"), Description(@"默认 Z:\\")]
        public string WcfAddress_Local { get; set; } = "";
        [Category("点云数据"), DisplayName("点云数据本地存储地址"), Description(@"默认 D:\Local\")]
        public string LocalFolder { get; set; } = @"D:\Local\";
        [Category("结果数据"), DisplayName("测量结果数据本地存储地址"), Description(@"默认 D:\Result\")]
        public string ResultFolder { get; set; } = @"D:\Result\";
        [Category("结果数据"), DisplayName("点检结果数据本地存储地址"), Description(@"默认 D:\Result_点检\")]
        public string ResultFolder_点检 { get; set; } = @"D:\Result_点检\";
        [Category("数据库信息"), DisplayName("远端数据库IP"), Description(@"默认 192.168.250.105")]
        public string Server_DbIP { get; set; }
        [Category("数据库信息"), DisplayName("远端数据库名"), Description("")]
        public string Server_DbName { get; set; }
        [Category("数据库信息"), DisplayName("本地数据库名"), Description(@"默认 192.168.250.107")]
        public string Local_DbIP { get; set; }
        [Category("数据库信息"), DisplayName("本地数据库名"), Description("")]
        public string Local_DbName { get; set; }
        [Category("温漂数据"), DisplayName("是否保存温漂信息"), Description("")]
        public bool IsSaveHotData { get; set; } = false;
        [Category("温漂数据"), DisplayName("温漂数据存储路径"), Description(@"D:\HotOffsetData\")]
        public string HotOffsetDic { get; set; } = @"D:\HotOffsetData\";

        [Category("点云数据"), DisplayName("是否保存计算出错时点云数据"), Description("默认false")]
        public bool IsSaveErrorData { get; set; } = false;

        [Category("点云数据"), DisplayName("计算出错时点云数据的保存路径"), Description(@"D:\ErrorData\")]
        public string ErrorDic { get; set; } = @"D:\ErrorData\";
        [Category("补偿信息"), DisplayName("允许的丢失数"), Description("默认：3")]
        public int AllowMissPoinNumber { get; set; } = 3;
        [Category("补偿信息"), DisplayName("允许的丢失时超时时间"), Description("默认：3000")]
        public int MissPoinTime_ms { get; set; } = 3000;
        [Category("数据解析"), DisplayName("数据解析时头的小大"), Description("默认：2；有效值：2 或 3")]
        public int ParseCount { get; set; } = 2;
        [Category("延时触发"), DisplayName("延时触发开始段"), Description("")]
        public int 延时触发开始段 { get; set; } = 0;
        [Category("延时触发"), DisplayName("延时触发结束段"), Description("")]
        public int 延时触发结束段 { get; set; } = 0;
    }

    public class FAI
    {
        public string Part { get; set; }
        public string ID { get; set; }
        public BindingList<FAIjudge> ListFAI { get; set; } = new BindingList<FAIjudge>();
    }

    

    public enum StationEnum
    {
        Min = -1,
        S1_A = 0,
        S1_B = 1,
        S2,
        Max
    }

    public class ProdutionFAI
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tips { get; set; }
        public List<FAI> ProFAI { get; set; }
        public List<FAI> 点检 { get; set; } = new List<FAI>();
        public BindingList<FAI> 点检_自动 { get; set; } = new BindingList<FAI>();
    }
    

    public class HotOffsetType
    {
        [Category("温漂信息"), DisplayName("传感器名称"), Description("传感器名称")]
        public string Name { get; set; }
        [Category("温漂信息"), DisplayName("工站"), Description("工站")]
        public StationEnum Station { get; set; }
        [Category("温漂信息"), DisplayName("温漂值"), Description("温漂值")]
        public double offset { get; set; }
        [Category("温漂信息取值规则"), DisplayName("温漂采样条数"), Description("已废弃")]
        public int range { get; set; } = 3;
        [Category("温漂信息"), DisplayName("上一次测量的温漂数值"), Description("上一次测量的温漂数值")]
        public double Last { get; set; }
        [Category("温漂信息"), DisplayName("上一次测量温漂的时间"), Description("上一次测量温漂的时间")]
        public DateTime LastTime { get; set; }
        [Category("温漂信息取值规则"), DisplayName("温漂数据Z向有效值下限"), Description("温漂数据Z向有效值下限")]
        public double Min { get; set; } = -48;
        [Category("温漂信息取值规则"), DisplayName("温漂数据Z向有效值上限"), Description("温漂数据Z向有效值上限")]
        public double Max { get; set; } = 48;
        [Category("温漂信息取值规则"), DisplayName("温漂数据点位取值开始点"), Description("温漂数据点位取值开始点")]
        public int StartSeg { get; set; } = 0;
        [Category("温漂信息取值规则"), DisplayName("温漂数据点位取值结束点"), Description("温漂数据点位取值结束点")]
        public int EndSeg { get; set; } = 1000;
        [Category("温漂信息"), DisplayName("温漂数据点标准值"), Description("温漂数据点位取值开始点")]
        public double Stand { get; set; } = 0;
        [Category("温漂信息跳动规则"), DisplayName("温漂数值跳动的范围"), Description("温漂数值跳动的范围-绝对值，默认：0.01")]
        public double OffsetAllowRange { get; set; } = 0.01;
        [Category("温漂信息跳动规则"), DisplayName("温漂数值跳动的时间间隔"), Description("温漂数值跳动的时间间隔,单位：分钟,默认：30")]
        public double OffsetAllowTime { get; set; } = 30;
    }

    public class ConfigParam
    {
        public int ProdutionId { get; set; }

        public string ProdutionYanse { get; set; }

        public BindingList<FuncEnable> Func { get; set; }

        public BindingList<DelayTime> Delay { get; set; }

        public DataSave DataSaveParam { get; set; }

        public List<ProdutionFAI> ProdutionList { get; set; }
        public string FaiKey { get; set; } = "FormulaResult;faiProfileLine;faiProfileMax;faiProfileMin;";
        public List<HotOffsetType> HotOffsetConfig { get; set; } = new List<HotOffsetType>();
        public List<FocalConfigDiffProdution> Foacl_横 { get; set; } = new List<FocalConfigDiffProdution>();
        public List<FocalConfigDiffProdution> Foacl_竖 { get; set; } = new List<FocalConfigDiffProdution>();

        public List<极差测量项> list极差1 { get; set; } = new List<极差测量项>();
        public List<极差测量项> list极差2 { get; set; } = new List<极差测量项>();

        public BindingList<自动点检公差> 公差管理 { get; set; } = new BindingList<自动点检公差>();

        public BindingList<二维码防呆> list二维码 { get; set; } = new BindingList<二维码防呆>();//无效20201202

        public BindingList<DataReport> listDataReportManage { get; set; } = new BindingList<DataReport>();

        public DateTime LastSaveTime { get; set; }

    }
    public class 极差测量项
    {
        public bool 是否输出 { get; set; } = false;
        public string 测量项名称 { get; set; }
        public double 标准值 { get; set; }
        public double 极差上限 { get; set; }
        public double 极差下限 { get; set; }
        public bool 是否判定 { get; set; }
        public string str { get; set; }
    }

    public class 二维码防呆
    {
        public string 颜色 { get; set; }
        public string 对应字符串 { get; set; }
    }

    public class FAI_点检数据分析
    {
        public string Station { set; get; }
        public string Part { set; get; }
        public double 标准值 { set; get; } = 0.1;
        public double 上公差 { set; get; } = 0.1;
        public double 下公差 { set; get; } = 0.1;
        public string Name { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double sum { get; set; }
        public double dsum { get; set; }

        /// <summary>
        /// 计算指定样本的标准偏差
        /// </summary>
        public double SIGMA { get; set; }
        public int OK { get; set; }
        public int NG { get; set; }
        public double 良率 { get; set; }
        public double CP { get; set; }
        public double CPKU { get; set; }
        public double CPKL { get; set; }
        public double CPK { get; set; }
        public List<double> ValueList { get; set; } = new List<double>();
    }
}
