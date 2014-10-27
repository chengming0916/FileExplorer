using FileExplorer.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.UIEventHub
{
    public static class UIParameterDicConvertRule 
    {
        public static IParameterDicConvertRule ConvertParameter =
            new ConvertParameterToVariable(0, "{Parameter}");        

        public static IParameterDicConvertRule ConvertUIParameters =
           ParameterDicConvertRule.Combine(
               new ConvertParameterToVariable(0, "{Parameter}"),
               new ConvertParameterToVariable(1, "{EventName}"),
               new ConvertParameterToVariable(2, "{Sender}"),
               new ConvertParameterToVariable(3, "{EventArgs}")
           );

        public static IParameterDicConvertRule ConvertUIInputParameters =
               ParameterDicConvertRule.Combine(
                   new ConvertParameterToVariable(0, "{Parameter}"),
                   new ConvertParameterToVariable(1, "{EventName}"),    
                   new ConvertParameterToVariable(2, "{Input}"),                   
                   new AddVariableFromGetter<object>("{Sender}", (pms) => (pms[2] as IUIInput).Sender),
                   new AddVariableFromGetter<EventArgs>("{EventArgs}", (pms) => (pms[2] as IUIInput).EventArgs),
                   new AddVariableFromGetter<List<IUIInputProcessor>>("{InputProcessors}", 
                       (pms) => (pms[3] as UIInputManager).Processors.ToList())                   
               );

    

    }
}
