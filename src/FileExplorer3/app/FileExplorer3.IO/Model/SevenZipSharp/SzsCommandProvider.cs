using FileExplorer.IO;
using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Utils;

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

    public class SzsNewArchiveCommandModel : CommandModel
    {

    }


    /// <summary>
    /// Transfer from a folder (Source:IEntryModel) to dest folder (from destPathFunc)
    /// </summary>
    public class SzsCommandModel : DirectoryCommandModel
    {
        //private Func<IEntryModel, string> _destPathFunc;
        public SzsCommandModel(Func<ParameterDic, IEntryModel[]> srcModelFunc)
            : base(ResultCommand.NoError)
        {
            HeaderIconExtractor = ResourceIconExtractor<ICommandModel>.ForSymbol(0xE188);
            IsHeaderVisible = true;
            Header = "Archive";
            //Func<IEntryModel, string> destPathFunc, 
            //_destPathFunc = destPathFunc;
            IsVisibleOnMenu = true;
        }

        public override void NotifySelectionChanged(IEntryModel[] appliedModels)
        {
            List<ICommandModel> subCommands = new List<ICommandModel>();

            if (appliedModels.Length >= 1)
            {

                #region Decompress
                if (appliedModels.Length >= 1 && appliedModels.All(em => em is SzsRootModel))
                {
                    SzsRootModel firstRoot = appliedModels[0] as SzsRootModel;

                    IPathHelper path = firstRoot.Profile.Path;
                    Header = path.GetExtension(firstRoot.Name).TrimStart('.').FirstCharToUppercase();
                    string parentPath = path.GetDirectoryName(firstRoot.FullPath);


                    //Extract to \\
                    subCommands.Add(new CommandModel(
                           ScriptCommands.ShowProgress("Extract",
                           ScriptCommands.ForEach(appliedModels, am =>
                                       IOScriptCommands.TransferChild(am, firstRoot.Parent, null, false),
                                ScriptCommands.HideProgress()))) { Header = "Extract Here", IsEnabled = true, IsVisibleOnMenu = true });


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
                                          Header = "Extract to \\" + path.RemoveExtension(appliedModels[0].Name),
                                          IsEnabled = true,
                                          IsVisibleOnMenu = true
                                      });
                    }
                    else
                        subCommands.Add(new CommandModel(
                                ScriptCommands.ShowProgress("Extract",
                                ScriptCommands.ForEach(appliedModels, am =>
                                     ScriptCommands.ParseOrCreatePath(firstRoot.Parent.Profile as IDiskProfile,
                                        path.Combine(parentPath, path.RemoveExtension(am.Name)), true,
                                        destFolder =>
                                            IOScriptCommands.TransferChild(am, destFolder, null, false)),
                                     ScriptCommands.HideProgress()))) { Header = "Extract to {ArchiveName}\\", IsEnabled = true, IsVisibleOnMenu = true });

                }
                #endregion


                #region Compress

                if (appliedModels.Length >= 1)
                {

                }

                #endregion

                SubCommands = subCommands;
            }


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
