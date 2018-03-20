namespace ComicListener
{
    using System.IO;

    public class CbrComicFile : ComicFile
    {
        public CbrComicFile(FileInfo fileObject) : base(fileObject)
        {
        }

        public override void CheckExtenstion()
        {
            this.HasProperExtenstion = (this.Extension == ".cbr") ? true : false;
        }
    }
}
