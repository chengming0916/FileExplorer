using Caliburn.Micro;
using FileExplorer.Defines;
using FileExplorer.IO.Compress;
using FileExplorer.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.Models.SevenZipSharp
{
    public class SzsProfile : DiskProfileBase, IWPFProfile
    {

        #region Constructors

        public SzsProfile(IProfile baseProfile, IEventAggregator events, IWindowManager windowManager)
            : base(events)
        {
            HierarchyComparer = baseProfile.HierarchyComparer;
            DragDrop = new FileBasedDragDropHandler(this, windowManager);
            DiskIO = new SzsDiskIOHelper(this);
            _baseProfile = baseProfile;
            _wrapper = new SevenZipWrapper();
        }

        #endregion

        #region Methods

        public IEntryModel Convert(IEntryModel entryModel)
        {
            if (entryModel != null && entryModel.Profile is IDiskProfile && _wrapper.IsArchive(entryModel.Name))
                return new SzsRootModel(entryModel, this);

            return entryModel;
        }

        public override async Task<IList<IEntryModel>> ListAsync(IEntryModel entry, System.Threading.CancellationToken ct, Func<IEntryModel, bool> filter = null, bool refresh = false)
        {
            var retList = new List<IEntryModel>();

            if (entry is ISzsItemModel)
            {
                filter = filter ?? (em => true);
                var szsEntry = entry as ISzsItemModel;
                var szsRoot = szsEntry.Root;
                var referencedEntry = szsRoot.ReferencedFile;
                var refEntryProfile = referencedEntry.Profile as IDiskProfile;
                if (refEntryProfile != null && !referencedEntry.IsDirectory)
                {
                    string listPattern = String.Format(RegexPatterns.CompressionListPattern, Regex.Escape(szsEntry.RelativePath));

                    using (var stream = await refEntryProfile.DiskIO.OpenStreamAsync(referencedEntry,
                        Defines.FileAccess.Read, CancellationToken.None))
                        foreach (object afi in _wrapper.List(stream, listPattern))
                        {
                            var em = new SzsChildModel(szsRoot, (SevenZip.ArchiveFileInfo)afi);
                            if (filter(em))
                                retList.Add(em);
                        }
                }
            }

            return retList;
        }

        public override async Task<IEntryModel> ParseAsync(string path)
        {
            string curPath = path;
            IEntryModel retVal = Convert(await _baseProfile.ParseAsync(path));
            while (retVal == null && curPath != null)
            {
                curPath = _baseProfile.Path.GetDirectoryName(curPath);
                retVal = Convert(await _baseProfile.ParseAsync(curPath));
            }

            if (retVal != null && curPath != path && path.StartsWith(curPath, StringComparison.CurrentCultureIgnoreCase))
            {                
                string[] trailingPaths = path.Substring(curPath.Length).Split(new char[] { '\\','/' }, StringSplitOptions.RemoveEmptyEntries );
                return await this.LookupAsync(retVal, trailingPaths, CancellationToken.None, 0);
            }

            return retVal;

            throw new NotImplementedException();
        }

        public override IEnumerable<IModelIconExtractor<IEntryModel>> GetIconExtractSequence(IEntryModel entry)
        {
            yield return GetFromSystemImageListUsingExtension.Instance;
            //return base.GetIconExtractSequence(entry);
        }

        #endregion

        #region Data

        private SevenZipWrapper _wrapper;
        private IProfile _baseProfile;

        #endregion

        #region Public Properties

        internal SevenZipWrapper Wrapper { get { return _wrapper; } }

        #endregion
    }
}
