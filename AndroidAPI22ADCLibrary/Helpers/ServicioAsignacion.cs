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
using Android.Util;
using System.Threading;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Database.Sqlite;
using Newtonsoft.Json;
using Android.Database;
using System.IO;
using System.Net;
using AndroidAPI22ADCLibrary.Fragments;

namespace AndroidAPI22ADCLibrary.Helpers
{
    [Service]
    public class ServicioAsignacion : Service
    {

        static readonly string TAG = "X:" + typeof(ServicioAsignacion).Name;
        static readonly int TimerWait = 4000;
        Timer _timer;
        private SQLiteDatabase db;
        string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "PJNotificaciones.db");

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(TAG, "Servicio de Asignacion {2}, flags={0}, startid={1}", flags, startId, DateTime.UtcNow);
            _timer = new Timer(o => 
            {
                AsignarNotificacionANotificador(0);
                Console.WriteLine("DETENIENDO SERVICIO DE ASIGNACION");
                this.StopSelf();
                //Log.Debug(TAG, "Hello from SimpleService. {0}", DateTime.UtcNow);
                

            },null, 0,TimerWait);

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

        private void AsignarNotificacionANotificador(int modo)
        {
            if (coneccionInternet.verificaConeccion(ApplicationContext))
            {
                try
                {
                    //se va a generar una consulta de todos los notificadores, y por cada notificador se consultan
                    //las notificaciones que se encuentran en estado AsignarParaNotificar
                    List<int> listaCodigoNotificacion = new List<int>();
                    List<string> listaCodigoNotificador = new List<string>();

                    db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                    string consulta = "";

                    switch (modo)
                    {
                        case 0:
                            consulta = "Select CodNotificador from Notificaciones WHERE PendienteSubir='S' AND Estado='AsignarParaNotificar'";
                            break;
                        case 1:
                            consulta = "Select CodNotificador from Notificaciones WHERE PendienteSubir='S' AND Estado='PendienteDeDistribucion'";
                            break;
                    }


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
                        foreach (var item in listaCodigoNotificador)
                        {
                            //item contiene el codigo del notificador 
                            db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                            switch (modo)
                            {
                                case 0:
                                    consulta = "Select CodigoNotificacion from Notificaciones WHERE PendienteSubir='S' AND Estado='AsignarParaNotificar' AND CodNotificador='" + item + "'";
                                    break;
                                case 1:
                                    consulta = "Select CodigoNotificacion from Notificaciones WHERE PendienteSubir='S' AND Estado='PendienteDeDistribucion' AND CodNotificador='" + item + "'";
                                    break;
                            }
                            cursor = db.RawQuery(consulta, null);
                            if (cursor.MoveToFirst())
                            {
                                do
                                {
                                    //se tiene la lista de notificaciones en estado pendiente de subir para un notificador 
                                    listaCodigoNotificacion.Add(Convert.ToInt32(cursor.GetString(0)));
                                }
                                while (cursor.MoveToNext());
                            }
                            cursor.Close();
                            db.Close();

                            string json = "";
                            switch (modo)
                            {
                                case 0:
                                    json = JsonConvert.SerializeObject(listaCodigoNotificacion);
                                    break;
                                case 1:
                                    json = JsonConvert.SerializeObject(listaCodigoNotificacion);
                                    break;
                            }
                            //var json = JsonConvert.SerializeObject(listaCodigoNotificacion);

                            Console.WriteLine("JSON representation of lista: {0}", json);

                            string request = "";
                            switch (modo)
                            {
                                case 0:
                                    request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/AsignarNotificacionAlOficialNotificador?PCodSupervisor=" + FragmentLogin.codNotificador + "&PCodlNotificador=" + item + "&PFecha=20161102";
                                    break;
                                case 1:
                                    request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ActaNotificacion/AprobarActasNotificacion";
                                    break;
                            }

                            if (coneccionInternet.verificaConeccion(ApplicationContext))
                            {

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
                                        foreach (var item2 in listaCodigoNotificacion)
                                        {
                                            db.ExecSQL(@"UPDATE Notificaciones SET PendienteSubir='N' WHERE CodigoNotificacion=" + item2.ToString() + " ");
                                        }
                                        db.Close();
                                    }
                                }
                                listaCodigoNotificacion.Clear();
                            }
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("ERROR IN POST WEB REQUEST: " + ex.ToString()); }
            }
        }




    }
}