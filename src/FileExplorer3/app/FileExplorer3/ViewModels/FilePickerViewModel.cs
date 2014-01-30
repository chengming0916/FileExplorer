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

        private void initOpenMode()
        {
            FileList.Selection.SelectionChanged += (o, e) =>
            {
                int selectedCount = FileList.Selection.SelectedItems.Count();
                FileName =
                    String.Join(",",
                    FileList.Selection.SelectedItems.Where(evm => !evm.EntryModel.IsDirectory)
                    .Select(evm => evm.EntryModel.Profile.Path.GetFileName(evm.EntryModel.FullPath)));
            };

            FileList.ScriptCommands.Open = vms.FileList.IfSelection(evm => evm.Count() == 1,
                   vms.FileList.IfSelection(evm => evm[0].EntryModel.IsDirectory,
                       OpenSelectedDirectory.FromFileList,  //Selected directory
                       new SimpleScriptCommand("", (pd) =>
                       {
                           Open();
                           return ResultCommand.NoError;
                       })),   //Selected non-directory
                   ResultCommand.NoError //Selected more than one item.                   
                   );

            FileList.EnableDrag = false;
            FileList.EnableDrop = false;
            FileList.EnableMultiSelect = false;


        }

        public FilePickerViewModel(IEventAggregator events, IWindowManager windowManager, string filterStr,
            FilePickerMode mode = FilePickerMode.Open, params IEntryModel[] rootModels)
            : base(events, windowManager, rootModels)
        {

            switch (mode)
            {
                case FilePickerMode.Open: initOpenMode(); break;
            }

            _filterStr = filterStr;
            Filters = new EntriesHelper<FileNameFilter>(loadFiltersTask);
            Filters.SetEntries(getFilters(filterStr).ToArray());
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

        public void Open()
        {
            List<IEntryModel> selectedFiles = new List<IEntryModel>();            
            Func<IEntryModel, IScriptCommand> addToSelectedFiles =
                m => new SimpleScriptCommand("AddToSelectedFiles", pd => { selectedFiles.Add(m); return ResultCommand.NoError; });
            var pm = FileList.ScriptCommands.ParameterDicConverter.Convert(new ParameterDic());
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
                new SimpleScriptCommand("TryClose", pd => {  TryClose(true); return ResultCommand.NoError; })
             );



            //List<IEntryModel> selectedFiles = new List<IEntryModel>();
            //foreach (string fname in FileName.Split(','))
            //{
            //    string fullPath = FileList.CurrentDirectory.Profile.Path.Combine(FileList.CurrentDirectory.FullPath, fname);
            //    var foundItem = FileList.ProcessedEntries.EntriesHelper.AllNonBindable.Select(evm => evm.EntryModel)
            //        .FirstOrDefault(em =>
            //        em.FullPath.Equals(fullPath, StringComparison.CurrentCultureIgnoreCase) ||
            //        em.Profile.Path.GetFileName(em.FullPath).Equals(fname));
            //    if (foundItem == null)
            //    {
            //        foundItem = FileList.CurrentDirectory.Profile.ParseAsync(fullPath).Result;
            //        if (foundItem == null)
            //        {
            //            MessageBox.Show(fname + " is not found.");
            //            return;
            //        }
            //    }

            //    selectedFiles.Add(foundItem);
            //}

            //SelectedFiles = selectedFiles.ToArray();
            //TryClose(true);
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

        private static ColumnFilter _directoryOnlyFilter = ColumnFilter.CreateNew("DirectoryOnly", "IsDirectory", em => em.IsDirectory);
        IEntryModel[] _selectedFiles;
        bool _canOpen = false;
        private string _filterStr;
        private string _selectedFilter;
        private string _selectedFileName = "";

        #endregion

        #region Public Properties

        public IEntriesHelper<FileNameFilter> Filters { get; set; }
        public string SelectedFilter { get { return _selectedFilter; } set { setFilter(value); } }
        public string FileName { get { return _selectedFileName; } set { setFileName(value); } }

        public bool CanOpen { get { return _canOpen; } set { _canOpen = value; NotifyOfPropertyChange(() => CanOpen); } }
        public IEntryModel[] SelectedFiles
        {
            get { return _selectedFiles; }
            set { _selectedFiles = value; NotifyOfPropertyChange(() => SelectedFiles); }
        }

        #endregion

    }
}
