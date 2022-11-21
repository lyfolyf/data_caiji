using Lead.Tool.CommonData_3D;
using Lead.Tool.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Proxy
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class WcfServer : IWcfService
    {
        private static ConcurrentQueue<PartResult> _fileInfoQueue = new ConcurrentQueue<PartResult>();
        private static ConcurrentQueue<PartResult> _resultInfoQueue = new ConcurrentQueue<PartResult>();
        private object _fileInfoQueueMutex = new object();
        private object _resultInfoQueueMutex = new object();
        private static Dictionary<PartEnum, ConcurrentQueue<CsvInfo>> _csvQueue = new Dictionary<PartEnum, ConcurrentQueue<CsvInfo>>();

        public  WcfServer()
        {
            _csvQueue.Add(PartEnum.S1_A_L, new ConcurrentQueue<CsvInfo>());
            _csvQueue.Add(PartEnum.S1_A_R, new ConcurrentQueue<CsvInfo>());
            _csvQueue.Add(PartEnum.S1_B_L, new ConcurrentQueue<CsvInfo>());
            _csvQueue.Add(PartEnum.S1_B_R, new ConcurrentQueue<CsvInfo>());
        }

        public PartResult GetUnits(string Ip)
        {
            PartResult info = null;
            _fileInfoQueue.TryDequeue(out info);

            if (info!= null)
            {
                Logger.Info(info.ID +" 被分布式-"+ Ip+" 计算");
            }

            return info;
        }

        public int PushDataResultInfo(PartResult result)
        {
            int iRet = 0;
            if(result == null) { return 0; }
            _resultInfoQueue.Enqueue(result);

            return iRet;
        }

        static public int PushDataFileInfo(PartResult path)
        {
            int iRet = 0;
            if (path == null) { return 0; }
            _fileInfoQueue.Enqueue(path);

            return iRet;
        }

        static public PartResult PopDataResultInfo()
        {
            PartResult info = null;
            _resultInfoQueue.TryDequeue(out info);

            return info;
        }

        public CsvInfo GetCsvInfo(PartEnum Part)
        {
            CsvInfo info = null;
            _csvQueue[Part].TryDequeue(out info);
            if (info != null)
            {
                ;
            }

            return info;
        }

        static public int PushCsvInfo(CsvInfo Info)
        {

            int iRet = 0;
            if (Info == null) { return 0; }
            _csvQueue[Info.Part].Enqueue(Info);
            return iRet;
        }

        static public void  ResetWCF(PartEnum Part)
        {
            CsvInfo info = null;
            bool re = true;
            while (re)
            {
                re = _csvQueue[Part].TryDequeue(out info);
            }
        }
    }
}
