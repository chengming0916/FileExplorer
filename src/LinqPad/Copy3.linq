<Query Kind="Statements">
  <Reference Relative="..\FileExplorer3\app\FileExplorer3.WPF\bin\Debug\Caliburn.Micro.Platform.dll">&lt;MyDocuments&gt;\QuickZipSvn\FileExplorer\src\FileExplorer3\app\FileExplorer3.WPF\bin\Debug\Caliburn.Micro.Platform.dll</Reference>
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
  <Namespace>MetroLog</Namespace>
  <Namespace>MetroLog.Targets</Namespace>
  <Namespace>System.Collections</Namespace>
</Query>

//Copy from c:\temp\file1.txt to file2.txt
LogManagerFactory.DefaultConfiguration.IsEnabled = true;

string tempDirectory = "C:\\Temp";
string destDirectory = "C:\\Temp\\Debug1";
string srcDirectory = "C:\\Temp\\aaaaa";

		   
IScriptCommand diskTransferCommand = 
	ScriptCommands.ParsePath(srcDirectory, "{Source}",
    ScriptCommands.DiskParseOrCreateFolder(destDirectory, "{Destination}",
    IOScriptCommands.DiskTransfer("{Source}", "{Destination}", false, false)));
	
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Profile", FileSystemInfoExProfile.CreateNew() }
            }, diskTransferCommand);