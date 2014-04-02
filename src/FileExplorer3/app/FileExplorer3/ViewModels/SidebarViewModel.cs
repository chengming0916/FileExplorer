using Caliburn.Micro;
using FileExplorer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public class SidebarViewModel : ViewAware, ISidebarViewModel
    {
        #region Constructors

        public SidebarViewModel(IEventAggregator events)
        {
            Commands = new SidebarCommandManager(this, events);
        }

        #endregion

        #region Methods

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            var uiEle = view as System.Windows.UIElement;
            Commands.RegisterCommand(uiEle, ScriptBindingScope.Local);
        }

        #endregion

        #region Data

        private bool _isVisible = false;

        #endregion

        #region Public Properties

        public bool IsVisible { get { return _isVisible; } set { _isVisible = value; NotifyOfPropertyChange(() => IsVisible);}}
        public ICommandManager Commands { get; private set; }

        #endregion

       
    }
      
}
