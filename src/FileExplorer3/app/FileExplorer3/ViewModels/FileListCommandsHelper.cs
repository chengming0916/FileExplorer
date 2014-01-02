using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Cinch;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Defines.Commands;
using FileExplorer.Models;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{
    public class FileListCommandsHelper : CommandsHelper, IHandle<SelectionChangedEvent>, IHandle<DirectoryChangedEvent>
    {
        #region Commands

        public class SelectGroupCommand : DirectoryCommandModel
        {
            public SelectGroupCommand(IFileListViewModel flvm)
                : base(ApplicationCommands.SelectAll,
                new CommandModel(ApplicationCommands.SelectAll) { Symbol = Convert.ToChar(0xE14E) },
                new CommandModel(FileListCommands.ToggleCheckBox) { Symbol = Convert.ToChar(0xe1ef) })
            {
                Symbol = Convert.ToChar(0xE10B);
                Header = null;
            }
        }
        //new SliderCommandModel(null,
        //    
        //    { Header="View" }
        public class ViewModeCommand : SliderCommandModel
        {
            private class ViewModeStepCommandModel : SliderStepCommandModel
            {
                public ViewModeStepCommandModel(string view)
                {
                    Header = view;
                    Stream imgStream = Application.GetResourceStream(
                           new Uri(String.Format(ViewModeViewModel.iconPathMask, view.ToLower()))).Stream;
                    if (imgStream != null)
                        HeaderIcon = new System.Drawing.Bitmap(imgStream);

                }
            }

            private IFileListViewModel _flvm;
            public ViewModeCommand(IFileListViewModel flvm)
                : base(FileListCommands.ToggleViewMode,
                     new ViewModeStepCommandModel("ExtraLargeIcon") { SliderStep = 200, ItemHeight = 60 },
                     new ViewModeStepCommandModel("LargeIcon") { SliderStep = 100, ItemHeight = 60 },
                     new ViewModeStepCommandModel("Icon") { SliderStep = 65 },
                     new ViewModeStepCommandModel("SmallIcon") { SliderStep = 60 },
                      new SeparatorCommandModel(),
                      new ViewModeStepCommandModel("List") { SliderStep = 55 },
                      new SeparatorCommandModel(),
                      new ViewModeStepCommandModel("Grid") { SliderStep = 50 }
                )
            {
                _flvm = flvm;
                IsHeaderVisible = false;
                SliderValue = flvm.ItemSize;
            }

            public override void NotifyOfPropertyChange(string propertyName = "")
            {
                base.NotifyOfPropertyChange(propertyName);
                switch (propertyName)
                {
                    case "SliderValue":
                        ViewModeStepCommandModel commandModel = null;
                        for (int i = SubCommands.Count - 1; i >= 0; i--)
                        {
                            ViewModeStepCommandModel vcm = SubCommands[i] as ViewModeStepCommandModel;
                            if (vcm != null)
                                if (SliderValue >= vcm.SliderStep)
                                    commandModel = vcm;
                                else break;
                        }

                        if (commandModel != null)
                            this.HeaderIcon = commandModel.HeaderIcon;

                        if (_flvm.ItemSize != SliderValue)
                        {
                            _flvm.ItemSize = SliderValue;
                            //Debug.WriteLine(commandModel.Header + SliderValue.ToString());
                            if (commandModel != null)
                                switch (commandModel.Header)
                                {
                                    case "ExtraLargeIcon":
                                    case "LargeIcon":
                                        _flvm.ViewMode = "Icon";
                                        break;
                                    default :
                                        _flvm.ViewMode = commandModel.Header;
                                        break;
                                }

                        }

                        break;
                }
            }
        }

        #endregion

        #region Constructor

        public FileListCommandsHelper(IFileListViewModel flvm, IEventAggregator events, IProfile[] rootProfiles)
            : base(rootProfiles, new SelectGroupCommand(flvm), new ViewModeCommand(flvm))
        {
            events.Subscribe(this);
            ToggleCheckBoxCommand = new SimpleCommand() { ExecuteDelegate = (e) => flvm.IsCheckBoxVisible = !flvm.IsCheckBoxVisible };
            ToggleViewModeCommand = new SimpleCommand()
            {
                ExecuteDelegate = (e) =>
                    {
                        //events.Publish(new ViewChangedEvent(this, 
                    }
            };
            flvm.ViewAttached += (o, e) =>
                {
                    UserControl uc = o as UserControl;
                    uc.CommandBindings.Add(new SimpleRoutedCommand(FileListCommands.ToggleCheckBox, ToggleCheckBoxCommand as SimpleCommand).CommandBinding);
                    uc.CommandBindings.Add(new SimpleRoutedCommand(FileListCommands.ToggleViewMode, ToggleViewModeCommand as SimpleCommand).CommandBinding);
                };
        }

        #endregion

        #region Methods


        public void Handle(SelectionChangedEvent message)
        {
            AppliedModels =
                message.SelectedModels.Count() == 0 ?
                new IEntryModel[] { _currentDirectoryModel } :
                message.SelectedModels.ToArray();
        }

        public void Handle(DirectoryChangedEvent message)
        {
            _currentDirectoryModel = message.NewModel;
            AppliedModels = new IEntryModel[] { _currentDirectoryModel };
        }


        #endregion

        #region Data

        IEntryModel _currentDirectoryModel = null;

        #endregion

        #region Public Properties


        public ICommand ToggleCheckBoxCommand { get; private set; }
        public ICommand ToggleViewModeCommand { get; private set; }


        #endregion







    }
}
