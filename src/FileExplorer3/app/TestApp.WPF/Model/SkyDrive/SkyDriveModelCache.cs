using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface ISkyDriveModelCache
    {
        SkyDriveItemModel RegisterModel(SkyDriveItemModel model);
        string GetUniqueId(string path);
        string GetPath(string uniqueId);

    }

    public class SkyDriveModelCache : ISkyDriveModelCache
    {
        #region Constructor

        public SkyDriveModelCache()
        {
            UniqueIdLookup = new ConcurrentDictionary<string, string>();
            ChildLookup = new ConcurrentDictionary<string, IList<string>>();
            ModelCache = new ConcurrentDictionary<string, SkyDriveItemModel>();
            ModelLookup = new Dictionary<string, SkyDriveItemModel>();
        }

        #endregion

        #region Methods

        public SkyDriveItemModel RegisterModel(SkyDriveItemModel model)
        {
            UniqueIdLookup[model.FullPath] = model.UniqueId;
            ModelCache[model.UniqueId] = model;
            return model;
        }

        public string GetUniqueId(string path)
        {
            if (!(UniqueIdLookup.ContainsKey(path)))
                throw new KeyNotFoundException();
            else return UniqueIdLookup[path];
        }

        public string GetPath(string uniqueId)
        {
            var ppair = UniqueIdLookup.FirstOrDefault(pp => pp.Value == uniqueId);
            if (ppair.Equals(default(KeyValuePair<string, string>)))
                return "";
            else return ppair.Key;
        }

        #endregion

        #region Data

        #endregion

        #region Public Properties

        /// <summary>
        /// Dictionary for path -> uniqueId
        /// </summary>
        public ConcurrentDictionary<string, string> UniqueIdLookup { get; private set; }

        /// <summary>
        /// Dictionary for uniqueId -> child UniqueId list
        /// </summary>
        public ConcurrentDictionary<string, IList<string>> ChildLookup { get; private set; }

        /// <summary>
        /// DIctionary for uniqueId -> SkyDriveInfoModel
        /// </summary>
        public ConcurrentDictionary<string, SkyDriveItemModel> ModelCache { get; private set; }


        /// <summary>
        /// Directory for uniqueId -> SkyDriveInfoModel
        /// </summary>
        public Dictionary<string, SkyDriveItemModel> ModelLookup { get; private set; }

        #endregion

    }
}
