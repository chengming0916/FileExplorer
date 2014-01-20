using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Tools;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cofe.Core;
using Cofe.Core.Script;
using Cofe.Core.Utils;
using FileExplorer.BaseControls;
using FileExplorer.Defines;
using FileExplorer.ViewModels;

namespace FileExplorer.Models
{
    public class OpenWithScriptCommand : ScriptCommandBase
    {
        OpenWithInfo _info;

        /// <summary>
        /// Launch a file (e.g. txt) using the OpenWithInfo, if null then default method will be used, Require Parameter (IEntryModel[])
        /// </summary>
        /// <param name="info"></param>
        public OpenWithScriptCommand(OpenWithInfo info = null)
            : base("OpenWith")
        {
            _info = info;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            return base.CanExecute(pm);
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var parameter = pm["Parameter"] as IEntryModel[];
            if (parameter != null && parameter.Count() == 1)
            {
                bool _isFolder = parameter[0].IsDirectory;
                IDiskPathMapper pathMapper = parameter[0].Profile.PathMapper;
                if (pathMapper is NullDiskPatheMapper)
                    return ResultCommand.Error(new NotSupportedException());
                var mapInfo0 = pathMapper[parameter[0]];
                if (mapInfo0 == null)
                    return ResultCommand.NoError;
                if (!mapInfo0.IsCached)
                    AsyncUtils.RunSync(() => pathMapper.UpdateCacheAsync(parameter[0]));

                string _appliedFileName = mapInfo0.IOPath;
                if (_isFolder || _appliedFileName.StartsWith("::{"))
                {
                    if (_appliedFileName.StartsWith("::{") || Directory.Exists(_appliedFileName))
                        try { Process.Start(_appliedFileName); }
                        catch (Exception ex) { return ResultCommand.Error(ex); }
                }
                else
                {

                    ProcessStartInfo psi = null;
                    if (File.Exists(_appliedFileName))
                        if (_info != null)
                        {
                            if (!_info.Equals(OpenWithInfo.OpenAs))
                                psi = new ProcessStartInfo(OpenWithInfo.GetExecutablePath(_info.OpenCommand), _appliedFileName);
                            else
                            {
                                //http://bytes.com/topic/c-sharp/answers/826842-call-windows-open-dialog
                                psi = new ProcessStartInfo("Rundll32.exe");
                                psi.Arguments = String.Format(" shell32.dll, OpenAs_RunDLL {0}", _appliedFileName);
                            }
                        }
                        else psi = new ProcessStartInfo(_appliedFileName);

                    if (psi != null)
                        try { Process.Start(psi); }
                        catch (Exception ex) { return ResultCommand.Error(ex); }

                }

                return ResultCommand.OK;
            }
            else return ResultCommand.Error(new Exception("Wrong Parameter type or more than one item."));
        }
    }

    public class NotifyChangedCommand : ScriptCommandBase
    {
        
        private string _fullParseName;
        private ChangeType _changeType;
        private IProfile _profile;
        public NotifyChangedCommand(IProfile profile, string fullParseName, ChangeType changeType)
            : base("NotifyChanges")
        {
            _profile = profile;
            _fullParseName = fullParseName;
            _changeType = changeType;
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {            
            _profile.Events.Publish(new EntryChangedEvent(_fullParseName, _changeType));
            return ResultCommand.NoError;
        }

        public override bool Equals(object obj)
        {
            return obj is NotifyChangedCommand && (obj as NotifyChangedCommand)._profile.Equals(_profile) &&
                (obj as NotifyChangedCommand)._fullParseName.Equals(_fullParseName);
        }
    }

    public class FileTransferScriptCommand : ScriptCommandBase
    {
        private IEntryModel _srcModel;
        private IEntryModel _destDirModel;
        private DragDropEffects _transferMode;

        public FileTransferScriptCommand(IEntryModel srcModel, IEntryModel destDirModel,
            DragDropEffects transferMode = DragDropEffects.Copy)
            : base(transferMode.ToString())
        {
            _srcModel = srcModel;
            _destDirModel = destDirModel;
            _transferMode = transferMode;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            try
            {
                var destMapping = _destDirModel.Profile.PathMapper[_destDirModel];
                var srcMapping = _srcModel.Profile.PathMapper[_srcModel];
                string destName = PathFE.GetFileName(srcMapping.IOPath);
                string destFullName = PathFE.Combine(destMapping.IOPath, destName);

                if (_srcModel.IsDirectory)
                {
                    switch (_transferMode)
                    {
                        case DragDropEffects.Move:
                            Directory.Move(srcMapping.IOPath, destFullName); //Move directly.
                            return new NotifyChangedCommand(_destDirModel.Profile, destFullName, ChangeType.Moved);

                        case DragDropEffects.Copy:
                            Directory.CreateDirectory(destFullName);
                            _destDirModel.Profile.Events.Publish(new EntryChangedEvent(destFullName, ChangeType.Created));

                            var destModel = (await _destDirModel.Profile.ListAsync(_destDirModel, em =>
                                    em.FullPath.Equals(destFullName,
                                    StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault();
                            var srcSubModels = (await _srcModel.Profile.ListAsync(_srcModel)).ToList();

                            var resultCommands = srcSubModels.Select(m => 
                                (IScriptCommand)new FileTransferScriptCommand(m, destModel, _transferMode)).ToList();
                            resultCommands.Insert(0, new NotifyChangedCommand(_destDirModel.Profile, destFullName, ChangeType.Created));

                            return new RunInSequenceScriptCommand(resultCommands.ToArray());
                        default:
                            throw new NotImplementedException();
                    }

                }
                else
                {
                    Directory.CreateDirectory(destMapping.IOPath);

                    switch (_transferMode)
                    {
                        case DragDropEffects.Move:
                            if (File.Exists(destFullName))
                                File.Delete(destFullName);
                            File.Move(srcMapping.IOPath, destFullName);

                            return new NotifyChangedCommand(_destDirModel.Profile, destFullName, ChangeType.Moved);
                        case DragDropEffects.Copy:
                            ChangeType ct = ChangeType.Created;
                            if (File.Exists(destFullName))
                            {
                                File.Delete(destFullName);
                                ct = ChangeType.Changed;
                            }
                            File.Copy(srcMapping.IOPath, destFullName);

                            return new NotifyChangedCommand(_destDirModel.Profile, destFullName, ct);
                    }
                }

                return ResultCommand.NoError;
            }
            catch (Exception ex)
            {
                return ResultCommand.Error(ex);
            }
        }
    }
}
