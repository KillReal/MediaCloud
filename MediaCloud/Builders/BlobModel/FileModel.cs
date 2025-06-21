using MediaCloud.Data.Models;
using Blob = MediaCloud.Data.Models.Blob;

namespace MediaCloud.WebApp;

public class FileModel(Blob blob, Preview preview)
{
   public readonly Blob Blob = blob;
   public readonly Preview Preview = preview;
}
