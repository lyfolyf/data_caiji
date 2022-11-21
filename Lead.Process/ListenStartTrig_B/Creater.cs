using Lead.Process.Interface;
using Lead.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Process.ListenStartTrig_B
{
    public class Creater : ICreatPorcess
    {
        public string Name
        {
            get
            {
                return "ListenStartTrig_B";
            }
        }

        public IProcess CreatInstance(ProxyData Data)
        {
            return new ListenStartTrig_B(Data);
        }
    }
}