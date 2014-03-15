using Caliburn.Micro;
using DropNet;
using DropNet.Models;
using FileExplorer.BaseControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class DropBoxProfile : DiskProfileBase
    {

        #region Constructors

        /// <summary>
        /// For DropBox
        /// </summary>
        /// <param name="events"></param>
        /// <param name="requestToken"></param>
        public DropBoxProfile(IEventAggregator events, IWindowManager windowManager, 
            Func<DropNetClient> clientFunc,
            string aliasMask = "{0}'s DropBox")
            : base(events)
        {
            ModelCache = new EntryModelCache<DropBoxItemModel>(m => m.FullPath, () => Alias, true);
            //_accessToken = accessToken;
            Path = PathHelper.Web;
            _clientFunc = clientFunc;
            
            _client = GetClient();

            _thumbnailExtractor = new DropBoxModelThumbnailExtractor(() => _client);
            Alias = String.Format(aliasMask, _client.AccountInfo().display_name);
            RootModel = ModelCache.RegisterModel(new DropBoxItemModel(this, _client.GetMetaData("/")));

            //DiskIO = new SkyDriveDiskIOHelper(this);
            HierarchyComparer = PathComparer.WebDefault;
            MetadataProvider = new NullMetadataProvider();
            CommandProviders = new List<ICommandProvider>();
            SuggestSource = new NullSuggestSource();
            //PathMapper = new SkyDriveDiskPathMapper(this, null);
            DragDrop = new FileBasedDragDropHandler(this, windowManager);


        }



        #endregion

        #region Methods

        public string ConvertRemotePath(string path)
        {
            path = ModelCache.CheckPath(path);

            if (path == Alias)
                return "/";

            if (path.StartsWith(Alias))
                return path.Substring(Alias.Length + 1);
            return path;
        }

        public override async Task<IList<IEntryModel>> ListAsync(IEntryModel entry, CancellationToken ct,
            Func<IEntryModel, bool> filter = null, bool refresh = false)
        {
            if (filter == null)
                filter = em => true;

            List<IEntryModel> retList = new List<IEntryModel>();
            List<DropBoxItemModel> cacheList = new List<DropBoxItemModel>();

            var dirModel = (entry as DropBoxItemModel);
            if (dirModel != null)
            {
                if (!refresh)
                {
                    var cachedChild = ModelCache.GetChildModel(dirModel);
                    if (cachedChild != null)
                        return cachedChild.Where(m => filter(m)).Cast<IEntryModel>().ToList();
                }
                ct.ThrowIfCancellationRequested();

                var fetchedMetadata = await _client.GetMetaDataTask(ConvertRemotePath(entry.FullPath));
                foreach (var metadata in fetchedMetadata.Contents)
                {
                    var retVal = new DropBoxItemModel(this, metadata, dirModel.FullPath);
                    cacheList.Add(ModelCache.RegisterModel(retVal));
                    if (filter(retVal))
                        retList.Add(retVal);
                }
                ModelCache.RegisterChildModels(dirModel, cacheList.ToArray());
            }
            return retList;

        }

        public override async Task<IEntryModel> ParseAsync(string path)
        {
            string fullPath = ModelCache.GetUniqueId(ModelCache.CheckPath(path));
            if (fullPath != null)
                return ModelCache.GetModel(fullPath);

            string remotePath = ConvertRemotePath(path);
            if (String.IsNullOrEmpty(remotePath))
                return RootModel;
            else
            {
                var fetchedMetadata = await _client.GetMetaDataTask(remotePath);
                return ModelCache.RegisterModel(
                    new DropBoxItemModel(this, fetchedMetadata, Path.GetDirectoryName(path)));
            }
        }


        public override IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry)
        {
            foreach (var extractor in base.GetIconExtractSequence(entry))
                yield return extractor;

            DropBoxItemModel model = entry as DropBoxItemModel;
            //if (model.ImageUrl != null)
            //    yield return new GetUriIcon(e => new Uri((e as SkyDriveItemModel).ImageUrl));
            //else
            if (model.Name.IndexOf('.') != -1)
                yield return GetFromSystemImageListUsingExtension.Instance;

            if (model.Metadata.Thumb_Exists)
                yield return _thumbnailExtractor;

            if (model.FullPath == Alias)
                yield return DropBoxLogo;
        }

        public DropNetClient GetClient() { return _clientFunc(); }

        #endregion

        #region Data
        private static GetResourceIcon DropBoxLogo = new GetResourceIcon("FileExplorer3.IO", "/Model/DropBox/DropBox_Logo.png");
        private UserLogin _accessToken;
        private DropBoxModelThumbnailExtractor _thumbnailExtractor;
        private Func<DropNetClient> _clientFunc;
        private DropNetClient _client;
        #endregion

        #region Public Properties

        public IEntryModel RootModel { get; protected set; }
        public string Alias { get; protected set; }
        public IEntryModelCache<DropBoxItemModel> ModelCache { get; private set; }

        #endregion
    }
}
