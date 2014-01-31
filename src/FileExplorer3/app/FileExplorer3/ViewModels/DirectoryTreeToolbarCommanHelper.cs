using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class DirectoryTreeToolbarCommandsHelper : ToolbarCommandsHelper, IHandle<DirectoryChangedEvent>
    {
        
        #region Constructor

        public DirectoryTreeToolbarCommandsHelper(IDirectoryTreeViewModel dlvm, IEventAggregator events, params IProfile[] rootProfiles)
            : base(rootProfiles)
        {
            events.Subscribe(this);
            _dlvm = dlvm;
        }

        #endregion

        #region Methods

        public void Handle(DirectoryChangedEvent message)
        {
            AppliedModels = new[] { message.NewModel };
        }

        #endregion

        #region Data

        private IDirectoryTreeViewModel _dlvm;

        #endregion

        #region Public Properties

        #endregion
        
    }
}
