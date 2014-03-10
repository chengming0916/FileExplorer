using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    public static class AuthorizationKeys
    {
        //https://account.live.com/developers/applications
        public static string SkyDrive_Client_Id = "Replace_your_SkyDrive_Client_Id";
        public static string SkyDrive_Client_Secret = "Replace_your_SkyDrive_Client_Secret";
        //https://cloud.google.com/console/project 
        //The code below is not used at this time, please download your secret to gapi_client_secret.json
        public static string GoogleDrive_Client_Id = "Replace_your_GoogleDrive_Client_Id";
        public static string GoogleDrive_Client_Secret = "Replace_your_GoogleDrive_Client_Secret";
    }
}
