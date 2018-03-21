# ComicListener


When I started creating my comic books library, i had two main problems:
 - CBR (comic book rar archieve) format, rar is proprietary archieve file format, so i don't want to use it
 - many ads in comic book archieves
 
 

ComicListener is a simple tool, that will convert all your comic book files to .cbz (comic book zip archieve) format, and will store ads files to ./ads folder.

It's currently working only on Windows. 
To use it, you should compile it with Visual Studio, at least 2015 community version.
Usage of program:
```
dotnet ComicListener.dll C:\path\to\your\comic\files
```

TODO:
- PDF image unpacking
- port to linux
- grouping files in folders (in progress)
- finding duplicates before and after processing files

