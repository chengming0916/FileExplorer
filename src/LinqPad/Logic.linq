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
  <Namespace>System.Collections</Namespace>
</Query>

IScriptCommand iterateCommand =
              ScriptCommands.ForEach("{Items}", "{Current}", 
			    ScriptCommands.PrintDebug("{Current}"));

await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Items", new[] { 1, 2, 3, 4, 5 } }
            }, iterateCommand);
			
IScriptCommand greaterThanCommand =   
  ScriptCommands.IfValue(ComparsionOperator.GreaterThan, "{Variable1}", "{Variable2}", 
    ScriptCommands.PrintDebug("{Variable1} is Greater than {Variable2}"),
	ScriptCommands.PrintDebug("{Variable1} is Smaller than {Variable2}"));
	
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Variable1", 1 },
				{ "Variable2", 2 }
            }, greaterThanCommand); 
			
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Variable1", DateTime.Now.AddDays(1) },
				{ "Variable2", DateTime.Now }
            }, greaterThanCommand);
									
IScriptCommand arrayCommand =   
  ScriptCommands.IfArrayLength(ComparsionOperator.GreaterThan, "{Variable1}", 5,  
    ScriptCommands.PrintDebug("Array length is Greater than 5"),
	ScriptCommands.PrintDebug("Array length is not greater than 5"));	
	
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Variable1", (new[] {1, 2, 3, 4, 5, 6 }) }				
            }, arrayCommand);
			
IScriptCommand caseCommand = 
  ScriptCommands.Switch("{Variable1}", 
    new Dictionary<int, IScriptCommand>()
	{
		{ 1 , ScriptCommands.PrintDebug("One") }, 
		{ 2 , ScriptCommands.PrintDebug("Two") }, 
		{ 3 , ScriptCommands.PrintDebug("Three") }, 
	}, ScriptCommands.PrintDebug("Otherwise"));
	
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Variable1", 4 }				
            }, caseCommand);