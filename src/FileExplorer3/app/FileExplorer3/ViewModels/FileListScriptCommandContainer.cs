﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.Utils;

namespace FileExplorer.ViewModels
{


    public interface IFileListScriptCommandContainer : IScriptCommandContainer
    {
        IScriptCommand Open { get; set; }
        IScriptCommand Refresh { get; set; }
        IScriptCommand Delete { get; set; }
    }

    public class FileListScriptCommandContainer : IFileListScriptCommandContainer, IExportCommandBindings
    {
        #region Constructor

        public FileListScriptCommandContainer(IFileListViewModel flvm, IEventAggregator events)
        {
            ParameterDicConverter =
                ParameterDicConverters.ConvertVMParameter(
                    new Tuple<string, object>("FileList", flvm),
                    new Tuple<string, object>("Events", events));                

            Open = new IfFileListSelection(evm => evm.Count == 1,
                   new IfFileListSelection(evm => evm[0].EntryModel.IsDirectory,
                       OpenSelectedDirectory.FromFileList,  //Selected directory
                       ResultCommand.NoError),   //Selected non-directory
                   ResultCommand.NoError //Selected more than one item.                   
                   );

            Delete = new NullScriptCommand();

            Refresh = new SimpleScriptCommand("Refresh", (pd) =>
            {
                pd.AsVMParameterDic().FileList.ProcessedEntries.EntriesHelper.LoadAsync(true);
                return ResultCommand.OK;
            });

            ExportedCommandBindings = new[] 
            {
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Open, this, (ch) => ch.Open, ParameterDicConverter, ScriptBindingScope.Local),
                ScriptCommandBinding.FromScriptCommand(FileListCommands.Refresh, this, (ch) => ch.Refresh, ParameterDicConverter, ScriptBindingScope.Explorer),
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Delete, this, (ch) => ch.Delete, ParameterDicConverter, ScriptBindingScope.Local),
            };
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public IParameterDicConverter ParameterDicConverter { get; private set; }
        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings { get; private set; }
        public IScriptCommand Open { get; set; }
        public IScriptCommand Refresh { get; set; }
        public IScriptCommand Delete { get; set; }

        #endregion
    }


}