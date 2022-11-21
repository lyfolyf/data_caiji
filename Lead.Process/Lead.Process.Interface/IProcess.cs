using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Process
{
    public delegate void SingleStep();

    public  class ProcessAttribute : Attribute
    {
        private string _Name;
        private Action _Ac;
        public ProcessAttribute(string name,Action ac)
        {
            _Name = name; _Ac = ac;
        }

        public string Name
        {
            get { return _Name; }
        }

        public Action Run
        {
            get { return _Ac; }
        }
    }

    public enum ProcessState
    {
        ProcessNA = 0,
        ProcessInit ,
        ProcessRunning ,
        ProcessPause,
        ProcessTerminate
    }
    public interface IProcess
    {
        int StartStep { get; set; }
        List< SingleStep> SingleStep { get; }
        ProcessState State{get;}
        int ThreadId { get; }
        void Init();
        void Start();
        void Pause();
        void Recovery();
        void Terminate();
    }
}
