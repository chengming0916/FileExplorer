using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Cinch;
using Cofe.Core;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{



    public class FileListToolbarCommandsHelper : ToolbarCommandsHelper, IHandle<SelectionChangedEvent>, IHandle<DirectoryChangedEvent>
    {
        #region Constructor

        public FileListToolbarCommandsHelper(IFileListViewModel flvm, IEventAggregator events, params IProfile[] rootProfiles)
            : base(rootProfiles)            
        {
            events.Subscribe(this);
            _flvm = flvm;

            ExtraCommandProviders = new [] { 
                new StaticCommandProvider(
                    new SelectGroupCommand(flvm),    
                    new ViewModeCommand(flvm),
                    new SeparatorCommandModel()                    
                    ) 
            };
            _exportedCommandBindings.Add(new ScriptCommandBinding(ExplorerCommands.ToggleCheckBox, p => true, p => ToggleCheckBox(), null, ScriptBindingScope.Explorer));
            _exportedCommandBindings.Add(new ScriptCommandBinding(ExplorerCommands.ToggleViewMode, p => true, p => ToggleViewMode(), null, ScriptBindingScope.Explorer));
        }

        #endregion

        #region Methods


        public void Handle(SelectionChangedEvent message)
        {
            AppliedModels =
                message.SelectedModels.Count() == 0 ?
                new IEntryModel[] { _currentDirectoryModel } :
                message.SelectedModels.ToArray();
        }

        public void Handle(DirectoryChangedEvent message)
        {
            _currentDirectoryModel = message.NewModel;
            AppliedModels = new IEntryModel[] { _currentDirectoryModel };
        }

        public void ToggleViewMode()
        {
            var viewModeWoSeparator = ViewModeCommand.ViewModes.Where(vm => vm.IndexOf(",-1") == -1).ToArray();

            int curIdx = ViewModeCommand.findViewMode(viewModeWoSeparator, _flvm.ItemSize);
            int nextIdx = curIdx + 1;
            if (nextIdx >= viewModeWoSeparator.Count()) nextIdx = 0;

            string viewMode; int step; int itemHeight;
            ViewModeCommand.parseViewMode(viewModeWoSeparator[nextIdx], out viewMode, out step, out itemHeight);
            ViewModeCommand vmc = this.CommandModels.AllNonBindable.First(c => c.CommandModel is ViewModeCommand)
                .CommandModel as ViewModeCommand;
            vmc.SliderValue = step;
        }

        public void ToggleCheckBox()
        {
            _flvm.IsCheckBoxVisible = !_flvm.IsCheckBoxVisible;
        }



        #endregion

        #region Data

        IFileListViewModel _flvm = null;
        IEntryModel _currentDirectoryModel = null;

        #endregion

        #region Public Properties



        #endregion







    }
}
