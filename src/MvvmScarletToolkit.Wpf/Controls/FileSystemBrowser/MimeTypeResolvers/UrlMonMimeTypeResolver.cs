using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed class UrlMonMimeTypeResolver : IMimeTypeResolver
    {
        public string? Get(IFileSystemFile fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return null;
            }

            var byteBuffer = new byte[256];

            using (var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (fileStream.Length >= 256)
                {
                    fileStream.Read(byteBuffer, 0, 256);
                }
                else
                {
                    fileStream.Read(byteBuffer, 0, (int)fileStream.Length);
                }
            }

            FindMimeFromData(0, null, byteBuffer, 256, null, 0, out var MimeTypeNum, 0);

            var mimeTypePtr = new IntPtr(MimeTypeNum);
            var mimeTypeFromFile = Marshal.PtrToStringUni(mimeTypePtr);

            Marshal.FreeCoTaskMem(mimeTypePtr);

            if (string.IsNullOrEmpty(mimeTypeFromFile)
                || mimeTypeFromFile == "text/plain"
                || mimeTypeFromFile == "application/octet-stream")
            {
                return null;
            }

            return mimeTypeFromFile;
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Auto)]
        private static extern uint FindMimeFromData(
            uint pBC,
            [MarshalAs(UnmanagedType.LPStr)] string? pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
            uint cbSize,
            [MarshalAs(UnmanagedType.LPStr)] string? pwzMimeProposed,
            uint dwMimeFlags,
            out uint ppwzMimeOut,
            uint dwReserverd);
    }
}
