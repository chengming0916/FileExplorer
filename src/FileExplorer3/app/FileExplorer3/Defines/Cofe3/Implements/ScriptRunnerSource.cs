﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofe.Core.Script
{
    public static class ScriptRunnerSources
    {
        public static IScriptRunnerSource Null = new NullScriptRunnerSource();
    }

    public class NullScriptRunnerSource : IScriptRunnerSource
    {
        public ScriptRunner GetScriptRunner()
        {
            return new ScriptRunner();
        }
    }
}
