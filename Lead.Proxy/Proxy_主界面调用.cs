using Lead.CPrim.PrimKeyenceLJ;
using Lead.Tool.CommonData_3D;
using Lead.Tool.Focal;
using Lead.Tool.INI;
using Lead.Tool.LMI;
using Lead.Tool.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lead.Proxy
{

    public partial class ProxyData
    {
        private enum IsInitEnum
        { 
            Focal_横,
            Focal_竖,
            Kenyce,
            LMI,
            ListenTask
        }

        private Dictionary<IsInitEnum, bool> IsInit = new Dictionary<IsInitEnum, bool>();

        public void FormOpen()
        {
            IsInit.Add(IsInitEnum.Focal_横,false);
            IsInit.Add(IsInitEnum.Focal_竖, false);
            IsInit.Add(IsInitEnum.Kenyce, false);
            IsInit.Add(IsInitEnum.LMI, false);
            IsInit.Add(IsInitEnum.ListenTask, false);

            WcfServer wcf = new WcfServer();
            ServiceHost host = new ServiceHost(wcf);
            host.Open();

            Reset触发信号();
        }

        public void Init()
        { 
            try
            {
                //该Sleep是终止手动状态下的各种线程
                IsAllRun = false;
                Thread.Sleep(2000);
                IsAllRun = true;

                Reset触发信号();
                WcfServer.ResetWCF(PartEnum.S1_A_L);
                WcfServer.ResetWCF(PartEnum.S1_A_R);
                WcfServer.ResetWCF(PartEnum.S1_B_L);
                WcfServer.ResetWCF(PartEnum.S1_B_R);

                //reset点检计数
                点检真值记录表.Clear();
                自动点检列表.Clear();

                _LastProdutionID = -1;
                if (IsInit[IsInitEnum.Focal_横] == false && !GetFunc("屏蔽Focal横"))
                {
                    _iFocal_横 = (FocalTool)_ToolConfig.ToolManager["Focal_横"];
                    _iFocal_横.Init();
                    _iFocal_横.Start();
                    IsInit[IsInitEnum.Focal_横] = true;

                    INIhelp.SetValue(Path3D_AB, "交互信号", "Focal_横", _iFocal_横._Config.ForeConfig.Freq.ToString());
                    Logger.Info(Path3D_AB + " 设置 Focal_横.Freq = " + _iFocal_横._Config.ForeConfig.Freq);

                    Logger.OK("Focal_横 初始化、启动成功");
                }

                if (IsInit[IsInitEnum.Focal_竖] == false && !GetFunc("屏蔽Focal竖"))
                {
                    _iFocal_竖 = (FocalTool)_ToolConfig.ToolManager["Focal_竖"];
                    _iFocal_竖.Init();
                    _iFocal_竖.Start();
                    IsInit[IsInitEnum.Focal_竖] = true;

                    INIhelp.SetValue(Path3D_AB, "交互信号", "Focal_竖", _iFocal_竖._Config.ForeConfig.Freq.ToString());
                    Logger.Info(Path3D_AB + " 设置 Focal_竖.Freq = " + _iFocal_竖._Config.ForeConfig.Freq);

                    Logger.OK("_iFocal_竖 初始化、启动成功");
                }

                if (IsInit[IsInitEnum.LMI] == false && !GetFunc("屏蔽LMI"))
                {
                    _iLMI = (LMITool)_ToolConfig.ToolManager["LMI"];
                    _iLMI.Init();
                    _iLMI.Start();
                    IsInit[IsInitEnum.LMI] = true;
                    Logger.OK("LMI 初始化、启动成功");
                }

                if (IsInit[IsInitEnum.Kenyce] == false && !GetFunc("屏蔽基恩士"))
                {
                    _iKencye = (KeyenceTool)_ToolConfig.ToolManager["Keyence"];
                    _iKencye.Init();
                    _iKencye.Start();
                    IsInit[IsInitEnum.Kenyce] = true;
                    Logger.OK("Kenyce 初始化、启动成功");
                }

                ClearOldDataFile();

                this.StopTrigTask();
                if (_TotalTrigInfo != null)
                {
                    foreach (var item in _TotalTrigInfo)
                    {
                        foreach (var itemSensor in item.SenserList)
                        {
                            switch (itemSensor.Type)
                            {
                                case SenserType.Focal:
                                    if (!GetFunc("屏蔽Focal横") || !GetFunc("屏蔽Focal竖"))
                                    {
                                        ((FocalTool)_ToolConfig.ToolManager[itemSensor.Name]).StopMeasure();
                                    }
                                    break;
                                case SenserType.LMI:
                                    if (!GetFunc("屏蔽LMI"))
                                    {
                                        ((LMITool)_ToolConfig.ToolManager[itemSensor.Name]).StopMeasure();
                                    }
                                    break;
                                case SenserType.基恩士:
                                    if (!GetFunc("屏蔽基恩士"))
                                    {
                                        ((KeyenceTool)_ToolConfig.ToolManager[itemSensor.Name]).StopMeasure();
                                    }
                                    break;
                            }
                        }
                    }
                }

                this.Start触发信号();

                //开启结果数据监听线程
                this.StartTaskHandle();

                Logger.OK("Proxy初始化成功");
            }
            catch (Exception ex)
            {
                Logger.ShowForm("Proxy初始化", FormMode.TipsForm, "加载工具失败：" + ex.Message);
                Logger.Warn("Proxy初始化失败：" + ex.Message);
            }
        }

        public void FormClose()
        {
            Reset触发信号();

            if (IsInit[IsInitEnum.Focal_横] == true && !GetFunc("屏蔽Focal横"))
            {
                _iFocal_横 = (FocalTool)_ToolConfig.ToolManager["Focal_横"];
                _iFocal_横.Terminate();
                IsInit[IsInitEnum.Focal_横] = false;
                Logger.OK("Focal_横 终止成功");
            }

            if (IsInit[IsInitEnum.Focal_竖] == true && !GetFunc("屏蔽Focal竖"))
            {
                _iFocal_竖 = (FocalTool)_ToolConfig.ToolManager["Focal_竖"];
                _iFocal_竖.Terminate();
                IsInit[IsInitEnum.Focal_竖] = false;
                Logger.OK("Focal_横 终止成功");
            }

            if (IsInit[IsInitEnum.LMI] == true && !GetFunc("屏蔽LMI"))
            {
                _iLMI = (LMITool)_ToolConfig.ToolManager["LMI"];
                _iLMI.Terminate();
                IsInit[IsInitEnum.LMI] = true;
                Logger.OK("LMI 终止成功");
            }
            if (IsInit[IsInitEnum.Kenyce] == true && !GetFunc("屏蔽基恩士"))
            {
                _iKencye = (KeyenceTool)_ToolConfig.ToolManager["Keyence"];
                _iKencye.Terminate();
                IsInit[IsInitEnum.Kenyce] = true;
                Logger.OK("Kenyce 终止成功");
            }
        }

        public bool GetIsReset()
        {
            var re = INIhelp.GetValue(Path3D_AB, "Config", "复位");
            if (re == "true")
            {
                INIhelp.SetValue(Path3D_AB, "Config", "复位","false");
                Thread.Sleep(1000);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetReseted(bool Value)
        {
            var va = Value ? "true" : "false";
            INIhelp.SetValue(Path3D_AB, "Config", "复位成功",va);
            Logger.Info("设置"+ Path3D_AB + "复位成功= " + va);
        }
    }
}
