using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;

namespace FileExplorer.ViewModels.Helpers
{
    public interface ICommandsHelper 
    {
        void RefreshCommands();
        EntriesHelper<ICommandViewModel> Commands { get; }
    }

    public class CommandsHelper : ICommandsHelper, IHandle<SelectionChangedEvent>, IHandle<DirectoryChangedEvent>
    {
        #region Constructor

        public CommandsHelper(IEventAggregator events)
        {
            Commands = new EntriesHelper<ICommandViewModel>(loadCommandsTask);
            events.Subscribe(this);
        }

        #endregion

        #region Methods

        public void RefreshCommands()
        {
            Commands.LoadAsync(true);
        }


        public void Handle(SelectionChangedEvent message)
        {
            _appliedModels = message.SelectedModels.ToArray();
            RefreshCommands();
        }

        public void Handle(DirectoryChangedEvent message)
        {
            _appliedModels = new IEntryModel[] { message.NewModel };
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

            if (_appliedModels != null && _appliedModels.Count() > 0)
            {
                foreach (ICommandProvider cp in _appliedModels.First().Profile.CommandProviders)
                    cmList.AddRange(await cp.GetCommandsAsync(_appliedModels));
            }
            return cmList.Select(cm => new CommandViewModel(cm)).ToArray();
        }


        #endregion

        #region Data

        IEntryModel[] _appliedModels = null;

        #endregion

        #region Public Properties

        public EntriesHelper<ICommandViewModel> Commands { get; private set; }

        #endregion




       
    }
}
