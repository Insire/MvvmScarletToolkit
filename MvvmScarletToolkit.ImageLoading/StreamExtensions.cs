using System.Security.Cryptography;

namespace MvvmScarletToolkit.ImageLoading
{
    public static class StreamExtensions
    {
        public static async Task<string> CalculateMd5(this Stream data, CancellationToken token = default)
        {
            using (var instance = MD5.Create())
            {
                var hashBytes = await instance.ComputeHashAsync(data, token).ConfigureAwait(false);

                return Convert.ToHexString(hashBytes);
            }
        }
    }
}
