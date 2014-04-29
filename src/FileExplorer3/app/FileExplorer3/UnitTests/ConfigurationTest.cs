using FileExplorer.Defines;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FileExplorer.UnitTests
{
    [TestFixture]
    public class ConfigurationTest
    {
        private static string writeXml(object entry)
        {
            MemoryStream ms = new FileExplorer.Utils.UnclosableMemoryStream();

            XmlSerializer ser = new XmlSerializer(typeof(Configuration),
                new Type[] { typeof(FileListConfiguration), typeof(ExplorerConfiguration) });

            ser.Serialize(ms, entry);

            ms.Seek(0, SeekOrigin.Begin);
            using (TextReader sr = new StreamReader(ms))
            {
                string contents = sr.ReadToEnd();
                Assert.AreNotEqual("", contents);
                return contents;
            }

        }

        private static object readXml(string serializedContent)
        {
            MemoryStream ms = new FileExplorer.Utils.UnclosableMemoryStream();
            using (var tw = new StreamWriter(ms))
                tw.Write(serializedContent);

            ms.Seek(0, SeekOrigin.Begin);
            XmlSerializer ser = new XmlSerializer(typeof(Configuration),
                  new Type[] { typeof(FileListConfiguration), typeof(ExplorerConfiguration) });

            return ser.Deserialize(ms);

        }

        [Test]
        public static void FullConfigurationTest()
        {
            FileExplorer.Defines.Configuration c1 = new FileExplorer.Defines.Configuration("Test") { };
            FileExplorer.Defines.Configuration c2 = new FileExplorer.Defines.Configuration("Test1") { };
            c1.Explorer.UIScale = 2;
            c2.Explorer.UIScale = 1.5f;
            c1.FileList.ItemSize = 10;
            c2.FileList.ItemSize = 20;


            string content = writeXml(c1);
            c2 = readXml(content) as FileExplorer.Defines.Configuration;

            Assert.AreEqual(2, c2.Explorer.UIScale);
            Assert.AreEqual(10, c2.FileList.ItemSize);

        }

        [Test]
        public static void ExplorerTest()
        {
            ExplorerConfiguration ec1 = new ExplorerConfiguration() { UIScale = 2 };
            ExplorerConfiguration ec2 = new ExplorerConfiguration() { UIScale = 1.5f };

            string content = writeXml(ec1);
            ec2 = readXml(content) as FileExplorer.Defines.ExplorerConfiguration;


            Assert.AreEqual(2.0f, ec2.UIScale);
        }

        [Test]
        public static void FileListTest()
        {
            FileListConfiguration fc1 = new FileListConfiguration() { ItemSize = 200, ViewMode = "TEST" };
            FileListConfiguration fc2 = new FileListConfiguration() { };

            string content = writeXml(fc1);
            fc2 = readXml(content) as FileExplorer.Defines.FileListConfiguration;

            Assert.AreEqual(200, fc2.ItemSize);
            Assert.AreEqual("TEST", fc2.ViewMode);
        }
    }
}
