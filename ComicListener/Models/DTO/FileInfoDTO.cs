using System.IO;

namespace ComicListener.Models.DTO
{
    public class FileInfoDTO
    {
        public string Extension { get; set; }
        public string FullPath { get; set; }
        public FileInfoDTO(FileInfo fileInfo)
        {
            this.Extension = fileInfo.Extension;
            this.FullPath = fileInfo.Directory.ToString();
        }

    }
}