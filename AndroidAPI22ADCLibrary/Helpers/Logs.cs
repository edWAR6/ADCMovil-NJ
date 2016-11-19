using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;

namespace AndroidAPI22ADCLibrary.Helpers
{
    class Logs
    {

        static string flatName = "ErrorLog.txt";

        public static void saveLogError(String message)
        {
            try
            {
                string path = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                string filename = Path.Combine(path,flatName);

                bool exist = File.Exists(filename);
                if (exist)
                {
                    using (var streamWriter = new StreamWriter(filename, true))
                    {
                        streamWriter.WriteLine(DateTime.UtcNow + " " + message);
                        streamWriter.WriteLine("");
                    }

                    using (var streamReader = new StreamReader(filename))
                    {
                        string content = streamReader.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine(content);
                    }
                }
                else
                {
                    FileStream writeStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                    ReadWriteStream(Android.App.Application.Context.Assets.Open(flatName), writeStream);

                    using (var streamWriter = new StreamWriter(filename, true))
                    {
                        streamWriter.WriteLine(DateTime.UtcNow + " " + message);
                    }
                }
            }
            catch (Exception logGen) { Console.WriteLine("Error when generating log, due to: "+logGen.ToString()); }
        }


        private static void ReadWriteStream(Stream readStream, Stream writeStream)
        {
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = readStream.Read(buffer, 0, Length);

            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = readStream.Read(buffer, 0, Length);
            }
            readStream.Close();
            writeStream.Close();
        }
    }
}