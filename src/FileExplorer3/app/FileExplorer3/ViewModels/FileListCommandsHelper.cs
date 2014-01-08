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
using Cofe.Core;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.Defines;
using FileExplorer.Models;
using FileExplorer.Utils;
using FileExplorer.ViewModels.Helpers;

namespace FileExplorer.ViewModels
{

    public class FileListCommandsHelper : ToolbarCommandsHelper, IHandle<SelectionChangedEvent>, IHandle<DirectoryChangedEvent>
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
                Header = Strings.txtSelection;
            }
        }

        //ViewModes, name then slider steps.
        public static string[] ViewModes = new string[]
        {
             "ExtraLargeIcon,200,60",
             "LargeIcon,100,60",
             "Icon,65",
             "SmallIcon,60",
             "Separator,-1",
             "List,55",
             "Separator,-1",
             "Grid,50"
        };

        //Given a view mode string (LargeIcon,100,60), parse usable value.
        internal static void parseViewMode(string viewModeStr, out string viewMode, out int step, out int itemHeight)
        {
            string[] vmSplit = viewModeStr.Split(',');
            viewMode = null;
            step = -1;
            itemHeight = 0;
            if (vmSplit.Count() >= 2)
            {
                viewMode = vmSplit[0];
                step = Int32.Parse(vmSplit[1]);
                itemHeight = 0;
                if (vmSplit.Count() >= 3)
                    itemHeight = Int32.Parse(vmSplit[2]);
            }
        }

        //Find ViewMode using a step number  (e.g. 103 return LargeIcon = 2)
        internal static int findViewMode(string[] viewModes, int forStep)
        {
            int retVal = 0;
            for (int i = viewModes.Count() - 1; i >= 0; i--)
            {
                string viewMode; int step; int itemHeight;
                parseViewMode(viewModes[i], out viewMode, out step, out itemHeight);
                if (step != -1)
                    if (forStep >= step)
                    {
                        retVal = i;
                    }
                    else break;
            }
            return retVal;
        }

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

            private static IEnumerable<ICommandModel> generateCommandModel()
            {
                foreach (string vm in ViewModes)
                {
                    string viewMode; int step; int itemHeight;
                    parseViewMode(vm, out viewMode, out step, out itemHeight);

                    if (viewMode != null)
                    {
                        if (step == -1)
                            yield return new SeparatorCommandModel();
                        else
                        {
                            var scm = new ViewModeStepCommandModel(viewMode) { SliderStep = step };
                            if (itemHeight != 0)
                                scm.ItemHeight = itemHeight;
                            yield return scm;
                        }
                    }
                }

            }

            private IFileListViewModel _flvm;
            public ViewModeCommand(IFileListViewModel flvm)
                : base(FileListCommands.ToggleViewMode,
                     generateCommandModel().ToArray()
                )
            {
                _flvm = flvm;
                IsHeaderVisible = false;
                SliderValue = flvm.ItemSize;
            }

            internal static void updateViewMode(IFileListViewModel flvm, string viewMode, int step)
            {
                flvm.ItemSize = step;
                switch (viewMode)
                {
                    case "ExtraLargeIcon":
                    case "LargeIcon":
                        flvm.ViewMode = "Icon";
                        break;
                    default:
                        flvm.ViewMode = viewMode;
                        break;
                }
            }


            public override void NotifyOfPropertyChange(string propertyName = "")
            {
                base.NotifyOfPropertyChange(propertyName);
                switch (propertyName)
                {
                    case "SliderValue":

                        int curIdx = findViewMode(ViewModes, SliderValue);
                        string viewMode; int step; int itemHeight;
                        parseViewMode(ViewModes[curIdx], out viewMode, out step, out itemHeight);
                        ViewModeStepCommandModel commandModel = SubCommands
                            .Where(c => c is ViewModeStepCommandModel)
                            .First(c => (c as ViewModeStepCommandModel).SliderStep == step) as ViewModeStepCommandModel;

                        if (commandModel != null)
                            this.HeaderIcon = commandModel.HeaderIcon;

                        if (_flvm.ItemSize != SliderValue)
                        {
                            updateViewMode(_flvm, commandModel.Header, SliderValue);
                            //Debug.WriteLine(commandModel.Header + SliderValue.ToString());                            
                        }

                        break;
                }
            }
        }

        #endregion

        #region Constructor

        public FileListCommandsHelper(IFileListViewModel flvm, IEventAggregator events, IProfile[] rootProfiles)
            : base(rootProfiles, new SelectGroupCommand(flvm), new ViewModeCommand(flvm), new SeparatorCommandModel())
        {
            events.Subscribe(this);
            _flvm = flvm;


            _exportedCommandBindings.Add(new ScriptCommandBinding(FileListCommands.ToggleCheckBox, p => true, p => ToggleCheckBox(), null, ScriptBindingScope.Explorer));
            _exportedCommandBindings.Add(new ScriptCommandBinding(FileListCommands.ToggleViewMode, p => true, p => ToggleViewMode(), null, ScriptBindingScope.Explorer));
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

        public void ToggleViewMode()
        {
            var viewModeWoSeparator = ViewModes.Where(vm => vm.IndexOf(",-1") == -1).ToArray();

            int curIdx = findViewMode(viewModeWoSeparator, _flvm.ItemSize);
            int nextIdx = curIdx + 1;
            if (nextIdx >= viewModeWoSeparator.Count()) nextIdx = 0;

            string viewMode; int step; int itemHeight;
            parseViewMode(viewModeWoSeparator[nextIdx], out viewMode, out step, out itemHeight);
            ViewModeCommand vmc = this.CommandModels.AllNonBindable.First(c => c.CommandModel is ViewModeCommand)
                .CommandModel as ViewModeCommand;
            vmc.SliderValue = step;
        }

        public void ToggleCheckBox()
        {
            _flvm.IsCheckBoxVisible = !_flvm.IsCheckBoxVisible;
        }



        #endregion

        #region Data

        IFileListViewModel _flvm = null;
        IEntryModel _currentDirectoryModel = null;

        #endregion

        #region Public Properties



        #endregion







    }
}
