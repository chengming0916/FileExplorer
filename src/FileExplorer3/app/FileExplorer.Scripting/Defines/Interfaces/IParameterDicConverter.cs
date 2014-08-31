﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Script
{
    /// <summary>
    /// Convert object parameter to ParameterDic
    /// </summary>
    public interface IParameterDicConverter
    {
        /// <summary>
        /// used by ExplorerAssignScriptParameters IScriptCommand to inject parameters to CommandManager.
        /// </summary>
        /// <param name="pd"></param>
        void AddAdditionalParameters(IParameterDic pd);
        IParameterDic Convert(object parameter, params object[] additionalParameters);
        object ConvertBack(IParameterDic pd, params object[] additionalParameters);
    }
}
