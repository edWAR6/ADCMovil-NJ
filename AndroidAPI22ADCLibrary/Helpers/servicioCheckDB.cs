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
using Android.Database.Sqlite;
using Android.Database;
using AndroidAPI22ADCLibrary.Helpers;
using AndroidAPI22ADCLibrary.Fragments;
using System.IO;
using System.Globalization;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;

namespace AndroidAPI22ADCLibrary.Helpers
{
    [Service]
    public class servicioCheckDB : Service
    {
        static readonly int TimerWait = 30000;
        static readonly string TAG = "X:" + typeof(servicioCheckDB).Name;
        Timer _timer;
        private bool done = false;

        //SQLiteConeccion dbConeccion = new SQLiteConeccion();

        private SQLiteDatabase db;
        private SQLiteDatabase db2;
        //string dbName = "PJNotificaciones.db";
        string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "PJNotificaciones.db");

        public override IBinder OnBind(Intent intent)
        {
            //throw new NotImplementedException();
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            //return base.OnStartCommand(intent, flags, startId);
            Log.Debug(TAG, "OnStartCommand called at {2}, flags={0}, startid={1}", flags, startId, DateTime.UtcNow);

            _timer = new Timer(o =>
            {
                

                //ActualizarPosicionMaps();

                if (FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal) || FragmentLogin.supervisor.Equals("True", StringComparison.Ordinal))
                {

                    //En modo 0 se realiza la asignación de notificaciones al notificador.
                    //AsignarNotificacionANotificador(0);
                    //ReasignarNotificacion();

                    if (bajarNotificaciones(4))
                        Log.Debug(TAG, "Bajando notificaciones a la base de datos. {0}", DateTime.UtcNow); 

                    else
                        Log.Debug(TAG, "Se reintentara bajar datos a la base de datos. {0}", DateTime.UtcNow);
                    
                    //SubirNotificacionRechazada();
                    //subirNotificacionAceptada();
                    //bajarNotificaciones(6);

                    //Subiendo pendientes de distribucion
                    //AsignarNotificacionANotificador(1);
                }
                else
                {
                    //Actualizacion de datos de notificador sin permisos de administrador.
                    //inicioJornada();
                    bajarNotificaciones(5);
                    //xsubirResultadoNotificacion();
                }
            }, null, 0, TimerWait);

            return StartCommandResult.NotSticky;

        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _timer.Dispose();
            _timer = null;
            Log.Debug(TAG, "SimpleService destroyed at {0}.", DateTime.UtcNow);
        }
        private void ActualizarPosicionMaps()
        {
            try
            {
                //dbConeccion.loadConnection();
                db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                List<string> data = new List<string>();
                List<string> codNotificacion = new List<string>();
                string consulta = "Select Provincia,Canton,Distrito,CodigoNotificacion from Notificaciones WHERE CalcularPosicion='S'";

                ICursor cursor = db.RawQuery(consulta, null);
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
                db.Close();
                // se procede a realizar el calculo sobre cada uno de los elementos en data con refenrencia al codigo de notificacion
                if (data.Count >= 0 && coneccionInternet.verificaConeccion(ApplicationContext))
                {
                    int counter = 0;
                    foreach (var item in data)
                    {
                        Console.WriteLine("string buscar " + item);
                        List<string> tempCoordenas = new List<string>();
                        tempCoordenas = Helper.obtenerCoordenadas(item);
                        if (tempCoordenas.Count >= 0)
                        {
                            //String.format("%.0f", priceValue)
                            Console.WriteLine("Coordenadas " + tempCoordenas[0] + "," + tempCoordenas[1]);

                            db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                            db.ExecSQL(@"UPDATE Notificaciones SET googleMapsX=" + tempCoordenas[0] + " WHERE CodigoNotificacion =" + codNotificacion[counter] + " ");
                            db.ExecSQL(@"UPDATE Notificaciones SET googleMapsY=" + tempCoordenas[1] + " WHERE CodigoNotificacion =" + codNotificacion[counter] + " ");
                            db.ExecSQL(@"UPDATE Notificaciones SET CalcularPosicion='N' WHERE CodigoNotificacion =" + codNotificacion[counter] + " ");
                            db.Close();
                        }
                        counter = counter + 1;
                    }
                    Console.WriteLine("Posiciones guardadas");
                }
                //
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR EN SERVICO DE CACLCULO DE POSICIONES GOOGLE MAPS" + ex.ToString());
            }

        }

        //lista
        private void AsignarNotificacionANotificador(int modo)
        {
            //if(!done)
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
        
        //lista para usar
        private bool bajarNotificaciones(int estado)
        {
            if (!ManejoBaseDatos.DataBaseOpen())
            {

                if (coneccionInternet.verificaConeccion(ApplicationContext))
                {
                    DateTime localDate = DateTime.Now;
                    string fechalineal = localDate.ToString("yyyyMMdd");
                    string fechaCompleja = localDate.ToString("o");

                    int jornada = 0;
                    Log.Debug(TAG, "Bajando notificaciones en estado" + estado + ". {0}", DateTime.UtcNow);
                    string url = "";
                    switch (estado)
                    {
                        //diligenciada
                        case 4:
                            jornada = 3;
                            url = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorSectorEstado?PCodOficina=" + FragmentLogin.codOficina + "&PSector=" + "" + "&PEstado=" + "NotificacionCompletada" + "&PJornada=" + jornada.ToString() + "";
                            break;
                        //En correccion
                        case 5:
                            //jornada = 3;
                            //Estado en correccion
                            //url = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorSectorEstado?PCodOficina=" + FragmentLogin.codOficina + "&PSector=" + "" + "&PEstado=" + "ResultadoEnCorreccion" + "&PJornada=" + jornada.ToString() + "";
                            url = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ActaNotificacion/ListarActasEnCorreccion?POficina=" + FragmentLogin.codOficina + "&PCodNotificador=" + FragmentLogin.codNotificador + "&PSector=&PJornada=3&PFecha1=" + fechalineal + "&PFecha2=" + fechalineal + "";
                            //url = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorNotificadorEstado?POficina=" + FragmentLogin.codOficina + "&PCodNotificador=" + FragmentLogin.codNotificador + "&PEstado=ResultadoEnCorreccion&PJornada=" + jornada + "";
                            break;
                        //En distribucion
                        case 6:
                            jornada = 3;
                            url = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorSectorEstado?PCodOficina=" + FragmentLogin.codOficina + "&PSector=" + "" + "&PEstado=" + "PendienteDeDistribucion" + "&PJornada=" + jornada.ToString() + "";
                            break;
                    }

                    WebRequest request = HttpWebRequest.Create(url);
                    request.ContentType = "application/json";
                    request.Method = "GET";
                    string content = "";

                    try
                    {
                        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                        {
                            if (response.StatusCode != HttpStatusCode.OK)
                                Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                content = reader.ReadToEnd();

                                if (string.IsNullOrWhiteSpace(content))
                                    Console.Out.WriteLine("fzeledon -- Response contained empty body...");
                                else
                                {
                                    //consulta a parsear en JArrray 
                                    var jsonParsed = JArray.Parse(content);

                                    try
                                    {

                                        List<string> codigosExistentes = new List<string>();
                                        ICursor newCursor;
                                        string query = "SELECT CodigoNotificacion FROM Notificaciones";
                                        ManejoBaseDatos.Abrir();
                                        newCursor = ManejoBaseDatos.Seleccionar(query);
                                        if (newCursor.MoveToFirst())
                                        {
                                            do
                                            {
                                                codigosExistentes.Add(newCursor.GetString(0));
                                            }
                                            while (newCursor.MoveToNext());
                                        }
                                        newCursor.Close();
                                        ManejoBaseDatos.Cerrar();

                                        //Se tiene una lista con todos los posibles codigos existentes en la tabla codigosExistentes
                                        //se va a recorrer el arreglo parseado de Json

                                        if (jsonParsed.Count > 0 && !ManejoBaseDatos.DataBaseOpen())
                                        {
                                            for (int k = 0; k < jsonParsed.Count; k++)
                                            {
                                                //si el codigo de la notificacion en el arreglo no existe, se inserta en la tabla. Si exist se actualiza.
                                                if (!codigosExistentes.Contains(jsonParsed[k]["CodNotificacion"].ToString()))
                                                {
                                                    //db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                                    ManejoBaseDatos.Abrir();
                                                    ManejoBaseDatos.InsertarMultiples("INSERT INTO Notificaciones (CodigoNotificacion,CodReferencia,Expediente, Notificando, Medio, Provincia, Canton, Distrito, Direccion, Sector, Urgente, FechaDocumento, FechaEmision, Estado,CodNotificador)" +
                                                                " VALUES (" + jsonParsed[k]["CodNotificacion"].ToString() + ", " +
                                                                "" + jsonParsed[k]["CodReferencia"].ToString() + "," +
                                                                //"'" + jsonParsed[k]["Estado"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["NumExpediente"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["Notificando"]["NombreCompleto"].ToString().Replace("'", "") + "'," +
                                                                "'" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["TipoMedio"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Provincia"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Canton"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Distrito"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Direccion"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Sector"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["Urgente"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["FechaDocumento"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["FechaEmision"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["Estado"].ToString() + "'," +
                                                                "'" + jsonParsed[k]["OficialNotificador"]["CodNotificador"].ToString() + "'" + ")");
                                                    ManejoBaseDatos.Cerrar();
                                                }
                                                else
                                                {
                                                    //db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                                    ManejoBaseDatos.Abrir();
                                                    ManejoBaseDatos.ActualizarMultiples("UPDATE Notificaciones SET CodReferencia=" + jsonParsed[k]["CodReferencia"].ToString() + "," +
                                                                "Estado='" + jsonParsed[k]["Estado"].ToString() + "'," +
                                                                "Expediente='" + jsonParsed[k]["NumExpediente"].ToString() + "'," +
                                                                "Notificando='" + jsonParsed[k]["Notificando"]["NombreCompleto"].ToString().Replace("'", "") + "'," +
                                                                "Medio='" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["TipoMedio"].ToString().Replace("'", "") + "'," +
                                                                "Provincia='" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Provincia"].ToString().Replace("'", "") + "'," +
                                                                "Canton='" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Canton"].ToString().Replace("'", "") + "'," +
                                                                "Distrito='" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Distrito"].ToString().Replace("'", "") + "'," +
                                                                "Direccion='" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Direccion"].ToString().Replace("'", "") + "'," +
                                                                "Sector='" + jsonParsed[k]["Notificando"]["MedioNotificacion"]["Sector"].ToString().Replace("'", "") + "'," +
                                                                "Urgente='" + jsonParsed[k]["Urgente"].ToString() + "'," +
                                                                "FechaDocumento='" + jsonParsed[k]["FechaDocumento"].ToString() + "'," +
                                                                "FechaEmision='" + jsonParsed[k]["FechaEmision"].ToString() + "'," +
                                                                "CodNotificador='" + jsonParsed[k]["OficialNotificador"]["CodNotificador"].ToString() + "' WHERE CodigoNotificacion=" + jsonParsed[k]["CodNotificacion"].ToString() + "");
                                                    ManejoBaseDatos.Cerrar();
                                                }
                                            }
                                            return true;
                                        }
                                        else
                                            return false;
                                        //

                                    }
                                    catch (Exception ex) { Console.WriteLine("ERROR BAJANDO DATOS DE ESTADO " + ex.ToString() + " :" + ex.ToString()); }
                                }
                            }
                        }
                        //
                    }
                    catch (Exception ex) { Console.WriteLine("Error bajando notificaciones de estado " + ex.ToString()); }

                    //OJO
                    return true;
                }
                else
                    return false;
            }
            else
                return false;



        }
        
        //lista en buzon correspondiente
        private void ReasignarNotificacion()
        {

            List<string> listaCodigoNotificacion = new List<string>();
            List<string> listaCodigoNotificador = new List<string>();
            db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
            string consulta = "Select CodNotificador,CodigoNotificacion from Notificaciones WHERE Reasignar='S'";
            ICursor cursor = db.RawQuery(consulta, null);
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
            db.Close();
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
                                db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                db.ExecSQL(@"UPDATE Notificaciones SET Reasignar='N' WHERE CodigoNotificacion=" + codigoNotificacion + " ");
                                db.Close();
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error Re-asignando notificacion: " + ex.ToString()); }
                    counter = counter + 1;
                }
            }

        }

        //lista en buzon correspondiente
        private void inicioJornada()
        {
            List<int> listaCodigoNotificacion = new List<int>();
            List<string> listaCodigoNotificador = new List<string>();
            List<string> descripcionOficina = new List<string>();

            if (coneccionInternet.verificaConeccion(ApplicationContext))
            {

                db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                string consulta = "";
                // Se guardan los diferentes codigos de notificadores con estado notificandose pendiente de subir 
                consulta = "Select CodNotificador from Notificaciones WHERE PendienteSubir='S' AND Estado='Notificandose'";

                ICursor cursor2 = db.RawQuery(consulta, null);
                if (cursor2.MoveToFirst())
                {
                    do
                    {
                        listaCodigoNotificador.Add(cursor2.GetString(0));
                    }
                    while (cursor2.MoveToNext());
                }
                cursor2.Close();
                db.Close();

                //Por cada notificador selecciono los códigos de interes y hago la respectiva asignación
                //existen notificadores con asignaciones pendientes de subir
                if (listaCodigoNotificador.Count > 0)
                {
                    consulta = "";

                    foreach (var codigoNotificador in listaCodigoNotificador)
                    {
                        db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                        consulta = "Select CodigoNotificacion,DespachoDescripcion from Notificaciones WHERE PendienteSubir='S' AND Estado='Notificandose' AND CodNotificador='" + codigoNotificador + "'";
                        cursor2 = db.RawQuery(consulta, null);

                        //si existen codigos de notificacion con la condicion anterior
                        if (cursor2.MoveToFirst())
                        {
                            do
                            {
                                //agrego los diferentes codigos de notificaciones pendientes de subir para un notificador en especifico
                                listaCodigoNotificacion.Add(Convert.ToInt32(cursor2.GetString(0)));
                                descripcionOficina.Add(cursor2.GetString(1));
                            }
                            while (cursor2.MoveToNext());
                        }
                        cursor2.Close();
                        db.Close();

                        int counter = 0;
                        //Por cada codigo de notificacion creo un webRequest con su respectivo cuerpo de Json
                        foreach (var codigoNotificacion in listaCodigoNotificacion)
                        {
                            try
                            {
                                //creo un objeto de tipo aperturaJornada
                                AperturaJornada jornada = new AperturaJornada
                                {
                                    codigo = Guid.NewGuid().ToString(),
                                    Apertura = "2015-08-20T00:00:00Z",
                                    oficialNotificador = new ClassOficialNotificador
                                    {
                                        CodNotificador = codigoNotificador,
                                        Oficinas = new List<ClassDespacho>()
                                        {
                                            new ClassDespacho()
                                            {
                                                Codigo =FragmentLogin.codOficina,

                                            }
                                        }
                                    },

                                    Notificaciones = new List<ClassNotificaciones>()
                                    {
                                        new ClassNotificaciones() {CodNotificacion=codigoNotificacion }
                                    },
                                    Justificacion = "",
                                };

                                string request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/AperturaJornadaOficialNotificador";
                                //En este punto se 
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(jornada);
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
                            }
                            catch (Exception ex) { Console.WriteLine("ERROR GENERANDO WEB REQUEST ININIO JORNADA: " + ex.ToString()); }

                            counter = counter + 1;
                        }
                        listaCodigoNotificacion.Clear();
                        descripcionOficina.Clear();
                        counter = 0;
                    }
                }
            }

        }
        //lista en servicio

        private void SubirNotificacionRechazada()
        {

            List<int> listaCodigoNotificacion = new List<int>();
            List<string> listaCodigoNotificador = new List<string>();
            List<string> motivo = new List<string>();
            List<string> listaCodigoReferencia = new List<string>();

            if (coneccionInternet.verificaConeccion(ApplicationContext))
            {
                Log.Debug(TAG, "Revisando si existen notificaciones con estado ResultadoEnCorrecion at {0}.", DateTime.UtcNow);
                db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                string consulta = "";
                // Se guardan los diferentes codigos de notificadores con estado notificandose pendiente de subir 
                consulta = "Select CodNotificador from Notificaciones WHERE PendienteSubir='S' AND Estado='ResultadoEnCorreccion'";
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
                        consulta = "Select CodigoNotificacion,Motivo,CodReferencia from Notificaciones WHERE PendienteSubir='S' AND Estado='ResultadoEnCorreccion' AND CodNotificador='" + codigoNotificador + "'";
                        cursor = db.RawQuery(consulta, null);

                        if (cursor.MoveToFirst())
                        {
                            Log.Debug(TAG, "Subiendo notificacion RECHAZADA at {0}.", DateTime.UtcNow);
                            do
                            {
                                //agrego los diferentes codigos de notificaciones pendientes de subir para un notificador en especifico
                                listaCodigoNotificacion.Add(Convert.ToInt32(cursor.GetString(0)));
                                motivo.Add(cursor.GetString(1));
                                listaCodigoReferencia.Add(cursor.GetString(2));
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
                                RechazarActas actasRechazadas = new RechazarActas()
                                {
                                    Fecha = "2015-07-13T19:33:38.034Z",
                                    OficialSupervisor = FragmentLogin.codNotificador,
                                    OficialNotificador = codigoNotificador,
                                    CodOficina = FragmentLogin.codOficina,
                                    Notificaciones = new List<ClassNotificaciones>()
                                    {
                                        new ClassNotificaciones()
                                        {
                                            CodNotificacion=codigoNotificacion,
                                            CodReferencia= Convert.ToInt32(listaCodigoReferencia[counter])
                                        }
                                    },
                                    Motivo = motivo[counter]
                                };
                                string request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ActaNotificacion/RechazarActasNotificacion";
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(actasRechazadas);
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
                        motivo.Clear();
                        listaCodigoReferencia.Clear();
                    }
                }
            }
        }

        //pendiente
        private void subirResultadoNotificacion()
        {
            List<int> listaCodigoNotificacion = new List<int>();
            List<string> listaCodigoNotificador = new List<string>();
            List<string> listaResultadoCodigo = new List<string>();
            List<string> listaResultadoDescripcion = new List<string>();
            List<string> listaResultadoDiligenciada = new List<string>();
            List<string> listaObservaciones = new List<string>();
            List<string> listaFirmaNotificando = new List<string>();
            List<string> listaFirmaTestigo = new List<string>();
            List<string> listaNombreTestigo = new List<string>();
            List<string> listaNotificando = new List<string>();


            if (coneccionInternet.verificaConeccion(ApplicationContext))
            {
                //Log.Debug(TAG, "Revisando si existen notificaciones con estado NotificacionCompletada at {0}.", DateTime.UtcNow);
                db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                string consulta = "";
                // Se guardan los diferentes codigos de notificadores con estado notificandose pendiente de subir 
                consulta = "Select CodNotificador from Notificaciones WHERE PendienteSubir='S' AND Estado='NotificacionCompletada'";
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

                //lista con los codigos de los notificadores
                if (listaCodigoNotificador.Count > 0)
                {

                    //Por cada codigo de notificador se va a ejecutar una consulta para obetener las diferentes notificaciones
                    foreach (var codigoNotificador in listaCodigoNotificador)
                    {
                        //tengo por notificador la lista de notificaciones en estado ResultadoEnCorreccion
                        db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                        consulta = "Select CodigoNotificacion,ResultadoCodigo,ResultadoDescripcion,Observaciones,FirmaNotificando,FirmaTestigo,NombreTestigo,Notificando,ResultadoDiligenciada from Notificaciones WHERE PendienteSubir='S' AND Estado='NotificacionCompletada' AND CodNotificador='" + codigoNotificador + "'";
                        cursor = db.RawQuery(consulta, null);

                        if (cursor.MoveToFirst())
                        {
                            //CodigoNotificacion,ResultadoCodigo,ResultadoDescripcion,Observaciones,FirmaNotificando,FirmaTestigo,NombreTestigo,Notificando
                            Log.Debug(TAG, "Subiendo notificacion COMPLETADA at {0}.", DateTime.UtcNow);
                            do
                            {
                                //agrego los diferentes codigos de notificaciones pendientes de subir para un notificador en especifico
                                listaCodigoNotificacion.Add(Convert.ToInt32(cursor.GetString(0)));
                                listaResultadoCodigo.Add(cursor.GetString(1));
                                listaResultadoDescripcion.Add(cursor.GetString(2));
                                listaObservaciones.Add(cursor.GetString(3));
                                listaFirmaNotificando.Add(cursor.GetString(4));
                                listaFirmaTestigo.Add(cursor.GetString(5));
                                listaNombreTestigo.Add(cursor.GetString(6));
                                listaNotificando.Add(cursor.GetString(7));
                                listaResultadoDiligenciada.Add(cursor.GetString(8));
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
                                //RechazarActas actasRechazadas = new RechazarActas()
                                //{
                                //    Fecha = "2015-07-13T19:33:38.034Z",
                                //    OficialSupervisor = FragmentLogin.codNotificador,
                                //    OficialNotificador = codigoNotificador,
                                //    Notificaciones = new List<int>() { Convert.ToInt32(codigoNotificacion) },
                                //    //Motivo = motivo[counter]
                                //};

                                ActaNotificacion actaNotificacion = new ActaNotificacion()
                                {
                                    fechaNotificacion = "2016-10-13T19:33:38.034Z",
                                    Notificacion = new NotificacionFisica()
                                    {
                                        CodNotificacion = codigoNotificacion,
                                        OficialNotificador = new OficialNotificador()
                                        {
                                            CodNotificador = FragmentLogin.codNotificador
                                        }
                                    },
                                    Resultado = new ResultadoNotificacion()
                                    {
                                        Codigo = Convert.ToInt32(listaResultadoCodigo[counter]),
                                        Descripcion = listaResultadoDescripcion[counter],
                                        Diligenciada = Convert.ToBoolean(listaResultadoDiligenciada[counter]),
                                    },
                                    Observaciones = listaObservaciones[counter],
                                    NombreReceptor = listaNotificando[counter],
                                    FirmaReceptor = listaFirmaNotificando[counter],
                                    NombreTestigo = listaNombreTestigo[counter],
                                    FirmaTestigo = listaFirmaTestigo[counter],

                                };

                                string request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ActaNotificacion/GuardarActaNotificacion";
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(actaNotificacion);
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
                                    Console.WriteLine("RESULTADO POST AL SUBIR RESULTADO NOTIFICACION: " + result);

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
                        listaResultadoCodigo.Clear();
                        listaResultadoDescripcion.Clear();
                        listaObservaciones.Clear();
                        listaFirmaNotificando.Clear();
                        listaFirmaTestigo.Clear();
                        listaNombreTestigo.Clear();
                        listaNotificando.Clear();
                    }
                }


            }

        }
        //pendiente


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

        public JArray ObtenerListaStrings(string consulta, Context context)
        {
            if (coneccionInternet.verificaConeccion(context))
            {
                WebRequest request = HttpWebRequest.Create(consulta);
                request.ContentType = "application/json";
                request.Method = "GET";
                string content = "";

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();

                        if (string.IsNullOrWhiteSpace(content))
                            Console.Out.WriteLine("fzeledon -- Response contained empty body...");
                        else
                        {
                            var jsonParsed = JArray.Parse(content);
                            return jsonParsed;
                        }
                    }
                }

            }

            return null;
        }

        public string ObtenerString(string consulta, Context context)
        {
            if (coneccionInternet.verificaConeccion(context))
            {
                WebRequest request = HttpWebRequest.Create(consulta);
                request.ContentType = "application/json";
                request.Method = "GET";
                string content = "";

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();

                        if (string.IsNullOrWhiteSpace(content))
                            Console.Out.WriteLine("fzeledon -- Response contained empty body...");
                        else
                        {
                            //var jsonParsed = JObject.Parse(content);
                            return content;
                        }
                    }
                }

            }

            return null;
        }

        public bool envioDatosWeb(string consulta, string json, Context contexto)
        {
            if (coneccionInternet.verificaConeccion(contexto))
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(consulta);
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
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }


        public class ResultadoNotificacion : IEquatable<ResultadoNotificacion>
        {
            public int Codigo { get; set; }
            public string Descripcion { get; set; }
            public bool Diligenciada { get; set; }
            public bool Equals(ResultadoNotificacion other)
            {
                if (other == null) return false;
                return (this.Codigo == other.Codigo);
            }
            public override string ToString()
            {
                return string.Format("Codigo:{0}, Descripcion:{1}, Diligenciada ", Codigo, Descripcion, Diligenciada);
            }

        }

        public class NotificacionFisica
        {
            //[DataContract]
            public enum EstadoNotificacion
            {
                //[EnumMember]
                Ninguno = 0,
                //[EnumMember]
                NuevasAImprimir = 1,
                //[EnumMember]
                AsignarParaNotificar = 2,
                //[EnumMember]
                Notificandose = 3,
                //[EnumMember]
                NotificacionCompletada = 4,
                //[EnumMember]
                ResultadoEnCorreccion = 5,
                //[EnumMember]
                PendienteDeDistribucion = 6,
                //[EnumMember]
                EntregadaAlDespacho = 7
            }

            //[DataContract]
            public enum Jornada
            {
                //[EnumMember]
                Ninguno = 0,
                //[EnumMember]
                Diurna = 1,
                //[EnumMember]
                Nocturna = 2,
                //[EnumMember]
                Ambas = 3
            }
            public ActaNotificacion ActaNotificacion { get; set; }
            public long CodNotificacion { get; set; }
            public long CodReferencia { get; set; }
            public bool Copias { get; set; }
            public Despacho Despacho { get; set; }
            public EstadoNotificacion Estado { get; set; }
            public DateTime FechaDocumento { get; set; }
            public DateTime FechaEmision { get; set; }
            public Jornada HorarioEntrega { get; set; }
            public Interviniente Notificando { get; set; }
            public string NumComision { get; set; }
            public string NumExpediente { get; set; }
            public string CodOficina { get; set; }
            public OficialNotificador OficialNotificador { get; set; }
            public List<ReintentoNotificacion> Reintentos { get; set; }
            public GeoLocalizacion Ubicacion { get; set; }
            public bool Urgente { get; set; }
            public string Representante { get; set; }
            public string Rotulado { get; set; }

            public override string ToString()
            {
                return string.Format("CodNotificacion:{0}, CodReferencia:{1}, OCJ:{2}, Despacho{3} ", CodNotificacion, CodReferencia, OficialNotificador.Oficinas[0].Codigo, Despacho.Codigo);
            }


            public NotificacionFisica()
            {
                Reintentos = new List<ReintentoNotificacion>();
            }


        }

        public class ActaNotificacion
        {
            public string fechaDistribucion { get; set; }
            public string fechaNotificacion { get; set; }
            public string FirmaReceptor { get; set; }
            public string FirmaTestigo { get; set; }
            public string NombreReceptor { get; set; }
            public string NombreTestigo { get; set; }
            public string Observaciones { get; set; }
            public NotificacionFisica Notificacion { get; set; }
            public ResultadoNotificacion Resultado { get; set; }

            public override string ToString()
            {
                return string.Format("Notificacion:{0}, Resultado:{1}, FechaNotificacion{2} ", Notificacion, Resultado, fechaNotificacion);
            }
        }

        public class Despacho : IEquatable<Despacho>
        {
            public string Codigo { get; set; }
            public string Descripcion { get; set; }
            public int CuotaMinima { get; set; }
            public bool EsOcj { get; set; }
            public string CodCirc { get; set; }
            public string Codtidej { get; set; }
            public string Codjuris { get; set; }
            public string Provincia { get; set; }
            public List<Sector> Sectores { get; set; }
            public Despacho()
            {
                Sectores = new List<Sector>();
            }

            public bool Equals(Despacho other)
            {
                if (other == null) return false;
                return (this.Codigo == other.Codigo);
            }
            public override string ToString()
            {
                return string.Format("Codigo:{0}, Descripcion:{1} ", Codigo, Descripcion);
            }


        }

        public class Sector : IEquatable<Sector>
        {
            public string Codigo { get; set; }
            public string Descripcion { get; set; }

            public bool Equals(Sector other)
            {
                if (other == null) return false;
                return (this == other);
            }
            public override string ToString()
            {
                return string.Format("Codigo:{0}, Descripcion:{1} ", Codigo, Descripcion);
            }

        }

        public class OficialNotificador : IEquatable<OficialNotificador>
        {
            public bool Activo { get; set; }
            public string CodNotificador { get; set; }
            public string CodReferencia { get; set; }
            public List<Despacho> Oficinas { get; set; }
            public string NombreCompleto { get; set; }
            public bool RolNocturno { get; set; }
            public bool Supervisor { get; set; }

            public OficialNotificador()
            {
                Oficinas = new List<Despacho>();
            }
            public bool Equals(OficialNotificador other)
            {
                if (other == null) return false;
                return (this.CodNotificador == other.CodNotificador);
            }
            public override string ToString()
            {
                return string.Format("CodNotificador:{0}, CodReferencia:{1}, NombreCompleto:{2}, RolNocturno:{3}, Supervisor:{4}, Despachos:{5} ", CodNotificador, CodReferencia, NombreCompleto,
                     RolNocturno, Supervisor, Oficinas.Count);
            }
        }

        public class GeoLocalizacion : IEquatable<GeoLocalizacion>
        {
            public double Latitud { get; set; }
            public double Longitud { get; set; }
            public bool Aproximado { get; set; }

            public bool Equals(GeoLocalizacion other)
            {
                if (other == null) return false;
                return (this == other);
            }
            public override string ToString()
            {
                return string.Format("Latitud:{0}, Longitud:{1}, Aproximado:{2} ", Latitud, Longitud, Aproximado);
            }

        }

        public class Interviniente : IEquatable<Interviniente>
        {
            public string Nombre { get; set; }
            public string Apellido_1 { get; set; }
            public string Apellido_2 { get; set; }
            public string NombreCompleto { get { return string.Format("{0} {1} {2}", Nombre, Apellido_1, Apellido_2); } }
            public TipoInterviniente TipoInterviniente { get; set; }

            public MedioNotificacion MedioNotificacion { get; set; }
            public bool Equals(Interviniente other)
            {
                if (other == null) return false;
                return (this == other);
            }
            public override string ToString()
            {
                return string.Format("NombreCompleto:{0}  ", NombreCompleto);
            }
        }

        public class MedioNotificacion
        {
            public string Codigo { get; set; }
            public string TipoMedio { get; set; }
            public string Provincia { get; set; }
            public string Canton { get; set; }
            public string Distrito { get; set; }
            public string Barrio { get; set; }
            public string Direccion { get; set; }
            public string Sector { get; set; }
            public override string ToString()
            {
                return string.Format("TipoMedio:{0}  ", TipoMedio);
            }
        }

        public class ReintentoNotificacion : IEquatable<ReintentoNotificacion>
        {
            public long Codigo { get; set; }
            public string Descripcion { get; set; }

            public long CodNotificacion { get; set; }
            public string CodOficina { get; set; }
            public DateTime Fecha { get; set; }
            public OficialNotificador OficialNotificador { get; set; }
            public GeoLocalizacion Ubicacion { get; set; }

            public bool Equals(ReintentoNotificacion other)
            {
                if (other == null) return false;
                return (this.Codigo == other.Codigo);
            }
            public override string ToString()
            {
                return string.Format("Codigo:{0}, Oficial:{1}, Fecha:{2}, Descripcion:{3}, Notificacion:{4} ", Codigo, OficialNotificador?.CodNotificador, Fecha, Descripcion, CodNotificacion);
            }

        }

        public class TipoInterviniente : IEquatable<TipoInterviniente>
        {
            public string Codigo { get; set; }
            public string Descripcion { get; set; }

            public bool Equals(TipoInterviniente other)
            {
                if (other == null) return false;
                return (this == other);
            }
            public override string ToString()
            {
                return string.Format("Codigo:{0}, Descripcion:{1} ", Codigo, Descripcion);
            }
        }


        // Entidades definidas por fzeledon

        public class CierreJornada
        {
            public string codigo { get; set; }
            public string Apertura { get; set; }
            public OficialNotificador OficialNotificador { get; set; }
            public List<NotificacionFisica> Notificaciones { get; set; }
            public string Justificacion { get; set; }
            public string Cierre { get; set; }
        }




        public class RechazarActas
        {
            public string Fecha { get; set; }
            public string OficialSupervisor { get; set; }
            public string OficialNotificador { get; set; }
            public string Motivo { get; set; }
            public string CodOficina { get; set; }
            public List<ClassNotificaciones> Notificaciones { get; set; }
        }

        public class AprobarActa
        {
            public List<ClassNotificaciones> Notificaciones { get; set; }
        }

        public class AperturaJornada
        {
            public string codigo { get; set; }
            public string Apertura { get; set; }
            public ClassOficialNotificador oficialNotificador { get; set;}
            public List<ClassNotificaciones> Notificaciones { get; set;}
            public string Justificacion { get; set; }
        }

        public class ClassOficialNotificador
        {
            public string CodNotificador { get; set; }
            public List<ClassDespacho> Oficinas { get; set; } 

        }

        public class ClassDespacho
        {
            public string Codigo { get; set; }
            public string Descripcion { get; set; }
            public List<ClassSector> Sectores { get; set; }
        }

        public class ClassNotificaciones
        {
            public long CodNotificacion { get; set; }
            public long CodReferencia { get; set; }
        }

        public class ClassSector
        {
            public string Codigo { get; set; }
            public string Descripcion { get; set; }
        }

    }
}