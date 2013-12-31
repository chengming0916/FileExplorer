using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class FileListCommandsHelper : CommandsHelper, IHandle<SelectionChangedEvent>, IHandle<DirectoryChangedEvent>
    {
        #region Constructor

        public FileListCommandsHelper(IEventAggregator events, IProfile[] rootProfiles)
            : base(rootProfiles)
        {
            events.Subscribe(this);
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
        

        #endregion

        #region Data

        IEntryModel _currentDirectoryModel = null;

        #endregion

        #region Public Properties

        #endregion





    }
}
