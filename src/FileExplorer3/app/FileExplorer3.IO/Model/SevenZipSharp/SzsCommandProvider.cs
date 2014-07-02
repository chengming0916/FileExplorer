﻿using FileExplorer.IO;
using FileExplorer.Script;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileExplorer.Utils;
using Caliburn.Micro;

namespace FileExplorer.Models.SevenZipSharp
{
    public class SzsCommandProvider : StaticCommandProvider
    {
        public static IScriptCommand NewZip = FileList.Do(flvm =>
                IOScriptCommands.CreateArchive(flvm.CurrentDirectory, "NewArchive.zip", true,
                       em => FileList.Refresh(FileList.Select(fm => fm.Equals(em), ResultCommand.OK), true)));
        public static IScriptCommand New7z = FileList.Do(flvm =>
                IOScriptCommands.CreateArchive(flvm.CurrentDirectory, "NewArchive.7z", true,
                       em => FileList.Refresh(FileList.Select(fm => fm.Equals(em), ResultCommand.OK), true)));

    }

    public class SzsNewArchiveCommandModel : CommandModel
    {

    }


    /// <summary>
    /// Transfer from a folder (Source:IEntryModel) to dest folder (from destPathFunc)
    /// </summary>
    public class SzsCommandModel : DirectoryCommandModel
    {        
        private IExplorerInitializer _initializer;
        //private Func<IEntryModel, string> _destPathFunc;
        public SzsCommandModel(IExplorerInitializer initializer)
            : base(ResultCommand.NoError)
        {
            _initializer = initializer;     
            HeaderIconExtractor = ResourceIconExtractor<ICommandModel>.ForSymbol(0xE188);
            IsHeaderVisible = true;
            Header = "Compress";
            //Func<IEntryModel, string> destPathFunc, 
            //_destPathFunc = destPathFunc;
            IsVisibleOnMenu = true;
        }

        public override void NotifySelectionChanged(IEntryModel[] appliedModels)
        {
            List<ICommandModel> subCommands = new List<ICommandModel>();

            if (appliedModels.Length >= 1)
            {

                #region Decompress - When selected archive.
                if (appliedModels.All(em => em is ISzsItemModel))
                {
                    

                    bool isRoot = appliedModels.All(em => em is SzsRootModel);
                    Func<IEntryModel, IScriptCommand, IScriptCommand> transferCommandFunc =
                        (destModel, thenCommand) => ScriptCommands.ForEach(appliedModels, am =>
                                     isRoot ? 
                                        IOScriptCommands.TransferChild(am, destModel, null, false) :
                                        IOScriptCommands.Transfer(am, destModel),
                              thenCommand);


                    //Extract to ...
                    subCommands.Add(new CommandModel(
                        ScriptCommands.ShowDirectoryPicker(_initializer, null,
                        dm =>
                         ScriptCommands.ShowProgress("Extract", transferCommandFunc(dm, ScriptCommands.HideProgress())),
                              ResultCommand.NoError)) { Header = "Extract to ...", IsEnabled = true, IsVisibleOnMenu = true });

                    if (isRoot)
                    {
                        SzsRootModel firstRoot = appliedModels[0] as SzsRootModel;

                        IPathHelper path = firstRoot.Profile.Path;
                        Header = path.GetExtension(firstRoot.Name).TrimStart('.').FirstCharToUppercase();
                        string parentPath = path.GetDirectoryName(firstRoot.FullPath);

                        //Extract Here
                        subCommands.Add(new CommandModel(
                               ScriptCommands.ShowProgress("Extract",
                               transferCommandFunc(firstRoot.Parent, ScriptCommands.HideProgress()))) { Header = "Extract Here", IsEnabled = true, IsVisibleOnMenu = true });


                        if (appliedModels.Length == 1)
                        {
                            //Extract to \\ArchiveName
                            subCommands.Add(new CommandModel(
                                ScriptCommands.ParseOrCreatePath(firstRoot.Parent.Profile as IDiskProfile,
                                path.Combine(parentPath, path.RemoveExtension(appliedModels[0].Name)), true,
                                destFolder => transferCommandFunc(destFolder, ScriptCommands.HideProgress())))
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
                                            destFolder => transferCommandFunc(destFolder, ScriptCommands.HideProgress()))))) { Header = "Extract to {ArchiveName}\\", IsEnabled = true, IsVisibleOnMenu = true });

                    }
                }

                //if (appliedModels.All(em => em is SzsRootModel))
                //{
                //    SzsRootModel firstRoot = appliedModels[0] as SzsRootModel;

                //    IPathHelper path = firstRoot.Profile.Path;
                //    Header = path.GetExtension(firstRoot.Name).TrimStart('.').FirstCharToUppercase();
                //    string parentPath = path.GetDirectoryName(firstRoot.FullPath);


                //    //Extract to ...
                //    subCommands.Add(new CommandModel(
                //        ScriptCommands.ShowDirectoryPicker(_initializer, null,
                //        dm =>
                //         ScriptCommands.ShowProgress("Extract",
                //         ScriptCommands.ForEach(appliedModels, am =>
                //                     IOScriptCommands.TransferChild(am, dm, null, false),
                //              ScriptCommands.HideProgress())),
                //              ResultCommand.NoError)) { Header = "Extract to ...", IsEnabled = true, IsVisibleOnMenu = true });

                //    //Extract Here
                //    subCommands.Add(new CommandModel(
                //           ScriptCommands.ShowProgress("Extract",
                //           ScriptCommands.ForEach(appliedModels, am =>
                //                       IOScriptCommands.TransferChild(am, firstRoot.Parent, null, false),
                //                ScriptCommands.HideProgress()))) { Header = "Extract Here", IsEnabled = true, IsVisibleOnMenu = true });


                //    if (appliedModels.Length == 1)
                //    {
                //        //Extract to \\ArchiveName
                //        subCommands.Add(new CommandModel(
                //            ScriptCommands.ParseOrCreatePath(firstRoot.Parent.Profile as IDiskProfile,
                //            path.Combine(parentPath, path.RemoveExtension(appliedModels[0].Name)), true,
                //            destFolder =>
                //                ScriptCommands.ShowProgress("Extract",
                //                    IOScriptCommands.TransferChild(appliedModels[0], destFolder, null, false,
                //                      ScriptCommands.HideProgress()))))
                //                      {
                //                          Header = "Extract to \\" + path.RemoveExtension(appliedModels[0].Name),
                //                          IsEnabled = true,
                //                          IsVisibleOnMenu = true
                //                      });
                //    }
                //    else
                //        subCommands.Add(new CommandModel(
                //                ScriptCommands.ShowProgress("Extract",
                //                ScriptCommands.ForEach(appliedModels, am =>
                //                     ScriptCommands.ParseOrCreatePath(firstRoot.Parent.Profile as IDiskProfile,
                //                        path.Combine(parentPath, path.RemoveExtension(am.Name)), true,
                //                        destFolder =>
                //                            IOScriptCommands.TransferChild(am, destFolder, null, false)),
                //                     ScriptCommands.HideProgress()))) { Header = "Extract to {ArchiveName}\\", IsEnabled = true, IsVisibleOnMenu = true });

                //}                
                #endregion


                #region Compress

                if (!appliedModels.Any(em => em is SzsChildModel) && !(appliedModels.Length == 1 && appliedModels[0] is SzsRootModel))
                {
                    Header = "Compress";
                    IEntryModel firstEntry = appliedModels[0];
                    IDiskProfile firstProfile = firstEntry.Profile as IDiskProfile;

                    if (firstProfile != null && !firstEntry.FullPath.StartsWith("::{"))
                    {
                        IPathHelper path = firstEntry.Profile.Path;
                        //Header = path.GetExtension(firstEntry.Name).TrimStart('.').FirstCharToUppercase();
                        string parentPath = path.GetDirectoryName(firstEntry.FullPath);

                        //e.g. C:\temp\abc.txt => C:\temp\temp.zip
                        string parentArchiveName = path.ChangeExtension(firstEntry.Parent.Name, ".zip");
                        string parentArchivePath = path.Combine(firstEntry.Parent.FullPath, parentArchiveName);

                        string firstArchiveName = path.ChangeExtension(firstEntry.Name, ".zip");
                        string firstArchivePath = path.Combine(firstEntry.Parent.FullPath, firstArchiveName);


                        subCommands.Add(new CommandModel(
                          ScriptCommands.SaveFilePicker(_initializer, "Zip archives (.zip)|*.zip|7z archives (.7z)|*.7z", firstArchivePath,
                              pmi => ScriptCommands.ShowProgress("Compress",
                                  IOScriptCommands.ParseOrCreateArchive(pmi.Profile as IDiskProfile, pmi.FileName,
                               pm => ScriptCommands.ForEach(appliedModels,
                                  am => IOScriptCommands.Transfer(am, pm, false, true),
                                      ScriptCommands.HideProgress()))),
                                        ResultCommand.NoError))
                        {
                            Header = "Compress to ...",
                            IsEnabled = true,
                            IsVisibleOnMenu = true
                        });

                        Action<string, string> addCompressToPath = (destName, destPath) =>
                            {
                                subCommands.Add(new CommandModel(
                                    ScriptCommands.ShowProgress("Compress",
                                        IOScriptCommands.ParseOrCreateArchive(firstProfile, destPath,
                                            pm => ScriptCommands.ForEach(appliedModels,
                                                am => IOScriptCommands.Transfer(am, pm, false, true,
                                            null), ScriptCommands.HideProgress()))))
                                {
                                    Header = "Compress to " + destName,
                                    IsEnabled = true,
                                    IsVisibleOnMenu = true
                                });
                            };

                        addCompressToPath(firstArchiveName, firstArchivePath);
                        addCompressToPath(parentArchiveName, parentArchivePath);

                    }
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
