using System.IO.Compression;
using System.Text;

namespace DnD_InventoryManager.Utils;

public static class BrotliHelper
{
    public static byte[] CompressToBrotli(string jsonText)
    {
        var bytesToCompress = Encoding.UTF8.GetBytes(jsonText);

        using var outputStream = new MemoryStream();
        using (var brotliStream = new BrotliStream(outputStream, CompressionLevel.Optimal))
        {
            brotliStream.Write(bytesToCompress, 0, bytesToCompress.Length);
        }
        
        return outputStream.ToArray();
    }

    public static string DecompressFromBrotli(byte[] compressedBytes)
    {
        using var inputStream = new MemoryStream(compressedBytes);
        using var brotliStream = new BrotliStream(inputStream, CompressionMode.Decompress);
        
        using var outputStream = new MemoryStream();
        brotliStream.CopyTo(outputStream);
        
        return Encoding.UTF8.GetString(outputStream.ToArray());
    }
}