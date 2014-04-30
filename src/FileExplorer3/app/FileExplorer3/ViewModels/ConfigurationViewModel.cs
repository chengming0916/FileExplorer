using FileExplorer.Defines;
using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public class ConfigurationViewModel : NotifyPropertyChanged, IConfigurationViewModel
    {
        #region Constructors

        public ConfigurationViewModel(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        public IConfiguration Configuration
        {
            get;
            private set;
        }

        #endregion
       
    
    }
}
