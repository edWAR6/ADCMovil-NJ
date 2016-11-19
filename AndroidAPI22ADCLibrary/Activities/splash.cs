using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Database.Sqlite;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Support.V7.App;
using AndroidAPI22ADCLibrary.Fragments;
using System.Threading.Tasks;
using System.Linq;

namespace AndroidAPI22ADCLibrary.Activities
{
    //[Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true, Icon = "@drawable/app_icon")]
    [Activity(Theme = "@style/MyTheme.Splash", NoHistory = true)]

    public class SplashActivity : Activity
    {
        //private ProgressBar pb;
        static readonly string TAG = "X:" + typeof(SplashActivity).Name;
        static string dbName = "PJNotificaciones.db";
        public static string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, dbName);

        private string sector = "";
        private string oficina = FragmentLogin.codOficina;
        private string rolNocturno = FragmentLogin.rolNocturno;
        private string supervisor = FragmentLogin.supervisor;
        private string codNotificador =FragmentLogin.codNotificador;

        /// <summary>
        /// En el OnCreate se va a crear un thread asincrono en el cual se llama a la funcion InitializaDataBase.
        /// Que es la encargada de inicializar la base de datos. 
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
                        
        }

        private void HandleButtonClick(object sender, DialogClickEventArgs e)
        {
            Java.Lang.JavaSystem.Exit(0);
        }

        public void InitializeDataBase(bool admin,string oficina,string sector, int estado,int jornada)
        {

            List<string> query = new List<string>();
            List<string> queryResult = new List<string>();
            List<string> tables = new List<string>();

            //tablas a borrar si no se encuentra informacion respaldada en base de datos.
            tables.Add("Notificaciones");
            tables.Add("OficialesNotificadores");
            tables.Add("Resultados");
            tables.Add("Sectores");
            
            Console.WriteLine("--fzeledon: Creating DataBase for app");
            ManejoBaseDatos.Abrir();
            string resultadoBorrar = ManejoBaseDatos.BorrarTablas(tables);
            ManejoBaseDatos.Cerrar();

            if (!string.IsNullOrEmpty(resultadoBorrar))
            {
                throw new System.ArgumentException(resultadoBorrar);
            }

            try
            {       
                //Consulta de sectores 
                query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/SectorNotificacion/ListarSectores?PCodOficina=" + oficina + "");
                //Consulta de resultados positivos
                query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ResultadoNotificacion/ListarResultadosNotificaciones?PDiligenciado=1");
                //Consulta de resultados negativos
                query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ResultadoNotificacion/ListarResultadosNotificaciones?PDiligenciado=0");
                //Consulta de Oficiales Notificadores
                query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/ListarOficialesNotificadores?PCodOficina=" + oficina + "");
                //consulta de Notificaciones en estado 0 jornada diurna y nocturna con parametro de admin
                if (admin)
                    query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorSectorEstado?PCodOficina=" + oficina + "&PSector=" + sector + "&PEstado=" + estado.ToString() + "&PJornada=" + jornada.ToString() + "");
                else
                    query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorNotificadorEstado?POficina="+oficina+"&PCodNotificador="+codNotificador+ "&PEstado="+estado+"&PJornada=" + jornada.ToString() + "");
                Stopwatch sw = Stopwatch.StartNew();

                //Se crean multiples consultas
                foreach (var item in query)
                {
                    try
                    {
                        Console.WriteLine("--fzeledon: Creating query for item: " + item);
                        WebRequest request = HttpWebRequest.Create(item);
                        request.ContentType = "application/json";
                        request.Method = "GET";
                        string content = "";
                        //se establece un tiempo de respuesta de 10 segundos por solicitud
                        request.Timeout = 10000; //tiempo en milisegundos

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
                                    queryResult.Add(content);
                                }
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine("WEB Exception: "+ex.ToString());
                        Android.Support.V7.App.AlertDialog.Builder alerta = new Android.Support.V7.App.AlertDialog.Builder(this);
                        alerta.SetTitle("Mensaje de alerta");
                        alerta.SetIcon(Resource.Drawable.alertaNuevo);
                        alerta.SetMessage("El servicio de Internet no se encuentra disponible, por favor revise su conexión e intente ingresar nuevamente");
                        alerta.SetNegativeButton("Salir", HandleButtonClick);
                        alerta.SetCancelable(false);
                        alerta.Create();
                        alerta.Show();

                    }
                }

                
                // Guargando notificaciones en estado 0 
                var jsonParsed = JArray.Parse(queryResult[4]);

                ManejoBaseDatos.Abrir();
                    
                for (int k = 0; k < jsonParsed.Count; k++)
                {
                    string resultado = "";
                    if (FragmentLogin.supervisor.Equals("True", StringComparison.Ordinal) || FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal))
                    {
                        
                        resultado = ManejoBaseDatos.Insertar("Notificaciones", "CodigoNotificacion,Expediente,DespachoDescripcion,Notificando,Medio,Provincia,Canton,Distrito,Direccion,Sector,Urgente,FechaDocumento,FechaEmision,HorarioEntrega,Estado",
                                                      jsonParsed[k]["CodNotificacion"].ToString() + ", " +
                                                "'" + jsonParsed[k]["NumExpediente"].ToString() + "'," +
                                                "'" + jsonParsed[k]["Despacho"]["Descripcion"].ToString() + "'," +
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
                                                "'" + jsonParsed[k]["HorarioEntrega"].ToString() + "'," +
                                                "'" + jsonParsed[k]["Estado"].ToString() + "'");
                    }
                    else
                    {
                        resultado = ManejoBaseDatos.Insertar("Notificaciones", "CodigoNotificacion,Expediente,HorarioEntrega,DespachoDescripcion,Notificando,Medio,Provincia,Canton,Distrito,Direccion,Sector,Urgente,FechaDocumento,FechaEmision,Estado,CodNotificador,RolNocturno,DespachoCodigo,DespachoDescripcion",
                                                      jsonParsed[k]["CodNotificacion"].ToString() + ", " +
                                                "'" + jsonParsed[k]["NumExpediente"].ToString() + "'," +
                                                "'" + jsonParsed[k]["HorarioEntrega"].ToString() + "'," +
                                                "'" + jsonParsed[k]["Despacho"]["Descripcion"].ToString() + "'," +
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
                                                "'" + jsonParsed[k]["OficialNotificador"]["CodNotificador"].ToString() + "'," +
                                                "'" + jsonParsed[k]["OficialNotificador"]["RolNocturno"].ToString() + "',"+
                                                "'" + jsonParsed[k]["Despacho"]["Codigo"].ToString() + "'," +
                                                "'" + jsonParsed[k]["Despacho"]["Descripcion"].ToString() + "'");
                    }
                    if (!String.IsNullOrEmpty(resultado))
                    {
                        throw new System.ArgumentException(resultado);
                    }
                }
 
                //Guardando resultado de oficiales notificadores
                jsonParsed = JArray.Parse(queryResult[3]);
                for (int k = 0; k < jsonParsed.Count; k++)
                {          string resultado = ManejoBaseDatos.Insertar("OficialesNotificadores","CodigoNotificador,NombreCompleto,DespachoCodigo,CuotaMinima,RolNocturno,Supervisor","'"+jsonParsed[k]["CodNotificador"].ToString()+"',"+"'"+ jsonParsed[k]["NombreCompleto"].ToString()+ "'," + "'" + jsonParsed[k]["Oficinas"][0]["Codigo"].ToString() + "'," + "" + jsonParsed[k]["Oficinas"][0]["CuotaMinima"].ToString() + ","+ "'" + jsonParsed[k]["RolNocturno"].ToString()+"',"+ "'" + jsonParsed[k]["Supervisor"].ToString()+"'");
                    if (!String.IsNullOrEmpty(resultado))
                    {
                        throw new System.ArgumentException(resultado);
                    }
                }
           

                jsonParsed = JArray.Parse(queryResult[2]);    
                for (int k = 0; k < jsonParsed.Count; k++)
                {
                    string resultado=ManejoBaseDatos.Insertar("Resultados", "Codigo,Descripcion,Diligenciada", jsonParsed[k]["Codigo"].ToString() + "," +
                                                "'" + jsonParsed[k]["Descripcion"].ToString() + "'," +
                                                "'" + jsonParsed[k]["Diligenciada"].ToString() + "'");
                    if (!String.IsNullOrEmpty(resultado))
                    {
                        throw new System.ArgumentException(resultado);
                    }
                }

                jsonParsed = JArray.Parse(queryResult[1]);
                for (int k = 0; k < jsonParsed.Count; k++)
                {
                   string resultado= ManejoBaseDatos.Insertar("Resultados", "Codigo,Descripcion,Diligenciada", jsonParsed[k]["Codigo"].ToString() + "," +
                                                "'" + jsonParsed[k]["Descripcion"].ToString() + "'," +
                                                "'" + jsonParsed[k]["Diligenciada"].ToString() + "'");
                    if (!String.IsNullOrEmpty(resultado))
                    {
                        throw new System.ArgumentException(resultado);
                    }
                }
                
                
                jsonParsed = JArray.Parse(queryResult[0]);
                for (int k = 0; k < jsonParsed.Count; k++)
                {

                    string resultado =ManejoBaseDatos.Insertar("Sectores", "Codigo,Descripcion", jsonParsed[k]["Codigo"].ToString() + "," +
                                                "'" + jsonParsed[k]["Descripcion"].ToString() + "'");
                    if (!String.IsNullOrEmpty(resultado))
                    {
                        throw new System.ArgumentException(resultado);
                    }
                }

                sw.Stop();
                ManejoBaseDatos.Cerrar();
                Console.WriteLine("--fzeledon: Tiempo en milisegundos que dura bajar los datos desde dB: " + sw.ElapsedMilliseconds);
            }
            catch (Exception ex) { Console.WriteLine("--fzeledon: Error while loading data: "+ ex.ToString()); }
        }

        protected override void OnResume()
        {
            base.OnResume();

            int jornada = 0;
            int estado = 0;

            bool admin = false;

            if (supervisor.Equals("true", StringComparison.Ordinal) || supervisor.Equals("True", StringComparison.Ordinal))
            {
                sector = "";//se va a generar informacion para todos los sectores
                jornada = 3; // se van a ver las notificaciones duirnas y nocturnas
                admin = true;//propiedad de admin verdadera
                estado = 1;// Se van a consultar las notificaciones que se encuentren nuevas a imprimir
            }
            else
            {
                admin = false; //propiedad de admin en false
                estado = 2;// asignado para notificar.

                if (rolNocturno.Equals("true", StringComparison.Ordinal) || rolNocturno.Equals("True", StringComparison.Ordinal))
                {
                    jornada = 3;//permite observar notificaciones de ambas jornadas
                }
                else
                {
                    jornada = 1;//permite observar notificaciones de jornada diurna
                }
            }

            Task startupWork = new Task(() =>
            {
                if (coneccionInternet.verificaConeccion(this.ApplicationContext))
                {
                    Stopwatch sw1 = Stopwatch.StartNew();
                    List<string> tables = new List<string>();

                    tables.Add("Notificaciones");
                    tables.Add("OficialesNotificadores");
                    tables.Add("Resultados");
                    tables.Add("Sectores");
                    tables.Add("Autenticacion");

                    Console.WriteLine("--fzeledon: Creating DataBase for app");

                    ManejoBaseDatos.Abrir();
                    string resultadoBorrar = ManejoBaseDatos.BorrarTablas(tables);
                    ManejoBaseDatos.Cerrar();

                    List<string> results = new List<string>();
                    List<string> query = new List<string>();
                    //Consulta de sectores 
                    query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/SectorNotificacion/ListarSectores?PCodOficina=" + oficina + "");
                    //Consulta de resultados positivos
                    query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ResultadoNotificacion/ListarResultadosNotificaciones?PDiligenciado=1");
                    //Consulta de resultados negativos
                    query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ResultadoNotificacion/ListarResultadosNotificaciones?PDiligenciado=0");
                    //Consulta de Oficiales Notificadores
                    query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/ListarOficialesNotificadores?PCodOficina=" + oficina + "");
                    //consulta de Notificaciones en estado 0 jornada diurna y nocturna con parametro de admin
                    if (admin)
                        query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorSectorEstado?PCodOficina=" + oficina + "&PSector=" + sector + "&PEstado=" + estado.ToString() + "&PJornada=" + jornada.ToString() + "");
                    else
                        query.Add(@"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorNotificadorEstado?POficina=" + oficina + "&PCodNotificador=" + codNotificador + "&PEstado=" + estado + "&PJornada=" + jornada.ToString() + "");

                    var urls =query.ToArray();

                    var tasks = urls.Select(GetAsync).ToArray();
                    var completed = Task.Factory.ContinueWhenAll(tasks,
                                        completedTasks => {
                                            foreach (var result in completedTasks.Select(t => t.Result))
                                            {
                                                //Console.WriteLine(result);
                                                results.Add(result);
                                                //Console.WriteLine(result);
                                            }
                                        });
                    completed.Wait();
                    Console.WriteLine("Tasks completed");
                    //anything that follows gets executed after all urls have finished downloading

                    var jsonParsed = JArray.Parse(results[4]);
                    Console.WriteLine("total de elementos a parsear: "+jsonParsed.Count.ToString());
                    

                    ManejoBaseDatos.Abrir();
                    for (int k = 0; k < jsonParsed.Count; k++)
                    {
                        string resultado = "";
                        if (FragmentLogin.supervisor.Equals("True", StringComparison.Ordinal) || FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal))
                        {
                            
                            resultado = ManejoBaseDatos.Insertar("Notificaciones", "CodigoNotificacion,Expediente,DespachoDescripcion,Notificando,Medio,Provincia,Canton,Distrito,Direccion,Sector,Urgente,FechaDocumento,FechaEmision,Visitas,HorarioEntrega,Estado",
                                                          jsonParsed[k]["CodNotificacion"].ToString() + ", " +
                                                    "'" + jsonParsed[k]["NumExpediente"].ToString() + "'," +
                                                    "'" + jsonParsed[k]["Despacho"]["Descripcion"].ToString() + "'," +
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
                                                    "" + jsonParsed[k]["Visitas"].ToString() + "," +
                                                    "'" + jsonParsed[k]["HorarioEntrega"].ToString() + "'," +
                                                    "'" + jsonParsed[k]["Estado"].ToString() + "'");
                            
                        }
                        else
                        {
                            resultado = ManejoBaseDatos.Insertar("Notificaciones", "CodigoNotificacion,Expediente,DespachoDescripcion,Notificando,Medio,Provincia,Canton,Distrito,Direccion,Sector,Urgente,FechaDocumento,FechaEmision,Visitas,Estado,CodNotificador,RolNocturno,DespachoCodigo,HorarioEntrega,DespachoDescripcion",
                                                          jsonParsed[k]["CodNotificacion"].ToString() + ", " +
                                                    "'" + jsonParsed[k]["NumExpediente"].ToString() + "'," +
                                                     "'" + jsonParsed[k]["Despacho"]["Descripcion"].ToString() + "'," +
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
                                                     "" + jsonParsed[k]["Visitas"].ToString() + "," +
                                                    "'" + jsonParsed[k]["Estado"].ToString() + "'," +
                                                    "'" + jsonParsed[k]["OficialNotificador"]["CodNotificador"].ToString() + "'," +
                                                    "'" + jsonParsed[k]["OficialNotificador"]["RolNocturno"].ToString() + "'," +
                                                    "'" + jsonParsed[k]["Despacho"]["Codigo"].ToString() + "'," +
                                                     "'" + jsonParsed[k]["HorarioEntrega"].ToString() + "'," +
                                                    "'" + jsonParsed[k]["Despacho"]["Descripcion"].ToString() + "'");
                        }
                        if (!String.IsNullOrEmpty(resultado))
                        {
                            throw new System.ArgumentException(resultado);
                        }
                    }
                    jsonParsed = JArray.Parse(results[3]);
                    Console.WriteLine("total de elementos a parsear: " + jsonParsed.Count.ToString());

                    for (int k = 0; k < jsonParsed.Count; k++)
                    {
                        string resultado = ManejoBaseDatos.Insertar("OficialesNotificadores", "CodigoNotificador,NombreCompleto,DespachoCodigo,CuotaMinima,RolNocturno,Supervisor", "'" + jsonParsed[k]["CodNotificador"].ToString() + "'," + "'" + jsonParsed[k]["NombreCompleto"].ToString() + "'," + "'" + jsonParsed[k]["Oficinas"][0]["Codigo"].ToString() + "'," + "" + jsonParsed[k]["Oficinas"][0]["CuotaMinima"].ToString() + "," + "'" + jsonParsed[k]["RolNocturno"].ToString() + "'," + "'" + jsonParsed[k]["Supervisor"].ToString() + "'");
                        if (!String.IsNullOrEmpty(resultado))
                        {
                            throw new System.ArgumentException(resultado);
                        }
                    }


                    jsonParsed = JArray.Parse(results[2]);
                    for (int k = 0; k < jsonParsed.Count; k++)
                    {
                        string resultado = ManejoBaseDatos.Insertar("Resultados", "Codigo,Descripcion,Diligenciada", jsonParsed[k]["Codigo"].ToString() + "," +
                                                    "'" + jsonParsed[k]["Descripcion"].ToString() + "'," +
                                                    "'" + jsonParsed[k]["Diligenciada"].ToString() + "'");
                        if (!String.IsNullOrEmpty(resultado))
                        {
                            throw new System.ArgumentException(resultado);
                        }
                    }

                    jsonParsed = JArray.Parse(results[1]);
                    for (int k = 0; k < jsonParsed.Count; k++)
                    {
                        string resultado = ManejoBaseDatos.Insertar("Resultados", "Codigo,Descripcion,Diligenciada", jsonParsed[k]["Codigo"].ToString() + "," +
                                                     "'" + jsonParsed[k]["Descripcion"].ToString() + "'," +
                                                     "'" + jsonParsed[k]["Diligenciada"].ToString() + "'");
                        if (!String.IsNullOrEmpty(resultado))
                        {
                            throw new System.ArgumentException(resultado);
                        }
                    }


                    jsonParsed = JArray.Parse(results[0]);
                    for (int k = 0; k < jsonParsed.Count; k++)
                    {

                        string resultado = ManejoBaseDatos.Insertar("Sectores", "Codigo,Descripcion", jsonParsed[k]["Codigo"].ToString() + "," +
                                                    "'" + jsonParsed[k]["Descripcion"].ToString() + "'");
                        if (!String.IsNullOrEmpty(resultado))
                        {
                            throw new System.ArgumentException(resultado);
                        }
                    }
                    ManejoBaseDatos.Cerrar();
                    sw1.Stop();
                    Console.WriteLine("tiempo transcurido en milisegundos: "+sw1.ElapsedMilliseconds.ToString());
                    Console.WriteLine("Parsing completado");

                }
                //InitializeDataBase(admin, oficina, sector, estado, jornada);
                else
                {
                    Android.App.AlertDialog.Builder alerta = new Android.App.AlertDialog.Builder(this.ApplicationContext);
                    alerta.SetTitle("Mensaje de alerta");
                    alerta.SetIcon(Resource.Drawable.alertaNuevo);
                    alerta.SetMessage("El servicio de Internet no se encuentra disponible, por favor revise su conexión e intente ingresar nuevamente");
                    alerta.SetNegativeButton("Salir", HandleButtonClick);
                    alerta.SetCancelable(false);
                    alerta.Create();
                    alerta.Show();
                }
            });
            startupWork.ContinueWith(t =>
            {
                if (admin)
                {
                    Intent intent = new Intent(this, typeof(MailBoxes));
                    intent.PutExtra(MailBoxes.EXTRA_SUPERVISOR, "supervisor");
                    StartActivity(intent);
                }
                else
                {
                    Intent intent = new Intent(this, typeof(MailBoxes));
                    intent.PutExtra(MailBoxes.EXTRA_SUPERVISOR, "notificador");
                    StartActivity(intent);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            startupWork.Start();
        }


        public Task<string> GetAsync(string url)
        {
            var tcs = new TaskCompletionSource<string>();
            var request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                request.BeginGetResponse(iar =>
                {
                    HttpWebResponse response = null;
                    try
                    {
                        response = (HttpWebResponse)request.EndGetResponse(iar);
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            tcs.SetResult(reader.ReadToEnd());
                        }
                    }
                    catch (Exception exc) { tcs.SetException(exc); }
                    finally { if (response != null) response.Close(); }
                }, null);
            }
            catch (Exception exc) { tcs.SetException(exc); }
            return tcs.Task;
        }
    }
}