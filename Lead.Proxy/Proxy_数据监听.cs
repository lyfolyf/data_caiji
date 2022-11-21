using Lead.CPrim.PrimKeyenceLJ;
using Lead.Tool.CommonData_3D;
using Lead.Tool.Focal;
using Lead.Tool.INI;
using Lead.Tool.LMI;
using Lead.Tool.Log;
using Lead.Tool.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lead.Proxy
{

    //单个Senser状态
    public class SenserInfo_State
    {
        public bool IsFirstAfterInitFocal { get; set; }

        public StationEnum Station { get; set; }//站号

        public bool IsStart { get; set; } = false;//是否开始
        public bool IsErr { get; set; } = false;//是否开始

        public bool IsErrContinue { get; set; } = false;//是否开始

        public int StartSeg { get; set; } = 0;//触发段 开始计数点数

        public int CountSeg { get; set; } = 0;//触发段  需要触发点数

        public int EndSeg { get; set; } = 0;//触发段  结束计数点数

        public int CurrentSegIndex { get; set; } = 0;//当前 触发段号

        public CMM_SenserInfo_3D TrigInfo { get; set; } = new CMM_SenserInfo_3D();

        public Thread Thread { get; set; } = null;

        public FocalTool Focal { get; set; }

        public KeyenceTool KeyenceTool { get; set; }

        public LMITool LMITool { get; set; }

        public string LocalFolder_Left { get; set; }//S1_A站数据本机保存地址

        public string LocalFolder_Right { get; set; }//S1_B站数据本机保存地址

        public string ServerFolder_Left { get; set; }//S1_A站数据远端保存地址

        public string ServerFolder_Right { get; set; }//S1_B站数据远端保存地址

        public DataSaveMode IsWCForCSV { get; set; }
        public bool 是否验证触发延时 { get; set; }
        public bool 是否包括热飘移 { get; set; }
    }

    public class KV
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public double StartPos { get; set; }
        public double Distance { get; set; }
    }

    public class FolderInfo
    {
        public string LocalFolder_Left { get; set; }

        public string LocalFolder_Right { get; set; }

        public string ServerFolder_Left { get; set; }

        public string ServerFolder_Right { get; set; }

        public string ServerFolder_Left_WCF { get; set; }

        public string ServerFolder_Right_WCF { get; set; }

        public PartResult ServerWCF_Left { get; set; }

        public PartResult ServerWCF_Right { get; set; }

        public int AllLeftCount { get; set; }

        public int AllRightCount { get; set; }

        public List<KV> ServerDataFileListl_Left { get; set; } = new List<KV>();

        public List<KV> ServerDataFileListl_Right { get; set; } = new List<KV>();

        public string ID1 { get; set; }

        public string ID2 { get; set; }

        public string ProdutionID { get; set; }
        public string ProdutionIndex { get; set; }
    }


    public enum 采集端交互信号
    {
        允许采集 = 1,
        开始采集 = 2,
        不允许采集 = -1,
    }

    public enum 运动端交互信号
    {
        复位,
        请求采集,
        开始采集,
        运动异常,
        触发结束,
    }
    public class 分布式计算信息
    {
        public bool IsDone1 { get; set; } = false;
        public bool IsDone2 { get; set; } = false;
        public string ID1 { get; set; } = "";
        public string ID2 { get; set; } = "";
    }

    public partial class ProxyData
    {

        private DateTime LastTrigInfoSaveTime = DateTime.Now;
        string Path3D_AB = @"D:\3D与运动控制交互\EX_AB.ini";
        string Path3D_A = @"D:\3D与运动控制交互\EX_A.ini";
        string Path3D_B = @"D:\3D与运动控制交互\EX_B.ini";
        DataSaveMode IsCSVorWCF = DataSaveMode.None;
        CMM_StationInfo_3D Data = null;
        FolderInfo StationInfo = new FolderInfo() { AllRightCount = 0, AllLeftCount = 0 };
        bool Is点检 = false;
        bool Is验证触发延时 = false;
        bool Is包括热飘移 = false;
        private Dictionary<StationEnum, 分布式计算信息> 上次分布式的计算结果 = new Dictionary<StationEnum, 分布式计算信息>();
        private int _LastProdutionID = -1;
        string fenBuShiPath = @"D:\3D与运动控制交互\Ex_Online.ini";

        public void 等待工站运动开始信号(StationEnum StationID)
        {
            try
            {
                var re = "";
                string Path = StationID == StationEnum.S1_A ? Path3D_A :
                    StationID == StationEnum.S1_B ? Path3D_B : "错误的信号交互路径";

                while (IsAllRun)
                {
                    re = INIhelp.GetValue(Path, "交互信号", "运动端交互信号");

                    if (re == 运动端交互信号.复位.ToString())
                    {
                        ;
                    }
                    else if (re == 运动端交互信号.请求采集.ToString())
                    {
                        DateTime start = DateTime.Now;
                        传感器切换参数();
                        传感器准备采集();
                        Logger.Info(StationID+ " 传感器准备采集 耗时："+(DateTime.Now-start).TotalMilliseconds);
                        INIhelp.SetValue(Path, "交互信号", "采集端交互信号", 采集端交互信号.开始采集.ToString());

                        DateTime Start = DateTime.Now;
                        Logger.Info("启动传感器采集 开始时间");
                        启动传感器采集(StationID);
                        Logger.Info("启动传感器采集 结束时间");
                        Logger.Info("启动传感器采集 总时间/ms:" + (DateTime.Now - Start).TotalMilliseconds);
                        return;
                    }
                    else if (re == 运动端交互信号.运动异常.ToString())
                    {
                        ;
                    }
                    else if (re == 运动端交互信号.开始采集.ToString())
                    {
                        ;
                    }
                    else if (re == 运动端交互信号.触发结束.ToString())
                    {
                        ;
                    }
                    else
                    {
                        Logger.Warn("运动端交互信号 无法识别");
                        throw new Exception("运动端交互信号 无法识别");
                    }
                    Thread.Sleep(30);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("等待" + StationID + "工站运动开始信号失败：" + ex.Message);
                throw new Exception("等待" + StationID + "工站运动开始信号失败：" + ex.Message);
            }
        }

        private void ChechDatsSaveMode(StationEnum StationID, ref DataSaveMode IsCSVorWCF)
        {
            string 是否启动分布式计算 = "";
            string 是否保存本地数据 = "";
            是否启动分布式计算 = INIhelp.GetValue(Path3D_AB, "Config", "是否启动分布式计算");
            是否保存本地数据 = INIhelp.GetValue(Path3D_AB, "Config", "是否保存本地数据");

            //功能判断            
            if (是否启动分布式计算 == "true")
            {
                if (是否保存本地数据 == "true")
                {
                    if (GetFunc("启动WCF传输数据"))
                    {
                        if (StationID == StationEnum.S1_A)
                        {
                            WcfServer.ResetWCF(PartEnum.S1_A_L);
                            WcfServer.ResetWCF(PartEnum.S1_A_R);
                        }
                        else
                        {
                            WcfServer.ResetWCF(PartEnum.S1_B_L);
                            WcfServer.ResetWCF(PartEnum.S1_B_R);
                        }
                        IsCSVorWCF = DataSaveMode.本地_and_远程_WCF;
                    }
                    else
                    {
                        IsCSVorWCF = DataSaveMode.本地_and_远程_CSV;
                    }
                }
                else
                {
                    if (GetFunc("启动WCF传输数据"))
                    {
                        if (StationID == StationEnum.S1_A)
                        {
                            WcfServer.ResetWCF(PartEnum.S1_A_L);
                            WcfServer.ResetWCF(PartEnum.S1_A_R);
                        }
                        else
                        {
                            WcfServer.ResetWCF(PartEnum.S1_B_L);
                            WcfServer.ResetWCF(PartEnum.S1_B_R);
                        }
                        IsCSVorWCF = DataSaveMode.远程_WCF;
                    }
                    else
                    {
                        IsCSVorWCF = DataSaveMode.远程_CSV;
                    }
                }
            }
            else
            {
                if (是否保存本地数据 == "true")
                {
                    IsCSVorWCF = DataSaveMode.本地;
                }
                else
                {
                    IsCSVorWCF = DataSaveMode.None;
                }
            }

            Logger.Info("IsCSVorWCF= " + IsCSVorWCF.ToString());
        }

        public void 启动传感器采集(StationEnum StationID)
        {
            IsCSVorWCF =  DataSaveMode.None;
            Data = null;
            StationInfo = new FolderInfo() { AllRightCount = 0, AllLeftCount = 0 };
            Is点检 = false;
            Is验证触发延时 = false;

            try
            {
                //更新ID号
                if (StationID == StationEnum.S1_A)
                {
                    StationInfo.ID1 = INIhelp.GetValue(Path3D_A, "Config", "ID1");
                    StationInfo.ID2 = INIhelp.GetValue(Path3D_A, "Config", "ID2");
                    Is点检 = INIhelp.GetValue(Path3D_A, "Config", "是否点检") == "true" ? true : false;
                    Index_点检_A_L = Convert.ToInt32(INIhelp.GetValue(Path3D_A, "Config", "左穴点检ID"));
                    Index_点检_A_R = Convert.ToInt32(INIhelp.GetValue(Path3D_A, "Config", "右穴点检ID"));
                    Is验证触发延时 = INIhelp.GetValue(Path3D_A, "Config", "是否验证触发延时") == "true" ? true : false;
                    Is包括热飘移 = INIhelp.GetValue(Path3D_A, "Config", "是否包括热飘移") == "true" ? true : false;
                    Logger.Info("A侧 是否点检= " + Is点检 + "; 左右穴点检产品号分别为: " + Index_点检_A_L + "," + Index_点检_A_R);
                    Logger.Info("A侧 Is验证触发延时= " + Is验证触发延时);
                    StationInfo.ProdutionID = INIhelp.GetValue(Path3D_A, "Config", "ProdutionID");
                    Logger.Info("A侧 数据采集端获取 ProdtuionID：" + StationInfo.ProdutionID);
                }
                else if (StationID == StationEnum.S1_B)
                {
                    StationInfo.ID1 = INIhelp.GetValue(Path3D_B, "Config", "ID1");
                    StationInfo.ID2 = INIhelp.GetValue(Path3D_B, "Config", "ID2");
                    Is点检 = INIhelp.GetValue(Path3D_B, "Config", "是否点检") == "true" ? true : false;
                    Index_点检_B_L = Convert.ToInt32(INIhelp.GetValue(Path3D_B, "Config", "左穴点检ID"));
                    Index_点检_B_R = Convert.ToInt32(INIhelp.GetValue(Path3D_B, "Config", "右穴点检ID"));
                    Is验证触发延时 = INIhelp.GetValue(Path3D_B, "Config", "是否验证触发延时") == "true" ? true : false;
                    Is包括热飘移 = INIhelp.GetValue(Path3D_B, "Config", "是否包括热飘移") == "true" ? true : false;
                    Logger.Info("B侧 是否点检= " + Is点检 + "; 左右穴点检产品号分别为: " + Index_点检_B_L + "," + Index_点检_B_R);
                    Logger.Info("B侧 Is验证触发延时= " + Is验证触发延时);
                    StationInfo.ProdutionID = INIhelp.GetValue(Path3D_B, "Config", "ProdutionID");
                    Logger.Info("B侧 数据采集端获取 ProdtuionID：" + StationInfo.ProdutionID);
                }
                Logger.Info(StationID.ToString() + "成功从文件中获取ID。");
            }
            catch (Exception ex)
            {
                Logger.Error(StationID.ToString() + "从文件中获取ID失败！");
                throw ex;
            }

            //更新触发信息(运动控制和数据采集交互\1.xml)
            InitTrigInfo();
            foreach (var item in _TotalTrigInfo)
            {
                if (item.StationName == StationID.ToString())
                {
                    Data = item;
                    break;
                }
            }

            //生成数据存储状态
            ChechDatsSaveMode(StationID, ref IsCSVorWCF);
            
            //生成本地和远端目录
            BeforeListen(ref StationInfo, Data,IsCSVorWCF,Is点检);

            //WCF发送计算器请求
            WCF发送任务(ref StationInfo, Data, IsCSVorWCF, Is点检);

            //sensor数据采集开始
            {
                foreach (var item in Data.SenserList)
                {
                    Logger.Info(item.Name + "为空闲线程，开始数据监听前的操作");

                    #region //配置Senser参数
                    _SenserInfo[item.Name].TrigInfo = item;
                    _SenserInfo[item.Name].Station = StationID;

                    _SenserInfo[item.Name].StartSeg = 0;
                    _SenserInfo[item.Name].CountSeg = 0;
                    _SenserInfo[item.Name].EndSeg = 0;
                    _SenserInfo[item.Name].CurrentSegIndex = 0;

                    _SenserInfo[item.Name].LocalFolder_Left = StationInfo.LocalFolder_Left;
                    _SenserInfo[item.Name].LocalFolder_Right = StationInfo.LocalFolder_Right;
                    _SenserInfo[item.Name].ServerFolder_Left = StationInfo.ServerFolder_Left;
                    _SenserInfo[item.Name].ServerFolder_Right = StationInfo.ServerFolder_Right;
                    Logger.Info(item.Name+ "ServerFolder_Left="+ StationInfo.ServerFolder_Left + ";ServerFolder_Right=" + StationInfo.ServerFolder_Right);
                    _SenserInfo[item.Name].IsWCForCSV = IsCSVorWCF;
                    _SenserInfo[item.Name].是否验证触发延时 = Is验证触发延时;
                    _SenserInfo[item.Name].是否包括热飘移 = Is包括热飘移;
                    #endregion

                    //将数据采集段循环使能
                    _SenserInfo[item.Name].IsErrContinue = false;
                    _SenserInfo[item.Name].IsErr = false;
                    _SenserInfo[item.Name].IsStart = true;
                    Logger.Info(item.Name + ": IsStart = true");

                    Logger.Info(item.Name + "开启当前产品的数据采集成功");
                }
            }

            //开始监听
            StartTrigTask();
        }

        public void 检测传感器采集完成(StationEnum Station)
        {
            string Path = Station == StationEnum.S1_A ? Path3D_A :
                Station == StationEnum.S1_B ? Path3D_B : "错误的信号交互路径";

            CMM_StationInfo_3D Data = null;

            foreach (var item in _TotalTrigInfo)
            {
                if (item.StationName == Station.ToString())
                {
                    Data = item;
                }
            }

            try
            {
                int total = 0;
                DateTime TrigEndTime = DateTime.Now;
                bool IsEndTrig = false;
                while (IsAllRun)
                {
                    var Single = INIhelp.GetValue(Path, "交互信号", "运动端交互信号");

                    //寻找触发结束时间
                    {
                        if (Single == 运动端交互信号.触发结束.ToString() && !IsEndTrig)
                        {
                            IsEndTrig = true;
                            TrigEndTime = DateTime.Now;
                            Logger.Info("接受到 触发结束 信号");
                        }
                    }

                    #region 异常
                    if (Single == 运动端交互信号.复位.ToString() ||
                        Single == 运动端交互信号.运动异常.ToString())
                    {
                        foreach (var itemSenser in Data.SenserList)
                        {
                            int xxxx = 0;
                            switch (itemSenser.Type)
                            {
                                case SenserType.Focal:
                                    xxxx = (((FocalTool)_ToolConfig.ToolManager[itemSenser.Name]).GetScanResult()).Count;
                                    break;
                                case SenserType.LMI:
                                    xxxx = (((LMITool)_ToolConfig.ToolManager[itemSenser.Name]).GetScanResult()).Count;
                                    break;
                                case SenserType.基恩士:
                                    xxxx = (((KeyenceTool)_ToolConfig.ToolManager[itemSenser.Name]).GetScanResult()).Count;
                                    break;
                            }
                            Logger.Info(itemSenser.Name + " 异常时的点位个数：" + xxxx);
                        }
                        throw new Exception(Data.StationName + " 在等待结果采集完毕的时候，收到运动控制端的信号：" + Single);
                    }
                    #endregion

                    total = 0;
                    foreach (var item in Data.SenserList)
                    {
                        //最后一段
                        if (IsEndTrig
                            && _SenserInfo[item.Name].IsErrContinue == false
                            && (DateTime.Now - TrigEndTime).TotalMilliseconds > _ProjectConfig.DataSaveParam.MissPoinTime_ms
                            && _SenserInfo[item.Name].IsStart == true)
                        {
                            _SenserInfo[item.Name].IsErrContinue = true;
                            Logger.Info(item.Name + " 在触发结束后的规定时间内，没有收到足够的点云，做跳过处理,IsErrContinue = true");
                            break;
                        }

                        if (_SenserInfo[item.Name].IsStart == false)//当前senser空闲，且已经接受的段数全部完毕
                        {
                            total++;
                        }
                    }

                    if (total == Data.SenserList.Count)
                    {

                        Logger.Info("所有传感器已完成数据采集-WCF发送计算器请求");

                        break;
                    }

                    Thread.Sleep(5);
                }
            }
            catch (Exception ex)
            {
                this.StopTrigTask();
                Logger.Error(Data.StationName + "未能正常完成所有数据采集，已停止StopTrigTask; 错误原因：" + ex.Message);
            }
            Logger.Info(Station + " 所有传感器数据采集完毕");

            foreach (var itemSenser in Data.SenserList)
            {
                switch (itemSenser.Type)
                {
                    case SenserType.Focal:
                        ((FocalTool)_ToolConfig.ToolManager[itemSenser.Name]).StopMeasure();
                        break;
                    case SenserType.LMI:
                        ((LMITool)_ToolConfig.ToolManager[itemSenser.Name]).StopMeasure();
                        break;
                    case SenserType.基恩士:
                        ((KeyenceTool)_ToolConfig.ToolManager[itemSenser.Name]).StopMeasure();
                        break;
                }
                Logger.Info(itemSenser.Name + " 停止传感器采集数据");
            }
            Logger.Info(Station + " 所有传感器 StopMeasure 完毕");

            foreach (var item in Data.SenserList)
            {
                if (_SenserInfo[item.Name].IsWCForCSV == DataSaveMode.本地 || _SenserInfo[item.Name].IsWCForCSV == DataSaveMode.None)
                {
                    INIhelp.SetValue(Path, "Config", "ID1_WcfResult", "Done");
                    INIhelp.SetValue(Path, "Config", "ID2_WcfResult", "Done");
                    Logger.Info(Station + " " + "因为屏蔽了分布式计算,所以设置左右穴位的测量计算结果为 Done");
                    break;
                }
            }

            //
            bool IsErrSenser = false;
            foreach (var item in Data.SenserList)
            {
                //最后一段
                if (_SenserInfo[item.Name].IsErr == true)
                {
                    IsErrSenser = true;
                    Logger.Error(item.Name + " 在处理温漂数据时，发生错误；IsErr == true");
                }
            }

            if (IsErrSenser == false)
            {
                INIhelp.SetValue(Path, "交互信号", "采集端交互信号", 采集端交互信号.允许采集.ToString());
                Logger.Info(Station + " 设置 采集端交互信号 = 允许采集");
                Logger.Info("zzc " + Station + " 采集运动结束");
            }
            else
            {
                INIhelp.SetValue(Path, "交互信号", "采集端交互信号", 采集端交互信号.不允许采集.ToString());
                Logger.Info(Station + " 设置 采集端交互信号 = 不允许采集");
            }
        }

        private void BeforeListen(ref FolderInfo FolderInfo, CMM_StationInfo_3D Data,DataSaveMode IsCSVorWCF, bool Is点检)
        {
            var TimeStr = "";
            string LeftStr = Data.StationName + "_Left";
            string RightStr = Data.StationName + "_Right";

            if ("" == TimeStr)
            {
                TimeStr = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            }

            #region//生成温漂目录
            if (!Directory.Exists(_ProjectConfig.DataSaveParam.HotOffsetDic))
            {
                Directory.CreateDirectory(_ProjectConfig.DataSaveParam.HotOffsetDic);
            }
            foreach (var item in Data.SenserList)
            {
                if (!Directory.Exists(_ProjectConfig.DataSaveParam.HotOffsetDic + "\\" + Data.StationName + "_" + item.Name))
                {
                    Directory.CreateDirectory(_ProjectConfig.DataSaveParam.HotOffsetDic + "\\" + Data.StationName + "_" + item.Name);
                    Logger.Info("创建温漂目录:" + _ProjectConfig.DataSaveParam.HotOffsetDic + "\\" + Data.StationName + "_" + item.Name);
                }
            }
            #endregion

            #region //生成本地目录
            if (IsCSVorWCF == DataSaveMode.本地 || IsCSVorWCF == DataSaveMode.本地_and_远程_CSV  || IsCSVorWCF == DataSaveMode.本地_and_远程_WCF )
            {
                if (!Directory.Exists(_ProjectConfig.DataSaveParam.LocalFolder))
                {
                    Directory.CreateDirectory(_ProjectConfig.DataSaveParam.LocalFolder);
                }
                FolderInfo.LocalFolder_Left = _ProjectConfig.DataSaveParam.LocalFolder + "\\" + LeftStr + "-" + TimeStr + "\\";
                FolderInfo.LocalFolder_Right = _ProjectConfig.DataSaveParam.LocalFolder + "\\" + RightStr + "-" + TimeStr + "\\";
                if (!Directory.Exists(FolderInfo.LocalFolder_Left))
                {
                    Directory.CreateDirectory(FolderInfo.LocalFolder_Left);
                }
                if (!Directory.Exists(FolderInfo.LocalFolder_Right))
                {
                    Directory.CreateDirectory(FolderInfo.LocalFolder_Right);
                }
            }
            #endregion

            #region //生成远端目录
            foreach (var item in Data.SenserList)
            {
                foreach (var itemSeg in item.SegmentList)
                {
                    if (!itemSeg.IsHotOffset)
                    {
                        double Distance = itemSeg.Pitch;
                        if (itemSeg.TrigPosList.Count > 1)
                        {
                            if ((itemSeg.TrigPosList[1] - itemSeg.TrigPosList[0]) > 0)
                            {
                                Distance = Distance * 1;
                            }
                            else
                            {
                                Distance = Distance * -1;
                            }
                        }

                        if (itemSeg.IsLeft)
                        {
                            FolderInfo.AllLeftCount++;
                            FolderInfo.ServerDataFileListl_Left.Add(new KV() {
                                Name = itemSeg.TestRealIndex + "-" + item.Name + ".csv",
                                Order = itemSeg.Order,
                                StartPos = itemSeg.StartPos,
                                Distance = Distance
                            });
                        }
                        else
                        {
                            FolderInfo.AllRightCount++;
                            FolderInfo.ServerDataFileListl_Right.Add(new KV() {
                                Name = itemSeg.TestRealIndex + "-" + item.Name + ".csv",
                                Order = itemSeg.Order,
                                StartPos = itemSeg.StartPos,
                                Distance = Distance
                            });
                        }
                    }
                }
            }
            FolderInfo.ServerDataFileListl_Left.Sort((a,b)=> a.Order.CompareTo(b.Order));
            FolderInfo.ServerDataFileListl_Right.Sort((a, b) => a.Order.CompareTo(b.Order));
            if (IsCSVorWCF == DataSaveMode.本地_and_远程_CSV || IsCSVorWCF == DataSaveMode.远程_CSV)
            {
                FolderInfo.ServerFolder_Left = _ProjectConfig.DataSaveParam.WcfAddress_Local + "\\" + LeftStr + "-" + TimeStr + "\\";
                FolderInfo.ServerFolder_Right = _ProjectConfig.DataSaveParam.WcfAddress_Local + "\\" + RightStr + "-" + TimeStr + "\\";
                FolderInfo.ServerFolder_Left_WCF = _ProjectConfig.DataSaveParam.WcfAddress + "\\" + LeftStr + "-" + TimeStr + "\\";
                FolderInfo.ServerFolder_Right_WCF = _ProjectConfig.DataSaveParam.WcfAddress + "\\" + RightStr + "-" + TimeStr + "\\";

                if (!Directory.Exists(FolderInfo.ServerFolder_Left))
                {
                    Directory.CreateDirectory(FolderInfo.ServerFolder_Left);
                    Logger.Info(FolderInfo.ID1+" 创建："+ FolderInfo.ServerFolder_Left);
                }
                if (!Directory.Exists(FolderInfo.ServerFolder_Right))
                {
                    Directory.CreateDirectory(FolderInfo.ServerFolder_Right);
                    Logger.Info(FolderInfo.ID2 + " 创建：" + FolderInfo.ServerFolder_Right);
                }

                //写入INI信息
                INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Left + "数据状态.ini", "CONFIG", "State", "0");
                INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Left + "数据状态.ini", "CONFIG", "AllCount", FolderInfo.AllLeftCount.ToString());
                INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Right + "数据状态.ini", "CONFIG", "State", "0");
                INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Right + "数据状态.ini", "CONFIG", "AllCount", FolderInfo.AllRightCount.ToString());
                Logger.Info(FolderInfo.ServerFolder_Left + "数据状态.ini");
                Logger.Info(FolderInfo.ServerFolder_Right + "数据状态.ini");

                //写入分段信息
                foreach (var item in FolderInfo.ServerDataFileListl_Left)
                {
                    INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Left + "数据状态.ini", "LIST", item.Name, "0");
                    //INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Left + "数据状态.ini", item.Name, "StartPos", item.StartPos.ToString());
                    //INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Left + "数据状态.ini", item.Name, "Distance", item.Distance.ToString());
                }
                foreach (var item in FolderInfo.ServerDataFileListl_Right)
                {
                    INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Right + "数据状态.ini", "LIST", item.Name, "0");
                    //INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Right + "数据状态.ini", item.Name, "StartPos", item.StartPos.ToString());
                    //INIHelper.iniHelper.WriteIniData(FolderInfo.ServerFolder_Right + "数据状态.ini", item.Name, "Distance", item.Distance.ToString());
                }
            }

            #region //WCF发送            
            //if (IsCSVorWCF == DataSaveMode.本地_and_远程_CSV || IsCSVorWCF == DataSaveMode.本地_and_远程_WCF ||
            //    IsCSVorWCF == DataSaveMode.远程_CSV || IsCSVorWCF == DataSaveMode.远程_WCF)
            //{
            //    FolderInfo.ServerWCF_Left = new PartResult()
            //    {
            //        StationName = Data.StationName,
            //        TaskName = Data.StationName == StationEnum.S1_A.ToString() ? PartEnum.S1_A_L.ToString() : PartEnum.S1_B_L.ToString(),
            //        FilePath = FolderInfo.ServerFolder_Left_WCF,
            //        CreateTime = DateTime.Now,
            //        ID = FolderInfo.ID1,
            //        IsCSVorWCF = IsCSVorWCF,
            //        Part = Data.StationName == StationEnum.S1_A.ToString() ? PartEnum.S1_A_L : PartEnum.S1_B_L,
            //        FileCount = FolderInfo.AllLeftCount,
            //        Calc_Result = "None",
            //        Measure_Result = "",
            //        FaiInfos = new List<FAIjudge>(),
            //        Is点检 = Is点检

            //    };
            //    for (int i = 0; i < FolderInfo.ServerDataFileListl_Left.Count; i++)
            //    {
            //        FolderInfo.ServerWCF_Left.FList.Add(FolderInfo.ServerDataFileListl_Left[i].Name);
            //    }
            //    FolderInfo.ServerWCF_Right = new PartResult()
            //    {
            //        StationName = Data.StationName,
            //        TaskName = Data.StationName == StationEnum.S1_A.ToString() ? PartEnum.S1_A_R.ToString() : PartEnum.S1_B_R.ToString(),
            //        FilePath = FolderInfo.ServerFolder_Right_WCF,
            //        CreateTime = DateTime.Now,
            //        ID = FolderInfo.ID2,
            //        IsCSVorWCF = IsCSVorWCF,
            //        Part = Data.StationName == StationEnum.S1_A.ToString() ? PartEnum.S1_A_R : PartEnum.S1_B_R,
            //        FileCount = FolderInfo.AllLeftCount,
            //        Calc_Result = "None",
            //        Measure_Result = "",
            //        FaiInfos = new List<FAIjudge>(),
            //        Is点检 = Is点检
            //    }; 
            //    for (int i = 0; i < FolderInfo.ServerDataFileListl_Right.Count; i++)
            //    {
            //        FolderInfo.ServerWCF_Right.FList.Add(FolderInfo.ServerDataFileListl_Right[i].Name);
            //    }
            //    WcfServer.PushDataFileInfo(FolderInfo.ServerWCF_Left);
            //    Logger.Info("WCF发送计算请求:"+ FolderInfo.ServerWCF_Left.ID);
            //    WcfServer.PushDataFileInfo(FolderInfo.ServerWCF_Right);
            //    Logger.Info("WCF发送计算请求:" + FolderInfo.ServerWCF_Right.ID);
            //}
            #endregion

            #endregion
        }

        public void WCF发送任务(ref FolderInfo FolderInfo, CMM_StationInfo_3D Data, DataSaveMode IsCSVorWCF, bool Is点检)
        {
            #region //WCF发送            
            if (IsCSVorWCF == DataSaveMode.本地_and_远程_CSV || IsCSVorWCF == DataSaveMode.本地_and_远程_WCF ||
                IsCSVorWCF == DataSaveMode.远程_CSV || IsCSVorWCF == DataSaveMode.远程_WCF)
            {
                FolderInfo.ServerWCF_Left = new PartResult()
                {
                    StationName = Data.StationName,
                    TaskName = Data.StationName == StationEnum.S1_A.ToString() ? PartEnum.S1_A_L.ToString() : PartEnum.S1_B_L.ToString(),
                    FilePath = FolderInfo.ServerFolder_Left_WCF,
                    CreateTime = DateTime.Now,
                    ID = FolderInfo.ID1,
                    IsCSVorWCF = IsCSVorWCF,
                    Part = Data.StationName == StationEnum.S1_A.ToString() ? PartEnum.S1_A_L : PartEnum.S1_B_L,
                    FileCount = FolderInfo.AllLeftCount,
                    Calc_Result = "None",
                    Measure_Result = "",
                    FaiInfos = new List<FAIjudge>(),
                    Is点检 = Is点检,
                    ProdutionID = FolderInfo.ProdutionID,
                    ParseCount = _ProjectConfig.DataSaveParam.ParseCount,
                    点检模式 = INIhelp.GetValue(Path3D_AB, "Config", "点检模式"),
                    ProdutionIndex = Convert.ToInt32(INIhelp.GetValue(Path3D_AB, "Config", "CurrentProdutionID")),
                    Task路径 = INIhelp.GetValue(Path3D_AB, "Config", "Task路径"),
                };
                for (int i = 0; i < FolderInfo.ServerDataFileListl_Left.Count; i++)
                {
                    FolderInfo.ServerWCF_Left.FList.Add(FolderInfo.ServerDataFileListl_Left[i].Name);
                }
                FolderInfo.ServerWCF_Right = new PartResult()
                {
                    StationName = Data.StationName,
                    TaskName = Data.StationName == StationEnum.S1_A.ToString() ? PartEnum.S1_A_R.ToString() : PartEnum.S1_B_R.ToString(),
                    FilePath = FolderInfo.ServerFolder_Right_WCF,
                    CreateTime = DateTime.Now,
                    ID = FolderInfo.ID2,
                    IsCSVorWCF = IsCSVorWCF,
                    Part = Data.StationName == StationEnum.S1_A.ToString() ? PartEnum.S1_A_R : PartEnum.S1_B_R,
                    FileCount = FolderInfo.AllLeftCount,
                    Calc_Result = "None",
                    Measure_Result = "",
                    FaiInfos = new List<FAIjudge>(),
                    Is点检 = Is点检,
                    ProdutionID = FolderInfo.ProdutionID,
                    ParseCount = _ProjectConfig.DataSaveParam.ParseCount,
                    点检模式 = INIhelp.GetValue(Path3D_AB, "Config", "点检模式"),
                    ProdutionIndex = Convert.ToInt32(INIhelp.GetValue(Path3D_AB, "Config", "CurrentProdutionID")),
                    Task路径 = INIhelp.GetValue(Path3D_AB, "Config", "Task路径"),
                };
                for (int i = 0; i < FolderInfo.ServerDataFileListl_Right.Count; i++)
                {
                    FolderInfo.ServerWCF_Right.FList.Add(FolderInfo.ServerDataFileListl_Right[i].Name);
                }
                WcfServer.PushDataFileInfo(FolderInfo.ServerWCF_Left);
                Logger.Info("WCF发送计算请求:" + FolderInfo.ServerWCF_Left.ID);
                WcfServer.PushDataFileInfo(FolderInfo.ServerWCF_Right);
                Logger.Info("WCF发送计算请求:" + FolderInfo.ServerWCF_Right.ID);
            }
            #endregion
        }

        public void Reset触发信号()
        {
            if (!Directory.Exists(@"D:\3D与运动控制交互\"))
            {
                Directory.CreateDirectory(@"D:\3D与运动控制交互\");
            }
            if (!File.Exists(Path3D_A))
            {
                File.Create(Path3D_A);
            }
            if (!File.Exists(Path3D_B))
            {
                File.Create(Path3D_B);
            }
            if (!File.Exists(fenBuShiPath))
            {
                File.Create(fenBuShiPath);
            }

            INIhelp.SetValue(Path3D_A, "交互信号", "采集端交互信号", 采集端交互信号.不允许采集.ToString());
            Logger.Info(Path3D_A + " 设置 采集端交互信号 = 不允许采集");
            INIhelp.SetValue(Path3D_B, "交互信号", "采集端交互信号", 采集端交互信号.不允许采集.ToString());
            Logger.Info(Path3D_B + " 设置 采集端交互信号 = 不允许采集");
            INIhelp.SetValue(Path3D_A, "交互信号", "采集端交互信号", 采集端交互信号.不允许采集.ToString());
        }

        public void Start触发信号()
        {
            if (!Directory.Exists(@"D:\3D与运动控制交互\"))
            {
                Directory.CreateDirectory(@"D:\3D与运动控制交互\");
            }
            if (!File.Exists(Path3D_A))
            {
                File.Create(Path3D_A);
            }
            if (!File.Exists(Path3D_B))
            {
                File.Create(Path3D_B);
            }

            INIhelp.SetValue(Path3D_A, "交互信号", "采集端交互信号", 采集端交互信号.允许采集.ToString());
            Logger.Info(Path3D_A+ " 设置 采集端交互信号 = 允许采集");
            INIhelp.SetValue(Path3D_B, "交互信号", "采集端交互信号", 采集端交互信号.允许采集.ToString());
            Logger.Info(Path3D_B + " 设置 采集端交互信号 = 允许采集");
        }

        public void 传感器切换参数()
        {
            if (!GetFunc("是否启动不同产品切换Focal参数"))
            {
                return;
            }

            var CurrentProdutionID = Convert.ToInt32(INIhelp.GetValue(Path3D_AB, "Config", "CurrentProdutionID"));
            if (_LastProdutionID != CurrentProdutionID)
            {
                DateTime StartChange = DateTime.Now;
                if (_iFocal_横 != null)
                {
                    _iFocal_横.SetDiffProdutionParam(_ProjectConfig.Foacl_横[CurrentProdutionID]);
                    Logger.Info("_iFocal_横 ("+ _LastProdutionID + ")切换至产品（" + CurrentProdutionID + "）成功！");
                }
                if (_iFocal_竖 != null)
                {
                    _iFocal_竖.SetDiffProdutionParam(_ProjectConfig.Foacl_竖[CurrentProdutionID]);
                    Logger.Info("_iFocal_竖 (" + _LastProdutionID + ")切换至产品（" + CurrentProdutionID + "）成功！");
                }
                _LastProdutionID = CurrentProdutionID;
                Logger.Info("传感器参数切换时间:" + (DateTime.Now - StartChange).TotalMilliseconds + "ms");
            }
        }

        public void 传感器准备采集()
        {
            if (_iFocal_横 != null)
            {
                _iFocal_横.ClearScanResult();
                _iFocal_横.StartMeasure();
            }
            if (_iFocal_竖 != null)
            {
                _iFocal_竖.ClearScanResult();
                _iFocal_竖.StartMeasure();
            }
            if (_iKencye != null)
            {
                _iKencye.ClearScanResult();
                _iKencye.ClearBatch(0);
                _iKencye.StopMeasure(0);
                _iKencye.RestartHighCommunication(false, 0);
                _iKencye.StartMeasure(0);
            }
            if (_iLMI != null)
            {
                _iLMI.ClearScanResult();
                _iLMI.StartMeasure();
            }
        }

    }
}
