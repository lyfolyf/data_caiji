using Lead.Tool.Log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Proxy
{

    public enum EnumCT
    {
        Min = 0,
        等待3D触发交互信号_开始,
        启动传感器采集,
        等待3D触发交互信号_开始反馈,
        按照Senser触发顺序收集数据,
        等待3D触发交互信号_结束,
        等待3D触发交互信号_结束反馈,
        检测传感器完成,

        MAX
    }

    public partial class ProxyData
    {
        private Dictionary<string, DateTime> CtStartStation = new Dictionary<string, DateTime>();

        public void CTInit()
        {
            CtStartStation.Clear();
            // CtInfo.Clear();
            for (int i = (int)StationEnum.Min + 1; i < (int)StationEnum.Max; i++)
            {
                for (int j = (int)EnumCT.Min + 1; j < (int)EnumCT.MAX; j++)
                {
                    CtStartStation.Add(((StationEnum)i).ToString() + ((EnumCT)j).ToString(), DateTime.Now);
                    //CtInfo.Add( new CTInfo { Name = ((EnumCT_MODE)i).ToString() + ((EnumCT)j).ToString(), Time = 0 });
                }
            }
        }

        public void LogCtStart(StationEnum Mode, EnumCT Key)
        {
            try
            {
                CtStartStation[Mode.ToString() + Key.ToString()] = DateTime.Now;
            }
            catch (Exception ex)
            {
                Logger.Warn("记录CT开始时间(" + Mode.ToString() + Key.ToString() + ")时间出错：" + ex.Message);
            }

        }

        public void LogCtEnd(StationEnum Mode, EnumCT Key)
        {
            double ct = 0;
            try
            {
                ct = (DateTime.Now - CtStartStation[Mode.ToString() + Key.ToString()]).TotalSeconds;
                Logger.Info("CT分析" + " ; " + "(" + DateTime.Now.ToString("HH:mm:ss,fff") + "-" + DateTime.Now.ToString("HH:mm:ss,fff") + ")" + "->" + Mode.ToString() + ":" + Key.ToString() + " = " + ct);
            }
            catch (Exception ex)
            {
                Logger.Warn("记录CT结束时间(" + Mode.ToString() + Key.ToString() + ")时间出错：" + ex.Message);
            }
        }
    }
}
