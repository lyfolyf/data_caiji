using Lead.Proxy;
using Lead.Tool.CommonData_3D;
using Lead.Tool.INI;
using Lead.Tool.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lead.Process.ListenStartTrig_B
{
    public class ListenStartTrig_B : IProcess
    {
        private ProxyData _ProxyData = null;    //数据集
        private readonly StationEnum currentStation = StationEnum.S1_B;

        private List<SingleStep> _SingleStep = new List<SingleStep>();  //流程步骤 
        private int _StepID = 0;

        private Thread _MainThread = null;  //运行线程
        private int _ThreadId = -1;
        private ProcessState _State = ProcessState.ProcessNA;

        private Stopwatch tm = new Stopwatch();

        public ListenStartTrig_B(ProxyData Data)
        {
            _ProxyData = Data;

            _SingleStep.Add(等待工站运动开始信号);

            _SingleStep.Add(检测传感器完成);
        }

        #region 属性

        public List<SingleStep> SingleStep
        {
            get
            {
                return _SingleStep;
            }
        }

        public ProcessState State
        {
            get
            {
                return _State;
            }
        }

        public int ThreadId
        {
            get
            {
                return _ThreadId;
            }
        }

        public int StartStep
        {
            get { return _StepID; }
            set { _StepID = value; }
        }

        #endregion

        #region 流程接口

        public void Init()
        {
            if (_MainThread == null)
            {
                _MainThread = new Thread(Loop);
            }
            _State = ProcessState.ProcessInit;
        }

        public void Pause()
        {
            _State = ProcessState.ProcessPause;
        }

        public void Recovery()
        {
            _State = ProcessState.ProcessRunning;
        }

        public void Start()
        {
            if (_MainThread != null && !_MainThread.IsAlive)
            {
                _MainThread.Start();
            }
            _State = ProcessState.ProcessRunning;
            Logger.Info("进入ListenStartTrig_B循环");
        }

        public void Terminate()
        {
            if (_MainThread != null)
            {
                _MainThread.Abort();
                _MainThread = null;
                _ThreadId = -1;
            }
            _State = ProcessState.ProcessTerminate;
            Logger.Info("退出ListenStartTrig_B循环");
        }

        private void Loop()
        {
            _ThreadId = Thread.CurrentThread.ManagedThreadId;

            while (true)
            {
                Thread.Sleep(30);
                if (_State == ProcessState.ProcessPause)
                {
                    continue;
                }

                try
                {
                    for (; _StepID < _SingleStep.Count; _StepID++)
                    {
                        _SingleStep[_StepID]();
                        Logger.Info(currentStation.ToString() + "站数据采集完成步骤:" + (_StepID + 1) + "/" + _SingleStep.Count + ": " + _SingleStep[_StepID].Method.ToString());
                    }

                    _StepID = 0;
                    Logger.Info(currentStation.ToString() + "站数据采集任务完成一个循环");
                }
                catch (Exception ex)
                {
                    _State = ProcessState.ProcessPause;
                    string ErrMes = string.Format(currentStation.ToString() + "站循环至第 {0} 步（{1}）报错,已切换至暂停状态，原因->{2}", (_StepID + 1), _SingleStep[_StepID].Method.ToString(), ex.Message);
                    Logger.Warn(ErrMes);
                }
            }
        }

        #endregion

        private void 等待工站运动开始信号()
        {
            _ProxyData.LogCtStart(currentStation, EnumCT.等待3D触发交互信号_开始);
            _ProxyData.等待工站运动开始信号(currentStation);
            _ProxyData.LogCtEnd(currentStation, EnumCT.等待3D触发交互信号_开始);
            Logger.Info(currentStation + "工站 监听触发信号:等待3D触发交互信号_开始 操作执行完成");
        }

        private void 检测传感器完成()
        {
            _ProxyData.LogCtStart(currentStation, EnumCT.检测传感器完成);
            _ProxyData.检测传感器采集完成(currentStation);
            _ProxyData.LogCtEnd(currentStation, EnumCT.检测传感器完成);
            Logger.Info(currentStation + "工站 检测传感器完成 操作执行完成");
        }
    }
}
