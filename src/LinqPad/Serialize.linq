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
  <Namespace>MetroLog</Namespace>
  <Namespace>MetroLog.Targets</Namespace>
  <Namespace>System.Collections</Namespace>
</Query>

LogManagerFactory.DefaultConfiguration.IsEnabled = true;

IScriptCommand downloadCommand =             
              CoreScriptCommands.Download("{Url}", "{Stream}",
                CoreScriptCommands.DiskParseOrCreateFile("{Profile}", "{DestinationFile}", "{Destination}",
                CoreScriptCommands.DiskOpenStream("{Destination}", "{DestinationStream}", FileExplorer.Defines.FileAccess.Write,
                CoreScriptCommands.CopyStream("{Stream}", "{DestinationStream}"))));

IScriptCommand copyCommand =
               CoreScriptCommands.ParsePath("{Profile}","{SourceFile}", "{Source}",
               CoreScriptCommands.DiskParseOrCreateFile("{Profile}","{DestinationFile}", "{Destination}",
               CoreScriptCommands.DiskOpenStream("{Source}", "{SourceStream}", FileExplorer.Defines.FileAccess.Read,
               CoreScriptCommands.DiskOpenStream("{Destination}", "{DestinationStream}", FileExplorer.Defines.FileAccess.Write,
               CoreScriptCommands.CopyStream("{SourceStream}", "{DestinationStream}")))),
               ResultCommand.Error(new FileNotFoundException()));			
			   
IScriptCommand iterateCommand =
              ScriptCommands.ForEach("{Items}", "{Current}", 
			    ScriptCommands.PrintDebug("{Current}"), ScriptCommands.Reset(null, "{Items}"));			   
				
Func<IScriptCommand, Stream> serialize = (cmd) =>				
				new ScriptCommandSerializer(new Type[] {typeof(FileExplorer3Commands)}).SerializeScriptCommand(cmd );
				
var stream = serialize(downloadCommand);
stream = serialize(copyCommand);
//stream = serialize(iterateCommand);
XDocument.Load(stream).Dump();
