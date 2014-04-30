using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace FileExplorer.Defines
{
    public interface IConfiguration : INotifyPropertyChanged
    {
        string Name { get; }
        IExplorerParameters Explorer { get; set; }
        IFileListParameters FileList { get; set; }
    }

    public interface IExplorerParameters : INotifyPropertyChanged
    {
        /// <summary>
        /// Default is 1.
        /// </summary>
        float UIScale { get; set; }

        /// <summary>
        /// In grid length format (e.g. 2*)
        /// </summary>
        string FileListSize { get; set; }

        /// <summary>
        /// In grid length format (e.g. *)
        /// </summary>
        string DirectoryTreeSize { get; set; }

        /// <summary>
        /// Actual pixel size (default 30)
        /// </summary>
        int NavigationSize { get; set; }

        /// <summary>
        /// Actual pixel size (default 30)
        /// </summary>
        int StatusbarSize { get; set; }
        //float SidebarSize { get; set; }
    }


    public interface IFileListParameters : INotifyPropertyChanged
    {
        int ItemSize { get; set; }
        string ViewMode { get; set; }
    }

    [XmlRoot("Configuration")]
    public class Configuration : NotifyPropertyChanged, IConfiguration
    {
        private string _name = "Unnamed";

        public Configuration() { }
        public Configuration(string name, ExplorerParameters explorer = null,
            FileListParameters fileList = null)
        {
            Name = name ?? "Unnamed";
            Explorer = explorer ?? new ExplorerParameters();
            FileList = fileList ?? new FileListParameters();
        }

        public string Name { get { return _name; } set { _name = value; NotifyOfPropertyChanged(() => Name); } }
        [XmlIgnore]
        public IExplorerParameters Explorer { get; set; }
        [XmlIgnore]
        public IFileListParameters FileList { get; set; }
       
        [XmlElement("Explorer")]
        public ExplorerParameters ExplorerSerialized
        {
            get { return Explorer as ExplorerParameters; }
            set { Explorer = value; }
        }

        [XmlElement("FileList")]
        public FileListParameters FileListSerialized
        {
            get { return FileList as FileListParameters; }
            set { FileList = value; }
        }

    }

    [XmlRoot("Explorer")]
    public class ExplorerParameters : NotifyPropertyChanged, IExplorerParameters
    {
        private float _uiScale = 1f;
        private string _directoryTreeSize = "*";
        private string _fileListSize = "2*";
        private int _navigationSize = 30;
        private int _statusbarSize = 30;

        public float UIScale { get { return _uiScale; } set { _uiScale = value; NotifyOfPropertyChanged(() => UIScale); } }
        public string DirectoryTreeSize { get { return _directoryTreeSize; } set { _directoryTreeSize = value; NotifyOfPropertyChanged(() => DirectoryTreeSize); } }
        public string FileListSize { get { return _fileListSize; } set { _fileListSize = value; NotifyOfPropertyChanged(() => FileListSize); } }

        public int NavigationSize { get { return _navigationSize; } set { _navigationSize = value; NotifyOfPropertyChanged(() => NavigationSize); } }
        public int StatusbarSize { get { return _statusbarSize; } set { _statusbarSize = value; NotifyOfPropertyChanged(() => StatusbarSize); } }

        //public float SidebarSize { get { return _sidebarSize; } set { _sidebarSize = value; NotifyOfPropertyChanged(() => SidebarSize); } } 
    }

    [XmlRoot("FileList")]
    public class FileListParameters : NotifyPropertyChanged, IFileListParameters
    {
        private int _itemSize = 60;
        private string _viewMode = "Icon";
        public int ItemSize { get { return _itemSize; } set { _itemSize = value; NotifyOfPropertyChanged(() => ItemSize); } }
        public string ViewMode { get { return _viewMode; } set { _viewMode = value; NotifyOfPropertyChanged(() => ViewMode); } }



    }
}
