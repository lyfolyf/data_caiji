
using Lead.Proxy;
using Lead.Tool.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lead.Process.Station_S1_IO
{
    public class S1_IO : IProcess
    {
        private ProxyData _ProxyData = null;
        private List<SingleStep> _SingleStep = new List<SingleStep>();
        private Thread _MainThread = null;
        private ProcessState _State = ProcessState.ProcessNA;
        private int _ThreadId = -1;
        private Stopwatch tm = new Stopwatch();
        private int _StepID = 0;

        public S1_IO(ProxyData Data)
        {
            _ProxyData = Data;
        }

        public List< SingleStep> SingleStep
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
            Logger.Info("进入S2循环");
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
            Logger.Info("退出S2循环");

        }

        private void 等待开始触发信号()
        {
            _ProxyData.StartMoveSignal(StationEnum.S1_A);
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
                    //A1开始信号
                    if (!_ProxyData.ReadIO(IN_IO.A1_启动) )
                    {
                        _ProxyData.WriteIO(OUT_IO.A1_启动反馈, false);//没有接收信号，不发送
                        Thread.Sleep(100);

                        _ProxyData.WaitIO(IN_IO.A1_启动,true,10);

                        _ProxyData.WriteIO(OUT_IO.A1_启动反馈, true);
                    }

                    //A1停止信号
                    if (!_ProxyData.ReadIO(IN_IO.A1_停止))
                    {
                        _ProxyData.WriteIO(OUT_IO.A1_停止反馈, false);//没有接收信号，不发送
                        Thread.Sleep(100);

                        _ProxyData.WaitIO(IN_IO.A1_停止, true, 10);

                        _ProxyData.WriteIO(OUT_IO.A1_停止反馈, true);
                    }

                    //B1开始信号
                    if (!_ProxyData.ReadIO(IN_IO.B1_启动))
                    {
                        _ProxyData.WriteIO(OUT_IO.B1_启动反馈, false);//没有接收信号，不发送
                        Thread.Sleep(100);

                        _ProxyData.WaitIO(IN_IO.B1_启动, true, 10);

                        _ProxyData.WriteIO(OUT_IO.B1_启动反馈, true);
                    }

                    //B1停止信号
                    if (!_ProxyData.ReadIO(IN_IO.B1_停止))
                    {
                        _ProxyData.WriteIO(OUT_IO.B1_停止反馈, false);//没有接收信号，不发送
                        Thread.Sleep(100);

                        _ProxyData.WaitIO(IN_IO.B1_停止, true, 10);

                        _ProxyData.WriteIO(OUT_IO.B1_停止反馈, true);
                    }
                }
                catch (Exception ex)
                {
                    _State = ProcessState.ProcessPause;
                    string ErrMes = string.Format("S2循环至第 {0} 步（{1}）报错,已切换至暂停状态，原因->{2}",_StepID,_SingleStep[_StepID].Method.ToString(), ex.Message);
                    Logger.Warn(ErrMes);
                }

            }
        }
    }
}
