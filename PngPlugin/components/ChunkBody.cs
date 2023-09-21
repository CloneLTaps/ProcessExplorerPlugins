using System;
using PluginInterface;

namespace PngPlugin.components
{
    public class ChunkBody : SuperHeader
    {
        public ChunkBody(int startingPoint, int endPoint, string sectionType) : base("section body", (int)Math.Ceiling((endPoint - startingPoint) / 16.0), 3)
        {
            Component = sectionType;
            StartPoint = startingPoint;
            EndPoint = endPoint;

            Size = null;
            Desc = null;
            HasSetEndPoint = true;
        }

        public override void OpenForm(int row, DataStorage dataStorage)
        {
            return; // No custom forms required here
        }
    }
}
