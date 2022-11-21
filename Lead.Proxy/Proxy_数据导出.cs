using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lead.Tool.Log;

namespace Lead.Proxy
{
    public partial class ProxyData
    {
        public string DataReportGetValue(string Name)
        {
            foreach (var item in _ProjectConfig.listDataReportManage)
            {
                if (item.Name == Name)
                {
                    return item.Value;
                }
            }
            Logger.Warn("未找到相关延时参数：" + Name);
            throw new Exception("未找到相关延时参数：" + Name);
        }
    }
}
