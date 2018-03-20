namespace ComicListener
{
    using System.IO;

    public abstract class ComicFile : IComicFile
    {
        public ComicFile(FileInfo fileObject)
        {
            this.Extension = fileObject.Extension;
            
            // path to file directory
            this.PathToDirectory = fileObject.Directory.ToString();
            this.FileName = fileObject.Name;
            var tempDirectoryName = this.FileName.Replace(fileObject.Extension, string.Empty);
            tempDirectoryName = string.Join(string.Empty, tempDirectoryName);

            // name of directory to extract archieve's content
            this.ExtractDirectoryName = tempDirectoryName;
            
            // path to extract content of archieve
            this.ExtractDirectoryPath = this.PathToDirectory + "\\tmp\\" + tempDirectoryName.Trim();
            this.CheckExtenstion();
        }

        public FileInfo FileInfoObj { get; set; }

        public string Extension { get; set; }

        public bool HasProperExtenstion { get; set; }

        public string FileName { get; set; }

        public string PathToDirectory { get; set; }

        public string ExtractDirectoryPath { get; set; }

        public string ExtractDirectoryName { get; set; }

        // convert self to zip
        public void ProcessSelf()
        {
            ArchivesConverter.ConvertArchieveToZip(this);
            return;
        }

        public abstract void CheckExtenstion();
    }
}
