using Lead.Process.Interface;
using Lead.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Process.ListenStartTrig_A
{
    public class Creater : ICreatPorcess
    {
        public string Name
        {
            get
            {
                return "ListenStartTrig_A";
            }
        }

        public IProcess CreatInstance(ProxyData Data)
        {
            return new ListenStartTrig_A(Data);
        }
    }
}
