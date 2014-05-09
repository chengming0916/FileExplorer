﻿using Cofe.Core.Utils;
using FileExplorer.Models;
using FileExplorer.ViewModels;
using FileExplorer.ViewModels.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FileExplorer;
using Cofe.Core.Script;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for ToolWindowTest.xaml
    /// </summary>
    public partial class ToolWindowTest : Window
    {
        private IEntryModel[] _rootDirs;
        private string _mask;
        private string _selectedPath;

        public ToolWindowTest(IEntryModel[] rootDirs, string mask, string selectedPath = "c:\\")
        {
            InitializeComponent();
            _rootDirs = rootDirs;
            _mask = mask;
            _selectedPath = selectedPath;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            FileExplorer.UserControls.Explorer exp = explorer as FileExplorer.UserControls.Explorer;
            exp.RootDirectories = _rootDirs;
            exp.ViewModel.FileList.ShowToolbar = false;
            exp.ViewModel.FileList.ShowGridHeader = false;
            exp.ViewModel.FileList.Parameters.ViewMode = "List";
            exp.ViewModel.FileList.Parameters.ItemSize = 16;
            exp.ViewModel.FileList.EnableDrag = true;
            exp.ViewModel.FileList.EnableDrop = false;
            exp.ViewModel.FileList.EnableMultiSelect = false;
            exp.ViewModel.FilterStr = _mask;
            testDroppable.DataContext = new TestDroppableViewModel();
            
            if (_selectedPath != null)
                exp.ViewModel.GoAsync(_selectedPath);
            //or exp.ViewModel.Commands.ExecuteAsync(new IScriptCommand[] { Explorer.GoTo("C:\\") });
        }
    }

    public class TestDroppableViewModel : NotifyPropertyChanged, ISupportDropHelper
    {

        #region Constructors

        public TestDroppableViewModel()
        {
            IProfile exProfile = new FileSystemInfoExProfile(null, null);
            DropHelper = new DropHelper<IEntryModel>(
                 () => "Test Droppable",
                 (ems, eff) =>
                     QueryDropResult.CreateNew(DragDropEffects.Copy),
                da =>
                    exProfile.DragDrop.GetEntryModels(da),
                (ems, da, eff) =>
                {
                    if (ems.Count() > 1)
                        Label = ems.Count() + " items.";
                    else Label = ems.First().FullPath;
                    return DragDropEffects.Copy;
                }, em => EntryViewModel.FromEntryModel(em));
        }

        #endregion

        #region Methods

        #endregion

        #region Data

        private string _label = "Drop here";

        #endregion

        #region Public Properties

        public string Label
        {
            get { return _label; }
            set { _label = value; NotifyOfPropertyChanged(() => Label); }
        }


        #endregion









        public ISupportDrop DropHelper
        {
            get;
            set;
        }
    }
}
