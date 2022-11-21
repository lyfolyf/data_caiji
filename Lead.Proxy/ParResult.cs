using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Proxy
{
    public enum PartEnum
    {
        S1_A_L = 0,
        S1_A_R,
        S1_B_L,
        S1_B_R,
        S2_L,
        S2_R,
    }

    public enum DataSaveMode
    {
        None,
        本地,
        远程_CSV,
        远程_WCF,
        本地_and_远程_CSV,
        本地_and_远程_WCF,
    }

    public class FAIjudge
    {
        public string 名称 { set; get; }
        public double 测量结果 { set; get; }
        public double 标准值 { set; get; }
        public double 上公差 { set; get; } = 0.1;
        public double 下公差 { set; get; } = 0.1;
        public double 补偿系数 { set; get; }
        public double 补偿值 { set; get; }
        public bool 是否判定 { set; get; } = false;
        public string 判定结果 { set; get; } = "";
    }

    public class PartResult
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { set; get; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndTime { set; get; }
        public DataSaveMode IsCSVorWCF { set; get; }
        public string ID { set; get; }//二维码
        public string Calc_Result { set; get; }//
        public string Measure_Result { set; get; }//
        public string StationName { set; get; }//工站名
        public string TaskName { set; get; }//3D任务名
        public string FilePath { set; get; }//任务路径
        public PartEnum Part { set; get; }//穴位
        public List<FAIjudge> FaiInfos { set; get; }//测量结果
        public bool IsSendToMES { set; get; } = false;//是否上传mes
        public bool Is点检 { set; get; } = false;
        public int FileCount { set; get; }
        public List<string> FList { set; get; } = new List<string>();
        public string ProdutionID { get; set; }//产品名称
        public int ParseCount { get; set; }

        //20201010
        public string 点检模式 { get; set; }//手动 自动 真值
        public string Task路径 { get; set; }//预留 暂时不用
        /// <summary>
        /// 产品序号
        /// </summary>
        public int ProdutionIndex { get; set; }
        /// <summary>
        /// 产品分类结果
        /// </summary>
        public string Bin { get; set; }
        /// <summary>
        /// 产品颜色
        /// </summary>
        public string ProductionColor { get; set; }
    }
}
