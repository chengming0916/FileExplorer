using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Cofe.Core;
using Cofe.Core.Script;
using FileExplorer.BaseControls;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;
using vms = FileExplorer.ViewModels;

namespace FileExplorer.ViewModels
{
    public class FileNameFilter
    {
        public string Description { get; set; }
        public string Filter { get; set; }

        public FileNameFilter(string descdription, string filter)
        {
            Description = descdription;
            Filter = filter;
        }
    }

    public enum FilePickerMode { Open, Save }

    public class FilePickerViewModel : ExplorerViewModel
    {
        #region Constructor

        public FilePickerViewModel(IEventAggregator events, IWindowManager windowManager, string filterStr,
            FilePickerMode mode = FilePickerMode.Open, params IEntryModel[] rootModels)
            : base(events, windowManager, rootModels)
        {
            WindowTitleMask = Enum.GetName(typeof(FilePickerMode), mode);
            _mode = mode;
            _tryCloseCommand = new SimpleScriptCommand("TryClose", pd => { TryClose(true); return ResultCommand.NoError; });

            FileList.Commands.ScriptCommands.Open = vms.FileList.IfSelection(evm => evm.Count() == 1,
                   vms.FileList.IfSelection(evm => evm[0].EntryModel.IsDirectory,
                       OpenSelectedDirectory.FromFileList,  //Selected directory
                       new SimpleScriptCommand("", (pd) =>
                       {
                           switch (mode)
                           {
                               case FilePickerMode.Open: Open(); break;
                               case FilePickerMode.Save: Save(); break;
                           }

                           return ResultCommand.NoError;
                       })),   //Selected non-directory
                   ResultCommand.NoError //Selected more than one item.                   
                   );

            FileList.Selection.SelectionChanged += (o1, e1) =>
            {
                string newFileName = String.Join(",",
                    FileList.Selection.SelectedItems.Where(evm => !evm.EntryModel.IsDirectory)
                    .Select(evm => evm.EntryModel.Profile.Path.GetFileName(evm.EntryModel.FullPath)));

                if (!String.IsNullOrEmpty(newFileName))
                    FileName = newFileName;
            };

            _filterStr = filterStr;
            Filters = new EntriesHelper<FileNameFilter>(loadFiltersTask);
            Filters.SetEntries(getFilters(filterStr).ToArray());

            FileList.EnableDrag = false;
            FileList.EnableDrop = false;
            FileList.EnableMultiSelect = false;
        }

        #endregion

        #region Methods

        IEnumerable<FileNameFilter> getFilters(string filterStr)
        {
            string[] filterSplit = filterStr.Split('|');
            List<FileNameFilter> ff = new List<FileNameFilter>();
            for (int i = 0; i < filterSplit.Count() / 2; i++)
                ff.Add(new FileNameFilter(filterSplit[i * 2], filterSplit[(i * 2) + 1]));

            return ff;
        }

        Task<IEnumerable<FileNameFilter>> loadFiltersTask()
        {
            return Task.Run(() =>
                {
                    string[] filterSplit = _filterStr.Split('|');
                    List<FileNameFilter> ff = new List<FileNameFilter>();
                    for (int i = 0; i < filterSplit.Count() / 2; i++)
                        ff.Add(new FileNameFilter(filterSplit[i * 2], filterSplit[(i * 2) + 1]));

                    return ff as IEnumerable<FileNameFilter>;
                });


        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
            UserControl uc = view as UserControl;
            uc.Loaded += (o, e) =>
                {
                    SelectedFilter = Filters.AllNonBindable.First().Filter;
                };
        }

        //public void Save()
        //{
        //    string fullPath = FileList.CurrentDirectory.Profile.Path.Combine(FileList.CurrentDirectory.FullPath, FileName);
        //    var foundItem = FileList.ProcessedEntries.EntriesHelper.AllNonBindable.Select(evm => evm.EntryModel)
        //            .FirstOrDefault(em =>
        //            em.FullPath.Equals(fullPath, StringComparison.CurrentCultureIgnoreCase));
        //    if (foundItem != null)
        //    {
        //        string name = foundItem.Profile.Path.GetFileName(foundItem.FullPath);
        //        new IfOkCancel(_windowManager, () => "Overwrite", String.Format("Overwrite {0}?", name), 
        //    }

        //}

        public void Save()
        {
            var pm = FileList.Commands.ParameterDicConverter.Convert(new ParameterDic());
            var profile = FileList.CurrentDirectory.Profile;

            //Update FileName in case user does not enter full path name.
            IScriptCommand updateFileName =
                new SimpleScriptCommand("updateFileName", pd =>
                { FileName = profile.Path.Combine(FileList.CurrentDirectory.FullPath, FileName); return ResultCommand.NoError; });

            //Update SelectedFiles property (if it's exists.")
            Func<IEntryModel, IScriptCommand> setSelectedFiles =
                m => new SimpleScriptCommand("SetSelectedFiles", pd =>
                {
                    SelectedFiles = new[] { m }; FileName = m.FullPath; return ResultCommand.NoError;
                });

            //Query whether to overwrite and if so, setSelectedFiles, otherwise throw a user cancel exception and no window close.
            Func<IEntryModel, IScriptCommand> queryOverwrite =
                m => new IfOkCancel(_windowManager,
                        pd => "Overwrite",
                        pd => "Overwrite " + profile.Path.GetFileName(FileName),
                        setSelectedFiles(m), ResultCommand.Error(new Exception("User cancel")) /* Cancel if user press cancel. */);

            new ScriptRunner().Run(pm,
                ScriptCommands.RunInSequence(
                     ViewModels.FileList.Lookup(
                               m => m.FullPath.Equals(FileName) || m.Profile.Path.GetFileName(m.FullPath).Equals(FileName),
                               queryOverwrite,
                               FileList.CurrentDirectory.Profile.Parse(FileName, queryOverwrite,
                               updateFileName))),
                    _tryCloseCommand
                );
        }

        public void Open()
        {
            List<IEntryModel> selectedFiles = new List<IEntryModel>();
            Func<IEntryModel, IScriptCommand> addToSelectedFiles =
                m => new SimpleScriptCommand("AddToSelectedFiles", pd => { selectedFiles.Add(m); return ResultCommand.NoError; });
            var pm = FileList.Commands.ParameterDicConverter.Convert(new ParameterDic());
            new ScriptRunner().Run(pm,
            ScriptCommands.RunInSequence(
                ScriptCommands.ForEach(
                    FileName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                    f => ViewModels.FileList.Lookup(
                            m => m.FullPath.Equals(f) || m.Profile.Path.GetFileName(m.FullPath).Equals(f),
                            addToSelectedFiles,
                            FileList.CurrentDirectory.Profile.Parse(f, addToSelectedFiles,
                            ResultCommand.Error(new System.IO.FileNotFoundException(f + " is not found.")))))),
                new SimpleScriptCommand("AssignSelectedFiles", pd => { SelectedFiles = selectedFiles.ToArray(); return ResultCommand.NoError; }),
                _tryCloseCommand
             );
        }

        public void Cancel()
        {
            TryClose(false);
        }

        private void setFilter(string value)
        {
            _selectedFilter = value;

            base.FileList.ProcessedEntries.SetFilters(
                ColumnFilter.CreateNew(value, "FullPath",
                e => e.IsDirectory || PathFE.MatchFileMasks(e.Profile.Path.GetFileName(e.FullPath), value)));
            NotifyOfPropertyChange(() => SelectedFilter);
        }


        private void setFileName(string value)
        {
            _selectedFileName = value;
            NotifyOfPropertyChange(() => FileName);
            CanOpen = !String.IsNullOrEmpty(value);
        }

        #endregion

        #region Data

        protected IScriptCommand _tryCloseCommand;
        private static ColumnFilter _directoryOnlyFilter = ColumnFilter.CreateNew("DirectoryOnly", "IsDirectory", em => em.IsDirectory);
        IEntryModel[] _selectedFiles;
        bool _canOpen = false;
        private string _filterStr;
        private string _selectedFilter;
        private string _selectedFileName = "";
        private FilePickerMode _mode;

        #endregion

        #region Public Properties

        public IEntriesHelper<FileNameFilter> Filters { get; set; }
        public string SelectedFilter { get { return _selectedFilter; } set { setFilter(value); } }
        public string FileName { get { return _selectedFileName; } set { setFileName(value); } }

        public bool IsOpenEnabled { get { return _mode == FilePickerMode.Open; } }
        public bool IsSaveEnabled { get { return _mode == FilePickerMode.Save; } }

        public bool CanOpen { get { return _canOpen; } set { _canOpen = value; NotifyOfPropertyChange(() => CanOpen); } }
        public IEntryModel[] SelectedFiles
        {
            get { return _selectedFiles; }
            set { _selectedFiles = value; NotifyOfPropertyChange(() => SelectedFiles); }
        }

        #endregion

    }
}
