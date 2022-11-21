using Lead.Tool.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Proxy
{
    public partial class ProxyData
    {
        public bool GetFunc(string Name, bool NotExistThenCreatKey = false,bool CreatKeyDefaultValue = false)
        {
            foreach (var item in _ProjectConfig.Func)
            {
                if (item.Name == Name)
                {
                    return item.Enable;
                }
            }

            if (NotExistThenCreatKey)
            {
                Action ac = () => {
                    try
                    {
                        _ProjectConfig.Func.Add(new FuncEnable() { Name = Name, Enable = CreatKeyDefaultValue, Tips = "此值为程序自动创建" });
                    }
                    catch(Exception ex)
                    {
                        Logger.Warn("创建功能开关"+Name+ "错误(但已加入配置文件中):"+ex.Message);
                    }
                };
                ac.BeginInvoke(null,null);
                return CreatKeyDefaultValue;
            }
            Logger.Warn("未找到相关功能参数：" + Name);
            throw new Exception("未找到相关功能参数：" + Name);
        }

        public int GetDelay(string Name)
        {
            foreach (var item in _ProjectConfig.Delay)
            {
                if (item.Name == Name)
                {
                    return item.Time;
                }
            }
            Logger.Warn("未找到相关延时参数：" + Name);
            throw new Exception("未找到相关延时参数：" + Name);
        }

        public bool IsOverTime(DateTime StartTime, double AllowTime)
        {
            try
            {
                if ((DateTime.Now - StartTime).TotalSeconds > AllowTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                throw new Exception("时间比较出错！");
            }
        }
    }
}
