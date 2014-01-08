﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Cofe.Core;
using FileExplorer.ViewModels;

namespace FileExplorer.BaseControls
{
    /// <summary>
    /// Specialized for ICommand related (IScriptCommandContainer, ICommandHelper)
    /// </summary>
    public class VMParameterDic : UIParameterDic
    {
        public IEventAggregator Events
        {
            get { return this.ContainsKey("Events") && this["Events"] is IEventAggregator ? this["Events"] as IEventAggregator : null; }
            set { if (this.ContainsKey("Events")) this["Events"] = value; else this.Add("Events", value); }
        }

        public IFileListViewModel FileList
        {
            get { return this.ContainsKey("FileList") && this["FileList"] is IFileListViewModel ? this["FileList"] as IFileListViewModel : null; }
            set { if (this.ContainsKey("FileList")) this["FileList"] = value; else this.Add("FileList", value); }
        }     

    }    

    public static partial class ExtensionMethods
    {
        public static VMParameterDic AsVMParameterDic(this ParameterDic dic)
        {
            if (dic is VMParameterDic) 
                return (VMParameterDic)dic;

            var retVal = new VMParameterDic();
            foreach (var pp in dic)
                retVal.Add(pp.Key, pp.Value);
            return retVal;                    
        }
    }
}
