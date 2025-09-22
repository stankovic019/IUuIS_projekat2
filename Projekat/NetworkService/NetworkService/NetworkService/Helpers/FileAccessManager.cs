using System.IO;
using System.Threading;

public static class FileAccessManager
{
    // One global mutex for read/write threads
    private static readonly Mutex fileMutex = new Mutex(false, "Global\\NetworkServiceFileMutex");

    public static void WriteToFile(string path, string content)
    {
        try
        {
            fileMutex.WaitOne();

            using (StreamWriter write = new StreamWriter(path, true))
            {
                write.WriteLine(content);
            }
        }
        finally
        {
            fileMutex.ReleaseMutex();
        }
    }

    public static string[] ReadFromFile(string path)
    {
        try
        {
            fileMutex.WaitOne();

            return File.ReadAllLines(path);
        }
        finally
        {
            fileMutex.ReleaseMutex();
        }
    }

}
