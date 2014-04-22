﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Cofe.Core
{
    public class ParameterPair
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public ParameterPair(string key, object value) { Key = key; Value = value; }
    }

    public class ParameterDic : Dictionary<string, object>
    {
        public static ParameterDic Empty = new ParameterDic();

        public ParameterDic()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {

        }

        public static ParameterDic FromParameterPair(params ParameterPair[] ppairs)
        {
            ParameterDic retVal = new ParameterDic();
            foreach (var ppair in ppairs)
                retVal.Add(ppair.Key, ppair.Value);
            return retVal;
        }

        public static ParameterDic FromNameValueCollection(NameValueCollection col, string[] paramToFetch)
        {
            ParameterDic retVal = new ParameterDic();

            foreach (var key in col.AllKeys)
            {
                if (paramToFetch.Contains(key, StringComparer.CurrentCultureIgnoreCase))
                {
                    retVal.Add(key, col[key]);
                }
            }
            return retVal;
        }


        public void AddOrUpdate(string key, object value)
        {
            if (this.ContainsKey(key))
                this[key] = value;
            else this.Add(key, value);
        }

        public void LoadParameterDic(ParameterDic pd)
        {
            this.Clear();
            foreach (var pp in pd)
                this.Add(pp.Key, pp.Value);
        }

        public ParameterDic Clone()
        {
            ParameterDic retVal = new ParameterDic();
            retVal.LoadParameterDic(this);
            return retVal;
        }

        public bool IsHandled
        {
            get { return this.ContainsKey("Handled") && true.Equals(this["Handled"]); }
            set { if (this.ContainsKey("Handled")) this["Handled"] = value; else this.Add("Handled", value); }
        }

        public object Parameter
        {
            get { return this.ContainsKey("Parameter") && this["Parameter"] is object ? this["Parameter"] as object : null; }
            set { if (this.ContainsKey("Parameter")) this["Parameter"] = value; else this.Add("Parameter", value); }
        }

        public CancellationToken CancellationToken
        {
            get { return this.ContainsKey("CancellationToken") && this["CancellationToken"] is CancellationToken ? 
                (CancellationToken)this["CancellationToken"] : CancellationToken.None; }
            set { if (this.ContainsKey("CancellationToken")) this["CancellationToken"] = value; else this.Add("CancellationToken", value); }
        }

        /// <summary>
        /// Most exception is throw directly, if not, it will set the Error property, which will be thrown 
        /// in PropertyInvoker.ensureNoError() method.
        /// </summary>
        public Exception Error
        {
            get { return this.ContainsKey("Error") ? this["Error"] as Exception : null; }
            set { if (this.ContainsKey("Error")) this["Error"] = value; else this.Add("Error", value); }
        }

        public List<string> CommandHistory = new List<string>();

    }
}
