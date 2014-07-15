using FileExplorer.Defines;
using MetroLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;


namespace FileExplorer
{
    public class ParameterPair
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public ParameterPair(string key, object value) { Key = key; Value = value; }
    }

    public static partial class ExtensionMethods
    {
        public static string ReplaceVariableInsideBracketed(this ParameterDic pd, string variableKey)
        {
            Regex regex = new Regex("{(?<TextInsideBrackets>.+)}");
            string value = variableKey;

            Match match = regex.Match(value);

            while (match.Success)
            {
                string key = match.Groups["TextInsideBrackets"].Value;
                value = value.Replace("{" + key + "}", pd.ContainsKey(key) ? pd[key].ToString() : "");
                match = regex.Match(value);
            }

            return value;
        }
    }

    public class ParameterDic : Dictionary<string, object>
    {
        private static ILogger logger = LogManagerFactory.DefaultLogManager.GetLogger<ParameterDic>();
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

        private static string getVariable(string variableKey)
        {
            if (!variableKey.StartsWith("{") && variableKey.EndsWith("}"))
                throw new ArgumentException(variableKey);
            return variableKey.TrimStart('{').TrimEnd('}');
        }

        public T GetValue<T>(string variableKey, T defaultValue)
        {
            string variable = getVariable(variableKey);
               
            if (this.ContainsKey(variable) && this[variable] is T)
                return (T)this[variable];

            return defaultValue;
        }

        public T GetValue<T>(string variableKey)
        {
            return GetValue<T>(variableKey, default(T));
        }

        public object GetValue(string variableKey)
        {
            return GetValue<Object>(variableKey);
        }

        public bool SetValue<T>(string variableKey, T value, bool skipIfExists = false)
        {
            string variable = getVariable(variableKey);
            if (this.ContainsKey(variable))
            {
                if (!skipIfExists)
                {
                    this[variable] = value;
                    return true;
                    
                }
                else
                {
                    
                    return false;
                }
            }
            else
            {
                this.Add(variable, value);
                return true;
                
            }
        }

        
        //public static ParameterDic FromNameValueCollection(NameValueCollection col, string[] paramToFetch)
        //{
        //    ParameterDic retVal = new ParameterDic();

        //    foreach (var key in col.AllKeys)
        //    {
        //        if (paramToFetch.Contains(key, StringComparer.CurrentCultureIgnoreCase))
        //        {
        //            retVal.Add(key, col[key]);
        //        }
        //    }
        //    return retVal;
        //}


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

        public IProgress<TransferProgress> Progress
        {
            get
            {
                return this.ContainsKey("Progress") && this["Progress"] is IProgress<TransferProgress> ?
                    (IProgress<TransferProgress>)this["Progress"] : NullTransferProgress.Instance;
            }
            set { if (this.ContainsKey("Progress")) this["Progress"] = value; else this.Add("Progress", value); }
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
