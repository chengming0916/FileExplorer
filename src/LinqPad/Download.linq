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

//Download a image to c:\temp\output.png
LogManagerFactory.DefaultConfiguration.IsEnabled = true;

string downloadUrl = "http://www.quickzip.org/logo2.png";
string tempDirectory = "C:\\Temp";
string destFile = System.IO.Path.Combine(tempDirectory, "output.png");  

IScriptCommand downloadCommand =
              ScriptCommands.Download("Url", "Stream",
                ScriptCommands.DiskParseOrCreateFile("DestinationFile", "Destination",				
                ScriptCommands.OpenStream("Destination", "DestinationStream", FileExplorer.Defines.FileAccess.Write,
                ScriptCommands.CopyStream("Stream", "DestinationStream"))));		
			
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Profile", FileSystemInfoExProfile.CreateNew() },
                { "Url", downloadUrl },
                { "DestinationFile", destFile },                
            }, downloadCommand);