using System.Collections.Generic;
using System.IO;

using Android.Database.Sqlite;
using Android.Database;
using System;
using Android.Support.V4.Widget;


namespace AndroidAPI22ADCLibrary.Helpers
{
    public class SQLiteConeccion
    {
        SQLiteDatabase db;
        string dbName = "PJNotificaciones.db";

        /// <summary>
        /// Se abre la conección a la base de datos
        /// </summary>
        public void loadConnection()
        {
            //string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, dbName);

            bool exist = File.Exists(dbPath);
            try
            {
                if (exist)
                    db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                else
                {
                    FileStream writeStream = new FileStream(dbPath, FileMode.OpenOrCreate, FileAccess.Write);
                    ReadWriteStream(Android.App.Application.Context.Assets.Open(dbName), writeStream);
                    db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR ABRIENDO BASE DE DATOS: " + ex.ToString());
            }
        }

        private void ReadWriteStream(Stream readStream, Stream writeStream)
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


        public void setAdaptadorCombo(string consulta, Android.Content.Context context, ref Android.Widget.Spinner spinner)
        {
            try {
                loadConnection();
                List<string> data = new List<string>();
                ICursor cursor = db.RawQuery(consulta, null);

                if (cursor.MoveToFirst())
                {
                    data.Add("");
                    do
                    {
                        data.Add(cursor.GetString(0));
                    } while (cursor.MoveToNext());

                }

                cursor.Close();
                db.Close();

                Android.Widget.ArrayAdapter<String> adapter;
                adapter = new Android.Widget.ArrayAdapter<String>(context, Android.Resource.Layout.SimpleSpinnerItem, data);

                spinner.Adapter = adapter;

            } catch (Exception e)
            {
                throw e;
            }

        }

        public void setAdaptadorCombo(string consulta, Android.Content.Context context, ref Android.Widget.Spinner spinner, int layout)
        {
            try
            {
                loadConnection();
                List<string> data = new List<string>();
                ICursor cursor = db.RawQuery(consulta, null);

                if (cursor.MoveToFirst())
                {
                    data.Add("");
                    do
                    {
                        data.Add(cursor.GetString(0));
                    } while (cursor.MoveToNext());

                }
                cursor.Close();
                db.Close();

                Android.Widget.ArrayAdapter<String> adapter;
                adapter = new Android.Widget.ArrayAdapter<String>(context, layout, data);
                //adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinner.Adapter = adapter;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Se realiza la consulta a la base de datos para obtener un string y llenar un textView con la informacion obtenida
        /// </summary>
        /// <param name="consulta"></param>
        /// <param name="context"></param>
        /// <param name="texto"></param>
        public void consultaDatos(string consulta, Android.Content.Context context, ref Android.Widget.TextView texto)
        {
            loadConnection();//Se abre la conección con la base de datos
            string resultado = "";//String que va a contener el resultado obtenido
            ICursor cursor = db.RawQuery(consulta, null);//Se realiza la consulta a la base de datos

            if (cursor.MoveToFirst())//Se posiciona el cursor en la primera fila obtenida
            {
                resultado = cursor.GetString(0);//Se carga el resultado
            }

            texto.Text = resultado;//Se asigna el resultado al textView que se quiere cargar
        }

        /// <summary>
        /// Se realiza la consulta a la base de datos para obtener un string
        /// </summary>
        /// <param name="consulta"></param>
        /// <param name="context"></param>
        /// <param name="texto"></param>
        public void consultaDatos(string consulta, Android.Content.Context context, ref string str)
        {

            try
            {
                loadConnection();//Se abre la conección con la base de datos
                string resultado = "";//String que va a contener el resultado obtenido
                ICursor cursor = db.RawQuery(consulta, null);//Se realiza la consulta a la base de datos

                if (cursor.MoveToFirst())//Se posiciona el cursor en la primera fila obtenida
                {
                    resultado = cursor.GetString(0);//Se carga el resultado
                }

                str = resultado;//Se asigna el resultado al textView que se quiere cargar

            }
            catch (Exception e)
            {
                throw e;//Se retorna un error
            }

        }
    }
}