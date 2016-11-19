using System;
using Newtonsoft.Json.Linq;
using Android.Database.Sqlite;
using System.Net;
using System.IO;
using System.Collections.Generic;
using Android.Widget;
using Android.Graphics;
using Android.Content;
using Android.Content.Res;

namespace AndroidAPI22ADCLibrary.Helpers
{
    class Helper
    {

        private static Random RANDOM = new Random();
        private List<string> consultas = new List<string>();

        public static int RandomImage
        {
            get
            {
                switch (RANDOM.Next(2))
                {
                    default:
                    case 0:
                        return Resource.Drawable.ic_home_1;
                    case 1:
                        return Resource.Drawable.ic_home_2;

                }
            }
        }

        public static int ImagePicker(int retry)
        {
            if (retry > 1)
            {
                if (retry % 2 == 0)
                {
                    return Resource.Drawable.ic_home_1;
                }
                else
                    return Resource.Drawable.ic_home_2;
            }
            else
            {
                if(retry==0)
                    return Resource.Drawable.ic_home_1;
                else
                    return Resource.Drawable.ic_home_2;
            }
        }

        public static Bitmap drawTextToBitmap(Context gContext, int gResId, String gText)
        {
            Resources resources = gContext.Resources;
            float scale = resources.DisplayMetrics.Density;
            Bitmap bitmap =
                BitmapFactory.DecodeResource(resources, gResId);

            Android.Graphics.Bitmap.Config bitmapConfig =
                bitmap.GetConfig();
            // set default bitmap config if none
            if (bitmapConfig == null)
            {
                bitmapConfig = Android.Graphics.Bitmap.Config.Argb8888;
            }
            // resource bitmaps are imutable, 
            // so we need to convert it to mutable one
            bitmap = bitmap.Copy(bitmapConfig, true);

            Canvas canvas = new Canvas(bitmap);
            // new antialised Paint
            //Paint paint = new Paint(Paint.ANTI_ALIAS_FLAG);
            Paint paint = new Paint(PaintFlags.AntiAlias);
            // text color - #3D3D3D
            paint.Color = new Color(61, 61, 61);
            // text size in pixels
            paint.TextSize = ((int)(15 * scale));
            // text shadow
            //paint.setShadowLayer(1f, 0f, 1f, Color.WHITE);

            // draw text to the Canvas center
            Rect bounds = new Rect();
            paint.GetTextBounds(gText, 0, gText.Length, bounds);

            int x = (bitmap.Width - bounds.Width()) / 2;
            int y = (bitmap.Height + bounds.Height()) / 2;

            canvas.DrawText(gText, x - 5, y, paint);

            return bitmap;
        }


        public static string WebServiceInteraction()
        {
            string queryResult = "";
            return queryResult;
        }

        public static List<string> obtenerCoordenadas(string direccion)
        {
            List<string> coordenadas = new List<string>();

            var direccionBuscar = string.Format("http://maps.googleapis.com/maps/api/geocode/xml?address={0}&sensor=false", Uri.EscapeDataString(direccion));

            var peticion = WebRequest.Create(direccionBuscar);
            var respuesta = peticion.GetResponse();
            var xdoc = System.Xml.Linq.XDocument.Load(respuesta.GetResponseStream());

            var resultado = xdoc.Element("GeocodeResponse").Element("result");
            var localizacionElemento = resultado.Element("geometry").Element("location");

            var lat = localizacionElemento.Element("lat");
            var lng = localizacionElemento.Element("lng");

            Console.WriteLine("lat: "+lat.ToString());
            Console.WriteLine("lon: " + lng.ToString());

            //double tempLat = (double)lat;
            //double tempLong = (double)lng;

            coordenadas.Add(lat.Value.ToString());
            coordenadas.Add(lng.Value.ToString());     
            return coordenadas;
        }


    }
}