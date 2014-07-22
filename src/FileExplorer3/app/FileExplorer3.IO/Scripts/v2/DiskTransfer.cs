﻿using FileExplorer.Defines;
using FileExplorer.IO;
using FileExplorer.Models;
using FileExplorer.WPF.Models;
using MetroLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    public static partial class IOScriptCommands
    {
        /// <summary>
        /// Serializable, transfer source entry (file or directory) to destination directory.
        /// </summary>
        /// <param name="srcEntryVariable">Entry to transfer(IEntryModel)</param>
        /// <param name="destDirectoryVariable">Destination of transfer (Directory IEntryModel)</param>
        /// <param name="removeOriginal"></param>
        /// <param name="allowCustomImplementation"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskTransfer(string srcEntryVariable = "Source", string destDirectoryVariable = "Destination", bool removeOriginal = false,
           bool allowCustomImplementation = true, IScriptCommand nextCommand = null)
        {
            return new DiskTransfer()
            {
                SourceEntryKey = srcEntryVariable,
                DestinationDirectoryEntryKey = destDirectoryVariable,
                RemoveOriginal = removeOriginal,
                AllowCustomImplementation = allowCustomImplementation,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        /// <summary>
        /// Not Serializable, transfer source entry to destentry.
        /// </summary>
        /// <param name="srcModel"></param>
        /// <param name="destDirModel"></param>
        /// <param name="removeOriginal"></param>
        /// <param name="allowCustomImplementation"></param>
        /// <param name="nextCommand"></param>
        /// <returns></returns>
        public static IScriptCommand DiskTransfer(IEntryModel[] srcModels, IEntryModel destDirModel, bool removeOriginal = false,
            bool allowCustomImplementation = true, IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Assign("SourceDiskTransferEntry", srcModels, false,
                ScriptCommands.Assign("DestinationDiskTransferEntry", destDirModel, false,
                DiskTransfer("SourceDiskTransferEntry", "DestinationDiskTransferEntry",
                removeOriginal, allowCustomImplementation, nextCommand)));
        }

        public static IScriptCommand DiskTransfer(IEntryModel srcModel, IEntryModel destDirModel, bool removeOriginal = false,
           bool allowCustomImplementation = true, IScriptCommand nextCommand = null)
        {
            return DiskTransfer(new IEntryModel[] { srcModel }, destDirModel, removeOriginal, allowCustomImplementation, nextCommand);
        }

        public static IScriptCommand DiskTransferChild(string srcDirectoryVariable = "Source",
           string destDirectoryVariable = "Destination",
            string mask = "*", ListOptions listOptions = ListOptions.File | ListOptions.Folder,
            bool removeOriginal = false, bool allowCustomImplementation = true, IScriptCommand nextCommand = null)
        {
            return CoreScriptCommands.List(srcDirectoryVariable, "{DTC-ItemToTransfer}", mask, listOptions,
                       ScriptCommands.ForEach("{DTC-ItemToTransfer}", "{DTC-CurrentItem}",
                           IOScriptCommands.DiskTransfer("{DTC-CurrentItem}", destDirectoryVariable, removeOriginal, allowCustomImplementation),
                                    ScriptCommands.Reset(nextCommand, "{DTC-DestDirectory}", "{DTC-SrcDirectory}")));
        }

        public static IScriptCommand DiskTransferChild(string srcDirectoryVariable = "Source",
           string destDirectoryVariable = "Destination", bool removeOriginal = false, bool allowCustomImplementation = true, IScriptCommand nextCommand = null)
        {
            return DiskTransferChild(srcDirectoryVariable, destDirectoryVariable, "*", ListOptions.File | ListOptions.Folder, removeOriginal, allowCustomImplementation, nextCommand);
        }

         public static IScriptCommand DiskTransferChild(IEntryModel[] srcModels, IEntryModel destDirModel, 
             bool removeOriginal = false, bool allowCustomImplementation = true, IScriptCommand nextCommand = null)
        {
            return ScriptCommands.Assign("SourceDiskTransferEntry", srcModels, false,
               ScriptCommands.Assign("DestinationDiskTransferEntry", destDirModel, false,
               DiskTransferChild("SourceDiskTransferEntry", "DestinationDiskTransferEntry",
               removeOriginal, allowCustomImplementation, nextCommand)));
        }

         public static IScriptCommand DiskTransferChild(IEntryModel srcModel, IEntryModel destDirModel,
            bool removeOriginal = false, bool allowCustomImplementation = true, IScriptCommand nextCommand = null)
         {
             return DiskTransferChild(new IEntryModel[] { srcModel }, destDirModel, removeOriginal, allowCustomImplementation, nextCommand);
         }
    }


    public class DiskTransfer : ScriptCommandBase
    {
        /// <summary>
        /// IEntryModel or IEntryModel[] to transfer to destination, default = "Source"
        /// </summary>
        public string SourceEntryKey { get; set; }

        /// <summary>
        /// Destination directory IEntryModel, default = "Destination"
        /// </summary>
        public string DestinationDirectoryEntryKey { get; set; }        

        /// <summary>
        /// Whether remove original (e.g. Move) after transfer, default = false
        /// </summary>
        public bool RemoveOriginal { get; set; }

        /// <summary>
        /// Whether to use custom transfer command defined or profile, which may be faster in some case.
        /// Default = true
        /// </summary>
        public bool AllowCustomImplementation { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<DiskTransfer>();

        public DiskTransfer() : base("DiskTransfer") { SourceEntryKey = "Source"; DestinationDirectoryEntryKey = "Destination"; RemoveOriginal = false; AllowCustomImplementation = true; }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IEntryModel[] srcEntries = null;
            IEntryModel destEntry = null;
            try
            {
                srcEntries = await pm.GetValueAsEntryModelArrayAsync(SourceEntryKey);
                destEntry = await pm.GetValueAsEntryModelAsync(DestinationDirectoryEntryKey, null);
            }
            catch (ArgumentException ex)
            {
                return ResultCommand.Error(ex);
            }
            
            if (!destEntry.IsDirectory) return ResultCommand.Error(new ArgumentException(DestinationDirectoryEntryKey + " is not a folder."));

            var srcProfile = srcEntries.First().Profile as IDiskProfile;
            var destProfile = destEntry.Profile as IDiskProfile;
            if (srcEntries == null || destProfile == null)
                return ResultCommand.Error(new ArgumentException("Either source or dest is not IDiskProfile."));

            var progress = pm.ContainsKey("Progress") ? pm["Progress"] as IProgress<TransferProgress> : NullTransferProgress.Instance;

            Action<string> log = implementation =>
                logger.Info(String.Format("{0} {1} -> {2} using " + implementation,
                RemoveOriginal ? "Move" : "Copy", srcEntries.GetDescription(), destEntry.Name));

            if (AllowCustomImplementation)
            {
                log("CustomImplementation");                
                await ScriptRunner.RunScriptAsync(pm, destProfile.DiskIO
                    .GetTransferCommand(SourceEntryKey, DestinationDirectoryEntryKey, RemoveOriginal, null));
            }
            else
            {
                var srcMapper = srcProfile.DiskIO.Mapper;
                var destMapping = destProfile.DiskIO.Mapper[destEntry];

                foreach (var srcEntry in srcEntries)
                {
                    var srcMapping = srcMapper[srcEntry];
                    string destName = PathFE.GetFileName(srcMapping.IOPath);
                    string destFullName = destProfile.Path.Combine(destEntry.FullPath, destName);


                    if (!srcMapping.IsVirtual && !destMapping.IsVirtual && RemoveOriginal)
                    {
                        //Disk based transfer
                        progress.Report(TransferProgress.From(srcEntry.FullPath, destFullName));
                        log("System.IO");
                        if (srcEntry.IsDirectory)
                        {
                            if (Directory.Exists(destFullName))
                                Directory.Delete(destFullName, true);
                            Directory.Move(srcMapping.IOPath, destFullName); //Move directly.
                            progress.Report(TransferProgress.IncrementProcessedEntries());
                        }
                        else
                        {
                            if (File.Exists(destFullName))
                                File.Delete(destFullName);
                            File.Move(srcMapping.IOPath, destFullName);
                        }
                        progress.Report(TransferProgress.IncrementProcessedEntries());
                        return CoreScriptCommands.NotifyChanged(RemoveOriginal ? ChangeType.Moved : ChangeType.Changed,
                            srcEntry, destEntry, NextCommand);
                    }
                    else
                    {
                        log("ScriptCommands");
                        //DiskParseOrCreateFolder -> List -> ForEach -> DiskTransfer
                        IScriptCommand cmd2Run;
                        if (srcEntry.IsDirectory)
                            cmd2Run =
                                ScriptCommands.Assign("{DT-SrcDirectory}", srcEntries, false,
                                    ScriptCommands.Assign("{DT-DestProfile}", destEntry.Profile, false,
                                       CoreScriptCommands.DiskParseOrCreateFolder("{DT-DestProfile}", destFullName, "{DT-DestDirectory}",
                                        IOScriptCommands.DiskTransferChild("{DT-SrcDirectory}", "{DT-DestDirectory}", RemoveOriginal, AllowCustomImplementation,
                                        ScriptCommands.Reset(ResultCommand.NoError, "{DT-DestDirectory}", "{DT-SrcDirectory}")))));
                        else
                        {
                            cmd2Run =
                                ScriptCommands.Assign("{DT-SrcFile}", srcEntries, false,
                                 ScriptCommands.Assign("{DT-SrcProfile}", srcEntry.Profile, false,
                                 ScriptCommands.Assign("{DT-DestProfile}", destEntry.Profile, false,
                                    CoreScriptCommands.DiskParseOrCreateFile("{DT-DestProfile}", destFullName, "{DT-DestFile}",
                                        CoreScriptCommands.DiskCopyFile("{DT-SrcProfile}", "{DT-SrcFile}", "{DT-DestProfile}", "{DT-DestFile}",
                                        ScriptCommands.Reset(ResultCommand.NoError, "{DT-SrcFile}", "{DT-DestFile}"))))));
                        }

                        await ScriptRunner.RunScriptAsync(pm, cmd2Run);

                    }
                }
            }
            return NextCommand;

        }

    }
}
