using System;
using System.Text;
using System.Linq;
using PluginInterface;
using System.Collections.Generic;

namespace PngPlugin.components
{
    public class ChunkHeader : SuperHeader
    {
        public bool End { get; private set; }

        public int bodyStartPoint { get; private set; }

        public int bodyEndPoint { get; private set; }

        //https://www.nayuki.io/page/png-file-chunk-inspector#:~:text=A%20PNG%20file%20is%20composed,depend%20on%20the%20chunk%20type.
        public ChunkHeader(DataStorage dataStorage, int startPoint, string signatureType) : base("chunk header", 4, 3)
        {
            FileFormatedLittleEndian = false;
            StartPoint = startPoint;
            Component = signatureType;

            SkipList.Add(2);

            Desc = new string[RowSize];
            Size = new int[RowSize];
            Size[0] = 4; Desc[0] = "Chunk Length (4 bytes) length of the chunk.";
            Size[1] = 4; Desc[1] = "Chunk Type (4 bytes) chunk type.";
            Size[2] = 0; Desc[2] = "Chunk Data (see chunk body for this)."; 
            Size[3] = 4; Desc[3] = "Checksum (4 bytes) this CRC-32 checksum calcualtes over the above chunk type, and chunk data.";

            string[] typeHex = GetData(3, 1, Enums.DataType.HEX, false, true, dataStorage).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int length = int.Parse(GetData(0, 1, Enums.DataType.DECIMAL, true, true, dataStorage));
            Size[2] = length; // Update the size of the data chunk with its length

            string type = HexToAscii(typeHex);

            if (type == "IEND") End = true;
            else End = false;

            SetEndPoint();

            bodyStartPoint = startPoint + 8;
            bodyEndPoint = bodyStartPoint + length;
            HasSetEndPoint = true;
        }

        public override void OpenForm(int row, DataStorage dataStorage)
        {
            return; // No custom forms required here
        }

        public static string HexToAscii(string[] hexArray)
        {
            byte[] byteArray = new byte[hexArray.Length];
            for (int i = 0; i < hexArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(hexArray[i], 16);
            }

            return Encoding.ASCII.GetString(byteArray);
        }

        public static bool IsValidChunkType(string[] asciiChunkType, int targetIndex)
        {
            //Console.WriteLine("IsValidChunkType Length:" + asciiChunkType.Length + " str:" + string.Join(" ", asciiChunkType));
            if (asciiChunkType.Length != 4 && asciiChunkType.Length != 1) return false; // The type is always 4 bytes but 1 byte can be used as a check
            
            List<string> validChunkTypes = new List<string> { // List of valid PNG chunk types 
                 "IHDR", "PLTE", "IDAT", "IEND",
                 "cHRM", "gAMA", "iCCP", "sBIT", "sRGB",
                 "bKGD", "hIST", "tRNS", "pHYs", "sPLT",
                 "tIME", "tEXt", "zTXt", "iTXt"
            };

            bool found = false;
            for(int i=0; i< asciiChunkType.Length; i++)
            {
                string s = asciiChunkType[i];

                //Console.WriteLine("s:" + s + " i:" + i + " HexLength:" + asciiChunkType.Length);

                for (int j = validChunkTypes.Count - 1; j >= 0; j--)
                {
                    string[] strArray = validChunkTypes[j].Select(c => c.ToString()).ToArray();

                   /* Console.WriteLine("i:" + i + " j:" + j + " CurrentStr:" + s + " StrArrayIndex:" + strArray[i] + " ListsStr:" + validChunkTypes[j]
                        + " StrArrayLength:" + strArray.Length + " TargetIndex:" + targetIndex);*/

                    if (asciiChunkType.Length > strArray.Length) return false;

                    if (strArray[i] == s)
                    {
                        if (targetIndex == i) return true; // This is called inside the PngPluginMain class
                        found = true;
                        break;
                    }
                    else validChunkTypes.RemoveAt(j);
                }

                //Console.WriteLine("Found:" + found + " Count:" + validChunkTypes.Count + " s:" + s);
                if (found == false) return false;
            }
            return found;
        }
    }
}
