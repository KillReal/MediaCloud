using MediaCloud.Data.Models;
using Blob = MediaCloud.Data.Models.Blob;

namespace MediaCloud.WebApp;

public class FileModel(Blob file, Preview preview)
{
   public Blob File = file;
   public Preview Preview = preview;
}
