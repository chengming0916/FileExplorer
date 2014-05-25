using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Utils
{
    public static class ResourceUtils
    {
        public static Stream GetEmbeddedResourceAsStream(object sender, string path2Resource)
        {
            var assembly = System.Reflection.Assembly.GetAssembly(sender.GetType());
            string libraryName = assembly.GetName().Name;
            string resourcePath = PathUtils.MakeResourcePath(libraryName, path2Resource);

            return assembly.GetManifestResourceStream(resourcePath);
        }

        public static byte[] GetEmbeddedResourceAsByteArray(object sender, string path2Resource)
        {
            return GetEmbeddedResourceAsStream(sender, path2Resource).ToByteArray();
        }
    }
}
