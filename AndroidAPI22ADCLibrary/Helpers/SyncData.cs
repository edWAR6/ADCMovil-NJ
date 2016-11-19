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
using AndroidAPI22ADCLibrary.Helpers;
using System.Net;
using System.IO;

namespace AndroidAPI22ADCLibrary.Helpers
{
    class SyncData
    {

        public static List<string> mRequest = new List<string>();
        public static List<string> mJson = new List<string>();
        //public static int synchronizationCounter = 0;

        public static bool synchronize(List<string> request, List<string> json, Context mContext)
        {
            //bool synchronizationDone = false;

            if (coneccionInternet.verificaConeccion(mContext))
            {

                //var httpWebRequest = (HttpWebRequest)WebRequest.Create();
                //httpWebRequest.ContentType = "application/json";
                //httpWebRequest.Method = "POST";

                //using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                //{
                //    streamWriter.Write();
                //    streamWriter.Flush();
                //    streamWriter.Close();
                //}

                //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                //{
                //    var result = streamReader.ReadToEnd();
                //    Console.WriteLine("RESULTADO POST: " + result);

                //    if (result.Equals("true", StringComparison.Ordinal) || result.Equals("True", StringComparison.Ordinal))
                //    {
                //        return true;
                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}
            }

            return false;
        }



    }
}