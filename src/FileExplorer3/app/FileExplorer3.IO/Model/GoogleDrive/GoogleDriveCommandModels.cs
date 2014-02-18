using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Caliburn.Micro;
using Cofe.Core.Script;
using FileExplorer.Utils;
using FileExplorer.ViewModels;

namespace FileExplorer.Models
{
    public class GoogleExportDirectoryCommandModel : DirectoryCommandModel
    {


        #region GoogleExportModel

        public class GoogleExportModel : CommandModel
        {
            private string _url;
            private string _ext;
            private IEntryModel[] _rootModels;
            private IWindowManager _windowManager;
            private IEventAggregator _events;
            public GoogleExportModel(string mimeType, string ext, string url,
                IWindowManager windowManager, IEventAggregator events, IEntryModel[] rootModels)                
            {
                Header = String.Format("{0} ({1})", mimeType, ext);
                _rootModels = rootModels;
                _windowManager = windowManager;
                _events = events;
                _ext = ext;
                _url = url;

                IsEnabled = true;
                this.IsVisibleOnMenu = true;
                this.IsHeaderVisible = true;

                //Command = new SimpleScriptCommand("GoogleExport", (pd) =>
                //    {
                //        string filter = String.Format("{0} ({1})|*{1}", mimeType, ext);
                //        var filePicker = new FilePickerViewModel(_events, _windowManager, filter, FilePickerMode.Save, _rootModels);
                //        if (_windowManager.ShowDialog(filePicker).Value)
                //        {
                //            MessageBox.Show(filePicker.FileName);
                //        }                        
                //        return ResultCommand.NoError;
                //    });
            }

            
        }

        #endregion


        #region Constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootModels">Root directory used when saving.</param>
        public GoogleExportDirectoryCommandModel(Func<string, string, string> getDestinationFunc, IEntryModel[] rootModels)
        {
            _rootModels = rootModels;
            //_events = events;
            //_windowManager = windowManager;
            this.Symbol = Convert.ToChar(0xE118);
            this.Header = "Export";
            this.IsVisibleOnToolbar = false;
            this.IsVisibleOnMenu = true;
            this.IsEnabled = false;
        }

        #endregion

        #region Methods

        public override void NotifySelectionChanged(IEntryModel[] appliedModels)
        {
            List<ICommandModel> subItemList = new List<ICommandModel>();
            if (appliedModels.Count() == 1)
            {
                GoogleDriveItemModel model = appliedModels[0] as GoogleDriveItemModel;
                if (model != null && model.Metadata != null && model.Metadata.ExportLinks != null)
                    foreach (var mimeType in model.Metadata.ExportLinks.Keys)
                    {
                        string url = model.Metadata.ExportLinks[mimeType];

                        string ext = null;
                        var match = Regex.Match(url, "[&]exportFormat=(?<ext>[\\w]*)$", RegexOptions.IgnoreCase);
                        if (match.Success)
                            ext = "." + match.Groups["ext"].Value;
                        else ext = ShellUtils.MIMEType2Extension(mimeType);

                        if (ext != null)
                            subItemList.Add(new GoogleExportModel(mimeType, ext, url, _windowManager, _events,_rootModels));
                    }
            }
            
            SubCommands = subItemList;
            this.IsEnabled = subItemList.Count() > 0;
        }

        #endregion

        #region Data

        private IEntryModel[] _rootModels;
        private IWindowManager _windowManager;
        private IEventAggregator _events;

        #endregion

        #region Public Properties

        #endregion

    }
}
