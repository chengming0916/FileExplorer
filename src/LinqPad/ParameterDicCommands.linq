<Query Kind="Statements">
  <Reference Relative="..\FileExplorer3\app\FileExplorer3.WPF\bin\Debug\Caliburn.Micro.Platform.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\FileExplorer3.WPF\bin\Debug\Caliburn.Micro.Platform.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer.Scripting.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer.Scripting.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\FileExplorer.UIEventHub\bin\Debug\FileExplorer.UIEventHub.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\FileExplorer.UIEventHub\bin\Debug\FileExplorer.UIEventHub.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.IO.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.IO.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.WPF.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.WPF.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\MetroLog.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\MetroLog.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\MetroLog.Platform.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\MetroLog.Platform.dll</Reference>
  <NuGetReference>Caliburn.Micro</NuGetReference>
  <Namespace>FileExplorer</Namespace>
  <Namespace>FileExplorer.IO</Namespace>
  <Namespace>FileExplorer.Models</Namespace>
  <Namespace>FileExplorer.Script</Namespace>
  <Namespace>System.Collections</Namespace>
</Query>

DiskParameterDicStore 

IScriptCommand getPropertyCommand = 
  ScriptCommands.PrintDebug("{ParamDic.DT.DayOfWeek}", 
    ScriptCommands.Assign("{ParamDic.DT}", DateTime.Now.AddDays(1), false, 
	ScriptCommands.PrintDebug("{ParamDic.DT.DayOfWeek}")));
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "ParamDic", new ParameterDic() { { "DT", DateTime.Now } } }				
            }, getPropertyCommand);