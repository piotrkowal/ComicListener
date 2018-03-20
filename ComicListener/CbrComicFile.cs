using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComicListener
{
    class CbrComicFile : ComicFile
    {
        public CbrComicFile(FileInfo fileObject) : base(fileObject)
        {
        }

        public void ProcessSelf()
        {
            //throw new NotImplementedException();
            return;
        }

        public override void CheckExtenstion()
        {
            this.HasProperExtenstion = (this.Extension == ".cbr") ? true : false;
        }

    }
}
