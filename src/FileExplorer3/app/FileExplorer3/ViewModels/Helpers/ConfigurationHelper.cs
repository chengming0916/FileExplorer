using FileExplorer.Defines;
using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileExplorer.ViewModels
{
    public class ConfigurationHelper : NotifyPropertyChanged, IConfigurationHelper
    {
        #region Constructors

        public ConfigurationHelper()
        {
            Configurations = new EntriesHelper<IConfigurationViewModel>();
        }

        #endregion

        #region Methods

        public IConfigurationViewModel Add(IConfiguration configuration)
        {
            var retVal = new ConfigurationViewModel(configuration);
            if (Configurations.AllNonBindable.Any(cvm => cvm.Configuration.Name.Equals(configuration.Name)))
                throw new Exception(String.Format("{0} already exists.", configuration.Name));
            Configurations.Add(retVal);
            return retVal;
        }

        public void Remove(string name)
        {
            var lookup = Configurations.AllNonBindable.FirstOrDefault(cvm => cvm.Configuration.Name == name);
            if (lookup == null)
                throw new KeyNotFoundException(name);
            Configurations.Remove(lookup);
        }

        private XmlSerializer getSerializer()
        {
            return new XmlSerializer(typeof(List<Configuration>),
              new Type[] { typeof(Configuration), typeof(FileListParameters), typeof(ExplorerParameters) });
        }

        public async Task LoadAsync(Stream stream)
        {
            var serializer = getSerializer();
            List<Configuration> configList =
                serializer.Deserialize(stream) as List<Configuration>;
            Configurations.SetEntries(configList.Select(c => new ConfigurationViewModel(c)).ToArray());
        }

        public async Task SaveAsync(Stream stream)
        {
            var serializer = getSerializer();
            List<Configuration> configList =
                Configurations.AllNonBindable
                .Select(cvm => cvm.Configuration as Configuration)
                .ToList();
            serializer.Serialize(stream, configList);
        }

        #endregion

        #region Data

        private int _selectedIndex = 0;

        #endregion

        #region Public Properties

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value; NotifyOfPropertyChanged(() => SelectedIndex);
                NotifyOfPropertyChanged(() => SelectedConfiguration);
            }
        }


        public IConfigurationViewModel SelectedConfiguration
        {
            get { return Configurations.AllNonBindable.Count() > SelectedIndex ? 
                Configurations.AllNonBindable.ToArray()[SelectedIndex] : null; }
        }

        public EntriesHelper<IConfigurationViewModel> Configurations
        {
            get;
            private set;
        }

        #endregion







    }
}
