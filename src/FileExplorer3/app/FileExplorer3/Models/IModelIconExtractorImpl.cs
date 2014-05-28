using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileExplorer.Utils;

namespace FileExplorer.Models
{
    public class ModelIconExtractor<T> : IModelIconExtractor<T>
    {
        Func<T, Task<byte[]>> _retFunc = null;

        public static ModelIconExtractor<T> FromTaskFunc(Func<T, Task<byte[]>> taskFunc)
        {
            return new ModelIconExtractor<T>() { _retFunc = taskFunc };
        }

        public static ModelIconExtractor<T> FromTaskFunc(Func<Task<byte[]>> taskFunc)
        {
            return FromTaskFunc(t => taskFunc());
        }

        public static ModelIconExtractor<T> FromBytes(byte[] bytes)
        {
            return FromTaskFunc(t => Task<byte[]>.FromResult(bytes));
        }

        public static ModelIconExtractor<T> FromStream(Stream stream)
        {
            return FromBytes(stream.ToByteArray());
        }

        public Task<byte[]> GetIconBytesForModelAsync(T model, CancellationToken ct)
        {
            if (_retFunc != null)
                return _retFunc(model);
            return Task<byte[]>.FromResult(new byte[] {});
        }
    }
}
