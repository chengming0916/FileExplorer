//using AppLimit.CloudComputing.SharpBox;
//using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
//using Caliburn.Micro;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FileExplorer.Models
//{
//    public class SharpBoxProfile : DiskProfileBase
//    {
       
//        #region Constructors

//        /// <summary>
//        /// For DropBox
//        /// </summary>
//        /// <param name="events"></param>
//        /// <param name="requestToken"></param>
//        public SharpBoxProfile(IEventAggregator events, IWindowManager windowManager, 
//            string clientId, string clientSecret, Func<string> authCodeFunc,
//            string aliasMask = "{0}'s DropBox",
//            string rootAccessPath = "/me")
//            : base(events)
//        {
//            _cloudStorage = new CloudStorage();
//            DropBoxConfiguration config = 
//                CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox) as DropBoxConfiguration;
//            requestToken = DropBoxStorageProviderTools
//                .GetDropBoxRequestToken(config,  clientId,  clientSecret); 
//            ICloudStorageAccessToken accessToken = _cloudStorage.DeserializeSecurityToken(tokenStream);

//            _cloudStorage.Open(cloudConfig, accessToken);
//        }


//        ~SharpBoxProfile()
//        {
//            if (_cloudStorage != null)
//            {
//                _cloudStorage.Close();
//                _cloudStorage = null;
//            }
//        }
//        #endregion

//        #region Methods

//        #endregion

//        #region Data

//        private CloudStorage _cloudStorage;

//        #endregion

//        #region Public Properties

//        #endregion
//    }
//}
