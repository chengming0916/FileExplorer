using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class SharpBoxProfile : DiskProfileBase
    {

        #region Constructors

        /// <summary>
        /// For DropBox
        /// </summary>
        /// <param name="events"></param>
        /// <param name="requestToken"></param>
        public SharpBoxProfile(IEventAggregator events, IWindowManager windowManager,
            ICloudStorageConfiguration config, ICloudStorageAccessToken accessToken,
            string aliasMask = "{0}'s DropBox")
            : base(events)
        {
            _cloudStorage = new CloudStorage();
            _cloudStorage.Open(config, accessToken);

            var rootFolder = _cloudStorage.GetFolder("/");
            RootModel = new SharpBoxItemModel(this, rootFolder);
            Alias = "DropBox";
            
            //DropBoxConfiguration config =
            //    CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox) as DropBoxConfiguration;
            //requestToken = DropBoxStorageProviderTools
            //    .GetDropBoxRequestToken(config, clientId, clientSecret);
            //ICloudStorageAccessToken accessToken = _cloudStorage.DeserializeSecurityToken(tokenStream);

            //_cloudStorage.Open(cloudConfig, accessToken);
        }


        ~SharpBoxProfile()
        {
            if (_cloudStorage != null)
            {
                if (_cloudStorage.IsOpened)
                    _cloudStorage.Close();
                _cloudStorage = null;
            }
        }
        #endregion

        #region Methods

        public override async Task<IList<IEntryModel>> ListAsync(IEntryModel entry, System.Threading.CancellationToken ct, Func<IEntryModel, bool> filter = null, bool refresh = false)
        {
            var dirEntry = (entry as SharpBoxItemModel).Metadata as ICloudDirectoryEntry;

            if (dirEntry == null)
                throw new ArgumentException("Entry");

            ct.ThrowIfCancellationRequested();

            List<IEntryModel> retVal = new List<IEntryModel>();

            foreach (var item in dirEntry)
                retVal.Add(new SharpBoxItemModel(this, item, entry.FullPath));

            return retVal;
        }

        public override Task<IEntryModel> ParseAsync(string path)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data

        private CloudStorage _cloudStorage;
        #endregion

        #region Public Properties

        public IEntryModel RootModel { get; protected set; }
        public string Alias { get; protected set; }

        #endregion
    }
}
