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
            DropNetClient client, UserLogin accessToken,
            string aliasMask = "{0}'s DropBox")
            : base(events)
        {
            _client = client;
            _accessToken = accessToken;
            Path = PathHelper.Web;

            Alias = String.Format(aliasMask, _client.AccountInfo().display_name);
            RootModel = new DropBoxItemModel(this, _client.GetMetaData("/"));

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

        private string convertRemotePath(string path)
        {
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

            var dirEntry = (entry as DropBoxItemModel);

            if (dirEntry == null)
                throw new ArgumentException("Entry");

            ct.ThrowIfCancellationRequested();

            var fetchedMetadata = await _client.GetMetaDataTask(convertRemotePath(entry.FullPath));


            List<IEntryModel> retVal = new List<IEntryModel>();

            foreach (var metadata in fetchedMetadata.Contents)
            {
                var newModel = new DropBoxItemModel(this, metadata, dirEntry.FullPath);
                if (filter(newModel))
                    retVal.Add(newModel);
            }

            return retVal;
        }

        public override async Task<IEntryModel> ParseAsync(string path)
        {
            string remotePath = convertRemotePath(path);
            if (String.IsNullOrEmpty(remotePath))
                return RootModel;
            else
            {
                var fetchedMetadata = await _client.GetMetaDataTask(remotePath);
                return new DropBoxItemModel(this, fetchedMetadata, Path.GetDirectoryName(path));
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

            if (model.FullPath == Alias)
                yield return DropBoxLogo;
        }

        #endregion

        #region Data
        private static GetResourceIcon DropBoxLogo = new GetResourceIcon("FileExplorer3.IO", "/Model/DropBox/DropBox_Logo.png");
        private DropNetClient _client;
        private UserLogin _accessToken;
        #endregion

        #region Public Properties

        public IEntryModel RootModel { get; protected set; }
        public string Alias { get; protected set; }

        #endregion
    }
}
