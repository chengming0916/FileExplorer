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
        void RegisterChildModels(SkyDriveItemModel parentModel, SkyDriveItemModel[] childModels);
        SkyDriveItemModel[] GetChildModel(SkyDriveItemModel parentModel);

        string GetUniqueId(string path);
        string GetPath(string uniqueId);
        SkyDriveItemModel GetModel(string uniqueId);

    }

    public class SkyDriveModelCache : ISkyDriveModelCache
    {
        #region Constructor

        public SkyDriveModelCache()
        {
            UniqueIdLookup = new ConcurrentDictionary<string, string>();
            ChildLookup = new ConcurrentDictionary<string, IList<string>>();
            ModelCache = new ConcurrentDictionary<string, SkyDriveItemModel>();            
        }

        #endregion

        #region Methods        

        public SkyDriveItemModel RegisterModel(SkyDriveItemModel model)
        {
            UniqueIdLookup[model.FullPath] = model.UniqueId;
            ModelCache[model.UniqueId] = model;
            return model;
        }

        public string GetUniqueId(string accessPath)
        {
            if (!(UniqueIdLookup.ContainsKey(accessPath)))
                return null;
            else return UniqueIdLookup[accessPath];
        }

        public string GetPath(string uniqueId)
        {
            var ppair = UniqueIdLookup.FirstOrDefault(pp => pp.Value == uniqueId);
            if (ppair.Equals(default(KeyValuePair<string, string>)))
                return null;
            else return ppair.Key;
        }

        public SkyDriveItemModel GetModel(string uniqueId)
        {
            return ModelCache[uniqueId];
        }

        public void RegisterChildModels(SkyDriveItemModel parentModel, SkyDriveItemModel[] childModels)
        {
            foreach (var cm in childModels)
                RegisterModel(cm);
            ChildLookup[parentModel.UniqueId] = (from cm in childModels select cm.UniqueId).ToList();
        }

        public SkyDriveItemModel[] GetChildModel(SkyDriveItemModel parentModel)
        {
            if (ChildLookup.ContainsKey(parentModel.UniqueId))
            {
                return (from uid in ChildLookup[parentModel.UniqueId] select GetModel(uid)).ToArray();
            }
            else return null;
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
      

        #endregion

    }
}
