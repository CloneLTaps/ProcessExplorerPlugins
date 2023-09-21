using System.Collections.Generic;
using PngPlugin.components;
using PluginInterface;
using System.Linq;
using System;

namespace PngPlugin
{
    public class PngPluginMain : IPlugin
    {
        public MyTreeNode MyTreeNode { get; private set; }

        private readonly Dictionary<string, SuperHeader> componentMap = new Dictionary<string, SuperHeader>();

        public PngPluginMain()
        {
            Name = "Png";
        }

        public string Name { get; }

        public Dictionary<string, SuperHeader> RecieveCompnents()
        {
            return componentMap;
        }

        public bool Initialized(DataStorage data)
        {
            MyTreeNode = new MyTreeNode(data.FileName); // This sets the main node equal to the FileName which is standard

            PngHeader pngHeader = new PngHeader(data);
            if (pngHeader.FailedToInitlize) return false;

            MyTreeNode headerNode = new MyTreeNode("Png Header");
            MyTreeNode.Children.Add(headerNode);

            componentMap.Add(pngHeader.Component, pngHeader);
            AssignSectionHeaders(pngHeader.EndPoint, 0, data);

            // Assign the chunk headers and data
            MyTreeNode mainChunkNode = new MyTreeNode("Chunks");
            foreach (var map in componentMap)
            {
                SuperHeader header = map.Value;
                string compString = header.Component.ToString();
                if (!compString.Contains("chunk header")) continue;

                MyTreeNode chunkNode = new MyTreeNode("");
                chunkNode.Data = header.Component;

                foreach (var innerMap in componentMap)
                {
                    SuperHeader body = innerMap.Value;
                    string newCompString = body.Component.ToString();

                    if (!newCompString.Contains("chunk body") || compString.Replace("chunk header", "") != newCompString.Replace("chunk body", "")) continue;

                    MyTreeNode sectionBodyNode = new MyTreeNode("")
                    {
                        Data = body.Component
                    };
                    chunkNode.Children.Add(sectionBodyNode);
                    break;
                }
                mainChunkNode.Children.Add(chunkNode); // Add the main section node for each section to our parent node
            }

            if (mainChunkNode.Children.Count > 0) MyTreeNode.Children.Add(mainChunkNode);

            return true;
        }

        private void AssignSectionHeaders(int startPoint, int sectionCount, DataStorage dataStorage)
        {
            int initialSkipAmount = startPoint % 16; // The amount we need to skip before we reach our target byte 
            int startingIndex = startPoint <= 0 ? 0 : (int)Math.Floor(startPoint / 16.0);

            string ascii = ""; // Section name
            int headerNameCount = 0;
            for (int row = startingIndex; row < dataStorage.FilesHex.GetLength(0); row++) // Loop through the rows
            {
                string[] hexBytes = dataStorage.FilesHex[row, 1].Split(' ');

                for (int j = initialSkipAmount; j < hexBytes.Length; j++) // Loop through each rows bytes
                {
                    if (sectionCount > 20) return;

                    if (byte.TryParse(hexBytes[j], System.Globalization.NumberStyles.HexNumber, null, out byte b))
                    {
                        char asciiChar = b > 32 && b <= 126 ? (char)b : ' '; // Space is normally 32 in decimal
                        int currentOffset = (row * 16) + j + 1;

                        ascii += asciiChar;

                        if (headerNameCount == 0)
                        {   // This means we are possibly at the start of a new section header

                            if (asciiChar == ' ' || !ChunkHeader.IsValidChunkType(ascii.Select(c => c.ToString()).ToArray(), 0))
                            {   // This means the first character does not match a valid 
                                ascii = "";
                                continue;
                            }

                            ++headerNameCount;
                        }
                        else if (++headerNameCount < 4) // The Signature field in chunk headers are 8 bytes long but only the first 4 bytes must contain valid ASCII
                        {
                            if (asciiChar == ' ')
                            {   // If the first few characters are not valid ASCII then we know its not a section header
                                headerNameCount = 0;
                                ascii = "";
                                continue;
                            }
                        }
                        else
                        {   // This means we must be at the 4th byte which always should be null terminating if its a valid section header name
                            if(!ChunkHeader.IsValidChunkType(ascii.Select(c => c.ToString()).ToArray(), -1))
                            {
                                headerNameCount = 0;
                                ascii = "";
                                continue;
                            }

                            string udpatedAscii = ascii.Replace(" ", "").ToLower(); 
                            string sectionType = udpatedAscii + " chunk header";
                            string sectionBodyType = udpatedAscii + " chunk body";

                            ChunkHeader header = new ChunkHeader(dataStorage, currentOffset - 8, sectionType);
                            ChunkBody body = new ChunkBody(header.bodyStartPoint, header.bodyEndPoint, sectionBodyType);

                            if (componentMap.ContainsKey(sectionType)) componentMap[sectionType] = header;
                            else componentMap.Add(sectionType, header);

                            if (componentMap.ContainsKey(sectionBodyType)) componentMap[sectionBodyType] = body;
                            else componentMap.Add(sectionBodyType, body);

                            if (!header.End) AssignSectionHeaders(header.EndPoint, ++sectionCount, dataStorage);
                            return;
                        }
                    }
                }
                initialSkipAmount = 0;
            }
            return;
        }

        public MyTreeNode RecieveTreeNodes()
        {
            return MyTreeNode;
        }

        public void Cleanup()
        {
            componentMap.Clear();
            MyTreeNode = null;
        }
    }
}
