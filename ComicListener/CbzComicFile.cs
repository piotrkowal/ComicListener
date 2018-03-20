using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComicListener
{
    class CbzComicFile : ComicFile
    {
        public CbzComicFile(FileInfo fileObject) : base(fileObject)
        {
        }

        public void ProcessSelf()
        {
            //throw new NotImplementedException();
            //ArchivesConverter.RecreateZip(file.Directory.ToString(), file.Name, dirName)
            //ArchivesConverter.RecreateZip(this.PathToDirectory, this.FileName, this.ExtractDirectoryName);
            ArchivesConverter.ConvertArchieveToZip(this);
            return;
        }

        public override void CheckExtenstion()
        {
            this.HasProperExtenstion = (this.Extension == ".cbz") ? true : false;
        }
    }
}
