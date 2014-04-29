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
        ExplorerConfiguration Explorer { get; set; }
        FileListConfiguration FileList { get; set; }
    }

    public interface IExplorerConfiguration : INotifyPropertyChanged
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


    public interface IFileListConfiguration : INotifyPropertyChanged
    {
        int ItemSize { get; set; }
        string ViewMode { get; set; }
    }

     [XmlRoot("Configuration")]
    public class Configuration : NotifyPropertyChanged, IConfiguration
    {
        private string _name = "Unnamed";

        public Configuration() { }
        public Configuration(string name, ExplorerConfiguration explorer = null, 
            FileListConfiguration fileList = null)
        {
            Name = name ?? "Unnamed";
            Explorer = explorer ?? new ExplorerConfiguration();
            FileList = fileList ?? new FileListConfiguration();
        }

        public string Name { get { return _name; } set { _name = value; NotifyOfPropertyChanged(() => Name); } }
        public ExplorerConfiguration Explorer { get; set; }
        public FileListConfiguration FileList { get; set; }

      
    }

     [XmlRoot("Explorer")]
    public class ExplorerConfiguration : NotifyPropertyChanged, IExplorerConfiguration
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
    public class FileListConfiguration: NotifyPropertyChanged, IFileListConfiguration
    {
        private int _itemSize = 60;
        private string _viewMode = "Icon";
        public int ItemSize { get { return _itemSize; } set { _itemSize = value; NotifyOfPropertyChanged(() => ItemSize); } }
        public string ViewMode { get { return _viewMode; } set { _viewMode = value; NotifyOfPropertyChanged(() => ViewMode); } }
        
     
    
    }
}
