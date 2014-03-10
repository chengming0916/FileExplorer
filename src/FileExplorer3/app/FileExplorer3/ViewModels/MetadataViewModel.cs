using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.Models;

namespace FileExplorer.ViewModels
{
    public class MetadataViewModel : PropertyChangedBase, IMetadataViewModel
    {
        #region Cosntructor

        public static MetadataViewModel FromMetadata(IMetadata metadata)
        {
            return new MetadataViewModel() { MetadataModel = metadata };
        }

        public static MetadataViewModel FromText(string header, string value, bool isHeader = false)
        {
            return FromMetadata(
                new Metadata(Defines.DisplayType.Text, header, value) { IsHeader = isHeader });
        }

        #endregion

        #region Methods

        #endregion

        #region Data
        

        #endregion

        #region Public Properties

        public IMetadata MetadataModel { get; private set; }

        #endregion
    }
}
