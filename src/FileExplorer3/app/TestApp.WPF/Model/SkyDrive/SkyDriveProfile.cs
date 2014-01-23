using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using FileExplorer.BaseControls;
using Microsoft.Live;

namespace FileExplorer.Models
{
    public class SkyDriveProfile : DiskProfileBase
    {
        #region Constructor

        public SkyDriveProfile(IEventAggregator events, string clientId, Func<string> authCodeFunc,
            string alias = "SkyDrive",
            string rootAccessPath = "/me/skydrive")
            : base(events)
        {
            ModelCache = new SkyDriveModelCache();
            Alias = alias;
            Path = PathHelper.Web;
            DiskIO = new SkyDriveDiskIOHelper(this);
            HierarchyComparer = PathComparer.WebDefault;
            MetadataProvider = new NullMetadataProvider();
            CommandProviders = new List<ICommandProvider>();
            SuggestSource = new NullSuggestSource();

            //PathMapper = new SkyDriveDiskPathMapper(this, null);
            DragDrop = new FileBasedDragDropHandler(this);
            _authClient = new LiveAuthClient(clientId);
            _authCodeFunc = authCodeFunc;
            _rootAccessPath = rootAccessPath;
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

        internal async Task<bool> checkLoginAsync()
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

        public async Task<IEntryModel> LookupAsync(IEntryModel currentModel, string[] path, int idx = 0)
        {
            if (idx < path.Length)
            {
                IEntryModel foundModel = (await ListAsync(currentModel, em => em.Label.Equals(path[idx],
                    StringComparison.CurrentCultureIgnoreCase), true)).FirstOrDefault();
                if (foundModel == null)
                    return null;
                else return await LookupAsync(foundModel, path, idx + 1);
            }
            else return currentModel;
        }

        public string GetAccessPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return _rootAccessPath;

            path = path.TrimStart('/');

            if (path.StartsWith(Alias, StringComparison.CurrentCultureIgnoreCase))
                if (path.Equals(Alias, StringComparison.CurrentCultureIgnoreCase))
                    path = "";
                else path = path.Substring((Alias).Length + 1);

            return String.IsNullOrEmpty(path) ? _rootAccessPath : _rootAccessPath + '/' + path;
        }

        public override async Task<IEntryModel> ParseAsync(string path)
        {
            await checkLoginAsync();

            string accessPath = GetAccessPath(path);

            if (Session == null)
                return null;

            string uid = ModelCache.GetUniqueId(accessPath);
            if (uid != null)
                return ModelCache.GetModel(uid);

            if (accessPath == _rootAccessPath)
            {
                LiveConnectClient client = new LiveConnectClient(Session);
                LiveOperationResult liveOpResult = await client.GetAsync(accessPath);
                dynamic dynResult = liveOpResult.Result;

                //data
                //   |-album
                //       |-id, etc

                return ModelCache.RegisterModel(new SkyDriveItemModel(this, accessPath, dynResult, null));
            }
            else
            {
                var rootModel = await ParseAsync("");
                return await LookupAsync(rootModel,
                    path.Replace(rootModel.FullPath, "")
                    .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public override async Task<IList<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null, bool refresh = false)
        {
            await checkLoginAsync();

            if (filter == null)
                filter = e => true;

            List<IEntryModel> retList = new List<IEntryModel>();
            List<SkyDriveItemModel> cacheList = new List<SkyDriveItemModel>();

            SkyDriveItemModel dirModel = entry as SkyDriveItemModel;
            if (dirModel != null)
            {
                if (!refresh)
                {
                    var cachedChild = ModelCache.GetChildModel(dirModel);
                    if (cachedChild != null)
                        return cachedChild.Where(m => filter(m)).Cast<IEntryModel>().ToList();
                }

                LiveConnectClient client = new LiveConnectClient(Session);
                LiveOperationResult listOpResult = await client.GetAsync(dirModel.UniqueId + "/files");
                dynamic listResult = listOpResult.Result;


                foreach (dynamic itemData in listResult.data)
                {
                    SkyDriveItemModel retVal = null;
                    retVal = new SkyDriveItemModel(this, dirModel.AccessPath + "/" + itemData.name, itemData, dirModel.FullPath);

                    if (retVal != null)
                    {
                        cacheList.Add(ModelCache.RegisterModel(retVal));
                        if (filter(retVal))
                            retList.Add(retVal);
                    }
                }
                ModelCache.RegisterChildModels(dirModel, cacheList.ToArray());
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
        private string _rootAccessPath;
        private Func<string> _authCodeFunc;

        #endregion

        #region Public Properties

        public string Alias { get; protected set; }
        public string RootAccessPath { get { return _rootAccessPath; } }
        public LiveConnectSession Session { get; private set; }
        public ISkyDriveModelCache ModelCache { get; private set; }


        #endregion


    }
}
