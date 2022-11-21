using Lead.Tool.Interface;
using Lead.Tool.Log;
using Lead.Tool.Login;
using Lead.Tool.Manager;
using Lead.Tool.ProjectPath;
using Lead.Tool.XML;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lead.Tool.ExchangeObject;
using Lead.Tool.IOC0640;
using Lead.Tool.Focal;
using Lead.Tool.MongoDB;
using Lead.CPrim.PrimKeyenceLJ;
using Lead.Tool.LMI;

namespace Lead.Proxy
{
    public enum ProjectSate
    {
        NA = 0,
        Init,
        Auto_Start,
        Auto_Stop,
        Manual,
    }

    public partial  class ProxyData
    {
        //默认成员
        private static ProxyData _Instance = null;
        private static object _Locker = new object(); 
        private LoginUI _LoginUI = null;
        private ConfigForm _ProjectConfigUI = null;
        private ConfigParam _ProjectConfig = null;
        private ToolConfig _ToolConfig = null;
        private static readonly string _ConfigDic = @"D:\Config_数据采集_3D\";
        private string _ConfigPath = _ConfigDic + @"\Config\ProjectConfig.xml";
        private ManualUI _ManualUI = null;
        private 测量结果UI _测量结果UI = new 测量结果UI();

        //整机状态
        private ProjectSate _State = ProjectSate.NA;

        //工具集
        public IOCTool _iIOC;
        public FocalTool _iFocal_横 = null;
        public FocalTool _iFocal_竖 = null;
        public KeyenceTool _iKencye = null;
        public LMITool _iLMI = null;

        //整机循环流程控制
        public bool IsAllRun = true;

        //首次加载标志位
        bool IsFirstLoad = true;

        //数据库
       public  MongoHelper LocalMongo = null;
        public MongoHelper ServerMongo = null;

        public ProjectSate State
        {
            get { return _State; }
            set { _State = value; }
        }

        private ProxyData()
        {
            if (File.Exists(_ConfigPath))
            {
                _ProjectConfig = (ConfigParam)XmlSerializerHelper.ReadXML(_ConfigPath, typeof(ConfigParam));
            }
            else
            {
                _ProjectConfig = new ConfigParam()
                {
                    Delay = new BindingList<DelayTime>(),
                    Func = new BindingList<FuncEnable>(),
                };
            }

            软件更新();
            XmlSerializerHelper.WriteXML(_ProjectConfig, _ConfigPath, typeof(ConfigParam));

            _ProjectConfigUI = _ProjectConfigUI == null ? new ConfigForm(this, _ConfigPath) : _ProjectConfigUI;

            _LoginUI = new LoginUI(_ConfigDic);

            _ToolConfig = ToolConfig.GetInstance(_ConfigDic);

            _ManualUI = new ManualUI(this);

            if (_ProjectConfig.DataSaveParam.Server_DbIP != null && _ProjectConfig.DataSaveParam.Server_DbIP != null)
            {
                ServerMongo = new MongoHelper(_ProjectConfig.DataSaveParam.Server_DbIP, _ProjectConfig.DataSaveParam.Server_DbName);
                LocalMongo = new MongoHelper(_ProjectConfig.DataSaveParam.Local_DbIP, _ProjectConfig.DataSaveParam.Local_DbName);
            }
        }

        public static ProxyData GetInstance()
        {
            if (null == _Instance)
            {
                lock (_Locker)
                {
                    _Instance = new ProxyData();
                }
            }
            return _Instance;
        }

        public ToolConfig ToolConfigUI
        {
            get { return _ToolConfig; }
        }

        public ConfigForm ProjectConfigUI
        {
            get { return _ProjectConfigUI; }
        }

        public ConfigParam ProjectConfig
        {
            get { return _ProjectConfig; }
        }
        public LoginUI LoginUI
        {
            get { return _LoginUI; }
        }

        public ManualUI ManualUI
        {
            get { return _ManualUI; }
        }
        public 测量结果UI 测量结果UI
        {
            get { return _测量结果UI; }
        }
    }
}
