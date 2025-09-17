using NetworkService.Model;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading;

public static class FileAccessManager
{
    // Globalni mutex, koristi ime koje je isto za sve instance aplikacije
    private static readonly Mutex fileMutex = new Mutex(false, "Global\\NetworkServiceFileMutex");
    
    public static void WriteToFile(string path, string content)
    {
        try
        {
            fileMutex.WaitOne(); // čekaj dok mutex ne bude dostupan

            // Piši u fajl
            using (StreamWriter write = new StreamWriter(path, true))
            {
                write.WriteLine(content);
            }
        }
        finally
        {
            fileMutex.ReleaseMutex(); // obavezno otpusti mutex
        }
    }

    public static string[] ReadFromFile(string path)
    {
        try
        {
            fileMutex.WaitOne(); // čekaj dok mutex ne bude dostupan

            // Pročitaj sve linije odjednom i vrati kao niz
            return File.ReadAllLines(path);
        }
        finally
        {
            fileMutex.ReleaseMutex();
        }
    }

}
