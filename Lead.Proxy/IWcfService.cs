using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;


namespace Lead.Proxy
{
    [ServiceContract]
    interface IWcfService
    {
        [OperationContract]
        int PushDataResultInfo(PartResult result);

        [OperationContract]
        PartResult GetUnits(string Ip);

        [OperationContract]
        CsvInfo GetCsvInfo(PartEnum Part);

    }
}
