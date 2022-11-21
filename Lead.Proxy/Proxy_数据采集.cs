using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lead.CPrim.PrimKeyenceLJ;
using Lead.Tool.CommonData_3D;
using Lead.Tool.Focal;
using Lead.Tool.LMI;
using Lead.Tool.Log;
using Lead.Tool.INI;
using Lead.Tool.XML;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Lead.Proxy
{
    public class CsvInfo
    {
        public PartEnum Part { get; set; }
        public string DataFileName { get; set; } = "";
        public List<int> FilePitch{ get; set; } = new List<int>();
        public List<string> FileName { get; set; } = new List<string>();
        public DataSaveMode IsWCForCSVorNone { get; set; }
        public string IniFile { get; set; }
        public SenserType Type { get; set; }
        public bool IsHotOffset { get; set; }
        public bool IsBase { get; set; }
        public List<List<FSPoint>> Data { get; set; } = new List<List<FSPoint>>();
    }

    public partial class ProxyData
    {
        private string MotionAndDataFile = @"D:\3D与运动控制交互\1.xml";

        Dictionary<string, SenserInfo_State> _SenserInfo = new Dictionary<string, SenserInfo_State>() { };

        List<CMM_StationInfo_3D> _TotalTrigInfo = null;

        public void SaveResultData(object value)
        {
            CsvInfo Info = (CsvInfo)value;

            try
            {
                if ((Info.IsWCForCSVorNone == DataSaveMode.本地_and_远程_CSV || Info.IsWCForCSVorNone == DataSaveMode.远程_CSV)
                    && Info.IsHotOffset == false)
                {
                    double Distance = 0;
                    double StartPos = 0;

                    if (Info.Data.Count > 1)
                    {
                        StartPos = Info.Data[0][0].Y;
                        Distance = Info.Data[1][0].Y - Info.Data[0][0].Y;
                    }
                    
                    INIHelper.iniHelper.WriteIniData(Info.IniFile, Info.DataFileName, "StartPos", StartPos.ToString());
                    INIHelper.iniHelper.WriteIniData(Info.IniFile, Info.DataFileName, "Distance", Distance.ToString());
                    Logger.Info(Info.IniFile + " 写入 " + Info.DataFileName + ":StartPos = " + StartPos + ":Distance = " + Distance);
                }

                if (Info.IsWCForCSVorNone == DataSaveMode.远程_WCF || Info.IsWCForCSVorNone == DataSaveMode.本地_and_远程_WCF)
                {
                    WcfServer.PushCsvInfo(Info);
                    Logger.Info("传输WCF数据");
                }
                for (int i = 0; i < Info.FileName.Count; i++)
                {
                    string FileName = Info.FileName[i];
                    bool IsZ纵向过滤 = GetFunc("Z向过滤");
                    double max = Info.Type == SenserType.Focal ? 100 :
                        Info.Type == SenserType.LMI ? 100:100;
                    double min = Info.Type == SenserType.Focal ? -100 :
                        Info.Type == SenserType.LMI ? -100 : -100;
                    Logger.Info(" 保存开始>>> FileName:" + FileName + " ; IniFile: " + Info.IniFile + " ; DataFileName: " + Info.DataFileName + " ; DataCount: " + Info.Data.Count);

                    FileStream fs = null;
                    StreamWriter sw = null;
                    fs = new FileStream(FileName, System.IO.FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    sw = new StreamWriter(fs);

                    if ((GetFunc("纵向保存") && Info.IsWCForCSVorNone == DataSaveMode.本地) || Info.IsHotOffset)
                    {
                        for (int j = 0; j < Info.Data.Count; j++)
                        {
                            List<FSPoint> item = Info.Data[j];
                            int index = 0;

                            foreach (var item1 in item)
                            {
                                index++;
                                if (IsZ纵向过滤)
                                {
                                    if (item1.Z > max || item1.Z < min) continue;
                                }
                                else
                                {
                                    if (item1.Z > max || item1.Z < min)
                                    {
                                        item1.Z = -99;
                                    }
                                }
                                if (Info.FilePitch[i] == 2)
                                {
                                    if (index == item.Count)
                                    {
                                        sw.Write(item1.X.ToString() + "," + item1.Z.ToString());
                                    }
                                    else
                                    {
                                        sw.Write(item1.X.ToString() + "," + item1.Z.ToString() + ",");
                                    }
                                }
                                if (Info.FilePitch[i] == 3)
                                {
                                    if (index == item.Count)
                                    {
                                        sw.Write(item1.X.ToString() + "," + item1.Y.ToString() + "," + item1.Z.ToString());
                                    }
                                    else
                                    {
                                        sw.Write(item1.X.ToString() + "," + item1.Y.ToString() + "," + item1.Z.ToString() + ",");
                                    }
                                }
                                sw.Write("\n");
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < Info.Data.Count; j++)
                        {
                            List<FSPoint> item = Info.Data[j];
                            int index = 0;
                            foreach (var item1 in item)
                            {
                                index++;
                                if (Info.FilePitch[i] == 2)
                                {
                                    if (index == item.Count)
                                    {
                                        sw.Write(item1.X.ToString() + "," + item1.Z.ToString());
                                    }
                                    else
                                    {
                                        sw.Write(item1.X.ToString() + "," + item1.Z.ToString() + ",");
                                    }
                                }
                                if (Info.FilePitch[i] == 3)
                                {
                                    if (index == item.Count)
                                    {
                                        sw.Write(item1.X.ToString() + "," + item1.Y.ToString() + "," + item1.Z.ToString());
                                    }
                                    else
                                    {
                                        sw.Write(item1.X.ToString() + "," + item1.Y.ToString() + "," + item1.Z.ToString() + ",");
                                    }
                                }
                            }
                            sw.Write("\n");
                        }
                    }

                    sw.Close();
                    fs.Close();

                    Logger.Info(" 保存结束<<< FileName:" + FileName + " ; IniFile: " + Info.IniFile + " ; DataFileName: " + Info.DataFileName + " ; DataCount: " + Info.Data.Count);
                }

                if ((Info.IsWCForCSVorNone == DataSaveMode.本地_and_远程_CSV || Info.IsWCForCSVorNone == DataSaveMode.远程_CSV)
                    && Info.IsHotOffset == false)
                {
                    INIHelper.iniHelper.WriteIniData(Info.IniFile, "LIST", Info.DataFileName, "1");
                    //Logger.Info(Info.IniFile + " 写入 " + Info.DataFileName + "=1");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Info.FileName+ "-保存出错: "+ex.Message);
            }
        }

        public List<List<FSPoint>> Kenyce_ChangeData(List<int[]> lines,  List<double> trigInfo, double offsetZ,bool IsNegative)
        {
            List<List<FSPoint>> Re = new List<List<FSPoint>>();

            //点云Y重新赋值
            for (int i = 0; i < lines.Count; i++)
            {
                List<FSPoint> singleLine = new List<FSPoint>();
                if (IsNegative == true)
                {
                    for (int j = 0; j < lines[i].Length; j++)
                    {
                        FSPoint point = new FSPoint();

                        point.X = -0.0125 * j;
                        point.Y = trigInfo[i];
                        point.Z = (double)(lines[i][j]) / 100000 - offsetZ;
                        point.Intensity = 0;

                        singleLine.Add(point);
                    }
                }
                else
                {
                    for (int j = 0; j < lines[i].Length; j++)
                    {
                        FSPoint point = new FSPoint();

                        point.X = 0.0125 * j;
                        point.Y = trigInfo[i];
                        point.Z = (double)(lines[i][j]) / 100000 - offsetZ;
                        point.Intensity = 0;

                        singleLine.Add(point);
                    }
                }
                Re.Add(singleLine);
            }

            return Re;
        }

        private List<T> Colne<T>(List<T> inputList)
        {
            BinaryFormatter Formadter = new BinaryFormatter(null,new StreamingContext(StreamingContextStates.Clone));

            MemoryStream Stream = new MemoryStream();
            Formadter.Serialize(Stream, inputList);

            Stream.Position = 0;
            var OutList = Formadter.Deserialize(Stream) as List<T>;

            Stream.Close();

            return OutList;
        }

        public List<List<FSPoint>> Focal_ChangeData( List<FSPoint[]> fsLines, List<double> trigInfo, double offsetZ,bool IsNegative)
        {
            DateTime start = DateTime.Now;
            List<List<FSPoint>> Re = new List<List<FSPoint>>();

            //点云Y重新赋值
            if (fsLines == null) 
            { return null; }

            for (int i = 0; i < fsLines.Count; i++)
            {
                if (fsLines[i] == null)
                { 
                    return null; 
                }

                List<FSPoint> temp = new List<FSPoint>();
                if (IsNegative == true)
                {
                    for (int j = 0; j < fsLines[i].Length; j++)
                    {
                        temp.Add(new FSPoint());
                        temp[j].X = fsLines[i][j].X;
                        temp[j].Y = -trigInfo[i];
                        temp[j].Z = fsLines[i][j].Z - offsetZ;
                    }
                }
                else
                {
                    for (int j = 0; j < fsLines[i].Length; j++)
                    {
                        temp.Add(new FSPoint());
                        temp[j].X = fsLines[i][j].X;
                        temp[j].Y = trigInfo[i];
                        temp[j].Z = fsLines[i][j].Z - offsetZ;
                    }
                }
                Re.Add(temp);                                      
            }

            return Re;
        }

        public List<List<FSPoint>> LMI_ChangeData(List<FSPoint[]> fsLines, List<double> trigInfo, double offsetZ, bool IsNegative)
        {
            List<List<FSPoint>> Re = new List<List<FSPoint>>();

            //点云Y重新赋值
            if (fsLines == null)
            { return null; }

            for (int i = 0; i < fsLines.Count; i++)
            {
                if (fsLines[i] == null)
                {
                    return null;
                }

                List<FSPoint> temp = new List<FSPoint>();

                if (IsNegative)
                {
                    for (int j = 0; j < fsLines[i].Length; j++)
                    {
                        temp.Add(new FSPoint());
                        temp[j].X = fsLines[i][j].X;
                        temp[j].Y = -trigInfo[i];
                        temp[j].Z = fsLines[i][j].Z - offsetZ;
                    }
                }
                else
                {
                    for (int j = 0; j < fsLines[i].Length; j++)
                    {
                        temp.Add(new FSPoint());
                        temp[j].X = fsLines[i][j].X;
                        temp[j].Y = trigInfo[i];
                        temp[j].Z = fsLines[i][j].Z - offsetZ;
                    }
                }

                Re.Add(temp);
            }

            return Re;
        }

        private void SaveHotOdffSetValue(string FileName, string value)
        {
            FileStream fs = null;
            StreamWriter sw = null;
            fs = new FileStream(FileName, System.IO.FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            sw = new StreamWriter(fs);
            StringBuilder text0 = new StringBuilder();

            text0.AppendFormat(value);
            sw.WriteLine(text0);

            sw.Close();
            fs.Close();
        }

        private bool CalHotOffsetToConfig(StationEnum Station, string Name, List<CsvInfo> Data, HotOffsetType OffsetData)
        {
            List<List<FSPoint>> AllData = new List<List<FSPoint>>();
            var Range = OffsetData.range;
            double Sum = 0;
            int Count = 0;
            bool Re = true;

            try
            {
                foreach (var item in Data)
                {
                    if (item.Data.Count == 0 || item.IsHotOffset == false)
                    {
                        continue;
                    }

                    //if (Range == 0)
                    //{
                    //    Logger.Error(Station + ":" + Name + " 温飘补偿轨迹Range=" + Range + ",不满足取样条件");
                    //    return false ;
                    //}

                    //if (item.Data.Count < Range)
                    //{
                    //    Logger.Error(Station + ":" + Name + " 温飘补偿轨迹的点小于" + Range + "不满足取样条件");
                    //    return false;
                    //}

                    //int start = item.Data.Count / 2 - Range / 2;
                    //int End = item.Data.Count / 2 + Range / 2;
                    //Logger.Info(Station + ":" + Name + " 温飘采样起始点：" + start + " ~ " + End);

                    //AllData.AddRange(item.Data.GetRange(start, Range));

                    AllData.AddRange(item.Data);
                }

                if (AllData.Count == 0)
                {
                    Logger.Info(Station + ":" + Name + " 没有温漂轨迹，不进行温漂补偿的更新");
                    return true; 
                }

                foreach (var item1 in AllData)
                {
                    if (item1.Count < OffsetData.EndSeg)
                    {
                        Logger.Error(Station + ":" + Name + " 温飘补偿单个触发点的采样率小于" + OffsetData.EndSeg + ";不满足取样条件");
                        return false;
                    }
                    for(int i = OffsetData.StartSeg; i< OffsetData.EndSeg; i++)
                    {
                        if (item1[i].Z < OffsetData.Max && item1[i].Z > OffsetData.Min)
                        {
                            Count++;
                            Sum += item1[i].Z;
                        }
                    }
                }

                if (Count<= 100)
                {
                    Logger.Error(Station + ":" + Name + " 温飘补偿的有效采样点数=" + Count + ",不满足取样条件");
                    return false;
                }

                bool 合法数据 = true;
                if ( (DateTime.Now - OffsetData.LastTime).TotalMinutes < OffsetData.OffsetAllowTime )
                {
                    if (Math.Abs(Sum / Count - OffsetData.Last) > OffsetData.OffsetAllowRange)
                    {
                        合法数据 = false;
                        Logger.ShowTickTipsForm("温漂数据不合法", Name+ " 温漂更新错误：" +OffsetData.OffsetAllowTime + "分钟内，温漂数据跳动值(" + Math.Abs(Sum / Count - OffsetData.Last).ToString("f6") + ") > 设定的跳动范围(" + OffsetData.OffsetAllowRange + ")，请检查温漂块无移动后，复位数据采集端！");
                        Logger.Warn(OffsetData.OffsetAllowTime +"分钟内，温漂数据跳动值("+ Math.Abs(Sum / Count - OffsetData.Last) + ") > 设定的跳动范围("+ OffsetData.OffsetAllowRange + ")");
                    }
                }

                if (合法数据)
                {
                    OffsetData.LastTime = DateTime.Now;
                    OffsetData.Last = Sum / Count;
                    OffsetData.offset = OffsetData.Last - OffsetData.Stand;

                    Logger.OK(Station + ":" + Name + " 温漂Stand：" + OffsetData.Stand);
                    Logger.OK(Station + ":" + Name + " 温漂Offset：" + OffsetData.offset);
                    Logger.Info(Station + ":" + Name + " 温飘值：" + OffsetData.Last);
                    Logger.Info(Station + ":" + Name + " 温飘值更新时间：" + OffsetData.LastTime.ToString("yyy/MM/dd-HH:mm:ss"));

                    XmlSerializerHelper.WriteXML(_ProjectConfig, _ConfigPath, typeof(ConfigParam));
                    Logger.OK(Station + ":" + Name + " 的温漂参数,自动保存");
                    // 增加AB工站不同的保存文件，用于区分
                    var strFileName = _ProjectConfig.DataSaveParam.HotOffsetDic + "\\" + Station + "-" + OffsetData.Name + "-" + DateTime.Now.ToString("yyy_MM_dd") + ".csv";
                    var strValue = DateTime.Now.ToString("yyy/MM/dd-HH:mm:ss") + "," + OffsetData.offset;
                    SaveHotOdffSetValue(strFileName, strValue);
                }
                else
                {
                    Logger.Error("温漂数据不合法,当前无法更新温漂数据");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Station + ":" + Name + " 温漂值参数更新失败:" + ex.Message);
                return false;
            }

            return true;
        }

        private void CalTrigTimeDelay(StationEnum Station, string Name, List<CsvInfo> Data)
        {
            try
            {
                CsvInfo Base = null;
                foreach (var item in Data)
                {
                    if (item.IsBase)
                    {
                        Base = item;
                        break;
                    }
                }

                if (Base != null)
                {
                    foreach (var item in Data)
                    {
                        if (!item.IsBase)
                        {
                            int count = 0;
                            double Ave = 0;
                            //Y  Need = 1
                            for (int i = 0; i < item.Data.Count; i++)
                            {
                                //X
                                for (int j = _ProjectConfig.DataSaveParam.延时触发开始段; j < _ProjectConfig.DataSaveParam.延时触发结束段; j++)
                                {
                                    if (item.Data[i][j].Z < 100 && item.Data[i][j].Z > -100 && Base.Data[i][j].Z < 100 && Base.Data[i][j].Z > -100)
                                    {
                                        Ave += item.Data[i][j].Z - Base.Data[i][j].Z;
                                        count++;
                                    }

                                }
                            }
                            if (count != 0)
                            {
                                Logger.OK(item.DataFileName + " Z平均为" + Ave / count);
                            }
                            else
                            {
                                Logger.Warn(item.DataFileName + " Z有效值不足");
                            }
                        }

                    }
                }
                else
                {
                    Logger.Error("验证触发延时时，没有选取基准触发点，须在触发轨迹表中选择");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("验证触发延时出错："+ex.Message);
            }        }

        public void TaskData(string Name)
        {
            SenserInfo_State State = _SenserInfo[Name];
            List<FSPoint[]> Result_Focal = new List<FSPoint[]>();
            List<int[]> Result_Kence = new List<int[]>();
            List<FSPoint[]> Result_LMI = new List<FSPoint[]>();
            var strStation = State.Station.ToString();
            DateTime Start = DateTime.Now;
            List<CsvInfo> DataSave = new List<CsvInfo>();
            //Dictionary<StationEnum, List<CsvInfo>> DataSave = new Dictionary<StationEnum, List<CsvInfo>>();
            //DataSave.Add(StationEnum.S1_A, new List<CsvInfo>());
            //DataSave.Add(StationEnum.S1_B, new List<CsvInfo>());
            StationEnum SaveStation = State.Station;
            string PathTak = State.Station == StationEnum.S1_A ? Path3D_A :
                    State.Station == StationEnum.S1_B ? Path3D_B : "错误的信号交互路径";

            while (IsAllRun)
            {
                Thread.Sleep(1);

                if (State.IsStart == false)
                {
                    Start = DateTime.Now;
                    continue;
                }

                //截取段数累加
                if (State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset && !State.是否包括热飘移)
                {
                    State.StartSeg = State.EndSeg;
                    State.EndSeg = State.StartSeg + 0;
                    State.CountSeg = 0;
                    Logger.Info(Name+": 第" + State.CurrentSegIndex+"段触发点，为热飘移点，但不需要采集相应数据");
                }
                else 
                {
                    //if (State.TrigInfo.Type  == SenserType.LMI && State.CurrentSegIndex== 0 && 
                    //    !State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset && State.是否包括热飘移)
                    //{
                    //    State.StartSeg = State.EndSeg;
                    //    State.EndSeg = State.StartSeg + State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList.Count-3;
                    //    State.CountSeg = State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList.Count-3;
                    //    Logger.Info("LMI非温漂轨迹第一段减去3个点，StartSeg ，EndSeg ，CountSeg ="+ State.StartSeg + " , " +  State.EndSeg +" , " + State.CountSeg);
                    //}
                    //else
                    if (State.TrigInfo.Type == SenserType.基恩士 && State.CurrentSegIndex == 0 &&
                        !State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset && State.是否包括热飘移)
                    {
                        State.StartSeg = State.EndSeg;
                        State.EndSeg = State.StartSeg + State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList.Count - 3;
                        State.CountSeg = State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList.Count - 3;
                        Logger.Info("Keyence非温漂轨迹第一段减去3个点，StartSeg ，EndSeg ，CountSeg =" + State.StartSeg + " , " + State.EndSeg + " , " + State.CountSeg);
                    }
                    else
                    {
                        State.StartSeg = State.EndSeg;
                        State.EndSeg = State.StartSeg + State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList.Count;
                        State.CountSeg = State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList.Count;
                    }
                }
                //针对focal第一段数据丢失问题
                //if (State.IsFirstAfterInitFocal && State.TrigInfo.Type == SenserType.Focal)
                //{
                //    State.EndSeg--;
                //    State.CountSeg--;
                //    State.IsFirstAfterInitFocal = false;
                //}
                Logger.Info(State.TrigInfo.Name + "：StartSeg=" + State.StartSeg + "; CountSeg=" + State.CountSeg + "; EndSeg=" + State.EndSeg + "; TestRealIndex=" + State.TrigInfo.SegmentList[State.CurrentSegIndex].TestRealIndex);

                //第N段数据读取及保存
                DateTime StartGetScanResult = DateTime.Now;
                bool IsStillGetData = true;
                while (IsAllRun && State.IsStart == true && IsStillGetData)
                {
                    Thread.Sleep(5);

                    switch (State.TrigInfo.Type)
                    {
                        case SenserType.Focal:
                            Result_Focal = ((FocalTool)_ToolConfig.ToolManager[State.TrigInfo.Name]).GetScanResult();
                            break;
                        case SenserType.LMI:
                            Result_LMI = ((LMITool)_ToolConfig.ToolManager[State.TrigInfo.Name]).GetScanResult();
                            break;
                        case SenserType.基恩士:
                            Result_Kence = ((KeyenceTool)_ToolConfig.ToolManager[State.TrigInfo.Name]).GetScanResult();
                            break;
                    }

                    switch (State.TrigInfo.Type)
                    {
                        case SenserType.Focal:
                            if (Result_Focal.Count < State.EndSeg)
                            {                
                                //最后一段
                                if (State.IsErrContinue
                                    && ((Result_Focal.Count + ProjectConfig.DataSaveParam.AllowMissPoinNumber) >= State.EndSeg))
                                {
                                    var xxx = (State.EndSeg - Result_Focal.Count);
                                    State.EndSeg -= xxx;
                                    State.CountSeg -= xxx;
                                    Logger.Info(Name + " 在触发结束后的规定时间内，没有收到足够的点云， State.StartSeg = " + State.StartSeg + "； State.EndSeg = " + State.EndSeg+ "; State.CountSeg = " + State.CountSeg);
                                    break;
                                }
                                continue;
                            }
                            else
                            {
                                Logger.Info(Name + " 总数目：" + Result_Focal.Count);
                                IsStillGetData = false;
                            }
                            break;
                        case SenserType.LMI:
                            if (Result_LMI.Count < State.EndSeg)
                            { 
                                //最后一段
                                if (State.IsErrContinue
                                    && ((Result_LMI.Count + ProjectConfig.DataSaveParam.AllowMissPoinNumber) >= State.EndSeg))
                                {
                                    var xxx = (State.EndSeg - Result_LMI.Count);
                                    State.EndSeg -= xxx;
                                    State.CountSeg -= xxx;
                                    Logger.Info(Name + " 在触发结束后的规定时间内，没有收到足够的点云， State.StartSeg = " + State.StartSeg + "； State.EndSeg = " + State.EndSeg + "; State.CountSeg = " + State.CountSeg);
                                    break;
                                }
                                continue;
                            }
                            else
                            {
                                Logger.Info(Name + " 总数目：" + Result_LMI.Count);
                                IsStillGetData = false;
                            }
                            break;
                        case SenserType.基恩士:
                            if (Result_Kence.Count < State.EndSeg)
                            {
                                //最后一段
                                if (State.IsErrContinue
                                    && ((Result_Kence.Count + ProjectConfig.DataSaveParam.AllowMissPoinNumber) >= State.EndSeg))
                                {
                                    var xxx = (State.EndSeg - Result_Kence.Count);
                                    State.EndSeg -= xxx;
                                    State.CountSeg -= xxx;
                                    Logger.Info(Name + " 在触发结束后的规定时间内，没有收到足够的点云， State.StartSeg = " + State.StartSeg + "； State.EndSeg = " + State.EndSeg + "; State.CountSeg = " + State.CountSeg);
                                    break;
                                }
                                continue;
                            }
                            else
                            {
                                Logger.Info(Name + " 总数目：" + Result_Kence.Count);
                                IsStillGetData = false;
                            }

                            break;
                    }
                }
                Logger.Info(Name + " GetScanResult: " + (DateTime.Now - StartGetScanResult).TotalMilliseconds + "ms");

                //温飘补偿      
                HotOffsetType HotOffsetValue = new HotOffsetType();
                if (IsAllRun && State.IsStart == true)
                {
                    bool IsFined = false;
                    foreach (var item in _ProjectConfig.HotOffsetConfig)
                    {
                        if (item.Station == State.Station && item.Name == Name)
                        {
                            HotOffsetValue = item;
                            IsFined = true;
                            break;
                        }
                    }
                    if (IsFined == false)
                    {
                        //throw new Exception("未找到温漂参数："+Name);
                        _ProjectConfig.HotOffsetConfig.Add(new HotOffsetType()
                        {
                            Name = Name,
                            Station = State.Station,
                            range = 2,
                        });
                        Logger.OK(State.Station + ":" + Name + " 的温漂参数,未在配置文件中找到，自动添加");
                    }
                }

                //提取保存数据
                if (IsAllRun && State.IsStart == true)
                {
                    DataSave.Add(new CsvInfo());
                    DataSave[State.CurrentSegIndex].IsHotOffset = State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset;
                    DataSave[State.CurrentSegIndex].IsBase = State.TrigInfo.SegmentList[State.CurrentSegIndex].IsBase;

                    if (State.TrigInfo.SegmentList[State.CurrentSegIndex].IsLeft)
                    {
                        DataSave[State.CurrentSegIndex].Part = State.Station == StationEnum.S1_A ? PartEnum.S1_A_L : PartEnum.S1_B_L;
                    }
                    else
                    {
                        DataSave[State.CurrentSegIndex].Part = State.Station == StationEnum.S1_A ? PartEnum.S1_A_R : PartEnum.S1_B_R;
                    }
                    DataSave[State.CurrentSegIndex].IsWCForCSVorNone = State.IsWCForCSV;
                    DataSave[State.CurrentSegIndex].DataFileName = State.TrigInfo.SegmentList[State.CurrentSegIndex].TestRealIndex + "-" + State.TrigInfo.Name + ".csv";

                    DateTime StartChangeData = DateTime.Now;
                    if (State.TrigInfo.Name == "Focal_横")
                    {
                        var reFocal_h = Result_Focal.GetRange(State.StartSeg, State.CountSeg);
                        var IsNegativeFocal_h = State.Station == StationEnum.S1_B ? true : false;
                        var offsetZ_Focal_h = State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset ? 0 : HotOffsetValue.offset;
                        DataSave[State.CurrentSegIndex].Data = Focal_ChangeData(reFocal_h, State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList, offsetZ_Focal_h, IsNegativeFocal_h);
                        DataSave[State.CurrentSegIndex].Type = SenserType.Focal;
                    }
                    else if (State.TrigInfo.Name == "Focal_竖")
                    {
                        var reFocal_s = Result_Focal.GetRange(State.StartSeg, State.CountSeg);
                        bool IsNegativeFocal_s;

                        if(GetFunc("Focal_竖Y轴是否取反"))
                        {
                            IsNegativeFocal_s = State.Station == StationEnum.S1_B ? true : false;
                        }
                        else
                        {
                            IsNegativeFocal_s = State.Station == StationEnum.S1_B ? false : true;
                        }
                        var offsetZ_Focal_s = State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset ? 0 : HotOffsetValue.offset;
                        DataSave[State.CurrentSegIndex].Data = Focal_ChangeData(reFocal_s, State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList, offsetZ_Focal_s, IsNegativeFocal_s);
                        DataSave[State.CurrentSegIndex].Type = SenserType.Focal;
                    }
                    else if (State.TrigInfo.Name == "LMI")
                    {
                        var reLMI = Result_LMI.GetRange(State.StartSeg, State.CountSeg);
                        var IsNegativeLMI = State.Station == StationEnum.S1_B ? true : false;
                        var offsetZ_LMI = State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset ? 0 : HotOffsetValue.offset;
                        DataSave[State.CurrentSegIndex].Data = LMI_ChangeData(reLMI, State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList, offsetZ_LMI, IsNegativeLMI);
                        DataSave[State.CurrentSegIndex].Type = SenserType.LMI;
                    }
                    else if (State.TrigInfo.Name == "Keyence")
                    {
                        var reKeyence = Result_Kence.GetRange(State.StartSeg, State.CountSeg);
                        var IsNegativeKeyence = State.Station == StationEnum.S1_B ? true : false;
                        var offsetZ_Keyence = State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset ? 0 : HotOffsetValue.offset;
                        DataSave[State.CurrentSegIndex].Data = Kenyce_ChangeData(reKeyence, State.TrigInfo.SegmentList[State.CurrentSegIndex].TrigPosList, offsetZ_Keyence, IsNegativeKeyence);
                        DataSave[State.CurrentSegIndex].Type = SenserType.基恩士;
                    }
                    Logger.Info(Name + " ChangeData: "+(DateTime.Now- StartChangeData).TotalMilliseconds);

                    if (State.TrigInfo.SegmentList[State.CurrentSegIndex].IsHotOffset == false)
                    {
                        //本地
                        if (State.IsWCForCSV == DataSaveMode.本地 ||
                            State.IsWCForCSV == DataSaveMode.本地_and_远程_CSV ||
                            State.IsWCForCSV == DataSaveMode.本地_and_远程_WCF)
                        {
                            if (State.TrigInfo.SegmentList[State.CurrentSegIndex].IsLeft)
                            {
                                DataSave[State.CurrentSegIndex].FileName.Add(State.LocalFolder_Left + State.TrigInfo.SegmentList[State.CurrentSegIndex].TestRealIndex + "-" + State.TrigInfo.Name + ".csv");
                            }
                            else
                            {
                                DataSave[State.CurrentSegIndex].FileName.Add(State.LocalFolder_Right + State.TrigInfo.SegmentList[State.CurrentSegIndex].TestRealIndex + "-" + State.TrigInfo.Name + ".csv");
                            }
                            DataSave[State.CurrentSegIndex].FilePitch.Add(3);
                        }

                        //远端
                        if (State.IsWCForCSV == DataSaveMode.远程_CSV ||
                            State.IsWCForCSV == DataSaveMode.本地_and_远程_CSV)
                        {
                            if (State.TrigInfo.SegmentList[State.CurrentSegIndex].IsLeft)
                            {
                                DataSave[State.CurrentSegIndex].IniFile = State.ServerFolder_Left + "数据状态.ini";
                                DataSave[State.CurrentSegIndex].FileName.Add(State.ServerFolder_Left + State.TrigInfo.SegmentList[State.CurrentSegIndex].TestRealIndex + "-" + State.TrigInfo.Name + ".csv");
                                Logger.Info(Name + " 添加保存的csv数据：" + State.ServerFolder_Left + State.TrigInfo.SegmentList[State.CurrentSegIndex].TestRealIndex + "-" + State.TrigInfo.Name + ".csv");
                            }
                            else
                            {
                                DataSave[State.CurrentSegIndex].IniFile = State.ServerFolder_Right + "数据状态.ini";
                                DataSave[State.CurrentSegIndex].FileName.Add(State.ServerFolder_Right + State.TrigInfo.SegmentList[State.CurrentSegIndex].TestRealIndex + "-" + State.TrigInfo.Name + ".csv");
                                Logger.Info(Name + " 添加保存的csv数据：" + State.ServerFolder_Right + State.TrigInfo.SegmentList[State.CurrentSegIndex].TestRealIndex + "-" + State.TrigInfo.Name + ".csv");
                            }
                            DataSave[State.CurrentSegIndex].FilePitch.Add(_ProjectConfig.DataSaveParam.ParseCount) ;
                        }
                    }
                    else
                    {
                        if (_ProjectConfig.DataSaveParam.IsSaveHotData && State.是否包括热飘移)
                        {
                            DataSave[State.CurrentSegIndex].FileName.Add(_ProjectConfig.DataSaveParam.HotOffsetDic + "\\"
                                + State.Station.ToString() + "_" + Name + "\\"
                                + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".csv");
                            DataSave[State.CurrentSegIndex].FilePitch.Add(3);
                            Logger.Info(DataSave[State.CurrentSegIndex].FileName[DataSave[State.CurrentSegIndex].FileName.Count-1]+" 温漂数据开始保存");
                        }
                    }

                    DateTime StartSave = DateTime.Now;
                    //开线程存数据
                    foreach (var item in DataSave[State.CurrentSegIndex].FileName)
                    {
                        Logger.Info(Name+" 开始保存文件："+ item);
                    }
                    ThreadPool.QueueUserWorkItem(SaveResultData, DataSave[State.CurrentSegIndex]);
                    //Thread Thread_Save = new Thread(SaveResultData);
                    //Thread_Save.Start(DataSave[State.CurrentSegIndex]);
                    Logger.Info(Name + " ThreadSaveResultData: " + (DateTime.Now - StartSave).TotalMilliseconds);

                    //计数段累加
                    State.CurrentSegIndex++;
                    Logger.Info(State.TrigInfo.Name + " CurrentSegIndex =" + State.CurrentSegIndex);

                    //保存完毕后退出
                    if (State.CurrentSegIndex == State.TrigInfo.SegmentList.Count)
                    {
                        var hotoffsetRE = CalHotOffsetToConfig(State.Station, Name, DataSave, HotOffsetValue);

                        if (State.是否验证触发延时)
                        {
                            CalTrigTimeDelay(State.Station, Name, DataSave);
                        }

                        DataSave.Clear();
                        Logger.Info(State.TrigInfo.Name + " TaskData已完成一轮循环 : DataSave[SaveStation].Clear ; CT(ms) = " + (DateTime.Now - Start).TotalMilliseconds);

                        if (!hotoffsetRE)
                        {
                            State.IsErr = true;
                            Logger.Error(State.TrigInfo.Name + " TaskData已完成一轮循环,但是计算温漂时出现错误 ");
                        }
                        else
                        {
                            State.IsErr = false;
                        }

                        State.CurrentSegIndex = 0;
                        State.IsStart = false;
                        SaveStation = State.Station;
                        Logger.Info(State.TrigInfo.Name + " TaskData已完成一轮循环 : IsStart = false ; CT(ms) = " + (DateTime.Now - Start).TotalMilliseconds);
                    }
                }
            }
        }

        public void InitTrigInfo()
        {
            var last = File.GetLastWriteTime(MotionAndDataFile);//获取文件最后修改时间（运动控制和数据采集交互\1.xml）

            if (last.CompareTo(LastTrigInfoSaveTime) != 0)
            {
                ParseCommonTrigInfo TrigXML = new ParseCommonTrigInfo();
                _TotalTrigInfo = TrigXML.ReadTrigInfo(MotionAndDataFile);
            }

            foreach (var itemStation in _TotalTrigInfo)
            {
                foreach (var itemSenser in itemStation.SenserList)
                {
                    if (!_SenserInfo.ContainsKey(itemSenser.Name))
                    {
                        _SenserInfo.Add(itemSenser.Name, new SenserInfo_State()
                        {
                            TrigInfo = null,
                            IsFirstAfterInitFocal = true,
                        });
                    }
                }
            }
        }

        private void StartTrigTask()
        {
            if (_SenserInfo != null)
            {
                foreach (var item in _SenserInfo)
                {
                    if (item.Value.Thread == null)
                    {
                        item.Value.Thread = new Thread((x) => { TaskData((string)x); });
                        item.Value.Thread.Priority = ThreadPriority.Highest;
                        item.Value.Thread.Start(item.Key);
                        Logger.Info(item.Key + " 数据监听线程开始成功");
                    }
                }
            }
        }

        private void StopTrigTask()
        {
            if (_SenserInfo != null)
            {
                foreach (var item in _SenserInfo)
                {
                    if (item.Value.Thread != null)
                    {
                        item.Value.Thread.Abort();
                    }

                    item.Value.Thread = null;
                }
            }
        }
    }
}
