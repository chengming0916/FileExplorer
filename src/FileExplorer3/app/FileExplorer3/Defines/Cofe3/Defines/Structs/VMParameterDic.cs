using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Cofe.Core;

namespace FileExplorer.BaseControls
{
    /// <summary>
    /// Specialized for ICommand related (IScriptCommandContainer, ICommandHelper)
    /// </summary>
    public class VMParameterDic : ParameterDic
    {
        public string EventName
        {
            get { return this.ContainsKey("EventName") && this["EventName"] is string ? this["EventName"] as string : null; }
            set { if (this.ContainsKey("EventName")) this["EventName"] = value; else this.Add("EventName", value); }
        }        

        public RoutedEventArgs EventArgs
        {
            get { return this.ContainsKey("EventArgs") && this["EventArgs"] is RoutedEventArgs ? this["EventArgs"] as RoutedEventArgs : null; }
            set { if (this.ContainsKey("EventArgs")) this["EventArgs"] = value; else this.Add("EventArgs", value); }
        }        

        public Object Sender
        {
            get { return this.ContainsKey("Sender") ? this["Sender"] : null; }
            set { if (this.ContainsKey("Sender")) this["Sender"] = value; else this.Add("Sender", value); }
        }        
    }    

    public static partial class ExtensionMethods
    {
        public static VMParameterDic AsVMParameterDic(this ParameterDic dic)
        {
            return (VMParameterDic)dic;
        }
    }
}
