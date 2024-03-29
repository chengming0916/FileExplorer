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

//IScriptCommand iterateCommand =
//              ScriptCommands.ForEach("{Items}", "{Current}", 
//			    ScriptCommands.PrintDebug("{Current}"));
//
//await ScriptRunner.RunScriptAsync(new ParameterDic() { 
//                { "Items", new[] { 1, 2, 3, 4, 5 } }
//            }, iterateCommand);
			
int[] array = new int[] { 1, 3, 5 };			

IScriptCommand filterCommand = ScriptCommands.FilterArray("{Array}", null, ComparsionOperator.GreaterThan, 
   2, "{Array}", 
     ScriptCommands.PrintDebug("{Array.Length}"));
		
await ScriptRunner.RunScriptAsync<int[]>("{Array}", new ParameterDic() { 
                { "Array", array }, 				
            }, filterCommand).Dump();
			
//IScriptCommand iterateCommand2 =
//              ScriptCommands.ForEachIfAnyValue<DateTime>("{Items}", null, ComparsionOperator.Equals, DateTime.Today,
//			    ScriptCommands.PrintDebug("True"), ScriptCommands.PrintDebug("False"));
//
//var pd = new ParameterDic() { 
//                { "Items", new[] { DateTime.Today.AddDays(0), DateTime.Today.AddDays(1) } },
//				{ "Equals", DateTime.Today.Day }
//				
//            };			
//await ScriptRunner.RunScriptAsync(pd, iterateCommand2);
//
//pd.Dump();