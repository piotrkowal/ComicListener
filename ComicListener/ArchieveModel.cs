namespace ComicListener
{
    using System.Collections.Generic;
    // <summary>
    // Class for Archieve Model
    // </summary>
    class ArchieveModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public List<int> Files { get; set; }
        public int Size { get; set; }
    }
}
