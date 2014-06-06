..\tools\nant\nant.exe init import build-patch copy pack -buildfile:fileExplorer3.build -logfile:compile.txt 
if not ERRORLEVEL 0 pause