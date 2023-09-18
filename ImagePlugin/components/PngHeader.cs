using System;
using PluginInterface;

namespace PngPlugin.components
{
    public class PngHeader : SuperHeader
    {
        public PngHeader(DataStorage dataStorage) : base("png header", 11, 3)
        {
            string[] hex = dataStorage.FilesHex[0, 1].Split(' ');
            if (hex.Length < 8 || hex[0] != "89" || hex[1] != "50" || hex[2] != "4E" || hex[3] != "47" || hex[4] != "0D" || hex[5] != "0A" || hex[6] != "1A" || hex[7] != "0A")
            {   // This means this file is not a png
                FailedToInitlize = true;
                return;
            }

            StartPoint = 0;

            Desc = new string[RowSize];
            Size = new int[RowSize];
            Size[0] = 8; Desc[0] = "Signature (8 bytes) signature indiciating this file is a PNG.";
            Size[1] = 4; Desc[1] = "Chunk Length (4 bytes) length of the IHDR chunk (this always equals 13 bytes).";
            Size[2] = 4; Desc[2] = "Chunk Type (4 bytes) chunk type this one is \"IHDR\".";
            Size[3] = 4; Desc[3] = "Width (4 bytes) width of this png in pixels (apart of chunk data).";
            Size[4] = 4; Desc[4] = "Height (4 bytes) height of this png in pixels (apart of chunk data).";
            Size[5] = 1; Desc[5] = "Bit Depth (1 byte) number of bits per sample or per palette index (e.g. 8 bits for 256 colors) (apart of chunk data).";
            Size[6] = 1; Desc[6] = "Color Type (1 byte) specifies the color type (e.g. grayscale, RGB, indexed color) (apart of chunk data).";
            Size[7] = 1; Desc[7] = "Compression Method (1 byte) specifies the compresion method (0 means DEFLATE) (apart of chunk data).";
            Size[8] = 1; Desc[8] = "Filter Method (1 byte) specifies the filter method (usually 0 for adaptive filtering) (apart of chunk data).";
            Size[9] = 1; Desc[9] = "Interlace Method: (1 byte) Specifies the interlace method (0 for no interlace, 1 for Adam7 interlace) (apart of chunk data).";
            Size[10] = 4; Desc[10] = "Checksum (4 bytes) this CRC-32 checksum calcualtes over the above chunk type, and chunk data.";

            SetEndPoint();
        }

        public override void OpenForm(int row, DataStorage dataStorage)
        {
            return; // No custom forms required here
        }
    }
}
