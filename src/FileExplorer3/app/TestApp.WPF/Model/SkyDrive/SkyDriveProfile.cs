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

        public SkyDriveProfile(string clientId, Func<string> authCodeFunc, string userId = "me")
        {
            ModelCache = new SkyDriveModelCache();

            HierarchyComparer = PathComparer.WebDefault;
            MetadataProvider = new NullMetadataProvider();
            CommandProviders = new List<ICommandProvider>();
            SuggestSource = new NullSuggestSource();

            PathMapper = new SkyDriveDiskPathMapper(this, null);
            DragDrop = new FileBasedDragDropHandler(this);
            _authClient = new LiveAuthClient(clientId);
            _authCodeFunc = authCodeFunc;
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

        private async Task<bool> connectAsync(string authCode)
        {
            if (authCode == null)
                return false;

            try
            {
                Session = await _authClient.ExchangeAuthCodeAsync(_authCode);                
            }
            catch
            {
                Session = null;
            }
            return Session != null;
        }

        private async Task<bool> checkLoginAsync()
        {                          
            if (Session != null && 
                Session.Expires.Subtract(DateTimeOffset.UtcNow) > TimeSpan.FromSeconds(1))
            {                
                return true;
            }

            else 
            {
                _authCode = _authCodeFunc();
                if (await connectAsync(_authCode))
                    return true;
            }

            return false;
        }



        public override async Task<IEntryModel> ParseAsync(string path)
        {
            await checkLoginAsync();

            string uid = ModelCache.GetUniqueId(path);
            if (uid != null)
                return ModelCache.GetModel(uid);

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
            await checkLoginAsync();

            if (filter == null)
                filter = e => true;

            List<IEntryModel> retList = new List<IEntryModel>();

            SkyDriveItemModel dirModel = entry as SkyDriveItemModel;
            if (dirModel != null)
            {
                var cachedChild = ModelCache.GetChildModel(dirModel);
                if (cachedChild != null)
                    return cachedChild.Where(m => filter(m)).ToList();


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
                ModelCache.RegisterChildModels(dirModel, retList.Cast<SkyDriveItemModel>().ToArray());
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
            else
                if (model.Name.IndexOf('.') != -1)
                    yield return GetFromSystemImageListUsingExtension.Instance;
        }

        #endregion

        #region Data

        private LiveAuthClient _authClient;
        private string _authCode = null;
        private string _userId;
        private Func<string> _authCodeFunc;

        #endregion

        #region Public Properties

        public LiveConnectSession Session { get; private set; }
        public ISkyDriveModelCache ModelCache { get; private set; }


        #endregion


    }
}
