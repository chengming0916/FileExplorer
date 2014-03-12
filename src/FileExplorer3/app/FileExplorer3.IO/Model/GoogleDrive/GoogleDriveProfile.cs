using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Utils;
using FileExplorer.BaseControls;
using Google.Apis.Drive.v2;
using Google.Apis.Http;
using Google.Apis.Services;

namespace FileExplorer.Models
{
    //Links - 
    //GoogleDrive API - https://developers.google.com/drive/web/file


    public class GoogleDriveProfile : DiskProfileBase
    {
        #region Constructor

        private static async Task<IConfigurableHttpClientInitializer> GetCredentialAsync(string clientSecretFile)
        {
            using (var stream = System.IO.File.OpenRead(clientSecretFile))
            {
                return await GetCredentialAsync(stream);
            }

        }

        private static async Task<IConfigurableHttpClientInitializer> GetCredentialAsync(Stream clientSecretStream)
        {
                var credential = await Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
                       Google.Apis.Auth.OAuth2.GoogleClientSecrets.Load(clientSecretStream).Secrets,
                       new[] { DriveService.Scope.Drive },
                       "user", CancellationToken.None);

                return credential;
        }

        public GoogleDriveProfile(IEventAggregator events, IWindowManager windowManager, 
                 string clientSecretFile,
                 string aliasMask = "{0}'s GoogleDrive",
                 string rootAccessPath = "/gdrive")
            : this(events, windowManager, AsyncUtils.RunSync(() => GoogleDriveProfile.GetCredentialAsync(clientSecretFile)),
                    aliasMask, rootAccessPath)
        {

        }

        public GoogleDriveProfile(IEventAggregator events, IWindowManager windowManager, 
                 Stream clientSecretStream,
                 string aliasMask = "{0}'s GoogleDrive",
                 string rootAccessPath = "/gdrive")
            : this(events, windowManager, AsyncUtils.RunSync(() => GoogleDriveProfile.GetCredentialAsync(clientSecretStream)),
                    aliasMask, rootAccessPath)
        {

        }

        internal GoogleDriveProfile(IEventAggregator events, IWindowManager windowManager, 
                   IConfigurableHttpClientInitializer credential,
                   string aliasMask = "{0}'s GoogleDrive",
                   string rootAccessPath = "/gdrive")
            : base(events)
        {            
            _driveService = new Google.Apis.Drive.v2.DriveService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "FileExplorer",
                    //ApiKey = clientId
                });
            _aboutInfo = AsyncUtils.RunSync(() => _driveService.About.Get().ExecuteAsync());

            Alias = String.Format(aliasMask, _aboutInfo.User.DisplayName);
            ModelCache = new EntryModelCache<GoogleDriveItemModel>(m => m.UniqueId, () => Alias, true);

            _aliasMask = aliasMask;
            Path = PathHelper.Web;
            DiskIO = new GoogleDriveDiskIOHelper(this);
            HierarchyComparer = PathComparer.WebDefault;
            MetadataProvider = new NullMetadataProvider();
            CommandProviders = new List<ICommandProvider>();
            SuggestSource = new NullSuggestSource();
            DragDrop = new FileBasedDragDropHandler(this, windowManager);
            _rootAccessPath = rootAccessPath;
            MimeTypeManager = new GoogleMimeTypeManager(_aboutInfo);
        }

        #endregion

        #region Methods

        public override async Task<IList<IEntryModel>> ListAsync(IEntryModel entry, CancellationToken ct, Func<IEntryModel, bool> filter = null,
            bool refresh = false)
        {
            if (filter == null)
                filter = e => true;

            GoogleDriveItemModel dirModel = (GoogleDriveItemModel)entry;

            if (dirModel != null)
            {
                if (!refresh)
                {
                    var cachedChild = ModelCache.GetChildModel(dirModel);
                    if (cachedChild != null)
                        return cachedChild.Where(m => filter(m)).Cast<IEntryModel>().ToList();
                }

                var listRequest = _driveService.Files.List();
                listRequest.Q = String.Format("'{0}' in parents", dirModel.UniqueId);
                var listResult = (await listRequest.ExecuteAsync().ConfigureAwait(false));
                var listResultItems = listResult.Items.ToList();
                var outputModels = listResultItems.Select(f =>  new GoogleDriveItemModel(this, f, dirModel.FullPath)).ToArray();
                ModelCache.RegisterChildModels(dirModel, outputModels);

                return outputModels.Where(m => filter(m)).Cast<IEntryModel>().ToList();

                //var listedItemIds = (await _driveService.Children.List(dirModel.UniqueId).ExecuteAsync(ct)).Items;
                //var listedItemGetFileTasks = listedItemIds.Select(fr => _driveService.Files.Get(fr.Id).ExecuteAsync()
                //    .ContinueWith<GoogleDriveItemModel>(f => ModelCache.RegisterModel(
                //        new GoogleDriveItemModel(this, f.Result, dirModel.FullPath))));
                //var listedItems = (await Task.WhenAll<GoogleDriveItemModel>(listedItemGetFileTasks)).ToList();

                //ModelCache.RegisterChildModels(dirModel, listedItems.ToArray());
                //return listedItems.Cast<IEntryModel>().Where(m => filter(m)).ToList();
            }
            return new List<IEntryModel>();
        }

        public override async Task<IEntryModel> ParseAsync(string path)
        {
            string uid = ModelCache.GetUniqueId(ModelCache.CheckPath(path));
            if (uid != null)
                return ModelCache.GetModel(uid);

            if (string.IsNullOrEmpty(path) || path == Alias)
                return ModelCache.RegisterModel(new GoogleDriveItemModel(this, Alias));

            return null;
        }

        private static bool isImage(string ext)
        {
            string imageMask = ".jpg.jpeg.png.gif.bmp";
            return !String.IsNullOrEmpty(ext) && (imageMask.IndexOf(ext, 0, StringComparison.CurrentCultureIgnoreCase) != -1);
        }

        public override IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry)
        {
            foreach (var extractor in base.GetIconExtractSequence(entry))
                yield return extractor;

            GoogleDriveItemModel model = entry as GoogleDriveItemModel;
            string ext = Path.GetExtension(model.Name);
            if (model.ImageUrl != null && isImage(ext))
                yield return new GetUriIcon(e => new Uri((e as GoogleDriveItemModel).ImageUrl), HttpClientFunc);
            else if (model.Name.IndexOf('.') != -1)
                yield return GetFromSystemImageListUsingExtension.Instance;

            if (entry.FullPath.Equals(Alias))
                yield return GoogleDriveLogo;
        }

        #endregion

        #region Data

        private DriveService _driveService;

        private string _authCode = null;
        private string _rootAccessPath;
        private Func<string> _authCodeFunc;
        private string _aliasMask;
        private Google.Apis.Drive.v2.Data.About _aboutInfo;

        #endregion

        #region Public Properties

        private static GetResourceIcon GoogleDriveLogo = new GetResourceIcon("FileExplorer3.IO", "/Model/GoogleDrive/GoogleDrive_Logo.png");
        public string Alias { get; protected set; }
        public string RootAccessPath { get { return _rootAccessPath; } }
        public IEntryModelCache<GoogleDriveItemModel> ModelCache { get; private set; }
        public IMimeTypeManager MimeTypeManager { get; private set; }
        public DriveService DriveService { get { return _driveService; } }
        public Func<HttpClient> HttpClientFunc { get { return () => _driveService.HttpClient; } }

        #endregion



    }
}
