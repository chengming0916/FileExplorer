﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Cofe.Core;
using FileExplorer.UserControls.InputProcesor;

namespace FileExplorer.BaseControls
{
    /// <summary>
    /// Specialized for IUIEventHub uses.
    /// </summary>
    public class UIParameterDic : ParameterDic
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

        public Object OriginalSource
        {
            get { return this.ContainsKey("OriginalSource") ? this["OriginalSource"] : null; }
            set { if (this.ContainsKey("OriginalSource")) this["OriginalSource"] = value; else this.Add("OriginalSource", value); }
        }      

        public Object Sender
        {
            get { return this.ContainsKey("Sender") ? this["Sender"] : null; }
            set { if (this.ContainsKey("Sender")) this["Sender"] = value; else this.Add("Sender", value); }
        }

        public IList<IInputProcessor> InputProcessors
        {
            get { return this.ContainsKey("InputProcessors") ? this["InputProcessors"] as IList<IInputProcessor> : null; }
            set { if (this.ContainsKey("InputProcessors")) this["InputProcessors"] = value; else this.Add("InputProcessors", value); }
        }
    }    

    public static partial class ExtensionMethods
    {
        public static DragInputProcessor GetDragInputProcessor(this UIParameterDic dic)
        {
            return dic.InputProcessors.First(p => p is DragInputProcessor) as DragInputProcessor;
        }

        public static UIParameterDic AsUIParameterDic(this ParameterDic dic)
        {
            if (dic is UIParameterDic)
                return (UIParameterDic)dic;

            var retVal = new UIParameterDic();
            foreach (var pp in dic)
                retVal.Add(pp.Key, pp.Value);
            return retVal;
        }
    }
}
