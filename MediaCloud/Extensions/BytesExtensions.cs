using System.Text;

namespace MediaCloud.WebApp.Extensions;

public static class BytesExtensions
{
    public static Encoding GetEncoding(this byte[] bytes)
    {
        if (bytes[0] == 0x2b && bytes[1] == 0x2f && bytes[2] == 0x76)
        {
            return Encoding.UTF7;
        }
        
        if (bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf)
        {
            return Encoding.UTF8;
        }
        
        if (bytes[0] == 0xff && bytes[1] == 0xfe && bytes[2] == 0 && bytes[3] == 0) 
        {
            return Encoding.UTF32;
        }
        
        if (bytes[0] == 0xff && bytes[1] == 0xfe) 
        {
            return Encoding.Unicode;
        }      
        
        if (bytes[0] == 0xfe && bytes[1] == 0xff) 
        {
            return Encoding.BigEndianUnicode;
        }
        
        if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0xfe && bytes[3] == 0xff) 
        {
            return new UTF32Encoding(true, true);
        }

        // We actually have no idea what the encoding is if we reach this point, so
        // you may wish to return null instead of defaulting to ASCII
        return Encoding.ASCII;
    }
}