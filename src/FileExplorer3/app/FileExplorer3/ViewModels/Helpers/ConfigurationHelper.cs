using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public class ConfigurationHelper : IConfigurationHelper
    {
        #region Constructors

        #endregion

        #region Methods

        #endregion

        #region Data

        #endregion

        #region Public Properties

        #endregion
        public Helpers.EntriesHelper<IConfigurationViewModel> Configurations
        {
            get { throw new NotImplementedException(); }
        }

        public Task LoadAsync(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task SaveAsync(System.IO.Stream stream)
        {
            throw new NotImplementedException();
        }

        public IConfigurationViewModel Insert(Defines.IConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        public void Remove(string name)
        {
            throw new NotImplementedException();
        }
    }
}
