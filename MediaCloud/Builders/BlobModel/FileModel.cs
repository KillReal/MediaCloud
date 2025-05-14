using MediaCloud.Data.Models;
using Blob = MediaCloud.Data.Models.Blob;

namespace MediaCloud.WebApp;

public class FileModel(Blob file, Preview preview)
{
   public readonly Blob File = file;
   public readonly Preview Preview = preview;
}
