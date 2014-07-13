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
string srcFile = System.IO.Path.Combine(tempDirectory, "file1.txt");  
string destFile = System.IO.Path.Combine(tempDirectory, "file2.txt");  

using (var sw = File.CreateText(srcFile))
	sw.WriteLine(DateTime.Now.ToString());

IScriptCommand copyCommand =
               ScriptCommands.ParsePath("SourceFile", "Source",
               ScriptCommands.DiskParseOrCreateFile("DestinationFile", "Destination",
               ScriptCommands.OpenStream("Source", "SourceStream", FileExplorer.Defines.FileAccess.Read,
               ScriptCommands.OpenStream("Destination", "DestinationStream", FileExplorer.Defines.FileAccess.Write,
               ScriptCommands.CopyStream("SourceStream", "DestinationStream")))),
               ResultCommand.Error(new FileNotFoundException()));				
			
await ScriptRunner.RunScriptAsync(new ParameterDic() { 
                { "Profile", FileSystemInfoExProfile.CreateNew() },
                { "SourceFile", srcFile },
                { "DestinationFile", destFile }
            }, copyCommand);