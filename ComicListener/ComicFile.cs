namespace ComicListener
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    abstract class ComicFile : IComicFile
    {
        public FileInfo FileInfoObj { get; set; }
        public string Extension { get; set; }
        public bool HasProperExtenstion { get; set; }
        public string FileName { get; set; }
        public string PathToDirectory { get; set; }
        public string ExtractDirectoryPath { get; set; }
        public string ExtractDirectoryName { get; set; }
        public ComicFile(FileInfo fileObject)
        {
            this.Extension = fileObject.Extension;
            this.PathToDirectory = fileObject.Directory.ToString();
            this.FileName = fileObject.Name;
            var tempDirectoryName = this.FileName.Replace(fileObject.Extension, "");
            tempDirectoryName = string.Join("", tempDirectoryName);
            this.ExtractDirectoryName = tempDirectoryName;
            this.ExtractDirectoryPath = this.PathToDirectory + "\\tmp\\" + tempDirectoryName.Trim();
            CheckExtenstion();
        }

        public void ProcessSelf()
        {
            ArchivesConverter.ConvertArchieveToZip(this);
            return;
        }
        public abstract void CheckExtenstion();
    }
}
