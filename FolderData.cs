using System.Collections.Generic;

namespace DesktopFolder
{
    public class FolderData
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> ItemPaths { get; set; } = new List<string>();
    }
}