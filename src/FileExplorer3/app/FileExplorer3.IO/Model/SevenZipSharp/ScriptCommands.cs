using FileExplorer.IO;
using FileExplorer.Script;
using FileExplorer.WPF.Defines;
using FileExplorer.WPF.Models;
using FileExplorer.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models.SevenZipSharp
{
    

    /// <summary>
    /// Uses SevenZipWrapper.CompressMultiple thus quicker then FileTransferScriptCommand which move one file at a time.    
    /// </summary>
    public class BatchTransferScriptCommand : ScriptCommandBase
    {
        private IEntryModel _srcModel;
        private IEntryModel _destDirModel;
        private bool _removeOriginal;

        /// <summary>
        /// DestDirModel must be ISzsItemModel
        /// </summary>
        /// <param name="srcModel"></param>
        /// <param name="destDirModel"></param>
        /// <param name="removeOriginal"></param>
        public BatchTransferScriptCommand(IEntryModel srcModel, IEntryModel destDirModel,
            bool removeOriginal = false)
            : base(removeOriginal ? "Move" : "Copy")
        {
            _srcModel = srcModel;
            _destDirModel = destDirModel;
            _removeOriginal = removeOriginal;

            if (!(srcModel.Profile is IDiskProfile) || !(destDirModel.Profile is SzsProfile))
                throw new NotSupportedException();
        }


        private async Task<IScriptCommand> transferAsync(ParameterDic pm, IProgress<TransferProgress> progress, IScriptCommand thenCommand)
        {
            List<IEntryModel> retList = (List<IEntryModel>)pm["Result"];
            Dictionary<string, Stream> compressDic = new Dictionary<string,Stream>();
            IDiskProfile srcProfile = _srcModel.Profile as IDiskProfile;
            string srcParentPath = srcProfile.Path.GetDirectoryName(_srcModel.FullPath);

            foreach (var em in retList)
            {
                string relativePath = em.FullPath.Replace(srcParentPath, "").TrimStart('\\');
                compressDic.Add(relativePath, await srcProfile.DiskIO.OpenStreamAsync(em, Defines.FileAccess.Read, pm.CancellationToken));
            }

            var destProfile = _destDirModel.Profile as SzsProfile;
            string archiveType = destProfile.Path.GetExtension(_destDirModel.Name);
            
            using (await destProfile.WorkingLock.LockAsync())
            using (var stream = await destProfile.DiskIO.OpenStreamAsync(_destDirModel, Defines.FileAccess.ReadWrite, pm.CancellationToken))
                destProfile.Wrapper.CompressMultiple(archiveType, stream, compressDic, null);

            return thenCommand;
        }

        public override async Task<IScriptCommand> ExecuteAsync(ParameterDic pm)
        {
            try
            {
                var srcProfile = _srcModel.Profile as IDiskProfile;
                var destProfile = _destDirModel.Profile as IDiskProfile;
                var progress = pm.ContainsKey("Progress") ? pm["Progress"] as IProgress<TransferProgress> : NullTransferProgress.Instance;

                var destMapping = (_destDirModel.Profile as IDiskProfile).DiskIO.Mapper[_destDirModel];
                var srcMapping = (_srcModel.Profile as IDiskProfile).DiskIO.Mapper[_srcModel];
                string destName = PathFE.GetFileName(srcMapping.IOPath);
                string destFullName = destProfile.Path.Combine(_destDirModel.FullPath, destName); //PathFE.Combine(destMapping.IOPath, destName);



                if (_srcModel.IsDirectory)
                {
                    pm["Directory"] = _srcModel;
                    pm["Recrusive"] = true;
                    return ScriptCommands.ListFile(
                        new SimpleScriptCommandAsync("BatchTransfer", pd => transferAsync(pm, progress,                            
                             new NotifyChangedCommand(_destDirModel.Profile, destFullName,
                                _srcModel.Profile, _srcModel.FullPath, Defines.ChangeType.Changed) )));                    
                }
                else
                    return new StreamFileTransferCommand(_srcModel, _destDirModel, _removeOriginal, progress);


                return ResultCommand.NoError;
            }
            catch (Exception ex)
            {
                return ResultCommand.Error(ex);
            }
        }
    }
}
