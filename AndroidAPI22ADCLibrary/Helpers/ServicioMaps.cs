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
using System.Threading;
using Android.Util;
using System.Net;
using Android.Database.Sqlite;
using System.IO;
using Android.Database;
using AndroidAPI22ADCLibrary.Fragments;
using static AndroidAPI22ADCLibrary.Helpers.servicioCheckDB;


namespace AndroidAPI22ADCLibrary.Helpers
{
    [Service]
    class ServicioMaps : Service
    {
        static readonly string TAG = "X:" + typeof(ServicioMaps).Name;
        static readonly int TimerWait = 32000;
        Timer _timer;

        private SQLiteDatabase db;
        string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "PJNotificaciones.db");

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            //Log.Debug(TAG, "Actualizacion Mapas at {2}, flags={0}, startid={1}", flags, startId, DateTime.UtcNow);
            _timer = new Timer(o =>
            {
                Log.Debug(TAG, "SERVICIO CALCULO DE COORDENADAS MAPS. {0}", DateTime.UtcNow);
            
                if(ActualizarPosicionMaps())
                    this.StopSelf();
                else
                    Log.Debug(TAG, "Reintentando Calcular posiciones at {2}, flags={0}, startid={1}", flags, startId, DateTime.UtcNow);


            }, null, 0, TimerWait);
            return StartCommandResult.NotSticky;
        }


        public override IBinder OnBind(Intent intent)
        {
            //throw new NotImplementedException();
            return null;
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            _timer.Dispose();
            _timer = null;
            Log.Debug(TAG, "ServicioMaps destroyed at {0}.", DateTime.UtcNow);
        }


        private bool ActualizarPosicionMaps()
        {
            if (!ManejoBaseDatos.DataBaseOpen())
            {
                if (coneccionInternet.verificaConeccion(ApplicationContext))
                {
                    List<string> data = new List<string>();
                    List<string> codNotificacion = new List<string>();
                    string consulta = "Select Provincia,Canton,Distrito,CodigoNotificacion from Notificaciones WHERE CalcularPosicion='S'";
                    ManejoBaseDatos.Abrir();
                    ICursor cursor = ManejoBaseDatos.Seleccionar(consulta);
                    if (cursor.MoveToFirst())
                    {
                        do
                        {
                            // se agregan a la lista los diferentes elementos a los cuales se les va a calcular la posicion
                            data.Add("Costa Rica," + cursor.GetString(0) + "," + cursor.GetString(1) + "," + cursor.GetString(2));
                            codNotificacion.Add(cursor.GetString(3));

                        } while (cursor.MoveToNext());
                    }
                    cursor.Close();
                    ManejoBaseDatos.Cerrar();

                    if (data.Count >= 0)
                    {
                        int counter = 0;
                        foreach (var item in data)
                        {
                            //Console.WriteLine("string buscar " + item);
                            List<string> tempCoordenas = new List<string>();
                            tempCoordenas = Helper.obtenerCoordenadas(item);


                            if (tempCoordenas.Count >= 0)
                            {
                                //Console.WriteLine("Coordenadas " + tempCoordenas[0] + "," + tempCoordenas[1]);
                                //db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                //db.ExecSQL(@"UPDATE Notificaciones SET googleMapsX=" + tempCoordenas[0] + ", googleMapsY=" + tempCoordenas[1] + ", CalcularPosicion='N' WHERE CodigoNotificacion =" + codNotificacion[counter] + " ");
                                //db.Close();
                                ManejoBaseDatos.Abrir();
                                //ManejoBaseDatos.Actualizar("Notificaciones", "googleMapsX", tempCoordenas[0], " WHERE CodigoNotificacion =" + codNotificacion[counter] + "");
                                //ManejoBaseDatos.Actualizar("Notificaciones", "googleMapsY", tempCoordenas[1], " WHERE CodigoNotificacion =" + codNotificacion[counter] + "");
                                //ManejoBaseDatos.Actualizar("Notificaciones", "CalcularPosicion", "'N'", " WHERE CodigoNotificacion =" + codNotificacion[counter] + "");
                                ManejoBaseDatos.ActualizarMultiples("UPDATE Notificaciones SET googleMapsX=" + tempCoordenas[0] + ", googleMapsY=" + tempCoordenas[1] + ", CalcularPosicion='N' WHERE CodigoNotificacion =" + codNotificacion[counter] + "");
                                ManejoBaseDatos.Cerrar();
                            }
                            counter = counter + 1;
                        }
                        Console.WriteLine("Posiciones guardadas");
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;

            }
            else
                return false;

        }
    }
}