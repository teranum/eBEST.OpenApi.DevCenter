using System.IO;
using System.Runtime.InteropServices;

namespace App.Helpers;

#if !NETCOREAPP

public static class FileStreamExtension
{
    public static void Write(this FileStream @this, byte[] buffer) => @this.Write(buffer, 0, buffer.Length);
    public static int Read(this FileStream @this, [In][Out] byte[] array) => @this.Read(array, 0, array.Length);

}

public static class StringExtension
{
    public static string[] Split(this string _this, char separator, StringSplitOptions options)
    {
        return _this.Split(new char[] { separator }, options);
    }
    public static string[] Split(this string _this, string separator, StringSplitOptions options)
    {
        return _this.Split(new string[] { separator }, options);
    }
}


#endif