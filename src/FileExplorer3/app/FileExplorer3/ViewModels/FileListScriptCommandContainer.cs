using System;
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
        IScriptCommand ContextMenu { get; set; }
    }

    public class FileListScriptCommandContainer : IFileListScriptCommandContainer, IExportCommandBindings
    {
        #region Constructor

        public FileListScriptCommandContainer(IFileListViewModel flvm, IEventAggregator events)
        {
            ParameterDicConverter = new ParameterDicConverterBase(p =>
              new ParameterDic() 
                {                     
                    { "FileList", flvm },
                    { "Events", events }
                },
              pd => null, ParameterDicConverters.ConvertParameterOnly);

            Open = new IfFileListSelection(evm => evm.Count == 1,
                   new IfFileListSelection(evm => evm[0].EntryModel.IsDirectory,
                       new OpenSelectedDirectory(),  //Selected directory
                       ResultCommand.NoError),   //Selected non-directory
                   ResultCommand.NoError //Selected more than one item.
                   );

            ContextMenu = new SimpleScriptCommand("ShowContextMenu", pm =>
            {
                (pm["FileList"] as IFileListViewModel).IsContextMenuVisible = true;
                return ResultCommand.OK;
            });

            ExportedCommandBindings = new[] 
            {
                ScriptCommandBinding.FromScriptCommand(ApplicationCommands.Open, this, (ch) => ch.Open, ParameterDicConverter),
                //ScriptCommandBinding.FromScriptCommand(ApplicationCommands.ContextMenu, this, (ch) => ch.ContextMenu, 
                //    ParameterDicConverter, ScriptBindingScope.Local)
            };
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public ParameterDicConverterBase ParameterDicConverter { get; private set; }
        public IEnumerable<IScriptCommandBinding> ExportedCommandBindings { get; private set; }
        public IScriptCommand Open { get; set; }
        public IScriptCommand ContextMenu { get; set; }

        #endregion
    }


}
