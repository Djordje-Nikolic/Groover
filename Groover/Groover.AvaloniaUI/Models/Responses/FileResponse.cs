using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.AvaloniaUI.Models.Responses
{
    public class FileResponse : BaseResponse, IDisposable, IAsyncDisposable
    {
        private bool disposedValue;

        public string FilePath { get; set; }
        public FileStream Stream { get; set; }

        public FileStream? OpenStream()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                return null;

            if (Stream != null)
                Stream.Dispose();

            Stream = File.OpenRead(FilePath);

            return Stream;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    if (Stream != null)
                        Stream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~StreamResponse()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            return ((IAsyncDisposable)Stream).DisposeAsync();
        }
    }
}
