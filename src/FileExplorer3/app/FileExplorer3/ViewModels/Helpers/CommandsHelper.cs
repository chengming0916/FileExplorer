using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ICommandsHelper
    {
        void RefreshCommands();
        EntriesHelper<ICommandViewModel> Commands { get; }
    }

    public class CommandsHelper : NotifyPropertyChanged, ICommandsHelper, IHandle<SelectionChangedEvent>, IHandle<DirectoryChangedEvent>
    {
        #region Constructor

        public CommandsHelper(IEventAggregator events, IProfile[] rootProfiles)
        {
            _rootProfiles = rootProfiles;
            Commands = new EntriesHelper<ICommandViewModel>(loadCommandsTask);
            AsyncUtils.RunSync(() => Commands.LoadAsync(true));
            events.Subscribe(this);
        }

        #endregion

        #region Methods

        public void RefreshCommands()
        {
            foreach (var commandVM in Commands.AllNonBindable)
                commandVM.CommandModel.NotifySelectionChanged(_appliedModels);
        }


        public void Handle(SelectionChangedEvent message)
        {
            AppliedModels = message.SelectedModels.ToArray();
            RefreshCommands();
        }

        public void Handle(DirectoryChangedEvent message)
        {
            AppliedModels = new IEntryModel[] { message.NewModel };
            RefreshCommands();
        }

        async Task<IEnumerable<ICommandViewModel>> loadCommandsTask()
        {
            List<ICommandModel> cmList = new List<ICommandModel>()
            {
                //new CommandModel(null) { Header = "Play", Symbol= Convert.ToChar(0xE102) },
                //new DirectoryCommandModel(null, 
                //    new CommandModel(null) { Header = "Play", Symbol= Convert.ToChar(0xE102) }
                //) { Header = "Folder" },
                //new SliderCommandModel(null,
                //    new SliderStepCommandModel() { Header = "ExtraLargeIcon", SliderStep = 200, ItemHeight=100 },
                //    new SliderStepCommandModel() { Header = "LargeIcon", SliderStep = 100, ItemHeight=60 },
                //    new SliderStepCommandModel() { Header = "SmallIcon", SliderStep = 20 },
                //    new SliderStepCommandModel() { Header = "List", SliderStep = 18 })
                //    { Header="View" }
            };


            foreach (var cp in _rootProfiles.SelectMany(p => p.CommandProviders))
                cmList.AddRange(cp.CommandModels);

            return cmList.Select(cm => new CommandViewModel(cm)).ToArray();
        }


        #endregion

        #region Data

        IEntryModel[] _appliedModels = null;
        IProfile[] _rootProfiles = null;

        #endregion

        #region Public Properties

        public IEntryModel[] AppliedModels { get { return _appliedModels; } set { _appliedModels = value; NotifyOfPropertyChanged(() => AppliedModels); } }
        public EntriesHelper<ICommandViewModel> Commands { get; private set; }

        #endregion





    }
}
