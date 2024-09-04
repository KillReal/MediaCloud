namespace MediaCloud.WebApp;

public class UploadedFile
{
   public required string Name {get; set;}
   public required string Type {get; set;}
   public required byte[] Content {get; set;}
}
