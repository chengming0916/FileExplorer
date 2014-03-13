using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class DropBoxLogin : ILoginInfo
    {
        DropBoxRequestToken _requestToken;
        string _clientId;
        string _clientSecret;
        private DropBoxConfiguration _config;

        public DropBoxLogin(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _config = CloudStorage.GetCloudConfigurationEasy
                (nSupportedCloudConfigurations.DropBox) as DropBoxConfiguration;
            _config.AuthorizationCallBack = new Uri("http://localhost");
            _requestToken =
                DropBoxStorageProviderTools.GetDropBoxRequestToken(_config, clientId, clientSecret);

            StartUrl = DropBoxStorageProviderTools.GetDropBoxAuthorizationUrl(_config, _requestToken);

        }

        public bool CheckLogin(BrowserStatus status)
        {
            if (status.Url.OriginalString.StartsWith("http://localhost"))
            {
                if (status.Url.OriginalString.Contains("oauth_token"))
                    AccessToken = DropBoxStorageProviderTools.ExchangeDropBoxRequestTokenIntoAccessToken(_config,
                        _clientId, _clientSecret, _requestToken);
                return true;
            }
            return false;
        }

        public string StartUrl
        {
            get;
            set;
        }

        public object AccessToken
        {
            get;
            set;
        }
    }
}
