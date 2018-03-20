namespace ComicListener
{
    using System.IO;

    public class PdfComicFile : ComicFile
    {
        public PdfComicFile(FileInfo fileObject) : base(fileObject)
        {
        }

        public new void ProcessSelf()
        {
            return;
        }

        public override void CheckExtenstion()
        {
            this.HasProperExtenstion = (this.Extension == ".pdf") ? true : false;
        }
    }
}
