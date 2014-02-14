using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Google.Apis.Http;
using Google.Apis.Services;

namespace FileExplorer.Models
{
    public class GoogleDriveProfile : DiskProfileBase
    {
        #region Constructor

        public GoogleDriveProfile(IEventAggregator events, IWindowManager windowManager, string clientId,
                   IConfigurableHttpClientInitializer credential,
                   string aliasMask = "{0}'s GoogleDrive",
                   string rootAccessPath = "/gdrive")
            : base(events)
        {
            //new Google.Apis.Drive.v2.DriveService(
            //    new BaseClientService.Initializer()
            //    {
            //         HttpClientInitializer = credential,
            //         ApplicationName = "FileExplorer",
            //         ApiKey = clientId
            //    };
        }

        #endregion

        #region Methods

        public override Task<IList<IEntryModel>> ListAsync(IEntryModel entry, System.Threading.CancellationToken ct, Func<IEntryModel, bool> filter = null, bool refresh = false)
        {
            throw new NotImplementedException();
        }

        public override Task<IEntryModel> ParseAsync(string path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<IEntryModelIconExtractor> GetIconExtractSequence(IEntryModel entry)
        {
            return base.GetIconExtractSequence(entry);
        }

        public override IComparer<IEntryModel> GetComparer(Defines.ColumnInfo column)
        {
            return base.GetComparer(column);
        }
        
        #endregion

        #region Data
        
        #endregion

        #region Public Properties
        
        #endregion


    }
}
