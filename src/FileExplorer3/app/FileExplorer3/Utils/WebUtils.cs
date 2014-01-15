using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Utils
{
    public static class WebUtils
    {
        public static async Task<byte[]> DownloadAsync(Uri uri)
        {
            byte[] bytes = null;
            using (WebClient webClient = new WebClient())
            {
                webClient.Proxy = null;  //avoids dynamic proxy discovery delay
                webClient.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                try
                {
                    bytes = await webClient.DownloadDataTaskAsync(uri);
                }
                catch { }
            }
            return bytes;
        }
    }
}
