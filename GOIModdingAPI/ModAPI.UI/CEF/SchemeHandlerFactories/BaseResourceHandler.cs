using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading;
using Xilium.CefGlue;

namespace ModAPI.UI.CEF.SchemeHandlerFactories
{
    internal class BaseResourceHandler : CefResourceHandler
    {
        protected readonly string RootPath;
        
        private FileStream fileStream;
        private IAsyncResult currentReadState;
        private string mimeType;
        
        public BaseResourceHandler(string rootPath)
        {
            RootPath = rootPath;
        }
        
        protected override bool Open(CefRequest request, out bool handleRequest, CefCallback callback)
        {
            handleRequest = true;
            
            // Only accept GET method
            if (request.Method != "GET")
            {
                handleRequest = false;
                return true;
            }

            string filePath = GetFilePath(request.Url);
            
            if (filePath == null)
                return true;

            mimeType = CefRuntime.GetMimeType(Path.GetExtension(filePath));
            fileStream = File.OpenRead(filePath);
            return true;
        }

        protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
        {
            if (fileStream == null || !fileStream.CanRead)
            {
                response.Status = 404;
                response.StatusText = "Not Found";
                responseLength = 0;
                redirectUrl = null;
                return;
            }

            response.MimeType = mimeType;
            response.Status = 200;
            response.StatusText = "OK";
            responseLength = fileStream.Length;
            redirectUrl = null;
        }

        protected override bool Skip(long bytesToSkip, out long bytesSkipped, CefResourceSkipCallback callback)
        {
            bytesToSkip = Math.Min((int) (fileStream.Length - fileStream.Position), bytesToSkip);
            
            long oldPosition = fileStream.Position;
            long newPosition = fileStream.Seek(bytesToSkip, SeekOrigin.Current);

            bytesSkipped = newPosition - oldPosition;
            return true;
        }

        protected override bool Read(IntPtr dataOut, int bytesToRead, out int bytesRead, CefResourceReadCallback callback)
        {
            if (fileStream.Position == fileStream.Length)
            {
                // EOF
                bytesRead = 0;
                return false;
            }
            
            bytesToRead = Math.Min((int) (fileStream.Length - fileStream.Position), bytesToRead);
            
            byte[] buffer = new byte[bytesToRead];
            currentReadState = fileStream.BeginRead(buffer, 0, bytesToRead, result =>
            {
                Marshal.Copy(buffer, 0, dataOut, buffer.Length);
                callback.Continue(buffer.Length);
            }, null);

            bytesRead = 0;
            return true;
        }

        protected override void Cancel()
        {
            if (currentReadState != null && !currentReadState.IsCompleted)
                fileStream.EndRead(currentReadState);
            
            fileStream?.Close();
        }

        protected virtual string GetFilePath(string urlPath)
        {
            var uri = new Uri(urlPath);
            string filePath = uri.AbsolutePath;

            if (filePath.StartsWith("/"))
            {
                filePath = filePath.Substring(1);
            }
            
            filePath = Path.Combine(GetRootDirectory(uri), filePath);

            if (!File.Exists(filePath))
            {
                if (Directory.Exists(filePath))
                {
                    filePath = Path.Combine(filePath, "index.html");

                    if (!File.Exists(filePath))
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return filePath;
        }

        protected virtual string GetRootDirectory(Uri uri)
        {
            return RootPath;
        }
    }
}