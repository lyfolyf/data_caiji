using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lead.Proxy;
using Lead.Process.Interface;

namespace Lead.Process.Station_S1_IO
{
    public class Creater : ICreatPorcess
    {
        public string Name
        {
            get
            {
                return "S1_IO";
            }
        }

        public IProcess CreatInstance(ProxyData Data)
        {
            return new S1_IO(Data);
        }
    }
}
