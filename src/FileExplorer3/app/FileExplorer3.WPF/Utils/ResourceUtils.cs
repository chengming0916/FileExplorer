﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;

namespace FileExplorer.Utils
{
    public static class ResourceUtils
    {
        public static Stream GetEmbeddedResourceAsStream(object sender, string path2Resource)
        {
            var assembly = System.Reflection.Assembly.GetAssembly(sender.GetType());
            string libraryName = assembly.GetName().Name;
            string resourcePath = PathUtils.MakeResourcePath(libraryName, path2Resource);
            if (assembly.GetManifestResourceNames().Contains(resourcePath))
                return assembly.GetManifestResourceStream(resourcePath);
            else return new MemoryStream();
        }

        public static byte[] GetEmbeddedResourceAsByteArray(object sender, string path2Resource)
        {
            return GetEmbeddedResourceAsStream(sender, path2Resource).ToByteArray();
        }



        public static Stream GetResourceAsStream(object sender, string path2Resource)
        {
            var assembly = System.Reflection.Assembly.GetAssembly(sender.GetType());
            string libraryName = assembly.GetName().Name;
            Uri resourceUri = PathUtils.MakeResourceUri(libraryName, path2Resource);
            try
            {
                StreamResourceInfo info = Application.GetResourceStream(resourceUri);
                return info.Stream;
            }
            catch { return new MemoryStream(); }
        }

        public static byte[] GetResourceAsByteArray(object sender, string path2Resource)
        {
            return GetResourceAsStream(sender, path2Resource).ToByteArray();
        }

    }
}
