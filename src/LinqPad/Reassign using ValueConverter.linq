<Query Kind="Statements">
  <Reference Relative="..\FileExplorer3\app\FileExplorer3.WPF\bin\Debug\Caliburn.Micro.Platform.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\FileExplorer3.WPF\bin\Debug\Caliburn.Micro.Platform.dll</Reference>
  <Reference Relative="..\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer.Scripting.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\TestApp.WPF\bin\Debug\FileExplorer.Scripting.dll</Reference>
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
  <Namespace>FileExplorer.Utils</Namespace>
  <Namespace>System.Collections</Namespace>
</Query>

IScriptCommand getPropertyCommand = 
  ScriptCommands.AssignValueConverter(ValueConverterType.GetProperty, "{Converter}", 
    ScriptCommands.Reassign("{Variable1}", "{Converter}", "{Variable2}", false, 
	  ScriptCommands.PrintDebug("{Variable1} -> {Variable2}")), "DayOfWeek");
			
IScriptCommand executeMethodCommand = 
  ScriptCommands.AssignValueConverter(ValueConverterType.ExecuteMethod, "{Converter}", 
    ScriptCommands.Reassign("{Variable1}", "{Converter}", "{Variable2}", false, 
	  ScriptCommands.PrintDebug("{Variable1} + 1 day -> {Variable2}")), "AddDays", 1);				  
			
IScriptCommand getItemInArrayCommand = 
  ScriptCommands.AssignValueConverter(ValueConverterType.GetArrayItem, "{Converter}", 
    ScriptCommands.Reassign("{Array}", "{Converter}", "{DestVariable}", false, 
	  ScriptCommands.PrintDebug("Item 1 -> {DestVariable}")), 1);	

//Shortcut method.	  
	  
getPropertyCommand = ScriptCommands.AssignProperty("{Variable1}", "DayOfWeek", "{Variable2}", 
  ScriptCommands.PrintDebug("{Variable1} -> {Variable2}"));
  
executeMethodCommand = ScriptCommands.AssignMethodResult("{Variable1}", "AddDays", new object[] { 1 }, "{Variable2}", 
  ScriptCommands.PrintDebug("{Variable1} + 1 day -> {Variable2}"));
	  
getItemInArrayCommand = ScriptCommands.AssignArrayItem("{Array}", 1, "{DestVariable}", 
  ScriptCommands.PrintDebug("Item 1 -> {DestVariable}"));


await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Variable1", DateTime.Now },
            }, getPropertyCommand);

await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Variable1", DateTime.Now },
            }, executeMethodCommand);	  
	  
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Array", new [] { 1,2,3} },
            }, getItemInArrayCommand);