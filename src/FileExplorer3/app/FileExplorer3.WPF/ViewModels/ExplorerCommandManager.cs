﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Script;
using FileExplorer.Defines;
using FileExplorer.WPF.Utils;
using FileExplorer.WPF.ViewModels.Helpers;
using System.Windows.Input;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Utils;

namespace FileExplorer.WPF.ViewModels
{
    public class ExplorerCommandManager : CommandManagerBase
    {
        #region Constructor

        public ExplorerCommandManager(IExplorerViewModel evm, IWindowManager windowManager, IEventAggregator globalEvents, IEventAggregator events,
             params ISupportCommandManager[] cMs)
            : this(evm, windowManager, globalEvents, events, cMs.Select(cm => cm.Commands).ToArray())
        {
            //Workaround: Add all properties to sub-commandManager, 
            //e.g. Add Explorer to FileList.Commands.ParameterDicConverter
            var paramDic = ParameterDicConverter.Convert(null);
            foreach (var iscm in cMs)
                iscm.Commands.ParameterDicConverter.AddAdditionalParameters(paramDic);
        }

        public ExplorerCommandManager(IExplorerViewModel evm, IWindowManager windowManager, IEventAggregator globalEvents, IEventAggregator events,
             params IExportCommandBindings[] additionalBindingExportSource)
            : base(additionalBindingExportSource)
        {
            _evm = evm;
            _windowManager = windowManager;
            _events = events;
            _globalEvents = globalEvents;


            InitCommandManager();
            ToolbarCommands = new ToolbarCommandsHelper(events,
               null,
               null)
               {
               };
            
        }

        #endregion

        #region Methods

        protected override IParameterDicConverter setupParamDicConverter()
        {
            return ParameterDicConverters.ConvertVMParameter(
               new Tuple<string, object>("Explorer", _evm),
                 new Tuple<string, object>("DirectoryTree", _evm.DirectoryTree),
                 new Tuple<string, object>("FileList", _evm.FileList),
                 new Tuple<string, object>("Statusbar", _evm.Statusbar),
                 new Tuple<string, object>("WindowManager", _windowManager),
                 new Tuple<string, object>("Events", _events),
                 new Tuple<string, object>("GlobalEvents", _globalEvents)
                );
        }

        protected override IEnumerable<string> getScriptCommands()
        {
            yield return "Refresh";
            yield return "Transfer";
            yield return "ZoomIn";
            yield return "ZoomOut";
            yield return "CloseTab";
        }

        protected override void setupScriptCommands(dynamic commandDictionary)
        {
            commandDictionary.Refresh = new SimpleScriptCommand("Refresh", (pd) =>
            {
                IExplorerViewModel elvm = pd.AsVMParameterDic().Explorer;
                elvm.FileList.ProcessedEntries.EntriesHelper.LoadAsync(UpdateMode.Replace, true);
                elvm.DirectoryTree.Selection.RootSelector.SelectedViewModel.Entries.LoadAsync(UpdateMode.Replace, true);
                return ResultCommand.NoError;
            });

            commandDictionary.ZoomIn = Explorer.Zoom(ZoomMode.ZoomIn);
            commandDictionary.ZoomOut = Explorer.Zoom(ZoomMode.ZoomOut);
        }

        protected override IExportCommandBindings[] setupExportBindings()
        {
            List<IExportCommandBindings> exportBindingSource = new List<IExportCommandBindings>();
            exportBindingSource.Add(
              new ExportCommandBindings(
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.IncreaseZoom, this, (ch) => ch.CommandDictionary.ZoomIn, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(NavigationCommands.DecreaseZoom, this, (ch) => ch.CommandDictionary.ZoomOut, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.Refresh, this, (ch) => ch.CommandDictionary.Refresh, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ExplorerCommands.CloseTab, this, (ch) => ch.CommandDictionary.CloseTab, ParameterDicConverter, ScriptBindingScope.Explorer)
                //ScriptCommandBinding.FromScriptCommand(ExplorerCommands.CloseWindow, this, (ch) => ch.ScriptCommands.Close, ParameterDicConverter, ScriptBindingScope.Explorer)
              ));

            return exportBindingSource.ToArray();
        }
        #endregion

        #region Data

        IExplorerViewModel _evm;
        private IWindowManager _windowManager;
        private IEventAggregator _events;
        private IEventAggregator _globalEvents;

        #endregion

        #region Public Properties

        #endregion
    }
}
