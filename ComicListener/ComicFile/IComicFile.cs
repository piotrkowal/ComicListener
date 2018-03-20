namespace ComicListener
{
    public interface IComicFile
    {
        string FileName { get; set; }

        string PathToDirectory { get; set; }

        string Extension { get; set; }

        string ExtractDirectoryName { get; set; }

        bool HasProperExtenstion { get; set; }
    }
}
