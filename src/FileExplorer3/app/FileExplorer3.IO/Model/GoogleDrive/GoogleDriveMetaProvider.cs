//using Cofe.Core.Utils;
//using FileExplorer.Defines;
//using FileExplorer.Models;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace FileExplorer.Models
//{

//    public class GoogleDriveMetadataProvider : MetadataProviderBase
//    {
//        public GoogleDriveMetadataProvider()
//            : base(new FileBasedMetadataProvider())
//        {

//        }

//        public override async Task<IEnumerable<IMetadata>> GetMetadataAsync(IEnumerable<IEntryModel> selectedModels, int modelCount, IEntryModel parentModel)
//        {
//            List<IMetadata> retList = new List<IMetadata>();
//            retList.AddRange(await base.GetMetadataAsync(selectedModels, modelCount, parentModel));

//            if (selectedModels.Count() == 1)
//            {
//                var itemModel = selectedModels.First() as GoogleDriveItemModel;
//                if (itemModel.Metadata != null)
//                {
//                    itemModel.Metadata.
//                    var dictionary = itemModel.Metadata as IDictionary<string, object>;
//                    foreach (var key in SupportedKeys)
//                    {
//                        if (dictionary.ContainsKey(key))
//                        {
//                            object val = dictionary[key];
//                            if (val != null)
//                                switch (key)
//                                {
//                                    case "when_taken":
//                                        retList.Add(new Metadata(DisplayType.TimeElapsed, "OneDrive", key, DateTime.Parse(val as string)) { IsVisibleInSidebar = true });
//                                        break;
//                                    case "link":
//                                        retList.Add(new Metadata(DisplayType.Link, "OneDrive", key, val) { IsVisibleInSidebar = true });
//                                        break;
//                                    default:
//                                        retList.Add(new Metadata(DisplayType.Auto, "OneDrive", key, val) { IsVisibleInSidebar = true });
//                                        break;
//                                }

//                        }
//                    }
//                }
//            }


//            return retList;
//        }

//    }
//}
