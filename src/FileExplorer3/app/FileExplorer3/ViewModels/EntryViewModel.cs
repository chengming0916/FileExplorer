using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{

    public class EntryViewModel : PropertyChangedBase, IEntryViewModel
    {
        #region Cosntructor

        private EntryViewModel()
        {

        }

        public static EntryViewModel FromEntryModel(IProfile profile, IEntryModel model)
        {
            return new EntryViewModel()
            {
                Profile = profile,
                EntryModel = model
            };
        }

        

        #endregion

       

        #region Methods

        #endregion

        #region Data
        

        #endregion

        #region Public Properties

        public IProfile Profile { get; private set; }
        public IEntryModel EntryModel { get; private set; }

        #endregion
    }
}
