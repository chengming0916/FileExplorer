using FileExplorer.Script;
using FileExplorer.Utils;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FileExplorer.UIEventHub
{

    public static partial class HubScriptCommands
    {
        public static IScriptCommand AssignDataContext(     
            string sourceElementVariable = "{EventArgs.OriginalSource}",
            DataContextType type = DataContextType.SupportDrag,             
            string destVariable = "{Variable}", bool skipIfExists = false, 
            IScriptCommand thenCommand = null, IScriptCommand notFoundCommand = null)
        {
            return new AssignDataContext()
            {
                SourceElementKey = sourceElementVariable,
                DataContextType = type,
                VariableKey = destVariable,
                SkipIfExists = skipIfExists,
                NextCommand = (ScriptCommandBase)thenCommand,
                NotFoundCommand = (ScriptCommandBase)notFoundCommand
            };
        }
      
    }

    public enum DataContextType { Any, SupportDrag, SupportShellDrag, SupportDrop, SupportShellDrop }

    /// <summary>
    /// Use DataContextFinder to lookup up from the Visual Tree of ElementKey to find a 
    /// DataContext that match  the type.
    /// </summary>
    public class AssignDataContext : Assign
    {
        /// <summary>
        /// Point to Element (FrameworkElement) to lookup, Default = {EventArgs.OriginalSource}
        /// </summary>
        public string SourceElementKey { get; set; }

        public ScriptCommandBase NotFoundCommand { get; set; }

        public DataContextType DataContextType { get; set; }

        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<AssignDataContext>();

        public override IScriptCommand Execute(ParameterDic pm)
        {
            FrameworkElement origSource = pm.GetValue<FrameworkElement>(SourceElementKey);
            if (origSource == null)
                return NotFoundCommand;
            
            Value = null;
            switch (DataContextType)
            {
                case UIEventHub.DataContextType.Any : 
                    Value = origSource.DataContext;
                    break;
                case UIEventHub.DataContextType.SupportDrag:
                    Value = DataContextFinder.GetDataContext(origSource, DataContextFinder.SupportDrag);
                    break;
                case UIEventHub.DataContextType.SupportShellDrag:
                    Value = DataContextFinder.GetDataContext(origSource, DataContextFinder.SupportShellDrag);
                    break;
                case UIEventHub.DataContextType.SupportShellDrop:
                    Value = DataContextFinder.GetDataContext(origSource, DataContextFinder.SupportShellDrop);
                    break;
                case UIEventHub.DataContextType.SupportDrop:
                    Value = DataContextFinder.GetDataContext(origSource, DataContextFinder.SupportDrop);
                    break;
                default:
                    return ResultCommand.Error(new NotSupportedException("DataContextType"));
            }

            if (Value != null)
                return base.Execute(pm);
            else return NotFoundCommand;            
        }

     

    }
}
