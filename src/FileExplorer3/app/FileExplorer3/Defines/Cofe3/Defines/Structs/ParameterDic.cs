﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Cofe.Core
{
    public class ParameterDic : Dictionary<string, object>
    {
        public static ParameterDic Empty = new ParameterDic();

        public ParameterDic()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {

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

        /// <summary>
        /// Most exception is throw directly, if not, it will set the Error property, which will be thrown 
        /// in PropertyInvoker.ensureNoError() method.
        /// </summary>
        public Exception Error
        {
            get { return this.ContainsKey("Error") ? this["Error"] as Exception : null; }
            set { if (this.ContainsKey("Error")) this["Error"] = value; else this.Add("Error", value); }
        }

    }
}
