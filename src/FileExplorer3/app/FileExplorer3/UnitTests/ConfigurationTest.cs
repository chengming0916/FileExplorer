using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Utils;
using FileExplorer.ViewModels;
using FileExplorer.ViewModels.Helpers;
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

            XmlSerializer ser = new XmlSerializer(entry.GetType(),
                new Type[] { typeof(Configuration), typeof(FileListParameters), typeof(ExplorerParameters) });

            ser.Serialize(ms, entry);

            ms.Seek(0, SeekOrigin.Begin);
            using (TextReader sr = new StreamReader(ms))
            {
                string contents = sr.ReadToEnd();
                Assert.AreNotEqual("", contents);
                return contents;
            }

        }

        private static T readXml<T>(string serializedContent)
        {
            MemoryStream ms = new FileExplorer.Utils.UnclosableMemoryStream();
            using (var tw = new StreamWriter(ms))
                tw.Write(serializedContent);

            ms.Seek(0, SeekOrigin.Begin);
            XmlSerializer ser = new XmlSerializer(typeof(T),
                new Type[] { typeof(Configuration), typeof(FileListParameters), typeof(ExplorerParameters) });

            return (T)ser.Deserialize(ms);

        }

        public static void FullConfigurationTest()
        {
            FileExplorer.Defines.Configuration c1 = new FileExplorer.Defines.Configuration("Test") { };
            FileExplorer.Defines.Configuration c2 = new FileExplorer.Defines.Configuration("Test1") { };
            c1.Explorer.UIScale = 2;
            c2.Explorer.UIScale = 1.5f;
            c1.FileList.ItemSize = 10;
            c2.FileList.ItemSize = 20;


            string content = writeXml(c1);
            c2 = readXml<Configuration>(content);

            Assert.AreEqual(2, c2.Explorer.UIScale);
            Assert.AreEqual(10, c2.FileList.ItemSize);

        }

        public static void ExplorerTest()
        {
            ExplorerParameters ec1 = new ExplorerParameters() { UIScale = 2 };
            ExplorerParameters ec2 = new ExplorerParameters() { UIScale = 1.5f };

            string content = writeXml(ec1);
            ec2 = readXml<ExplorerParameters>(content);


            Assert.AreEqual(2.0f, ec2.UIScale);
        }

        public static void FileListTest()
        {
            FileListParameters fc1 = new FileListParameters() { ItemSize = 200, ViewMode = "TEST" };
            FileListParameters fc2 = new FileListParameters() { };

            string content = writeXml(fc1);
            fc2 = readXml<FileListParameters>(content);

            Assert.AreEqual(200, fc2.ItemSize);
            Assert.AreEqual("TEST", fc2.ViewMode);
        }

        public static void EntriesHelper_Insert_And_Remove_Test()
        {
            EntriesHelper<IConfiguration> helper = new EntriesHelper<IConfiguration>();
            IConfiguration config1 = new Configuration("Config1");
            IConfiguration config2 = new Configuration("Config2");


            helper.Add(config2);
            Assert.AreEqual(1, helper.AllNonBindable.Count());
            
            helper.Insert(0, config1);
            Assert.AreEqual(2, helper.AllNonBindable.Count());
            Assert.AreEqual(2, helper.All.Count());
            Assert.AreEqual(config1, helper.AllNonBindable.First());
           
            helper.Remove(config1);
            Assert.AreEqual(1, helper.AllNonBindable.Count());
            Assert.AreEqual(config2, helper.AllNonBindable.First());

            helper.RemoveAt(0);
            Assert.AreEqual(0, helper.AllNonBindable.Count());
        }

        public static void ConfigurationHelper_Test()
        {
            ConfigurationHelper ch = new ConfigurationHelper();
            ConfigurationHelper ch2 = new ConfigurationHelper();
            ch.Add(new Configuration("Config1"));
            ch.Add(new Configuration("Config2"));

            UnclosableMemoryStream ms = new UnclosableMemoryStream();
            AsyncUtils.RunSync(() => ch.SaveAsync(ms));
            //ms.Seek(0, SeekOrigin.Begin);
            //string content;
            //using (var sr = new StreamReader(ms))
            //    content = sr.ReadToEnd();
            ms.Seek(0, SeekOrigin.Begin);
            AsyncUtils.RunSync(() => ch2.LoadAsync(ms));


            Assert.AreEqual(2, ch2.Configurations.AllNonBindable.Count());
        }

        [Test]
        public static void Test()
        {
            FileListTest();
            ExplorerTest();
            FullConfigurationTest();
            EntriesHelper_Insert_And_Remove_Test();
            ConfigurationHelper_Test();
        }
    }
}
