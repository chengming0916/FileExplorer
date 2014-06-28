using FileExplorer.IO;
using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models.SevenZipSharp
{
    public class SzsCommandProvider : StaticCommandProvider
    {
        public SzsCommandProvider()
            : base(


            )
        {

        }

    }

    /// <summary>
    /// Transfer from a folder (Source:IEntryModel) to dest folder (from destPathFunc)
    /// </summary>
    public class ExtractDirectoryCommandModel : DirectoryCommandModel
    {
        private Func<IEntryModel, string> _destPathFunc;
        public ExtractDirectoryCommandModel(Func<ParameterDic, IEntryModel[]> srcModelFunc)
            : base(ResultCommand.NoError)
        {
            IsHeaderVisible = true;
            Header = "Extract";
            //Func<IEntryModel, string> destPathFunc, 
            //_destPathFunc = destPathFunc;
        }

        public override void NotifySelectionChanged(IEntryModel[] appliedModels)
        {
            List<ICommandModel> subCommands = new List<ICommandModel>();

            if (appliedModels.Length >= 1)
            {

                if (appliedModels.Length >= 1 && appliedModels.All(em => em is SzsRootModel))
                {
                    SzsRootModel firstRoot = appliedModels[0] as SzsRootModel;

                    IPathHelper path = firstRoot.Profile.Path;
                    string parentPath = path.GetDirectoryName(firstRoot.FullPath);

                    //Extract to \\
                    subCommands.Add(new CommandModel(
                            ScriptCommands.ShowProgress("Extract",
                            ScriptCommands.ForEach(appliedModels, am =>
                                 ScriptCommands.ParseOrCreatePath(firstRoot.Parent.Profile as IDiskProfile,
                                    path.Combine(parentPath, path.RemoveExtension(am.Name)), true,
                                    destFolder => 
                                        IOScriptCommands.TransferChild(am, destFolder, null, false)),
                                 ScriptCommands.HideProgress()))) { Header = "Here", IsEnabled = true });

                    if (appliedModels.Length == 1)
                    {
                        //Extract to \\ArchiveName
                        subCommands.Add(new CommandModel(
                            ScriptCommands.ParseOrCreatePath(firstRoot.Parent.Profile as IDiskProfile,
                            path.Combine(parentPath, path.RemoveExtension(appliedModels[0].Name)), true,
                            destFolder =>
                                ScriptCommands.ShowProgress("Extract",
                                    IOScriptCommands.TransferChild(appliedModels[0], destFolder, null, false,
                                      ScriptCommands.HideProgress()))))
                                      {
                                          Header = "\\" + path.RemoveExtension(appliedModels[0].Name),
                                          IsEnabled = true
                                      });
                    }

                    //ScriptCommands.List(root, null, false, ems =>
                    //    ScriptCommands.ReportProgress(WPF.Defines.TransferProgress.IncrementTotalEntries(ems.Length)
                    //    ScriptCommands.ForEach(ems, em => 
                    //        IOScriptCommands.Transfer(em, destFolder), 
                    //            ScriptCommands.HideProgress())))))

                }


                SubCommands = subCommands;
            }

            IsVisibleOnToolbar = IsEnabled = SubCommands.Count > 0;
        }


    }

    //public class ExtractToCommandModel : CommandModel
    //{
    //    private Func<IEntryModel, string> _destPathFunc;
    //    public ExtractToCommandModel(Func<ParameterDic, IEntryModel[]> srcModelFunc)
    //        : base(ResultCommand.NoError)
    //    {

    //        //Func<IEntryModel, string> destPathFunc, 
    //        //_destPathFunc = destPathFunc;
    //    }



    //}
}
