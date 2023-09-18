using System.Collections.Generic;
using PngPlugin.components;
using PluginInterface;
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

            PngHeader header = new PngHeader(data);
            if (header.FailedToInitlize) return false;

            MyTreeNode headerNode = new MyTreeNode("png header");
            MyTreeNode.Children.Add(headerNode);

            componentMap.Add(header.Component, header);


            return true;
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
