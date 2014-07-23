using FileExplorer.Defines;
using FileExplorer.Models;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace FileExplorer.Script
{
    public static partial class CoreScriptCommands
    {
        private static IScriptCommand notifyChanged(
            ChangeType changeType, object sourceProfileKey, string sourceEntryKey,
         object destinationProfileKey, string destinationEntryKey, IScriptCommand nextCommand)
        {
            return new NotifyChanged()
            {
                ChangeType = changeType,
                SourceProfileKey = sourceProfileKey,
                SourceEntryKey = sourceEntryKey,
                DestinationProfileKey = destinationProfileKey,
                DestinationEntryKey = destinationEntryKey,
                NextCommand = (ScriptCommandBase)nextCommand
            };
        }

        public static IScriptCommand NotifyMoved(object sourceProfileKey = null, string sourceEntryKey = null,
         object destinationProfileKey = null, string destinationEntryKey = "Entry", IScriptCommand nextCommand = null)
        {
            return notifyChanged(ChangeType.Moved, sourceProfileKey, sourceEntryKey, 
                destinationProfileKey, destinationEntryKey, nextCommand);
        }

        public static IScriptCommand NotifyCreated(object profileKey = null, 
            string entryKey = "Entry", IScriptCommand nextCommand = null)
        {
            return notifyChanged(ChangeType.Created, null, null, profileKey, entryKey, nextCommand);
        }

        public static IScriptCommand NotifyDeleted(object profileKey = null,
           string entryKey = "Entry", IScriptCommand nextCommand = null)
        {
            return notifyChanged(ChangeType.Deleted, null, null, profileKey, entryKey, nextCommand);
        }

        public static IScriptCommand NotifyChanged(object profileKey = null,
          string entryKey = "Entry", IScriptCommand nextCommand = null)
        {
            return notifyChanged(ChangeType.Changed, null, null, profileKey, entryKey, nextCommand);
        }


        public static IScriptCommand NotifyMoved(IEntryModel srcEntry, IEntryModel destEntry, IScriptCommand nextCommand = null)
        {
            srcEntry = srcEntry ?? NullEntryModel.Instance;
            return NotifyMoved(srcEntry.Profile, srcEntry.FullPath, destEntry.Profile, destEntry.FullPath, nextCommand);
        }

        public static IScriptCommand NotifyCreated(IEntryModel entry, IScriptCommand nextCommand = null)
        {
            entry = entry ?? NullEntryModel.Instance;
            return NotifyCreated(entry, nextCommand);
        }

        public static IScriptCommand NotifyDeleted(IEntryModel entry, IScriptCommand nextCommand = null)
        {
            entry = entry ?? NullEntryModel.Instance;
            return NotifyDeleted(entry, nextCommand);
        }

        public static IScriptCommand NotifyChanged(IEntryModel entry, IScriptCommand nextCommand = null)
        {
            entry = entry ?? NullEntryModel.Instance;
            return NotifyChanged(entry, nextCommand);
        }

    }

    public class NotifyChanged : ScriptCommandBase
    {
        /// <summary>
        /// Profile used report source changed, can be a key or actual IProfile, default = null.
        /// </summary>
        public object SourceProfileKey { get; set; }

        /// <summary>
        /// Can be either a csv path (prase from sourceProfile) or IEntryModel, default = null.
        /// </summary>
        public string SourceEntryKey { get; set; }        

        /// <summary>
        /// Profile used report destination changed, can be a key or actual IProfile, default = "Profile".
        /// </summary>
        public object DestinationProfileKey { get; set; }

        /// <summary>
        /// Can be either a csv path (prase from sourceProfile) or IEntryModel, default = "Entry".
        /// </summary>
        public string DestinationEntryKey { get; set; }

        /// <summary>
        /// Change type (Changed, Created, Deleted, Moved) , default = Changed.
        /// </summary>
        public ChangeType ChangeType { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<NotifyChanged>();

        public NotifyChanged()
            : base("NotifyChanged")
        {
            SourceProfileKey = null;
            SourceEntryKey = null;
            DestinationProfileKey = "Profile";
            DestinationEntryKey = "Entry";
            ChangeType = Defines.ChangeType.Changed;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            IProfile sourceProfile = SourceProfileKey as IProfile ?? pm.GetValue<IProfile>(SourceProfileKey as string) ??
                pm.GetValue<IEntryModel>(SourceEntryKey, NullEntryModel.Instance).Profile;
            IProfile destinationProfile = DestinationProfileKey as IProfile ?? pm.GetValue<IProfile>(DestinationProfileKey as string) ??
                pm.GetValue<IEntryModel>(DestinationEntryKey, NullEntryModel.Instance).Profile;
            
            string sourcePath = pm.GetValue<string>(SourceEntryKey) ??
                pm.GetValue<IEntryModel>(SourceEntryKey, NullEntryModel.Instance).FullPath;
            string[] sourcePaths = sourcePath == null ? new string[] {} : 
                sourcePath.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            string destinationPath = pm.GetValue<string>(DestinationEntryKey) ??
               pm.GetValue<IEntryModel>(DestinationEntryKey, NullEntryModel.Instance).FullPath;
            string[] destinationPaths =
                destinationPath == null ? new string[] { } :
                destinationPath.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            logger.Info(String.Format("({0}) {1} -> {2}", ChangeType, sourcePath, destinationPath));
            if (ChangeType == ChangeType.Moved && sourceProfile != null && destinationProfile != null && sourcePath != null)
            {
                if (sourceProfile != destinationProfile)
                {
                    sourceProfile.Events.PublishOnCurrentThread(new EntryChangedEvent(ChangeType.Deleted, sourcePaths));
                    destinationProfile.Events.PublishOnCurrentThread(new EntryChangedEvent(ChangeType.Created, destinationPaths));
                }
                else
                    destinationProfile.Events.PublishOnCurrentThread(new EntryChangedEvent(destinationPath, sourcePath));
            }
            else
                destinationProfile.Events.PublishOnCurrentThread(new EntryChangedEvent(ChangeType, destinationPaths));

            return NextCommand;
        }

        //public override bool Equals(object obj)
        //{
        //    return obj is NotifyChangedCommand && (obj as NotifyChangedCommand)._destProfile.Equals(_destProfile) &&
        //        (obj as NotifyChangedCommand)._destParseName.Equals(_destParseName);
        //}

        //public override int GetHashCode()
        //{
        //    return _destParseName.GetHashCode() + _changeType.GetHashCode();
        //}
    }
}
