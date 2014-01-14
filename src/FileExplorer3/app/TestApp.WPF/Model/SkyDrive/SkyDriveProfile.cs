using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.BaseControls;
using Microsoft.Live;

namespace FileExplorer.Models
{
    public class SkyDriveProfile : ProfileBase
    {
        #region Constructor

        public SkyDriveProfile(string clientId, string authCode, string userId = "me")
        {
            ModelCache = new SkyDriveModelCache();

            HierarchyComparer = PathComparer.WebDefault;
            MetadataProvider = new NullMetadataProvider();
            CommandProviders = new List<ICommandProvider>();
            SuggestSource = new NullSuggestSource();

            _authClient = new LiveAuthClient(clientId);
            _authCode = authCode;
            _userId = userId;
        }

        #endregion

        #region Methods

        private static string GetDirectoryName(string path)
        {
            if (path.EndsWith("/"))
                path = path.Substring(0, path.Length - 1); //Remove ending slash.

            int idx = path.LastIndexOf('/');
            if (idx == -1)
                return "";
            return path.Substring(0, idx);
        }

        private async Task<bool> CheckLoginAsync()
        {
            if (Session != null)
                return true;
            LiveConnectSession session = await _authClient.ExchangeAuthCodeAsync(_authCode);
            Session = session;
            return Session != null;

            //if (_authResult.Status == LiveConnectSessionStatus.Connected)
            //{
            //    Session = _authResult.Session;
            //    return true;
            //}
            //return false;
        }



        public override async Task<IEntryModel> ParseAsync(string path)
        {
            await CheckLoginAsync();

            LiveConnectClient client = new LiveConnectClient(Session);
            LiveOperationResult liveOpResult = await client.GetAsync(path);
            dynamic dynResult = liveOpResult.Result;

            //data
            //   |-album
            //       |-id, etc

            return ModelCache.RegisterModel(new SkyDriveItemModel(this, path, dynResult, null));
        }

        public override async Task<IEnumerable<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null)
        {
            if (filter == null)
                filter = e => true;

            List<IEntryModel> retList = new List<IEntryModel>();

            SkyDriveItemModel dirModel = entry as SkyDriveItemModel;
            if (dirModel != null)
            {
                LiveConnectClient client = new LiveConnectClient(Session);
                LiveOperationResult listOpResult = await client.GetAsync(dirModel.UniqueId + "/files");
                dynamic listResult = listOpResult.Result;


                foreach (dynamic itemData in listResult.data)
                {
                    SkyDriveItemModel retVal = null;
                    //if (itemData.type == "folder")
                        retVal = new SkyDriveItemModel(this, dirModel.AccessPath + "/" + itemData.name, itemData, dirModel.UniqueId);
                    //else retVal = new SkyDriveItemModel(this, dirModel.AccessPath + "/" + itemData.name, itemData, dirModel.UniqueId);

                    if (retVal != null && filter(retVal))
                        retList.Add(ModelCache.RegisterModel(retVal));
                }
            }
            
            return retList;
        }

        public override IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry)
        {
            foreach (var extractor in base.GetIconExtractSequence(entry))
                yield return extractor;

            SkyDriveItemModel model = entry as SkyDriveItemModel;
            if (model.ImageUrl != null)
                yield return new GetUriIcon(e => new Uri((e as SkyDriveItemModel).ImageUrl));            
        }

        #endregion

        #region Data

        private LiveAuthClient _authClient;
        private string _authCode;
        private string _userId;

        #endregion

        #region Public Properties

        public LiveConnectSession Session { get; private set; }
        public ISkyDriveModelCache ModelCache { get; private set; }


        #endregion


    }
}
