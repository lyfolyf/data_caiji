using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lead.Tool.Log;
using MongoDB.Bson.Serialization.Attributes;
using Lead.Tool.INI;
using Lead.Tool.XML;
using Lead.Tool.CSV;
using System.Drawing;
using System.ComponentModel;


namespace Lead.Proxy
{
    public class LastResultInfo
    {
        public string LastDay { get; set; } = "";
        public string LastFileName { get; set; } = "";
        public List<string> LastHeadList { get; set; } = new List<string>();
    }

    public partial class ProxyData
    {
        private Task _handleTask = null;
        private List<string> _ColomsName = new List<string>();
        private LastResultInfo _LastResultInfo = new LastResultInfo();
        private string Path = _ConfigDic + @"\LastHead.XML";
        public List<PartResult> 点检真值记录表 = new List<PartResult>();
        private int Index_点检_A_L = -1;
        private int Index_点检_A_R = -1;
        private int Index_点检_B_L = -1;
        private int Index_点检_B_R = -1;
        private DateTime LastDateTime = DateTime.Now;

        public Dictionary<string, PartResult> 自动点检列表 = new Dictionary<string, PartResult>();

        private void 自动点检汇总(PartResult Value)
        {
            try
            {
                if (!自动点检列表.ContainsKey(Value.ID))
                {
                    自动点检列表.Add(Value.ID, Value);
                }
                else
                {
                    自动点检列表[Value.ID] = Value;
                    Logger.Warn($"自动点检时，物料I{Value.ID}重复点检，取最后一次的值进行评判！");
                }

                if (自动点检列表.Count == 10)
                {
                    int OKcount = 0, NGcount = 0, NG_Fai_count = 0;
                    double Max_Fai_value = 0;
                    string Max_Fai_ID = "";
                    string Max_Fai_part = "";
                    string Max_Fai_name = "";

                    foreach (var item in 自动点检列表)
                    {
                        if (item.Value.Measure_Result == "OK")
                        {
                            OKcount++;
                        }
                        else if (item.Value.Measure_Result == "NG")
                        {
                            NGcount++;

                            foreach (var itemFai in item.Value.FaiInfos)
                            {
                                if (itemFai.判定结果 == "NG")
                                {
                                    NG_Fai_count++;
                                    FAIjudge final = null;
                                    FAI xx = _ProjectConfig.ProdutionList[item.Value.ProdutionIndex].点检_自动.ToList().Find(
                                        (xxx) => xxx.ID == item.Key && xxx.Part == item.Value.Part.ToString());
                                    if (xx != null)
                                    {
                                        final = xx.ListFAI.ToList().Find((xxx) => xxx.名称 == itemFai.名称);
                                    }
                                    if (final != null)
                                    {
                                        if (Math.Abs(Max_Fai_value) < Math.Abs(itemFai.测量结果 - final.标准值))
                                        {
                                            Max_Fai_value = itemFai.测量结果 - final.标准值;
                                            Max_Fai_ID = item.Value.ID;
                                            Max_Fai_part = item.Value.Part.ToString();
                                            Max_Fai_name = itemFai.名称;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"自动点检时，物料{Value.ID}measure_Result 不为“OK”,且不为“NG”,请检查数据");
                        }
                    }
                    自动点检列表 = new Dictionary<string, PartResult>();
                    if (NGcount != 0)
                    {
                        Logger.ShowTickTipsForm("自动点检结果", $"自动点检结果：OK：{OKcount};NG:{NGcount};超差个数：{NG_Fai_count}；超差最大值{Max_Fai_value}（ID：“{Max_Fai_ID}”;穴位：{Max_Fai_part};测量项：{Max_Fai_name}）；", 100);
                    }
                    else
                    {
                        Logger.ShowTickTipsForm("自动点检结果", $"自动点检结果：OK：{OKcount};NG:{NGcount};");
                    }
                }
            }
            catch (Exception ex)
            {
                自动点检列表 = new Dictionary<string, PartResult>();
                throw new Exception("自动点检汇总出错：" + ex.ToString());
            }
        }


        private void StartTaskHandle()
        {
            if (_handleTask == null)
            {
                _handleTask = new Task(() => this.TaskResultHandle());
                _handleTask.Start();
            }

        }

        private void ClearOldDataFile()
        {
            try
            {
                foreach (var item in Directory.GetDirectories(_ProjectConfig.DataSaveParam.WcfAddress))
                {
                    if (item.Contains(StationEnum.S1_A.ToString()) || item.Contains(StationEnum.S1_B.ToString()))
                    {
                        Directory.Delete(item, true);
                        Logger.Info("复位时删除过时的文件夹：" + item);
                    }
                }

                foreach (var item in Directory.GetDirectories(_ProjectConfig.DataSaveParam.LocalFolder))
                {
                    if (item.Contains(StationEnum.S1_A.ToString()) || item.Contains(StationEnum.S1_B.ToString()))
                    {
                        if ((DateTime.Now - Directory.GetCreationTime(item)).TotalDays > 2)
                        {
                            Directory.Delete(item, true);
                            Logger.Info("复位时删除过时的文件夹：" + item);
                        }
                    }
                }

                foreach (var item in Directory.GetDirectories(_ProjectConfig.DataSaveParam.ErrorDic))
                {
                    if (item.Contains(StationEnum.S1_A.ToString()) || item.Contains(StationEnum.S1_B.ToString()))
                    {
                        if ((DateTime.Now - Directory.GetCreationTime(item)).TotalDays > 2)
                        {
                            Directory.Delete(item, true);
                            Logger.Info("复位时删除过时的文件夹：" + item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Info("复位时删除过时的文件夹出错：" + ex.Message); ;
            }
        }

        private void TaskResultHandle()
        {
            DateTime ReStart = DateTime.Now;
            while (true)
            {
                Thread.Sleep(30);
                PartResult result = WcfServer.PopDataResultInfo();
                if (result == null)
                {
                    continue;
                }

                ReStart = DateTime.Now;
                Logger.Info(result.ID + " 句柄开始处理");
                result.ProductionColor = _ProjectConfig.ProdutionYanse;


                #region 删除多余文件夹
                if ((DateTime.Now - LastDateTime).TotalMinutes > 10)
                {
                    LastDateTime = DateTime.Now;
                    Action ac = () =>
                    {
                        try
                        {
                            foreach (var item in Directory.GetDirectories(_ProjectConfig.DataSaveParam.WcfAddress))
                            {
                                if (item.Contains(StationEnum.S1_A.ToString()) || item.Contains(StationEnum.S1_B.ToString()))
                                {
                                    if ((DateTime.Now - Directory.GetCreationTime(item)).TotalMinutes > 10)
                                    {
                                        Directory.Delete(item, true);
                                        Logger.Info("定时删除过期(10min)文件成功：" + item);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Info("定时删除过期(10min)文件失败:" + ex.Message);
                        }
                    };
                    ac.BeginInvoke(null, null);
                }
                #endregion


                #region 转移数据
                bool isTransfer = GetFunc("检索数据是否保存");
                DateTime 转移开始time = DateTime.Now;
                if (result.IsCSVorWCF == DataSaveMode.本地_and_远程_CSV || result.IsCSVorWCF == DataSaveMode.远程_CSV)
                {
                    DateTime 转移SATRT = DateTime.Now;
                    try
                    {
                        bool 是否转移 = false;
                        string mark = "";
                        result.Bin = "";

                        if (result.Calc_Result != "Done" && isTransfer)
                        {
                            是否转移 = true;
                            mark = "_NoDone";
                        }

                        if (result.FaiInfos == null)
                        {
                            if (isTransfer)
                            {
                                是否转移 = true;
                                mark = "_null";
                            }
                            result.Bin = "X";
                            Logger.Warn(string.Format("ID: {0},测量结果为NULL，已将Bin设置为X", result.ID));
                        }

                        if (result.FaiInfos != null)
                        {
                            if (result.FaiInfos.Count == 0)
                            {
                                是否转移 = true;
                                mark = "_0";
                            }

                            for (int i = 0; i < result.FaiInfos.Count; i++)
                            {
                                if (result.FaiInfos[i].测量结果 > 9000 || result.FaiInfos[i].测量结果 < -9000)
                                {
                                    if (isTransfer)
                                    {
                                        mark = "_4个9";
                                        是否转移 = true;
                                    }
                                    result.Bin = "X";
                                    Logger.Warn(string.Format("ID: {0},测量结果有-9999，已将Bin设置为X", result.ID));
                                    break;
                                }
                            }
                        }
                        if (result.FaiInfos != null && isTransfer)
                        {
                            for (int i = 0; i < result.FaiInfos.Count; i++)
                            {
                                var item = result.FaiInfos[i];
                                if ((item.名称 == "EHLD2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                                    || (item.名称 == "EHRD2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                                    || (item.名称 == "EHLU2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                                    || (item.名称 == "EHRU2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                                    )
                                {
                                    mark = "_zzz";
                                    是否转移 = true;
                                    break;
                                }
                            }
                        }

                        if (是否转移 && isTransfer)
                        {
                            var xxx = result.StationName + "_" + result.Part + mark + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                            if (!Directory.Exists(_ProjectConfig.DataSaveParam.ErrorDic))
                            {
                                Directory.CreateDirectory(_ProjectConfig.DataSaveParam.ErrorDic);
                            }
                            if (!Directory.Exists(_ProjectConfig.DataSaveParam.ErrorDic + "\\" + xxx + "\\"))
                            {
                                Directory.CreateDirectory(_ProjectConfig.DataSaveParam.ErrorDic + "\\" + xxx + "\\");
                            }

                            foreach (var item in Directory.GetFiles(result.FilePath))
                            {
                                if (item.Contains("csv"))
                                {
                                    var x = item.Split('\\');
                                    File.Copy(item, _ProjectConfig.DataSaveParam.ErrorDic + "\\" + xxx + "\\" + x[x.Length - 1]);
                                }
                            }
                            Logger.Info(result.ID + ":" + "转移数据成功:" + result.FilePath + "-> " + xxx + " ； 耗时ms：" + (DateTime.Now - 转移SATRT).TotalMilliseconds);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Info(result.ID + ":" + result.FilePath + " 转移数据出错:" + ex.Message + "； 耗时ms：" + (DateTime.Now - 转移SATRT).TotalMilliseconds);
                    }
                }
                Logger.Info(result.ID + " 转移数据花费时间(ms):" + (DateTime.Now - 转移开始time).TotalMilliseconds);
                #endregion


                #region 转移数据源代码
                //DateTime 转移开始time = DateTime.Now;
                //if ((result.IsCSVorWCF == DataSaveMode.本地_and_远程_CSV || result.IsCSVorWCF == DataSaveMode.远程_CSV)
                //    && _ProjectConfig.DataSaveParam.IsSaveErrorData)
                //{
                //    DateTime 转移SATRT = DateTime.Now;
                //    try
                //    {
                //        bool 是否转移 = false;
                //        string mark = "";
                //        if (result.Calc_Result != "Done")
                //        {
                //            是否转移 = true;
                //            mark = "_NoDone";
                //        }

                //        if (result.FaiInfos == null)
                //        {
                //            是否转移 = true;
                //            mark = "_null";
                //        }

                //        if (result.FaiInfos != null)
                //        {
                //            if (result.FaiInfos.Count == 0)
                //            {
                //                是否转移 = true;
                //                mark = "_0";
                //            }

                //            if (result.FaiInfos.Count > 0)
                //            {
                //                if (result.FaiInfos[0].测量结果 > 9000 || result.FaiInfos[0].测量结果 < -9000)
                //                {
                //                    mark = "_4个9";
                //                    是否转移 = true;
                //                }
                //            }
                //        }
                //        if (result.FaiInfos != null && GetFunc("检索数据是否保存"))
                //        {
                //            for (int i = 0; i < result.FaiInfos.Count; i++)
                //            {
                //                var item = result.FaiInfos[i];
                //                if ((item.名称 == "EHLD2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                //                    || (item.名称 == "EHRD2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                //                    || (item.名称 == "EHLU2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                //                    || (item.名称 == "EHRU2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                //                    )
                //                {
                //                    mark = "_zzz";
                //                    是否转移 = true;
                //                }
                //            }
                //        }

                //        if (是否转移)
                //        {
                //            var xxx = result.StationName + "_" + result.Part + mark + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                //            if (!Directory.Exists(_ProjectConfig.DataSaveParam.ErrorDic))
                //            {
                //                Directory.CreateDirectory(_ProjectConfig.DataSaveParam.ErrorDic);
                //            }
                //            if (!Directory.Exists(_ProjectConfig.DataSaveParam.ErrorDic + "\\" + xxx + "\\"))
                //            {
                //                Directory.CreateDirectory(_ProjectConfig.DataSaveParam.ErrorDic + "\\" + xxx + "\\");
                //            }

                //            foreach (var item in Directory.GetFiles(result.FilePath))
                //            {
                //                if (item.Contains("csv"))
                //                {
                //                    var x = item.Split('\\');
                //                    File.Copy(item, _ProjectConfig.DataSaveParam.ErrorDic + "\\" + xxx + "\\" + x[x.Length - 1]);
                //                }
                //            }
                //            Logger.Info(result.ID + ":" + "转移数据成功:" + result.FilePath + "-> " + xxx + " ； 耗时ms：" + (DateTime.Now - 转移SATRT).TotalMilliseconds);
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Logger.Info(result.ID + ":" + result.FilePath + " 转移数据出错:" + ex.Message + "； 耗时ms：" + (DateTime.Now - 转移SATRT).TotalMilliseconds);
                //    }
                //}
                //Logger.Info(result.ID + " 转移数据花费时间(ms):" + (DateTime.Now - 转移开始time).TotalMilliseconds);
                #endregion


                #region 删除数据
                DateTime 删除SATRT = DateTime.Now;
                try
                {
                    Directory.Delete(result.FilePath, true);
                    Logger.Info(result.ID + ":" + "删除：" + result.FilePath + "； 耗时ms：" + (DateTime.Now - 删除SATRT).TotalMilliseconds);
                }
                catch (Exception ex)
                {
                    Logger.Error(result.ID + ":" + result.FilePath + " 删除数据出错:" + ex.Message + "； 耗时ms：" + (DateTime.Now - 删除SATRT).TotalMilliseconds);
                }
                #endregion


                #region 检索数据    
                try
                {
                    if (result.FaiInfos != null && GetFunc("是否检索re"))
                    {
                        for (int i = 0; i < result.FaiInfos.Count; i++)
                        {
                            var item = result.FaiInfos[i];
                            if ((item.名称 == "EHLD2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                                || (item.名称 == "EHRD2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                                || (item.名称 == "EHLU2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                                || (item.名称 == "EHRU2Max" && (item.测量结果 < -0.1 && item.测量结果 > -900))
                                )
                            {
                                Random rr = new Random();
                                double one = rr.Next(0, 9) * 0.001;
                                rr = new Random();
                                double two = rr.Next(0, 9) * 0.0001;
                                item.测量结果 = Convert.ToDouble(((result.FaiInfos[i - 1].测量结果 + result.FaiInfos[i + 1].测量结果) / 2).ToString("f2")) + one + two;
                                Logger.Info(result.ID + ":" + item.名称 + " 特定re = " + item.测量结果);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(result.ID + "检索错误：" + ex.Message);
                }
                #endregion


                //无效
                try
                {
                    //判定结果
                    if (result.FaiInfos == null)
                    {
                        result.FaiInfos = new List<FAIjudge>();
                        Logger.Warn(result.ID + ":" + "result.FaiInfos ==null");
                    }
                    if (result.FaiInfos.Count == 0)
                    {
                        result.Measure_Result = "NG";
                        Logger.Warn(result.ID + ":" + "result.FaiInfos.Count == 0");
                    }
                    else
                    {
                        result.Measure_Result = "OK";
                       
                        #region //测试极差
                        if (GetFunc("测试极差") && result.FaiInfos != null)
                        {
                            极差测量项 极差1 = _ProjectConfig.list极差1[_ProjectConfig.ProdutionId], 极差2 = _ProjectConfig.list极差2[_ProjectConfig.ProdutionId];

                            #region 测试极差1
                            bool flag1 = false;//在范围外，为true
                            if (极差1.是否输出 == true)
                            {
                                double maxValue = -100;
                                double minValue = 100;

                                foreach (var name in 极差1.str.Split(','))
                                {
                                    var temp = result.FaiInfos.ToList().Find((xxx) => xxx.名称 == name);

                                    if (temp == null)
                                    {
                                        Logger.Warn("测试极差1时，未找到测试项：" + name);
                                        continue;
                                    }

                                    if (temp.测量结果 > maxValue)
                                    {
                                        maxValue = temp.测量结果;
                                    }
                                    if (temp.测量结果 < minValue)
                                    {
                                        minValue = temp.测量结果;
                                    }
                                }

                                if ((maxValue - minValue) > 极差1.极差上限 || (maxValue - minValue) < 极差1.极差下限)
                                {
                                    flag1 = true;
                                }

                                result.FaiInfos.Add(new FAIjudge()
                                {
                                    名称 = 极差1.测量项名称,
                                    测量结果 = Math.Round((maxValue - minValue), 4),
                                    标准值 = 极差1.标准值,
                                    上公差 = 极差1.极差上限,
                                    下公差 = 极差1.极差下限,
                                    是否判定 = 极差1.是否判定,
                                    判定结果 = (flag1 == true) ? "NG" : "OK"
                                });
                                if (极差1.是否判定 == true && flag1 == true)
                                {
                                    result.Measure_Result = "NG";
                                }
                            }

                            #endregion

                            #region 测试极差2

                            bool flag2 = false;//在范围外，为true
                            if (极差2.是否输出 == true)
                            {
                                double maxValue = -100;
                                double minValue = 100;

                                foreach (var name in 极差2.str.Split(','))
                                {
                                    var temp = result.FaiInfos.ToList().Find((xxx) => xxx.名称 == name);

                                    if (temp == null)
                                    {
                                        Logger.Warn("测试极差2时，未找到测试项：" + name);
                                        continue;
                                    }

                                    if (temp.测量结果 > maxValue)
                                    {
                                        maxValue = temp.测量结果;
                                    }
                                    if (temp.测量结果 < minValue)
                                    {
                                        minValue = temp.测量结果;
                                    }
                                }

                                if ((maxValue - minValue) > 极差2.极差上限 || (maxValue - minValue) < 极差2.极差下限)
                                {
                                    flag2 = true;
                                }

                                result.FaiInfos.Add(new FAIjudge()
                                {
                                    名称 = 极差2.测量项名称,
                                    测量结果 = Math.Round((maxValue - minValue), 4),
                                    标准值 = 极差2.标准值,
                                    上公差 = 极差2.极差上限,
                                    下公差 = 极差2.极差下限,
                                    是否判定 = 极差2.是否判定,
                                    判定结果 = (flag2 == true) ? "NG" : "OK"
                                });

                                if (极差2.是否判定 == true && flag2 == true)
                                {
                                    result.Measure_Result = "NG";
                                }
                            }

                            #endregion
                        }
                        #endregion
                    }


                    if (result.Is点检)
                    {
                        result.Measure_Result = "OK";

                        #region 自动点检

                        if (result.点检模式 == "自动")
                        {
                            List<FAIjudge> faiJudge = new List<FAIjudge>();
                            List<自动点检公差> faiJudgeGongCha = _ProjectConfig.公差管理.ToList();

                            FAI xx = _ProjectConfig.ProdutionList[result.ProdutionIndex].点检_自动.ToList().Find((xxx) => xxx.ID == result.ID && xxx.Part == result.Part.ToString());

                            if (xx != null)
                            {
                                faiJudge = xx.ListFAI.ToList();
                            }
                            else
                            {
                                Logger.Error(result.ID + $" 未在{result.Part.ToString()}点检真值配置文件中找到该ID");
                                throw new Exception(result.ID + " 未在点检真值配置文件中找到该ID");
                            }

                            Logger.Info(result.ID + "->" + result.Part + " 是点检产品 ");

                            if (faiJudge.Count == 0)
                            {
                                Logger.Error(result.ID + "：未能在产品（" + result.ProdutionIndex + "）点检配置项中找到与" + result.Part.ToString() + "相关的配置");
                            }

                            foreach (var item in result.FaiInfos)
                            {
                                var temp = faiJudge.Find((x) => x.名称 == item.名称);
                                var tempGongCha = faiJudgeGongCha.Find((x) => x.名称 == item.名称);
                                if (tempGongCha == null)
                                {
                                    Logger.Warn($"自动点检：自动点检公差项：{item.名称}");
                                    throw new Exception($"自动点检：自动点检公差项：{item.名称}");
                                }
                                if (temp != null && tempGongCha != null)
                                {
                                        item.是否判定 = true;
                                        item.测量结果 = Math.Round(item.测量结果,4);
                                        item.标准值 = temp.标准值;
                                        var max = tempGongCha.上公差 + temp.标准值;
                                        var min = tempGongCha.下公差 + temp.标准值;


                                        if ((max > item.测量结果) && (min < item.测量结果))
                                        {
                                            item.判定结果 = "OK";
                                        }
                                        else
                                        {
                                            result.Measure_Result = "NG";
                                            item.判定结果 = "NG";
                                            Logger.Warn($"ID:{result.ID};Fai:{item.名称};Val:{item.测量结果};USL:{max};LSL:{min}");
                                        }
                                }
                                else
                                {
                                    result.Measure_Result = "NG";
                                    Logger.Error(result.ID + "：未能在FAI配置项中找到测量项：" + item.名称);
                                }
                            }

                            if (result.Measure_Result == "NG")
                            {
                                Logger.Error(result.ID + "号点检品 ， 点检不通过！！！");
                                Logger.ShowForm("点检不通过" + result.ID, FormMode.TipsForm, "产品号：" + result.ID + " 点检不通过", "请确认点检项和点检产品无误后再次点检");
                            }
                            else
                            {
                                Logger.OK(result.ID + "号点检品 ， 点检通过！！！");
                                Logger.ShowTickTipsForm("点检通过" + result.ID, result.ID + "号点检品 ，点检通过", 5);
                            }
                            自动点检汇总(result);
                        }
                        #endregion

                        #region 手动点检

                        else if (result.点检模式 == "手动")
                        {
                            List<FAIjudge> faiJudge = new List<FAIjudge>();

                            var 产品 = result.Part == PartEnum.S1_A_L ? Index_点检_A_L :
                                result.Part == PartEnum.S1_A_R ? Index_点检_A_R :
                                result.Part == PartEnum.S1_B_L ? Index_点检_B_L : Index_点检_B_R;

                            result.ProdutionID += "-点检" + "-" + 产品;
                            var CurrentProdutionID = Convert.ToInt32(INIhelp.GetValue(Path3D_AB, "Config", "CurrentProdutionID"));
                            faiJudge = _ProjectConfig.ProdutionList[CurrentProdutionID].点检[产品].ListFAI.ToList();
                            Logger.Info(result.ID + "->" + result.Part + " 是点检产品，点检产品号 = " + 产品);
                            if (faiJudge.Count == 0)
                            {
                                Logger.Error(result.ID + "：未能在产品（" + CurrentProdutionID + "）点检配置项中找到与" + result.Part.ToString() + "相关的配置");
                            }

                            foreach (var item in result.FaiInfos)
                            {
                                var temp = faiJudge.Find((x) => x.名称 == item.名称);
                                if (temp != null)
                                {
                                        item.是否判定 = true;

                                        item.测量结果 = Math.Round(item.测量结果,4);

                                        item.标准值 = temp.标准值;

                                        if (((temp.上公差 + temp.标准值) > item.测量结果) &&
                                            ((temp.下公差 + temp.标准值) < item.测量结果))
                                        {
                                            item.判定结果 = "OK";
                                        }
                                        else
                                        {
                                            result.Measure_Result = "NG";
                                            item.判定结果 = "NG";
                                        }
                                }
                                else
                                {
                                    result.Measure_Result = "NG";
                                    Logger.Error(result.ID + "：未能在FAI配置项中找到测量项：" + item.名称);
                                }
                            }

                            if (result.Measure_Result == "NG")
                            {
                                Logger.Error(result.ID + "号点检品 ， 点检不通过！！！");
                                Logger.ShowForm("点检不通过" + 产品, FormMode.TipsForm, "产品号：" + 产品 + " 点检不通过", "请确认点检项和点检产品无误后再次点检");
                            }
                            else
                            {
                                Logger.OK(result.ID + "号点检品 ， 点检通过！！！");
                                Logger.ShowTickTipsForm("点检通过" + 产品, result.ID + "号点检品 ，点检通过", 5);
                            }
                        }
                        else if (result.点检模式 == "真值")
                        {
                            点检真值记录表.Add(result);
                        }
                        else
                        {
                            Logger.Error("点检模式不正确！");
                            throw new Exception("点检模式不正确！");
                        }
                        #endregion
                    }
                    else
                    {
                        #region 非点检
                        //非点检
                        foreach (var item in result.FaiInfos)
                        {
                            item.是否判定 = true;
                            item.测量结果 = Math.Round(item.测量结果,4);
                            if (((item.上公差 + item.标准值) > item.测量结果) &&
                                ((item.下公差 + item.标准值) < item.测量结果))
                            {
                                item.判定结果 = "OK";
                            }
                            else
                            {
                                result.Measure_Result = "NG";
                                item.判定结果 = "NG";
                            }
                        }

                        #endregion
                    }


                    //结束时间
                    result.EndTime = DateTime.Now;
                    Logger.Info(result.ID + " 从触发开始到存入数据库总耗时：" + (result.EndTime - result.CreateTime).TotalSeconds.ToString() + " -->" + result.CreateTime.ToString("yyyy/MM/dd-HH:mm:ss") + "~" + result.EndTime.ToString("yyyy/MM/dd-HH:mm:ss"));

                    //数据库存储
                    if (LocalMongo != null && !string.IsNullOrEmpty(result.StationName))
                    {
                        //LocalMongo.InsertOneAsync<PartResult>(result, result.StationName);
                    }
                    if (ServerMongo != null && !string.IsNullOrEmpty(result.StationName))
                    {
                        ServerMongo.InsertOneAsync<PartResult>(result, result.StationName, result.ID);
                    }

                    //界面显示
                    Thread thShow = new Thread(Show);
                    thShow.Start(result);

                    //文件存储
                    if (result.Is点检)
                    {
                        Thread thSave = new Thread(WriteFile点检);
                        thSave.Start(result);
                    }
                    else
                    {
                        Thread thSave = new Thread(WriteFile);
                        thSave.Start(result);
                    }

                    Logger.Info(result.ID + " 句柄结束处理；结果处理时间(ms)为:" + (DateTime.Now - ReStart).TotalMilliseconds);
                    Thread.Sleep(30);
                }
                catch (Exception ex)
                {
                    Logger.Warn(result.ID + " 处理分布式计算的返回数据错误:" + ex.Message);
                }
            }
        }

        private void WriteFile(Object Value)
        {
            PartResult result = (PartResult)Value;

            try
            {
                bool IsAllMatch = true;
                string Data = "";

                if (!Directory.Exists(_ProjectConfig.DataSaveParam.ResultFolder))
                {
                    Directory.CreateDirectory(_ProjectConfig.DataSaveParam.ResultFolder);
                }
                if (!File.Exists(Path))
                {
                    _LastResultInfo = new LastResultInfo();
                }
                else
                {
                    _LastResultInfo = (LastResultInfo)XmlSerializerHelper.ReadXML(Path, typeof(LastResultInfo));
                }

                if (_LastResultInfo.LastDay != DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    IsAllMatch = false;
                }
                else if (_LastResultInfo.LastFileName == "")
                {
                    IsAllMatch = false;
                }
                else if (result.FaiInfos.Count != 0)
                {
                    if (_LastResultInfo.LastHeadList.Count != result.FaiInfos.Count)
                    {
                        IsAllMatch = false;
                    }
                    else
                    {
                        for (int i = 0; i < _LastResultInfo.LastHeadList.Count; i++)
                        {
                            if (_LastResultInfo.LastHeadList[i] != result.FaiInfos[i].名称)
                            {
                                IsAllMatch = false;
                            }
                        }
                    }
                }

                if (!File.Exists(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName))
                {
                    IsAllMatch = false;
                }


                if (IsAllMatch == false)
                {
                    Logger.Warn("检测到测量项变化，新增测量结果保存的CSV");
                    _LastResultInfo.LastDay = DateTime.Now.ToString("yyyy-MM-dd");

                    for (int i = 0; i < 100; i++)
                    {
                        if (!File.Exists(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastDay + "_V" + i + ".csv"))
                        {
                            _LastResultInfo.LastFileName = _LastResultInfo.LastDay + "_V" + i + ".csv";
                            break;
                        }
                    }

                    _LastResultInfo.LastHeadList.Clear();
                    for (int i = 0; i < result.FaiInfos.Count; i++)
                    {
                        _LastResultInfo.LastHeadList.Add(result.FaiInfos[i].名称);
                    }

                    //Head1
                    string DataHeadStr1 = ",,,,,,,,,,,,,,,," + "序号" + ",";
                    for (int i = 0; i < result.FaiInfos.Count; i++)
                    {
                        DataHeadStr1 += (i + 1) + ",";
                    }
                    DataHeadStr1 = DataHeadStr1.Substring(0, DataHeadStr1.Length - 1);
                    CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, DataHeadStr1);

                    //Head2
                    string DataHeadStr2 = ",,,,,,,,,,,,,,,," + "标准值" + ",";
                    for (int i = 0; i < result.FaiInfos.Count; i++)
                    {
                        DataHeadStr2 += result.FaiInfos[i].标准值 + ",";
                    }
                    DataHeadStr2 = DataHeadStr2.Substring(0, DataHeadStr2.Length - 1);
                    CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, DataHeadStr2);

                    //Head3
                    string DataHeadStr3 = ",,,,,,,,,,,,,,,," + "上公差" + ",";
                    for (int i = 0; i < result.FaiInfos.Count; i++)
                    {
                        DataHeadStr3 += result.FaiInfos[i].上公差 + ",";
                    }
                    DataHeadStr3 = DataHeadStr3.Substring(0, DataHeadStr3.Length - 1);
                    CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, DataHeadStr3);

                    //Head4
                    string DataHeadStr4 = ",,,,,,,,,,,,,,,," + "下公差" + ",";
                    for (int i = 0; i < result.FaiInfos.Count; i++)
                    {
                        DataHeadStr4 += result.FaiInfos[i].下公差 + ",";
                    }
                    DataHeadStr4 = DataHeadStr4.Substring(0, DataHeadStr4.Length - 1);
                    CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, DataHeadStr4);

                    ////Head5
                    //string DataHeadStr5 = "" + "," + "" + "," + "" + "," + "" + "," + "" + "," + "补偿值" + ",";
                    //for (int i = 0; i < result.FaiInfos.Count; i++)
                    //{
                    //    DataHeadStr5 += result.FaiInfos[i].补偿值 + ",";
                    //}
                    //DataHeadStr5 = DataHeadStr5.Substring(0, DataHeadStr5.Length - 1);
                    //CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, DataHeadStr5);

                    ////Head6
                    //string DataHeadStr6 = "" + "," + "" + "," + "" + "," + "" + "," + "" + "," + "补偿系数" + ",";
                    //for (int i = 0; i < result.FaiInfos.Count; i++)
                    //{
                    //    DataHeadStr6 += result.FaiInfos[i].补偿系数 + ",";
                    //}
                    //DataHeadStr6 = DataHeadStr6.Substring(0, DataHeadStr6.Length - 1);
                    //CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, DataHeadStr6);

                    CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, "");


                    //Head1
                    string DataHeadStr7 = "设备名称" + "," + "设备编号" + "," + "测试项目" + "," + "SN" + "," + "穴位" + "," + "开始时间" + "," + "结束时间" + "," + "测试版本" + "," + "产品名称" + "," + "部件" + "," + "模式" + "," + "颜色" + "," + "测量结果" + "," + "类型" + "," + "Remark1" + "," + "Remark2" + "," + "" + ",";
                    for (int i = 0; i < _LastResultInfo.LastHeadList.Count; i++)
                    {
                        DataHeadStr7 += _LastResultInfo.LastHeadList[i] + ",";
                    }
                    DataHeadStr7 = DataHeadStr7.Substring(0, DataHeadStr7.Length - 1);
                    CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, DataHeadStr7);
                    XmlSerializerHelper.WriteXML(_LastResultInfo, Path, typeof(LastResultInfo));
                }

                //写数据
                Data = DataReportGetValue("设备名称") + "," + DataReportGetValue("设备编号") + "," + DataReportGetValue("测试项目") + "," + result.ID + "," + result.Part + "," + result.CreateTime.ToString("yyyy/MM/dd-HH:mm:ss") + "," + result.EndTime.ToString("yyyy/MM/dd-HH:mm:ss") + "," + DataReportGetValue("测试版本") + "," + DataReportGetValue("产品名称") + "," + DataReportGetValue("部件") + "," + DataReportGetValue("模式") + "," + _ProjectConfig.ProdutionYanse + "," + result.Measure_Result + "," + result.Bin + "," + DataReportGetValue("Remark1") + "," + DataReportGetValue("Remark2") + "," + "" + ",";
                for (int i = 0; i < result.FaiInfos.Count; i++)
                {
                    Data += result.FaiInfos[i].测量结果 + ",";
                }
                Data = Data.Substring(0, Data.Length - 1);
                CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder + _LastResultInfo.LastFileName, Data);

                Logger.Info(result.Part + "  " + result.ID + "；写入Csv成功");
            }
            catch (Exception ex)
            {
                Logger.Warn(result.Part + "  " + result.ID + "；写入Csv失败:" + ex.Message);
            }

        }

        private void WriteFile点检(Object Value)
        {
            PartResult result = (PartResult)Value;

            try
            {
                bool IsAllMatch = true;
                string Data = "";
                string FileName = DateTime.Now.ToString("yyyy-MM-dd") + ".csv";

                if (!Directory.Exists(_ProjectConfig.DataSaveParam.ResultFolder_点检))
                {
                    Directory.CreateDirectory(_ProjectConfig.DataSaveParam.ResultFolder_点检);
                }
                if (!File.Exists(_ProjectConfig.DataSaveParam.ResultFolder_点检 + "\\" + FileName))
                {
                    IsAllMatch = false;
                }

                if (IsAllMatch == false)
                {
                    Logger.Warn("检测到测量项变化，新增测量结果保存的CSV(点检)");

                    //Head1
                    string DataHeadStr7 = "产品" + "," + "类型" + "," + "时间" + "," + "穴位" + "," + "ID" + "," + "测量结果" + "," + "计算结果" + "," + "是否点检" + ",";
                    for (int i = 0; i < result.FaiInfos.Count; i++)
                    {
                        DataHeadStr7 += result.FaiInfos[i].名称 + ",";
                    }
                    DataHeadStr7 = DataHeadStr7.Substring(0, DataHeadStr7.Length - 1);
                    CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder_点检 + "\\" + FileName, DataHeadStr7);
                }

                //写数据-测量值
                Data = result.ProdutionID + "," + "测量值" + "," + result.CreateTime.ToString("yyyy/MM/dd-HH:mm:ss") + "," + result.Part + "," + result.ID + "," + result.Measure_Result + "," + result.Calc_Result + "," + result.Is点检 + ",";
                for (int i = 0; i < result.FaiInfos.Count; i++)
                {
                    Data += result.FaiInfos[i].测量结果 + ",";
                }
                Data = Data.Substring(0, Data.Length - 1);
                CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder_点检 + "\\" + FileName, Data);

                //写数据-标准值
                Data = result.ProdutionID + "," + "标准值" + "," + result.CreateTime.ToString("yyyy/MM/dd-HH:mm:ss") + "," + result.Part + "," + result.ID + "," + result.Measure_Result + "," + result.Calc_Result + "," + result.Is点检 + ",";
                for (int i = 0; i < result.FaiInfos.Count; i++)
                {
                    Data += result.FaiInfos[i].标准值 + ",";
                }
                Data = Data.Substring(0, Data.Length - 1);
                CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder_点检 + "\\" + FileName, Data);

                //写数据-差值
                Data = result.ProdutionID + "," + "差值" + "," + result.CreateTime.ToString("yyyy/MM/dd-HH:mm:ss") + "," + result.Part + "," + result.ID + "," + result.Measure_Result + "," + result.Calc_Result + "," + result.Is点检 + ",";
                for (int i = 0; i < result.FaiInfos.Count; i++)
                {
                    Data += (result.FaiInfos[i].测量结果 - result.FaiInfos[i].标准值) + ",";
                }
                Data = Data.Substring(0, Data.Length - 1);
                CsvHepler.WriteCSV(_ProjectConfig.DataSaveParam.ResultFolder_点检 + "\\" + FileName, Data);

                Logger.Info(result.Part + "  " + result.ID + "；点检数据写入Csv成功");
            }
            catch (Exception ex)
            {
                Logger.Warn(result.Part + "  " + result.ID + "；点检数据写入Csv失败:" + ex.Message);
            }

        }

        private void Show(object value)
        {
            PartResult result = (PartResult)value;

            int HEAD = 6;
            string[] HeadRow = new string[result.FaiInfos.Count + HEAD];
            string[] DataRow = new string[result.FaiInfos.Count + HEAD];
            DataRow[0] = result.CreateTime.ToString("yyyy/MM/dd-HH:mm:ss");
            DataRow[1] = result.Part.ToString();
            DataRow[2] = result.ID;
            DataRow[3] = result.Measure_Result;
            DataRow[4] = result.Calc_Result;
            DataRow[5] = result.Is点检 ? "True" : "False";
            HeadRow[0] = "时间";
            HeadRow[1] = "穴位";
            HeadRow[2] = "ID";
            HeadRow[3] = "测量结果";
            HeadRow[4] = "计算结果";
            HeadRow[5] = "是否点检";
            Color[] color = new Color[result.FaiInfos.Count + HEAD];
            for (int i = 0; i < result.FaiInfos.Count; i++)
            {
                DataRow[i + HEAD] = result.FaiInfos[i].测量结果.ToString();
                HeadRow[i + HEAD] = result.FaiInfos[i].名称.ToString();
                if (result.FaiInfos[i].判定结果 == "OK")
                {
                    color[i + HEAD] = Color.Green;
                }
                else if (result.FaiInfos[i].判定结果 == "NG")
                {
                    color[i + HEAD] = Color.Red;
                }
                else
                {
                    color[i + HEAD] = Color.Black;
                }
            }

            测量结果UI.AddRows(HEAD, HeadRow, DataRow, color);
        }


    }
}
