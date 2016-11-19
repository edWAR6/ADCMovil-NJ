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
    class ServicioAceptacion : Service
    {

        static readonly string TAG = "X:" + typeof(ServicioReasignacion).Name;
        static readonly int TimerWait = 4000;
        Timer _timer;

        private SQLiteDatabase db;
        string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "PJNotificaciones.db");



        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(TAG, "SERVICIO RECHAZO at {2}, flags={0}, startid={1}", flags, startId, DateTime.UtcNow);
            _timer = new Timer(o =>
            {
                Log.Debug(TAG, "Hello from SimpleService. {0}", DateTime.UtcNow);
                //ReasignarNotificacion();
                subirNotificacionAceptada();
                this.StopSelf();

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

            Log.Debug(TAG, "SimpleService destroyed at {0}.", DateTime.UtcNow);
        }


        private void subirNotificacionAceptada()
        {
            List<int> listaCodigoNotificacion = new List<int>();
            List<string> listaCodigoNotificador = new List<string>();
            //List<string> motivo = new List<string>();
            List<string> listaCodigoReferencia = new List<string>();

            if (coneccionInternet.verificaConeccion(ApplicationContext))
            {
                Log.Debug(TAG, "Revisando si existen notificaciones con estado PendienteDeDistribucion at {0}.", DateTime.UtcNow);
                db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                string consulta = "";
                // Se guardan los diferentes codigos de notificadores con estado notificandose pendiente de subir 
                consulta = "Select CodNotificador from Notificaciones WHERE PendienteSubir='S' AND Estado='PendienteDeDistribucion'";
                ICursor cursor = db.RawQuery(consulta, null);
                if (cursor.MoveToFirst())
                {
                    do
                    {
                        listaCodigoNotificador.Add(cursor.GetString(0));
                    }
                    while (cursor.MoveToNext());
                }
                cursor.Close();
                db.Close();

                if (listaCodigoNotificador.Count > 0)
                {

                    consulta = "";
                    foreach (var codigoNotificador in listaCodigoNotificador)
                    {
                        //tengo por notificador la lista de notificaciones en estado ResultadoEnCorreccion
                        db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                        consulta = "Select CodigoNotificacion,CodReferencia from Notificaciones WHERE PendienteSubir='S' AND Estado='PendienteDeDistribucion' AND CodNotificador='" + codigoNotificador + "'";
                        cursor = db.RawQuery(consulta, null);

                        if (cursor.MoveToFirst())
                        {
                            Log.Debug(TAG, "Subiendo notificacion ACEPTADA at {0}.", DateTime.UtcNow);
                            do
                            {
                                //agrego los diferentes codigos de notificaciones pendientes de subir para un notificador en especifico
                                listaCodigoNotificacion.Add(Convert.ToInt32(cursor.GetString(0)));
                                //motivo.Add(cursor.GetString(1));
                                listaCodigoReferencia.Add(cursor.GetString(1));
                            }
                            while (cursor.MoveToNext());
                        }
                        cursor.Close();
                        db.Close();

                        //
                        int counter = 0;
                        //Por cada codigo de notificacion creo un webRequest con su respectivo cuerpo de Json
                        foreach (var codigoNotificacion in listaCodigoNotificacion)
                        {
                            try
                            {
                                AprobarActa aprobarActa = new AprobarActa()
                                {
                                    Notificaciones = new List<ClassNotificaciones>()
                                    {
                                        new ClassNotificaciones()
                                        {
                                            CodNotificacion=codigoNotificacion,
                                            CodReferencia= Convert.ToInt32(listaCodigoReferencia[counter]),

                                        }
                                    }
                                };

                                string request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ActaNotificacion/AprobarActasNotificacion";
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(aprobarActa);
                                Console.WriteLine("Json: " + json);
                                var httpWebRequest = (HttpWebRequest)WebRequest.Create(request);
                                httpWebRequest.ContentType = "application/json";
                                httpWebRequest.Method = "POST";

                                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                                {
                                    streamWriter.Write(json);
                                    streamWriter.Flush();
                                    streamWriter.Close();
                                }

                                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                                {
                                    var result = streamReader.ReadToEnd();
                                    Console.WriteLine("RESULTADO POST: " + result);

                                    if (result.Equals("true", StringComparison.Ordinal) || result.Equals("True", StringComparison.Ordinal))
                                    {
                                        db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                        db.ExecSQL(@"UPDATE Notificaciones SET PendienteSubir='N' WHERE CodigoNotificacion=" + codigoNotificacion + " ");
                                        db.Close();
                                    }
                                }
                                counter = counter + 1;

                            }
                            catch (Exception ex) { Console.WriteLine("Error subiendo datos para ResultadoEnCorreccion: " + ex.ToString()); }

                        }
                        listaCodigoNotificacion.Clear();
                        listaCodigoReferencia.Clear();
                    }
                }
            }
        }

    }
}