namespace ComicListener
{
    public class Container
    {
        public Container(string name)
        {
            this.FileName = name;
        }

        public string Group { get; set; }

        public string FileName { get; set; }

        public string OriginalName { get; set; }

        public string OriginalSize { get; set; }

        public string OriginalFormat { get; set; }

    }
}
