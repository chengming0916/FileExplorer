using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Utils;

namespace FileExplorer.Models
{
    public class SkyDriveLogin : ILoginInfo
    {
        public SkyDriveLogin(string clientId)
        {
            StartUrl = "https://login.live.com/oauth20_authorize.srf?client_id=" + clientId +
                "&redirect_uri=https%3A%2F%2Flogin.live.com%2Foauth20_desktop.srf" +
                "&scope=wl.signin%20wl.basic%20wl.offline_access%20wl.skydrive&response_type=code&display=windesktop&locale=en-GB&state=&theme=win7";
        }

        public bool CheckLogin(Uri url)
        {
            var dic = ParamStringUtils.ParseParamString(url.AbsoluteUri);
            if (dic.ContainsKey("code"))
            {
                AuthCode = dic["code"];
                return true;
            }
            return false;
        }

        public string StartUrl { get; private set; }
        public string AuthCode { get; private set; }
    }
}
