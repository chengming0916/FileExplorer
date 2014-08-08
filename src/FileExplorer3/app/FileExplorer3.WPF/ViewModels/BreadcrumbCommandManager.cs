using Caliburn.Micro;
using FileExplorer.Script;
using FileExplorer.Utils;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.WPF.ViewModels
{
    public class BreadcrumbCommandManager : CommandManagerBase
    {
        
        #region Constructor

        public BreadcrumbCommandManager(IBreadcrumbViewModel bvm, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
        {
            _bvm = bvm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("Breadcrumb", _bvm),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            CommandDictionary = new DynamicRelayCommandDictionary() { ParameterDicConverter = ParameterDicConverter };
            CommandDictionary.ToggleBreadcrumb = new SimpleScriptCommand("ToggleBreadcrumb", 
                pd => { IBreadcrumbViewModel bread = pd["Breadcrumb"] as IBreadcrumbViewModel; 
                    bread.ShowBreadcrumb = !bread.ShowBreadcrumb; return ResultCommand.NoError; });

            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(                                        
                    ScriptCommandBinding.FromScriptCommand(ExplorerCommands.ToggleBreadcrumb, this, (ch) => ch.CommandDictionary.ToggleBreadcrumb, ParameterDicConverter, ScriptBindingScope.Explorer)
                ));

            _exportBindingSource = exportBindingSource.ToArray();             
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private IBreadcrumbViewModel _bvm;

        #endregion

        #region Public Properties

        #endregion

    }
}
