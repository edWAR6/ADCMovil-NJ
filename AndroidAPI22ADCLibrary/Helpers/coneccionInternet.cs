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
using Android.Net;

namespace AndroidAPI22ADCLibrary.Helpers
{
    public static class coneccionInternet
    {
        public static bool verificaConeccion(Context context)
        {
            ConnectivityManager connectivity = ((ConnectivityManager)context.GetSystemService(Context.ConnectivityService));
            if (connectivity != null)
            {

                if (connectivity != null)
                {
                    NetworkInfo[] info = connectivity.GetAllNetworkInfo();

                    if (info != null)
                    {
                        for (int i = 0; i < info.Length; i++)
                        {
                            if (info[i].GetState() == NetworkInfo.State.Connected)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


    }
}