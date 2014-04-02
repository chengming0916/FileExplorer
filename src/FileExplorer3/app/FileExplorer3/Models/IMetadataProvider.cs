﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface IMetadataProvider
    {
        [Obsolete]
        IEnumerable<IMetadata> GetMetadata
            (IEnumerable<IEntryModel> selectedModels,
            int modelCount, IEntryModel parentModel);

        Task<IEnumerable<IMetadata>> GetMetadataAsync
           (IEnumerable<IEntryModel> selectedModels,
           int modelCount, IEntryModel parentModel);
    }
}
