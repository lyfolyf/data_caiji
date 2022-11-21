using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lead.Process.Interface;
using Lead.Tool.ProjectPath;
using Lead.Proxy;

namespace Lead.Process.Manager
{
    public partial class ProcessManager : Form
    {
        private Dictionary<string, IProcess> CreatList = new Dictionary<string, IProcess>();
        private string CreaterClassName = "Lead.Process.Interface.ICreatPorcess";
        private string primFolderPath = PathManager.ConfigPath + @"\Bin\MyProcess\";
        private ProxyData _ProxyData = ProxyData.GetInstance();
        public ProcessStateManagerUI ProcessStateUI = null;
        private Dictionary<string, DebugUI> DebugUItList = new Dictionary<string, DebugUI>();
        private string ShowKey = "";

        public ProcessManager()
        {
            InitializeComponent();

            LoadPrimTypeAttributes(primFolderPath);

            ProcessStateUI = new ProcessStateManagerUI(ref CreatList);

            foreach (var item in CreatList)
            {
                TreeNode node = new TreeNode();
                node.Text = item.Key;

                this.treeView1.Nodes.Add(node);

                var X = new DebugUI(item.Key, item.Value);
                X.Dock = DockStyle.Fill;
                DebugUItList.Add(item.Key, X);
            }
        }

        public Dictionary<string, IProcess> ProcessList
        {
            get { return CreatList; }
        }

        private T GetFactoryClass<T>(string dllPath, string className)
        {
            T factory = default(T);
            if (!string.IsNullOrEmpty(dllPath) && !string.IsNullOrEmpty(className))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    Type[] types = assembly.GetExportedTypes();
                    foreach (Type type in types)
                    {
                        var temp = type.GetInterfaces();
                        if (type.IsClass)
                        {
                            if (null != type.GetInterface(className))
                            {
                                factory = (T)Activator.CreateInstance(type);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }

            return factory;
        }

        private int LoadPrimTypeAttributes(string primFolderPath)
        {
            int ret = 0;
            int result;
            if (string.IsNullOrEmpty(primFolderPath))
            {
                result = -1;
            }
            else
            {
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(primFolderPath);
                    if (dir == null)
                    {
                        result = -1;
                        return result;
                    }
                    FileSystemInfo[] files = dir.GetFileSystemInfos("*.dll");
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo file = files[i] as FileInfo;
                        if (file != null)
                        {
                            string primFileName = file.FullName;
                            //不包含
                            if (!this.CreatList.Keys.Contains(primFileName))
                            {
                                ICreatPorcess CreaterInstance = GetFactoryClass<ICreatPorcess>(primFileName, this.CreaterClassName);

                                if (CreaterInstance != null)
                                {
                                    this.CreatList.Add(CreaterInstance.Name, CreaterInstance.CreatInstance(_ProxyData));
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                result = ret;
            }
            return result;
        }

        private void ProcessManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var t = e.Node.Text;
            foreach (var item in CreatList)
            {
                if (t == item.Key )
                {
                    this.Text = "ProcessManager      当前任务名：" + t;
                    ShowKey = item.Key;
                    this.panel1.Controls.Clear();
                    this.panel1.Controls.Add(DebugUItList[item.Key]);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ShowKey != "")
            {
                DebugUItList[ShowKey].UpdateState(CreatList[ShowKey].State);
                DebugUItList[ShowKey].UpdateStep(CreatList[ShowKey].StartStep);
            }
        }
    }
}
