using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Tools;
using System.Linq;
using System.Text;
using System.Threading;
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
    public static class FileBasedScriptCommandsHelper
    {
        /// <summary>
        /// Return pd["Parameter"] as IEntryModel[]
        /// </summary>
        /// <param name="pd"></param>
        /// <returns></returns>
        public static IEntryModel[] GetEntryModelFromParameter(ParameterDic pd)
        {
            return pd["Parameter"] as IEntryModel[];
        }

        public static IEntryModel GetFirstEntryModelFromParameter(ParameterDic pd)
        {
            return (pd["Parameter"] as IEntryModel[]).FirstOrDefault();
        }
    }


    /// <summary>
    /// These Commands require IDIskProfile.
    /// </summary>    
    public class OpenWithScriptCommand : ScriptCommandBase
    {
        private Func<ParameterDic, IEntryModel[]> _srcModelFunc;     
        OpenWithInfo _info;

        /// <summary>
        /// Launch a file (e.g. txt) using the OpenWithInfo, if null then default method will be used, Require Parameter (IEntryModel[])
        /// </summary>
        /// <param name="info"></param>
        public OpenWithScriptCommand(OpenWithInfo info = null, Func<ParameterDic, IEntryModel[]> srcModelFunc = null)
            : base("OpenWith")
        {
            _info = info;
            _srcModelFunc = srcModelFunc ?? FileBasedScriptCommandsHelper.GetEntryModelFromParameter;
        }

        public override bool CanExecute(ParameterDic pm)
        {
            return base.CanExecute(pm);
        }

        public override IScriptCommand Execute(ParameterDic pm)
        {
            var parameter = _srcModelFunc(pm);
            if (parameter != null && parameter.Count() == 1)
            {
                bool _isFolder = parameter[0].IsDirectory;
                IDiskProfile profile = parameter[0].Profile as IDiskProfile;
                if (profile == null)
                    return ResultCommand.Error(new NotSupportedException("IDiskProfile"));

                if (profile.DiskIO.Mapper is NullDiskPatheMapper)
                    return ResultCommand.Error(new NotSupportedException());

                string appliedFileName = AsyncUtils.RunSync(() => profile.DiskIO.WriteToCacheAsync(parameter[0]));

                if (_isFolder || appliedFileName.StartsWith("::{"))
                {
                    if (appliedFileName.StartsWith("::{") || Directory.Exists(appliedFileName))
                        try { Process.Start(appliedFileName); }
                        catch (Exception ex) { return ResultCommand.Error(ex); }
                }
                else
                {

                    ProcessStartInfo psi = null;
                    if (File.Exists(appliedFileName))
                        if (_info != null)
                        {
                            if (!_info.Equals(OpenWithInfo.OpenAs))
                                psi = new ProcessStartInfo(OpenWithInfo.GetExecutablePath(_info.OpenCommand), appliedFileName);
                            else
                            {
                                //http://bytes.com/topic/c-sharp/answers/826842-call-windows-open-dialog
                                psi = new ProcessStartInfo("Rundll32.exe");
                                psi.Arguments = String.Format(" shell32.dll, OpenAs_RunDLL {0}", appliedFileName);
                            }
                        }
                        else psi = new ProcessStartInfo(appliedFileName);

                    if (psi != null)
                        try { Process.Start(psi); }
                        catch (Exception ex) { return ResultCommand.Error(ex); }

                }

                return ResultCommand.OK;
            }
            else return ResultCommand.Error(new Exception("Wrong Parameter type or more than one item."));
        }
    }

    

    public class DeleteFileBasedEntryCommand : ScriptCommandBase
    {
        public static DeleteFileBasedEntryCommand FromParameter = new DeleteFileBasedEntryCommand(FileBasedScriptCommandsHelper.GetEntryModelFromParameter);

        private Func<ParameterDic, IEntryModel[]> _srcModelFunc;        
        private string _path;
        public DeleteFileBasedEntryCommand(Func<ParameterDic, IEntryModel[]> srcModelFunc)
            : base("Delete")
        {
            _srcModelFunc = srcModelFunc;
        }
        

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var _srcModels = _srcModelFunc(pm);

            if (_srcModels != null)
            {
                Task[] tasks = _srcModels.Select(m => 
                    (m.Profile as IDiskProfile).DiskIO.DeleteAsync(m)                    
                    ).ToArray();
                await Task.WhenAll(tasks);

                return new RunInSequenceScriptCommand(
                    _srcModels.Select(m => new NotifyChangedCommand(m.Profile, m.FullPath, ChangeType.Deleted))
                    .ToArray());
            }

            return ResultCommand.NoError;
        }
    }




    #region FileTransferScriptCommand and it's subcommands

    public class StreamFileTransferCommand : ScriptCommandBase
    {
        private IEntryModel _srcModel;
        private IEntryModel _destDirModel;
        private bool _removeOriginal;

        public StreamFileTransferCommand(IEntryModel srcModel, IEntryModel destDirModel, bool removeOriginal)
            : base("StreamFileTransfer")
        {
            if (srcModel.Profile is IDiskProfile && destDirModel.Profile is IDiskProfile)
            {
                _srcModel = srcModel;
                _destDirModel = destDirModel;
                _removeOriginal = removeOriginal;
            }
            else throw new ArgumentException("Transfer work with IDiskProfile only.");
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var srcProfile = _srcModel.Profile as IDiskProfile;
            var destProfile = _destDirModel.Profile as IDiskProfile;
            string destName = _srcModel.GetName();
            string destFullName = _destDirModel.Combine(destName);

            ChangeType ct = ChangeType.Created;
            if (File.Exists(destFullName))
            {
                File.Delete(destFullName);
                ct = ChangeType.Changed;
            }

            using (var srcStream = await srcProfile.DiskIO.OpenStreamAsync(_srcModel.FullPath, FileAccess.Read))
            using (var destStream = await destProfile.DiskIO.OpenStreamAsync(destFullName, FileAccess.Write))
                await StreamUtils.CopyStreamAsync(srcStream, destStream);

            if (_removeOriginal)
                await srcProfile.DiskIO.DeleteAsync(_srcModel);

            return new NotifyChangedCommand(_destDirModel.Profile, destFullName, ct);
        }

    }
   
    public class CopyDirectoryTransferCommand : ScriptCommandBase
    {
        private IEntryModel _srcModel;
        private IEntryModel _destDirModel;
        private bool _removeOriginal;
        public CopyDirectoryTransferCommand(IEntryModel srcModel, IEntryModel destDirModel, bool removeOriginal)
            : base("CopyDirectoryTransfer")
        {
            if (srcModel.Profile is IDiskProfile && destDirModel.Profile is IDiskProfile)
            {
                _srcModel = srcModel;
                _destDirModel = destDirModel;
                _removeOriginal = removeOriginal;
            }
            else throw new ArgumentException("Transfer work with IDiskProfile only.");
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            var destProfile = _destDirModel.Profile as IDiskProfile;

            var destMapping = (_destDirModel.Profile as IDiskProfile).DiskIO.Mapper[_destDirModel];
            var srcMapping = (_srcModel.Profile as IDiskProfile).DiskIO.Mapper[_srcModel];
            string destName = PathFE.GetFileName(srcMapping.IOPath);
            string destFullName = destProfile.Path.Combine(_destDirModel.FullPath, destName); //PathFE.Combine(destMapping.IOPath, destName);

            IEntryModel destModel = await _destDirModel.Profile.ParseAsync(destFullName);

            if (destModel == null)
            {
                destModel = await destProfile.DiskIO.CreateAsync(destFullName, true);

                destModel = (await _destDirModel.Profile.ListAsync(_destDirModel, CancellationToken.None, em =>
                        em.FullPath.Equals(destFullName,
                        StringComparison.CurrentCultureIgnoreCase), true)).FirstOrDefault();
                _destDirModel.Profile.Events.Publish(new EntryChangedEvent(destFullName, ChangeType.Created));


            }

            if (destModel == null)
                return ResultCommand.Error(new Exception("Cannot construct destination " + destFullName));

            var srcSubModels = (await _srcModel.Profile.ListAsync(_srcModel, CancellationToken.None)).ToList();

            var resultCommands = srcSubModels.Select(m =>
                (IScriptCommand)new FileTransferScriptCommand(m, destModel, _removeOriginal)).ToList();
            resultCommands.Insert(0, new NotifyChangedCommand(_destDirModel.Profile, destFullName, ChangeType.Created));

            //if (_removeOriginal)
            //    resultCommands.Add(new DeleteEntryCommand(_srcModel));

            return new RunInSequenceScriptCommand(resultCommands.ToArray());
        }
    }

    public class FileTransferScriptCommand : ScriptCommandBase
    {
        private IEntryModel _srcModel;
        private IEntryModel _destDirModel;
        private bool _removeOriginal;

        public FileTransferScriptCommand(IEntryModel srcModel, IEntryModel destDirModel,
            bool removeOriginal = false)
            : base(removeOriginal ? "Move" : "Copy")
        {
            _srcModel = srcModel;
            _destDirModel = destDirModel;
            _removeOriginal = removeOriginal;

            if (!(srcModel.Profile is IDiskProfile) || !(destDirModel.Profile is IDiskProfile))
                throw new NotSupportedException();
        }




        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            try
            {
                var srcProfile = _srcModel.Profile as IDiskProfile;
                var destProfile = _destDirModel.Profile as IDiskProfile;

                var destMapping = (_destDirModel.Profile as IDiskProfile).DiskIO.Mapper[_destDirModel];
                var srcMapping = (_srcModel.Profile as IDiskProfile).DiskIO.Mapper[_srcModel];
                string destName = PathFE.GetFileName(srcMapping.IOPath);
                string destFullName = destProfile.Path.Combine(_destDirModel.FullPath, destName); //PathFE.Combine(destMapping.IOPath, destName);


                if (!srcMapping.IsVirtual && !destMapping.IsVirtual && _removeOriginal)
                {
                    if (_srcModel.IsDirectory)
                    {
                        if (Directory.Exists(destFullName))
                            Directory.Delete(destFullName, true);
                        Directory.Move(srcMapping.IOPath, destFullName); //Move directly.
                    }
                    else
                    {
                        if (File.Exists(destFullName))
                            File.Delete(destFullName);
                        File.Move(srcMapping.IOPath, destFullName);
                    }
                    return new NotifyChangedCommand(_destDirModel.Profile, destFullName, ChangeType.Moved);
                }
                else
                {
                    if (_srcModel.IsDirectory)
                        return new CopyDirectoryTransferCommand(_srcModel, _destDirModel, _removeOriginal);
                    else
                        return new StreamFileTransferCommand(_srcModel, _destDirModel, _removeOriginal);
                }



                //switch (_transferMode)
                //{
                //    case DragDropEffects.Move:
                //      
                //       

                //    case DragDropEffects.Copy:
                //        Directory.CreateDirectory(destFullName);
                //        _destDirModel.Profile.Events.Publish(new EntryChangedEvent(destFullName, ChangeType.Created));

                //        var destModel = (await _destDirModel.Profile.ListAsync(_destDirModel, em =>
                //                em.FullPath.Equals(destFullName,
                //                StringComparison.CurrentCultureIgnoreCase))).FirstOrDefault();
                //        var srcSubModels = (await _srcModel.Profile.ListAsync(_srcModel)).ToList();

                //        var resultCommands = srcSubModels.Select(m => 
                //            (IScriptCommand)new FileTransferScriptCommand(m, destModel, _transferMode)).ToList();
                //        resultCommands.Insert(0, new NotifyChangedCommand(_destDirModel.Profile, destFullName, ChangeType.Created));

                //        return new RunInSequenceScriptCommand(resultCommands.ToArray());
                //    default:
                //        throw new NotImplementedException();
                //}

                //}
                //else
                //{

                //}

                return ResultCommand.NoError;
            }
            catch (Exception ex)
            {
                return ResultCommand.Error(ex);
            }
        }
    }

    #endregion
}
