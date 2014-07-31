//using Caliburn.Micro;
//using FileExplorer.Defines;
//using FileExplorer.Models;
//using MetroLog;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FileExplorer.Script
//{
//    public static partial class CoreScriptCommands
//    {
//        public static IScriptCommand BroadcastEntryChangedEvent(
//            ChangeType changeType,
//            string eventsVariable = "{Events}",
//            string entriesVariable = "{Entry}",
//            IScriptCommand nextCommand = null)
//        {
//            return new BroadcastEntryChangedEvent()
//            {
//                NextCommand = (ScriptCommandBase)nextCommand,
//                ChangeType = changeType,
//                EventsKey = eventsVariable,
//                EntriesKey = entriesVariable
//            };
//        }

//        public static IScriptCommand BroadcastEntryChanged(string entriesVariable = "{Entry}",
//            IScriptCommand nextCommand = null)
//        {
//            return BroadcastEntryChangedEvent(ChangeType.Changed, "{Events}", entriesVariable, nextCommand);
//        }

//        public static IScriptCommand BroadcastEntryMoved(string entriesVariable = "{Entry}",
//            IScriptCommand nextCommand = null)
//        {
//            return BroadcastEntryChangedEvent(ChangeType.Moved, "{Events}", entriesVariable, nextCommand);
//        }

//        public static IScriptCommand BroadcastEntryCreated(string entriesVariable = "{Entry}",
//            IScriptCommand nextCommand = null)
//        {
//            return BroadcastEntryChangedEvent(ChangeType.Created, "{Events}", entriesVariable, nextCommand);
//        }

//        public static IScriptCommand BroadcastEntryDeleted(string entriesVariable = "{Entry}",
//            IScriptCommand nextCommand = null)
//        {
//            return BroadcastEntryChangedEvent(ChangeType.Deleted, "{Events}", entriesVariable, nextCommand);
//        }
//    }


//    public class BroadcastEntryChangedEvent : ScriptCommandBase
//    {
//        /// <summary>
//        /// EventAggregator (IEventAggregator) used to broadcast the event, Default = "{Events}".
//        /// </summary>
//        public string EventsKey { get; set; }

//        /// <summary>
//        /// EntryChangeType
//        /// </summary>
//        public ChangeType ChangeType { get; set; }

//        /// <summary>
//        /// Entry affected (IEntryModel or IEntryModel[]). Default = "{Entry}"
//        /// </summary>
//        public string EntriesKey { get; set; }

//        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<BroadcastEntryChangedEvent>();

//        public BroadcastEntryChangedEvent()
//            : base("BroadcastEntryChangedEvent")
//        {
//            EventsKey = "{Events}";
//            ChangeType = Defines.ChangeType.Changed;
//            EntriesKey = "{Entry}";
//        }

//        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
//        {      
//            IEntryModel[] entryModels = EntriesKey == null ? new IEntryModel[] { } :
//              (await pm.GetValueAsEntryModelArrayAsync(EntriesKey));

//            object evnt = new EntryChangedEvent(ChangeType.Created,
//                        entryModels.Select(em => em.FullPath).ToArray());            

//            return CoreScriptCommands.BroadcastEvent(EventsKey, evnt, NextCommand);

//        }
//    }
//}
