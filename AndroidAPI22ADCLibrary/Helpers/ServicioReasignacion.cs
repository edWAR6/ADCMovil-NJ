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

namespace AndroidAPI22ADCLibrary.Helpers
{
    [Service]
    class ServicioReasignacion : Service
    {
        static readonly string TAG = "X:" + typeof(ServicioReasignacion).Name;
        static readonly int TimerWait = 4000;
        Timer _timer;

        private SQLiteDatabase db;
        string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "PJNotificaciones.db");

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(TAG, "SERVICIO REASIGNACION at {2}, flags={0}, startid={1}", flags, startId, DateTime.UtcNow);
            _timer = new Timer(o => 
            {
                Log.Debug(TAG, "Hello from SimpleService. {0}", DateTime.UtcNow);

                if(ReasignarNotificacion())
                    this.StopSelf();
                else
                    Log.Debug(TAG, "Reintentando Reasignar notificacion at {2}, flags={0}, startid={1}", flags, startId, DateTime.UtcNow);

            },null,0,TimerWait);
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

            Log.Debug(TAG, "servicio reasignacion destroyed at {0}.", DateTime.UtcNow);
        }

        private bool ReasignarNotificacion()
        {

            List<string> listaCodigoNotificacion = new List<string>();
            List<string> listaCodigoNotificador = new List<string>();

            if (!ManejoBaseDatos.DataBaseOpen())
            {
                ManejoBaseDatos.Abrir();
                string query = "Select CodNotificador,CodigoNotificacion from Notificaciones WHERE Reasignar='S'";
                ICursor cursor = ManejoBaseDatos.Seleccionar(query);

                if (cursor.MoveToFirst())
                {
                    do
                    {
                        listaCodigoNotificador.Add(cursor.GetString(0));
                        listaCodigoNotificacion.Add(cursor.GetString(1));
                    }
                    while (cursor.MoveToNext());
                }

                cursor.Close();
                ManejoBaseDatos.Cerrar();

                int counter = 0;
                if (coneccionInternet.verificaConeccion(ApplicationContext))
                {
                    foreach (var codigoNotificacion in listaCodigoNotificacion)
                    {
                        try
                        {
                            string request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/ReasignarNotificacion?PCodSupervisor=" + FragmentLogin.codNotificador + "&PCodlNotificador=" + listaCodigoNotificador[counter] + "&PCodNotificacion=" + codigoNotificacion + "&PFecha=20160915";
                            var httpWebRequest = (HttpWebRequest)WebRequest.Create(request);
                            httpWebRequest.ContentType = "application/json";
                            httpWebRequest.Method = "POST";
                            httpWebRequest.ContentLength = 0;
                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var result = streamReader.ReadToEnd();
                                Console.WriteLine("Resultado:" + result);

                                if (result.Equals("true", StringComparison.Ordinal) || result.Equals("True", StringComparison.Ordinal))
                                {
                                    ManejoBaseDatos.Abrir();
                                    //CAMBIAR ESTE QUERY A UN SOLO STRING
                                    //ManejoBaseDatos.Actualizar("Notificaciones", "Reasignar", "'N'", "WHERE CodigoNotificacion=" + codigoNotificacion + "");
                                    ManejoBaseDatos.ActualizarMultiples("UPDATE Notificaciones SET Reasignar='N' WHERE CodigoNotificacion=" + codigoNotificacion + "");
                                    ManejoBaseDatos.Cerrar();
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error Re-asignando notificacion: " + ex.ToString());
                            //return false;
                        }
                        counter = counter + 1;
                    }
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
    }
}