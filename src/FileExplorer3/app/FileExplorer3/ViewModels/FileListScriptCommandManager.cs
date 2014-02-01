﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Models;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;
using FileExplorer.BaseControls;
using System.Windows.Input;
using FileExplorer.Defines;

namespace FileExplorer.ViewModels
{
    public class FileListScriptCommandManager : IScriptCommandManager, IExportCommandBindings
    {
        #region Constructor

        public FileListScriptCommandManager(IFileListViewModel flvm, IEventAggregator events,  
            params IExportCommandBindings[] additionalBindingExportSource)
        {
            _flvm = flvm;

            ParameterDicConverter =
             ParameterDicConverters.ConvertVMParameter(
                 new Tuple<string, object>("FileList", flvm),
                 new Tuple<string, object>("Events", events));

            #region Set ScriptCommands

            ScriptCommands = new DynamicDictionary<IScriptCommand>();
            ScriptCommands.Open = FileList.IfSelection(evm => evm.Count() == 1,
                   FileList.IfSelection(evm => evm[0].EntryModel.IsDirectory,
                       FileList.OpenSelectedDirectory,  //Selected directory
                       ResultCommand.NoError),   //Selected non-directory
                   ResultCommand.NoError //Selected more than one item.                   
                   );

            ScriptCommands.Delete = NullScriptCommand.Instance;

            ScriptCommands.Refresh = new SimpleScriptCommand("Refresh", (pd) =>
            {
                pd.AsVMParameterDic().FileList.ProcessedEntries.EntriesHelper.LoadAsync(true);
                return ResultCommand.OK;
            });

            ScriptCommands.ToggleRename = FileList.IfSelection(evm => evm.Count() == 1 && evm[0].IsRenamable,
                FileList.ToggleRename, NullScriptCommand.Instance);


            #endregion

            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.AddRange(additionalBindingExportSource);
            exportBindingSource.Add(
                new ExportCommandBindings(
                    ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Open, this, (ch) => ch.ScriptCommands.Open, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Refresh, this, (ch) => ch.ScriptCommands.Refresh, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.ScriptCommands.Delete, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Rename, this, (ch) => ch.ScriptCommands.ToggleRename, ParameterDicConverter, ScriptBindingScope.Local),

                new ScriptCommandBinding(ExplorerCommands.ToggleCheckBox, p => true, p => ToggleCheckBox(), ParameterDicConverter, ScriptBindingScope.Explorer),
                new ScriptCommandBinding(ExplorerCommands.ToggleViewMode, p => true, p => ToggleViewMode(), ParameterDicConverter, ScriptBindingScope.Explorer)
                ));

            _exportBindingSource = exportBindingSource.ToArray();
            
            IEntryModel _currentDirectoryModel = null;
            ToolbarCommands = new ToolbarCommandsHelper(events,
                message => { _currentDirectoryModel = message.NewModel; return new IEntryModel[] { _currentDirectoryModel }; },
                message => message.SelectedModels.Count() == 0 ? new IEntryModel[] { _currentDirectoryModel } : message.SelectedModels.ToArray())
                {
                    ExtraCommandProviders = new[] { 
                        new StaticCommandProvider(new SelectGroupCommand(flvm), new ViewModeCommand(flvm), new SeparatorCommandModel()) 
                    }
                };
        }

        #endregion

        #region Methods

        private IEnumerable<IScriptCommandBinding> getCommandBindings()
        {
            return _exportBindingSource.SelectMany(eb => eb.ExportedCommandBindings);
        }

        public void ToggleViewMode()
        {
            var viewModeWoSeparator = ViewModeCommand.ViewModes.Where(vm => vm.IndexOf(",-1") == -1).ToArray();

            int curIdx = ViewModeCommand.findViewMode(viewModeWoSeparator, _flvm.ItemSize);
            int nextIdx = curIdx + 1;
            if (nextIdx >= viewModeWoSeparator.Count()) nextIdx = 0;

            string viewMode; int step; int itemHeight;
            ViewModeCommand.parseViewMode(viewModeWoSeparator[nextIdx], out viewMode, out step, out itemHeight);
            ViewModeCommand vmc = this.ToolbarCommands.CommandModels.AllNonBindable.First(c => c.CommandModel is ViewModeCommand)
                .CommandModel as ViewModeCommand;
            vmc.SliderValue = step;
        }

        public void ToggleCheckBox()
        {
            _flvm.IsCheckBoxVisible = !_flvm.IsCheckBoxVisible;
        }

        #endregion

        #region Data
        
        private IExportCommandBindings[] _exportBindingSource;
        private IFileListViewModel _flvm;

        #endregion

        #region Public Properties

        public IParameterDicConverter ParameterDicConverter { get; private set; }
        public dynamic ScriptCommands { get; private set; }

        public IToolbarCommandsHelper ToolbarCommands { get; private set; }
        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings { get { return getCommandBindings(); } }

        #endregion




    }
}
