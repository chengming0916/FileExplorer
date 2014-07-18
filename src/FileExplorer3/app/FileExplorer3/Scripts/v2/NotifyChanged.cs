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
        public static IScriptCommand NotifyChanged(ChangeType changeType, string sourceProfileKey = null, string sourceEntryKey = null,
         string destinationProfileKey = "Profile", string destinationEntryKey = "Entry", IScriptCommand nextCommand = null)
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
    }

    public class NotifyChanged : ScriptCommandBase
    {
        /// <summary>
        /// Profile used report source changed, default = null.
        /// </summary>
        public string SourceProfileKey { get; set; }

        /// <summary>
        /// Can be either a path (prase from sourceProfile) or IEntryModel, default = null.
        /// </summary>
        public string SourceEntryKey { get; set; }

        /// <summary>
        /// Profile used report destination changed, default = "Profile".
        /// </summary>
        public string DestinationProfileKey { get; set; }

        /// <summary>
        /// Can be either a path (prase from sourceProfile) or IEntryModel, default = "Entry".
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
            IProfile sourceProfile = pm.GetValue<IProfile>(SourceProfileKey) ??
                pm.GetValue<IEntryModel>(SourceEntryKey, NullEntryModel.Instance).Profile;
            IProfile destinationProfile = pm.GetValue<IProfile>(DestinationProfileKey) ??
                pm.GetValue<IEntryModel>(DestinationEntryKey, NullEntryModel.Instance).Profile;
            string sourcePath = pm.GetValue<string>(SourceEntryKey) ??
                pm.GetValue<IEntryModel>(SourceEntryKey, NullEntryModel.Instance).FullPath;
            string destinationPath = pm.GetValue<string>(DestinationEntryKey) ??
               pm.GetValue<IEntryModel>(DestinationEntryKey, NullEntryModel.Instance).FullPath;

            logger.Info(String.Format("({0}) {1} -> {2}", ChangeType, sourcePath, destinationPath));
            if (ChangeType == ChangeType.Moved && sourceProfile != null && destinationProfile != null && sourcePath != null)
            {
                if (sourceProfile != destinationProfile)
                {
                    sourceProfile.Events.PublishOnCurrentThread(new EntryChangedEvent(ChangeType.Deleted, sourcePath));
                    destinationProfile.Events.PublishOnCurrentThread(new EntryChangedEvent(ChangeType.Created, destinationPath));
                }
                else
                    destinationProfile.Events.PublishOnCurrentThread(new EntryChangedEvent(destinationPath, sourcePath));
            }
            else
                destinationProfile.Events.PublishOnCurrentThread(new EntryChangedEvent(ChangeType, destinationPath));

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
