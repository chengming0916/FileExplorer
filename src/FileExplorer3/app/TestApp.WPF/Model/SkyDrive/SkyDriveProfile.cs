using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Live;

namespace FileExplorer.Models
{
    public class SkyDriveProfile : ProfileBase 
    {
        #region Constructor

        public SkyDriveProfile(string clientId)
        {
            ModelCache = new SkyDriveModelCache();

            HierarchyComparer = PathComparer.Default;
            MetadataProvider = new NullMetadataProvider();
            CommandProviders = new List<ICommandProvider>();

            _authClient = new LiveAuthClient(clientId);
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

        private async Task<bool> CheckLoginAsync()
        {
            if (Session != null)
                return true;
            string url = _authClient.GetLoginUrl(new[] { "wl.signin", "wl.basic", "wl.skydrive" });
            var authResult = await _authClient.IntializeAsync(new[] { "wl.signin", "wl.basic", "wl.skydrive" });
            if (authResult.Status == LiveConnectSessionStatus.Connected)
            {
                Session = authResult.Session;
                return true;
            }
            return false;
        }

        public override async Task<IEntryModel> ParseAsync(string path)
        {
            await CheckLoginAsync();

            LiveConnectClient client = new LiveConnectClient(Session);
            LiveOperationResult liveOpResult = await client.GetAsync(path);
            dynamic dynResult = liveOpResult.Result;

            return new SkyDriveDirectoryModel(this, dynResult, null);
        }

        public override Task<IEnumerable<IEntryModel>> ListAsync(IEntryModel entry, Func<IEntryModel, bool> filter = null)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Data

        private LiveAuthClient _authClient;

        #endregion

        #region Public Properties
        
        public LiveConnectSession Session { get; private set; }
        public ISkyDriveModelCache ModelCache { get; private set; }


        #endregion


    }
}
