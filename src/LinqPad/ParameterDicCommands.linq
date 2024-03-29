<Query Kind="Statements">
  <Reference Relative="..\FileExplorer3\app\FileExplorer3.WPF\bin\Debug\Caliburn.Micro.Platform.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\FileExplorer3.WPF\bin\Debug\Caliburn.Micro.Platform.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer.Scripting.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer.Scripting.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\FileExplorer.UIEventHub\bin\Debug\FileExplorer.UIEventHub.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\FileExplorer.UIEventHub\bin\Debug\FileExplorer.UIEventHub.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.IO.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.IO.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.WPF.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer3.WPF.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\MetroLog.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\MetroLog.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\MetroLog.Platform.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\MetroLog.Platform.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationCore.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\WPF\WindowsBase.dll</Reference>
  <NuGetReference>Caliburn.Micro</NuGetReference>
  <Namespace>FileExplorer</Namespace>
  <Namespace>FileExplorer.IO</Namespace>
  <Namespace>FileExplorer.Models</Namespace>
  <Namespace>FileExplorer.Script</Namespace>
  <Namespace>FileExplorer.WPF.Utils</Namespace>
  <Namespace>System.Collections</Namespace>
</Query>

System.IO.File.Delete(@"C:\Temp\aaaaabc\pd7.xml");
DiskParameterDicStore store = new DiskParameterDicStore(@"C:\Temp\aaaaabc\pd7.xml");
store.Add("ABC", "CDE");
store.Add("Number", 123);
AsyncUtils.RunSync(() => store.SaveAsync());


ParameterDic pd = new ParameterDic(new DiskParameterDicStore(@"C:\Temp\aaaaabc\pd7.xml"));
pd.GetValue<int>("{Number}").Dump();

IScriptCommand getPropertyCommand = 
  ScriptCommands.AssignGlobalParameterDic("{GlobalPD}", false, 
    ScriptCommands.AssignValueFunc("{GlobalPD.CreationTime}", () => DateTime.UtcNow, true, 
	ScriptCommands.AssignValueFunc("{GlobalPD.LastAccessTime}", () => DateTime.UtcNow, false, 
	ScriptCommands.Delay(100, 
  ScriptCommands.PrintDebug("CreationTime:  {GlobalPD.CreationTime.Ticks}, LastAccessTime: {GlobalPD.LastAccessTime.Ticks}")))));
  
await ScriptRunner.RunScriptAsync(new ParameterDic() , getPropertyCommand);
await ScriptRunner.RunScriptAsync(new ParameterDic() , getPropertyCommand);
await ScriptRunner.RunScriptAsync(new ParameterDic() , getPropertyCommand);
await ScriptRunner.RunScriptAsync(new ParameterDic() , getPropertyCommand);
await ScriptRunner.RunScriptAsync(new ParameterDic() , getPropertyCommand);