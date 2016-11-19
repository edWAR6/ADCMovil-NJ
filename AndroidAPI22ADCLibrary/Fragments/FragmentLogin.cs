//using Android.App;
using System;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Content;
using System.Net;
using AndroidAPI22ADCLibrary.Activities;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Support.V7.App;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
//using Microsoft.Identity.Client;

using System.Linq;
using Android.Database;
using static AndroidAPI22ADCLibrary.Helpers.servicioCheckDB;
using System.Threading.Tasks;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class FragmentLogin : Fragment
    {
        private string usuario = "";
        private string contrasena = "";

        public static string supervisor = "";
        public static string rolNocturno = "";
        public static string codOficina = "";
        public static string codNotificador = "";
        public static string cuotaMinima = "";

        

        //private AuthenticationContext authContext;
        //private TokenCacheItem tokenCacheItem;

        //Client ID from from step 1. point 6
        public static string clientId = "411efdd1-9497-4774-96c4-dbe012dd01f9";
        public static string commonAuthority = "https://login.windows.net/common";
        //Redirect URI from step 1. point 5<br />
        public static Uri returnUri = new Uri("http://www.poder-judicial.go.cr");
        //Graph URI if you've given permission to Azure Active Directory in step 1. point 6
        const string graphResourceUri = "https://graph.windows.net";
        public static string graphApiVersion = "2013-11-08";
        //AuthenticationResult will hold the result after authentication completes
        public static AuthenticationResult authResult = null;


        public static FragmentLogin NewInstance()
        {
            var loginFragment = new FragmentLogin { Arguments = new Bundle() };
            return loginFragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here

            //var authContext = new AuthenticationContext(commonAuthority);
            //if (authContext.TokenCache.ReadItems().Count() > 0)
            //    authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);

            //var platformParams = new PlatformParameters(this.Activity);
            //authResult = await authContext.AcquireTokenAsync(graphResourceUri, clientId, returnUri, platformParams);


        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            //return base.OnCreateView(inflater, container, savedInstanceState);


            View view = inflater.Inflate(Resource.Layout.fragmentLogin, container, false);

            ((MainActivity)Activity).habilitarMenuLateral(false);
            ((MainActivity)Activity).cambiarTituloAplicacion("Autenticación de usuario");

            Button btnLogin = view.FindViewById<Button>(Resource.Id.btnLogin);
            EditText txtUsuario = view.FindViewById<EditText>(Resource.Id.txtUsuario);
            EditText txtContrasena = view.FindViewById<EditText>(Resource.Id.txtPassword);

            TextInputLayout passwordWrapper = view.FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutPassword);
            string txtPassword = passwordWrapper.EditText.Text;
            Context context;
            context = view.Context;
            txtUsuario.Text = "";
            txtContrasena.Text = "";

            txtUsuario.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
            {
                usuario = e.Text.ToString();
            };

            txtContrasena.TextChanged+= (object sender, Android.Text.TextChangedEventArgs e) => 
            {
                contrasena = e.Text.ToString();
            };

            if (!coneccionInternet.verificaConeccion(this.Context))
            {
                AlertDialog.Builder alerta = new AlertDialog.Builder(this.Context);
                alerta.SetTitle("Mensaje de alerta");
                alerta.SetIcon(Resource.Drawable.alertaNuevo);
                alerta.SetMessage("El servicio de Internet no se encuentra disponible, por favor revise su conexión e intente ingresar nuevamente");
                alerta.SetNegativeButton("Salir", HandleButtonClick);
                alerta.SetCancelable(false);
                alerta.Create();
                alerta.Show();
            }

            //se limpia la tabla de Oficial Notificador 
            ManejoBaseDatos.CrearDB();
            ManejoBaseDatos.Abrir();
            List<string> tablas = new List<string>();
            tablas.Add("OficialNotificador");
            ManejoBaseDatos.BorrarTablas(tablas);
            ManejoBaseDatos.Cerrar();

            btnLogin.Click += (o, e) =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(usuario) && (contrasena.Equals("12345", StringComparison.Ordinal)) && (usuario.Equals("supervisor", StringComparison.Ordinal) || usuario.Equals("demo", StringComparison.Ordinal)))
                    {
                        string query = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/ConsultarOficialNotificador?PCodNotificador=" + usuario + "";
                        if (coneccionInternet.verificaConeccion(this.Context))
                        {
                            try
                            {
                                WebRequest request = HttpWebRequest.Create(query);
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
                                            try
                                            {
                                                var jsonParsed = JObject.Parse(content);
                                                codOficina = jsonParsed["Oficinas"][0]["Codigo"].ToString();
                                                supervisor = jsonParsed["Supervisor"].ToString();
                                                rolNocturno = jsonParsed["RolNocturno"].ToString();
                                                codNotificador = jsonParsed["CodNotificador"].ToString();
                                                cuotaMinima = jsonParsed["Oficinas"][0]["CuotaMinima"].ToString();

                                                ManejoBaseDatos.Abrir();
                                                ManejoBaseDatos.Insertar("OficialNotificador", "Activo,CodigoNotificador,DespachoCodigo,Supervisor,RolNocturno", "'" + jsonParsed["Activo"].ToString() + "','" + jsonParsed["CodNotificador"].ToString() + "','" + jsonParsed["Oficinas"][0]["Codigo"].ToString() + "','" + jsonParsed["Supervisor"].ToString() + "','" + jsonParsed["RolNocturno"].ToString() + "'");
                                                ManejoBaseDatos.Cerrar();

                                                //se realiza la comprobacion si se tiene una sesion abierta

                                                servicioCheckDB coneccion = new servicioCheckDB();
                                                var sesionAbierta = coneccion.ObtenerString("https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/OficialConCierrePendiente?PCodNotificador=" + codNotificador + "", Activity);
                                                //   "\"\""
                                                if (!sesionAbierta.Equals("\"\"", StringComparison.Ordinal))
                                                {
                                                    Console.WriteLine("Sesion abierta");
                                                    AlertDialog.Builder alerta = new AlertDialog.Builder(this.Context);
                                                    alerta.SetTitle("Mensaje de alerta");
                                                    alerta.SetIcon(Resource.Drawable.alertaNuevo);
                                                    alerta.SetMessage("El usuario tiene una jornada abierta.");
                                                    alerta.SetPositiveButton("Cerrar jornada", HandlePositiveButtonClick);
                                                    alerta.SetNegativeButton("Continuar", HandleButonContinuar);
                                                    alerta.SetCancelable(false);
                                                    alerta.Create();
                                                    alerta.Show();
                                                }
                                                else
                                                {
                                                    // se inicia la carga de datos para la actividad de buzones(mailboxes) a traves de la actividad splash
                                                    Toast.MakeText(this.Activity, "Cargando Datos ", ToastLength.Long).Show();
                                                    Intent intent = new Intent(this.Activity, typeof(SplashActivity));
                                                    StartActivity(intent);
                                                }
                                            }
                                            catch (Exception ex) { Console.WriteLine("Error descargando datos de usuario: " + ex.ToString()); }

                                        }
                                    }
                                }
                            }
                            catch (WebException webEx) { Console.WriteLine("Error en solicitud Web: " + webEx.ToString()); }
                        }
                        else
                        {
                            AlertDialog.Builder alerta = new AlertDialog.Builder(this.Context);
                            alerta.SetTitle("Mensaje de alerta");
                            alerta.SetIcon(Resource.Drawable.alertaNuevo);
                            alerta.SetMessage("El servicio de Internet no se encuentra disponible, por favor revise su conexión e intente ingresar nuevamente");
                            alerta.SetNegativeButton("Salir", HandleButtonClick);
                            alerta.SetCancelable(false);
                            alerta.Create();
                            alerta.Show();
                        }
                    }
                    else { Toast.MakeText(this.Activity, "Usted ha digitado un usuario y contraseña incorrecta.", ToastLength.Short).Show(); }

                }
                catch (Exception ex) { Console.WriteLine("Error en autenticación " + ex.ToString()); }

            };

            return view;
        }

        private void HandleButtonClick(object sender, DialogClickEventArgs e)
        {
            Java.Lang.JavaSystem.Exit(0);
        }


        private void HandleButonContinuar(object sender, DialogClickEventArgs e)
        {
            Intent intent = new Intent(this.Activity, typeof(SplashActivity));
            StartActivity(intent);
        }


        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            //VERIFICAR SI HAY CONECTIVIDAD

            try
            {
                //Se procede a cerrar la sesion, primero se procede a determinar la lista de las 
                //notificaciones que se encuentran en estado notificandose.
                string consulta = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorNotificadorEstado?POficina=" + codOficina + "&PCodNotificador=" + codNotificador + "&PEstado=2&PJornada=3";
                servicioCheckDB coneccion = new servicioCheckDB();
                var resultados = coneccion.ObtenerListaStrings(consulta, Activity);

                if (resultados != null)
                {
                    string fechaApertura = "";
                    DateTime localDate = DateTime.Now;
                    //string fechalineal = localDate.ToString("yyyyMMdd");
                    string fechaCompleja = localDate.ToString("o");

                    List<string> notificacionesPendientes = new List<string>();
                    for (int k = 0; k < resultados.Count; k++)
                    {
                        notificacionesPendientes.Add(resultados[k]["CodNotificacion"].ToString());
                    }

                    // se extrae de la base de datos la fecha de apertura 
                    ManejoBaseDatos.Abrir();
                    
                    ICursor mCursor = ManejoBaseDatos.Seleccionar("SELECT FechaHoraApertura FROM Autenticacion");
                    if (mCursor.MoveToFirst() && mCursor.Count == 1)
                    {
                        do
                        {
                            fechaApertura = mCursor.GetString(0);
                            Console.WriteLine("fecha de Apertura: " + fechaApertura);
                        }
                        while (mCursor.MoveToNext());
                    }
                    else
                    {
                        //Se genera una fecha de apertura
                        fechaApertura = fechaCompleja;
                    }
                    mCursor.Close();
                    ManejoBaseDatos.Cerrar();

                    //se crea la instancial de la clase cierreJornada

                    CierreJornada cierre = new CierreJornada()
                    {
                        codigo = Guid.NewGuid().ToString(),
                        Apertura = fechaApertura,
                        OficialNotificador = new OficialNotificador
                        {
                            CodNotificador = FragmentLogin.codNotificador,
                        },
                        Notificaciones = new List<NotificacionFisica>(),
                        Justificacion = "",
                        Cierre = fechaCompleja,
                    };

                    foreach (var codigoNotificacion in notificacionesPendientes)
                    {
                        cierre.Notificaciones.Add(new NotificacionFisica { CodNotificacion = Convert.ToInt32(codigoNotificacion) });
                    }

                    string requestCierre = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/CierreJornadaOficialNotificador";
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(cierre);
                    Console.WriteLine("JSON: "+json);

                    bool respuesta = coneccion.envioDatosWeb(requestCierre, json, this.Activity);
                    if (respuesta)
                    {
                        //observaciones.Text = "";
                        Toast.MakeText(Activity, "Cierre de apertura exitoso.", ToastLength.Short).Show();
                        //Se dirige al usuario a la pagina de buzones donde podra realizar el inicio de apertura.
                        Toast.MakeText(this.Activity, "Cargando Datos ", ToastLength.Long).Show();
                        Intent intent = new Intent(this.Activity, typeof(SplashActivity));
                        StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(Activity, "Error al intentar cerar apertura.", ToastLength.Short).Show();
                    }

                }
            }
            catch (Exception ex) { Console.WriteLine("Error en manejo de boton positivo: "+ex.ToString()); }


        }


        public async Task<AuthenticationResult> Authenticate(string authority, string resource, string clientId, string returnUri)
        {
            var authContext = new AuthenticationContext(authority);
            if (authContext.TokenCache.ReadItems().Any())
            {
                authContext.TokenCache.Clear();
                authContext = new AuthenticationContext(authContext.TokenCache.ReadItems().First().Authority);
            }
    

            var uri = new Uri(returnUri);
            var platformParams = new PlatformParameters(Activity);
            var authResult = await authContext.AcquireTokenAsync(resource, clientId, uri, platformParams);
            return authResult;
        }

    }
}