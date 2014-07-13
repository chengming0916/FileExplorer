using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.ObjectModel;
using FileExplorer.Utils;
using MetroLog;
using FileExplorer.UnitTests;

namespace TestScript.WPF
{
    [Export(typeof(IScreen))]
    public class AppViewModel : Screen
    {
        #region Constructor

        public AppViewModel()
        {

            LogManagerFactory.DefaultConfiguration.AddTarget(LogLevel.Debug, LogLevel.Fatal, new ConsoleTarget());
            LogManagerFactory.DefaultConfiguration.IsEnabled = true;
            
            LogManagerFactory.DefaultLogManager.GetLogger<AppViewModel>().Log(LogLevel.Debug, "Test");
            AsyncUtils.RunSync(() => ScriptCommandTests.Test_DownloadFile());      
            
        }

        #endregion

        #region Methods

        public void Serialize()
        {
            //CSharpCodeProvider.cre
            CSharpCodeProvider prov = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });            

            CompilerParameters cp = new CompilerParameters(new [] 
            {
                 "mscorlib.dll", "System.dll", "System.IO.dll", "System.xml.dll", "System.Xml.Serializer.dll", "System.Runtime.dll", 
                 "System.ObjectModel.dll", "System.Collections.dll",
                 "FileExplorer3.dll", "FileExplorer3.IO.dll", "FileExplorer3.WPF.dll", 
                 "Caliburn.Micro.dll", "Caliburn.Micro.Platform.dll"
            });
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
          
            Progress.Clear();
            
            CompilerResults cr = prov.CompileAssemblyFromSource(cp, Script);

            if (cr.Errors.HasErrors)
            {
                Progress.Add("Error when serializing:");
                cr.Errors.Cast<CompilerError>().ToList().ForEach(err => 
                    Progress.Add(String.Format("{0} at line {1} column {2} ", err.ErrorText, err.Line, err.Column)));
            }
        }

        #endregion

        #region Data

        private string _script;
        private string _xml;
        private ObservableCollection<string> _progress = new ObservableCollection<string>();

        #endregion

        #region Public Properties

        public string Script { get { return _script; } set { _script = value; NotifyOfPropertyChange(() => Script); } }
        public string Xml { get { return _xml; } set { _xml = value; NotifyOfPropertyChange(() => Xml); } }

        public ObservableCollection<string> Progress { get { return _progress; } set { _progress = value; NotifyOfPropertyChange(() => Progress); } }

        #endregion

    }
}
