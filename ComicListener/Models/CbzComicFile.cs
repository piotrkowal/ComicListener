namespace ComicListener.Models
{
    using System.IO;

    public class CbzComicFile : ComicFile
    {
        public CbzComicFile(FileInfo fileObject) : base(fileObject)
        {
        }

        public override void CheckExtenstion()
        {
            this.HasProperExtenstion = (this.Extension == ".cbz") ? true : false;
        }
    }
}
