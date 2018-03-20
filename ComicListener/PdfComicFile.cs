using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComicListener
{
    class PdfComicFile : ComicFile
    {
        public PdfComicFile(FileInfo fileObject) : base(fileObject)
        {
        }

        public void ProcessSelf()
        {
            //throw new NotImplementedException();
            return;
        }

        public override void CheckExtenstion()
        {
            this.HasProperExtenstion = (this.Extension == ".pdf") ? true : false;
        }
    }
}
