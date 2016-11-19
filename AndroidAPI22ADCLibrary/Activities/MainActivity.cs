using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using AndroidAPI22ADCLibrary.Fragments;
using Android.Support.Design.Widget;
using Android.Gms.Common;
using Android.Util;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using System;
using AndroidAPI22ADCLibrary.Helpers;
using System.Collections.Generic;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Android.Widget;
using Android.Database;
using static AndroidAPI22ADCLibrary.Helpers.servicioCheckDB;

namespace AndroidAPI22ADCLibrary.Activities
{
    [Activity(Label = "Notificaciones", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, Icon = "@drawable/Icon")]
    //[Activity(Label = "Home")]
    public class MainActivity : BaseActivity
    {
        public static readonly int InstallGooglePlayServicesId = 1000;
        public static readonly string Tag = "XamarinMap";

        //public const string EXTRA_INFO_MAIN = "";
        //public const string EXTRA_CODIGO_NOTIFICACION = "";
        //public const string EXTRA_NOTIFICANDO = "";

        private string codigoNotificacion = "";
        private string info = "";
        private string notificado = "";

        DrawerLayout drawerLayout;
        NavigationView navigationView;

        private bool _isGooglePlayServicesInstalled;

        private GoogleMap mapa;//Objeto de la clase de googleMap que contiene el mapa
        private SupportMapFragment fragmentoMapa;//El fragmento del mapa que se va a mostrar
    
        //Coordenadas de la ubicación
        private static LatLng coordenadasMapa = new LatLng(0,0);

        public double latitud = 0;//Coordenadas de la latitud
        public double longitud = 0;//Coordenadas de la longitud

        protected override int LayoutResource
        {
            get
            {
                return Resource.Layout.main;
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {

            base.OnActivityResult(requestCode, resultCode, data);
            //AuthenticationAgentContinuationHelper.SetAuthenticationAgentContinuationEventArgs(requestCode, resultCode, data);
            
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (!string.IsNullOrEmpty(Intent.GetStringExtra("EXTRA_CODIGO_NOTIFICACION")))
            {
                codigoNotificacion = Intent.GetStringExtra("EXTRA_CODIGO_NOTIFICACION");
                Console.WriteLine("codigo notificacion en Main "+ codigoNotificacion);
            }
            else
            {
                //HACK PARA HACER PRUEBAS
                //codigoNotificacion = "4";
            }

            if (!string.IsNullOrEmpty(Intent.GetStringExtra("EXTRA_INFO")))
            {
                info = Intent.GetStringExtra("EXTRA_INFO");
                Console.WriteLine("info en Main "+info);
            }

            drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            //Set hamburger items menu
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);

            //setup navigation view
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            StartService(new Intent(this, typeof(Helpers.servicioLocalizacion)));

            //StartService(new Intent(this, typeof(Helpers.servicioCheckDB)));

            //handle navigation

            if (navigationView!=null)
            {
                if (FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal) || FragmentLogin.supervisor.Equals("True", StringComparison.Ordinal))
                {
                    navigationView.InflateMenu(Resource.Menu.adminMenu);
                }
                else
                {
                    navigationView.InflateMenu(Resource.Menu.notifMenu);
                }
            }

            //diferentes casos en la navegacion
            navigationView.NavigationItemSelected += (sender, e) =>
            {
                e.MenuItem.SetChecked(true);

                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.notif_reportes:
                        ListItemClicked(1);
                        break;
                    case Resource.Id.buzones:
                        this.Finish();
                        //ListItemClicked(1);
                        //Intent intent = new Intent(this, typeof(MailBoxes));
                        //intent.PutExtra("");
                        //StartActivity(intent);
                        break;
                    case Resource.Id.notif_jornada:

                        if (!coneccionInternet.verificaConeccion(this))
                        {
                            AlertDialog.Builder alerta = new AlertDialog.Builder(this);
                            alerta.SetTitle("Mensaje de alerta");
                            alerta.SetIcon(Resource.Drawable.alertaNuevo);
                            alerta.SetMessage("¿Desea cerrar la jornada laboral?");
                            alerta.SetPositiveButton("Cerrar sesión", HandleButtonClick);
                            alerta.SetNegativeButton("Cerrar ventana", HandleNegativeClick);
                            alerta.SetCancelable(false);
                            alerta.Create();
                            alerta.Show();
                        }
                        //ListItemClicked(3);
                        break;
                    case Resource.Id.notif_cierre:
                        //ListItemClicked(4);

                        break;

                        //administrdor

                    case Resource.Id.admin_rolnocturno:
                        ListItemClicked(4);
                        break;
                    case Resource.Id.admin_reportes:
                        ListItemClicked(1);
                        break;
                    case Resource.Id.admin_cierre:
                        Toast.MakeText(this,"En construccion",ToastLength.Short).Show();
                        break;


                }
                //Snackbar.Make(drawerLayout, "You selected: " + e.MenuItem.TitleFormatted, Snackbar.LengthLong).Show();
                drawerLayout.CloseDrawers();
            };

            //if first time you will want to go ahead and click first item.
            _isGooglePlayServicesInstalled = TestIfGooglePlayServicesIsInstalled();
            Console.WriteLine("--fzeledon valor de variable INFO: "+info);

            if (savedInstanceState == null && String.IsNullOrEmpty(info))
            {
                ListItemClicked(0);
            }
            else
            {
                if (String.IsNullOrEmpty(info))
                {
                    //ListItemClicked(0);
                }
                if (info.Equals("DETALLE", StringComparison.Ordinal))
                {
                    Android.Support.V4.App.Fragment fragment = Fragments.Fragment2.NewInstance(codigoNotificacion);
                    SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
                }
                if (info.Equals("REPORTES", StringComparison.Ordinal))
                {
                    ListItemClicked(1);
                }
                if (info.Equals("MAPS", StringComparison.Ordinal))
                {
                    ListItemClicked(2);
                }
                if (info.Equals("EditarNotificacion", StringComparison.Ordinal)) 
                {
                    ListItemClicked(3);
                }
                if (info.Equals("ROLNOCTURNO", StringComparison.Ordinal))
                {
                    ListItemClicked(4);
                }
                if (info.Equals("ReintentoNotificacion", StringComparison.Ordinal))
                {
                    Android.Support.V4.App.Fragment fragment = Fragments.ReintentoNotificacion.NewInstance(codigoNotificacion);
                    SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
                }
            }

        }

        private void HandleNegativeClick(object sender, DialogClickEventArgs e)
        {
            //throw new NotImplementedException();
            var dialog = (AlertDialog)sender;
            dialog.Dismiss();

        }

        private void HandleButtonClick(object sender , DialogClickEventArgs e)
        {
            try
            {
                //Se procede a cerrar la sesion, primero se procede a determinar la lista de las 
                //notificaciones que se encuentran en estado notificandose.
                string consulta = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorNotificadorEstado?POficina=" + FragmentLogin.codOficina + "&PCodNotificador=" + FragmentLogin.codNotificador + "&PEstado=2&PJornada=3";
                servicioCheckDB coneccion = new servicioCheckDB();
                var resultados = coneccion.ObtenerListaStrings(consulta,this);

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
                    Console.WriteLine("JSON: " + json);

                    bool respuesta = coneccion.envioDatosWeb(requestCierre, json, this);
                    if (respuesta)
                    {
                        //observaciones.Text = "";
                        Toast.MakeText(this, "Cierre de apertura exitoso.", ToastLength.Short).Show();
                        //Se dirige al usuario a la pagina de buzones donde podra realizar el inicio de apertura.
                    }
                    else
                    {
                        Toast.MakeText(this, "Error al intentar cerar apertura.", ToastLength.Short).Show();
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("Error en manejo de boton positivo: " + ex.ToString()); }
        }



        int oldPosition = -1;

        private void ListItemClicked(int position)
        {
            //this way we don't load twice, but you might want to modify this a bit.
            if (position == oldPosition)
                return;

            oldPosition = position;

            Android.Support.V4.App.Fragment fragment = null;

            switch (position)
            {
                case 0:
                    //Se resetea el fragmento del mapa
                    resetearMapa();
                    fragment = FragmentLogin.NewInstance();
                    //SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
                    break;
                case 1:
                    //Se resetea el fragmento del mapa
                    resetearMapa();
                    fragment = Fragment1.NewInstance();
                    //SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
                    break;
                case 2:
                    //Se resetea el fragmento del mapa
                    resetearMapa();
                    fragment = FragmentMap.NewInstance();
                    //SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
                    break;
                case 3:
                    //Se resetea el fragmento del mapa
                    resetearMapa();
                    fragment = Impresion.NewInstance(codigoNotificacion, notificado);
                    //SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
                    break;
                case 4:
                    //Se resetea el fragmento del mapa
                    resetearMapa();
                    fragment = RolNocturno.NewInstance();
                   // SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
                    break;
                case 5:
                    //Es el de prueba
                    fragment = Fragment2.NewInstance("12345");
                    break;
                default:
                    Console.WriteLine("Elemento seleccionado " + position.ToString());
                    break;

            }

            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    drawerLayout.OpenDrawer(Android.Support.V4.View.GravityCompat.Start);
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void cambiarTituloAplicacion(string titulo)
        {
            this.Title = titulo;//Se agrega el titulo
        }

        public void mostrarVentanaAgregartexto(int idCampo, string titulo)
        {
            //Se crea el layoutInflater en base a la actividad que esta ejecutando el fragmento actual
            LayoutInflater layoutInflater = LayoutInflater.From(this);
            //Se crea la vista que contiene los controles que muestra la ventana de observaciones
            View contenedorVentanaModal = layoutInflater.Inflate(Resource.Layout.VentanaObservaciones, null);

            //Se instancia el constructor de la ventana
            Android.App.AlertDialog.Builder constructorVentana = new Android.App.AlertDialog.Builder(this);
            constructorVentana.SetView(contenedorVentanaModal);//Se agrega la vista que va a mostrar la ventana de observaciones

            //Se crea la instancia del edit text donde se colocara la observación
            Android.Widget.EditText observacion = (Android.Widget.EditText)contenedorVentanaModal.FindViewById(Resource.Id.txtObservacionesVentana);
            //Se cambia el titulo de la ventana
            Android.Widget.TextView tituloVentana = (Android.Widget.TextView)contenedorVentanaModal.FindViewById(Resource.Id.txtTituloVentana);
            tituloVentana.Text = titulo;

            //Se crea la instancia del edit text al que se le asignara el texto de la ventana
            Android.Widget.EditText campoTexto = (Android.Widget.EditText)this.FindViewById(idCampo);
            //Se evalua el campo de texto, si no esta vacio se asigna al campo de texto de la ventana
            if (campoTexto.Text != "")
                observacion.Text = campoTexto.Text;//Se realiza la asignación del texto

            //Se agregan los eventos para los botones de cancelar y de Aceptar
            constructorVentana.SetCancelable(false).SetPositiveButton("Aceptar", (sender, args) => {
                //Al presionar el botón de Aceptar se asigna el texto agregado en la ventana
                //en el campo de texto
                campoTexto.Text = observacion.Text;

            }).SetNegativeButton("Cancelar", (sender, args) => {
                //Evento al presionar el botón de cancelar
            }).SetNeutralButton("Borrar todo", (System.EventHandler<DialogClickEventArgs>)null);

            //Se crea la ventana de dialogo y se instancia con el constructor de la ventana creado anteriormente
            Android.App.AlertDialog ventana = constructorVentana.Create();
            ventana.Show();//Se muestra la ventana

            //Se crea una instancia del botón neutral de la ventana
            var btnBorrar = ventana.GetButton((int)DialogButtonType.Neutral);

            //Se crea el evento al presionar el botón de borrar
            btnBorrar.Click += (sender, args) => {

                observacion.Text = "";//Se limpia el texto de la ventana

            };

        }


        private bool TestIfGooglePlayServicesIsInstalled()
        {
            int queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info(Tag, "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                string errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error(Tag, "There is a problem with Google Play Services on this device: {0} - {1}", queryResult, errorString);
                Dialog errorDialog = GoogleApiAvailability.Instance.GetErrorDialog(this, queryResult, InstallGooglePlayServicesId);
                Fragments.ErrorDialogFragment dialogFrag = new Fragments.ErrorDialogFragment(errorDialog);

                dialogFrag.Show(FragmentManager, "GooglePlayServicesDialog");
            }
            return false;
        }

        public void IniciarFragmentoMapa(int frame)
        {
            try
            {
                //Se obtiene el fragmento del mapa
                fragmentoMapa = SupportFragmentManager.FindFragmentByTag("map") as SupportMapFragment;
                //Si es null se va a crear
                if (fragmentoMapa == null)
                {
                    //Se agregan las opciones del mapa
                    GoogleMapOptions mapOptions = new GoogleMapOptions()
                        .InvokeMapType(GoogleMap.MapTypeSatellite)
                        .InvokeZoomControlsEnabled(true)
                        .InvokeCompassEnabled(true);

                    //Se crea la instancia del mapa, se agregan las opciones y se carga en el frame
                    Android.Support.V4.App.FragmentTransaction transaccion = SupportFragmentManager.BeginTransaction();
                    fragmentoMapa = SupportMapFragment.NewInstance(mapOptions);
                    transaccion.Add(frame, fragmentoMapa, "map");
                    transaccion.Commit();
                }
                
            }
            catch (Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("MainActivity.IniciarFragmentoMapa " + ex.Message + " " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Se elimina el fragmento del mapa
        /// </summary>
        public void resetearMapa()
        {
            try
            {
                //Si el fragmento del mapa es diferente de nulo se remueve
                if (SupportFragmentManager.FindFragmentByTag("map") != null)
                {
                    //Se remueve el fragmento del mapa
                    SupportFragmentManager.BeginTransaction().Remove(SupportFragmentManager.FindFragmentByTag("map")).Commit();
                }
            }
            catch (Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("MainActivity.resetearMapa " + ex.ToString() + " " + ex.StackTrace);
            }

        }

        public void habilitarMenuLateral(bool habilitar)
        {
            //Basado en un valor (verdadero o falso) oculta o muestra el menú lateral
            SupportActionBar.SetDisplayHomeAsUpEnabled(habilitar);
        }

        public void navegacionFragment(Android.Support.V4.App.Fragment fragment)
        {
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
        }

        public void AgregarUbicacionMapa(double lat, double longi)
        {
            try
            {
                mapa = null;//Se limpia el mapa para colocar otra ubicación
                if (mapa == null)
                {
                    mapa = fragmentoMapa.Map;

                    if (mapa != null)
                    {

                        latitud = servicioLocalizacion.latitud;
                        longitud =servicioLocalizacion.longitud;

                        Console.WriteLine("--fzeledon coordenadas acutales latitud: " + latitud.ToString() + " ,longitud: " + longitud.ToString());

                        if (latitud != 0 && longitud != 0)
                        {
                            coordenadasMapa.Latitude = latitud;
                            coordenadasMapa.Longitude = longitud;
                        }
                        else
                        {
                            coordenadasMapa.Latitude = lat;
                            coordenadasMapa.Longitude = longi;
                        }

                        //Se crea un marcador
                        MarkerOptions marcador = new MarkerOptions();
                        //Se agrega la posición latitud y longitud
                        marcador.SetPosition(coordenadasMapa);
                        //mapa.Clear();
                        mapa.AddMarker(marcador);//Se agrega el marcador en el mapa

                        // Se crea una instancia del la camara y se agrega un zoom a las coordenadas agregadas
                        CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(coordenadasMapa, 19);
                        mapa.MoveCamera(cameraUpdate);//Se mueve la camara del mapa
                    }
                    else
                        Console.WriteLine("MAPA NULL FRAGMENT MAP");
                }

            }
            catch (Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("MainActivity.AgregarUbicacionMapa " + ex.Message + " " + ex.StackTrace);
            }
        }

        public void AgregarUbicacionMapaLista(List<LatLng> coordenadasList)
        {
            try
            {
                mapa = null;//Se limpia el mapa para colocar otra ubicación
                if (mapa == null)
                {
                    mapa = fragmentoMapa.Map;

                    if (mapa != null)
                    {
                        Console.WriteLine("NOT NULL MAP");
                        //double totalLatitud = 0;
                        //double totalLongitud = 0;
                        //int total = 0;
                        foreach (var item in coordenadasList)
                        {
                            //totalLatitud = totalLatitud + item.Latitude;
                            //totalLongitud = totalLongitud + item.Longitude;
                            MarkerOptions mMarker = new MarkerOptions();
                            mMarker.SetPosition(item);
                            mapa.AddMarker(mMarker);
                            //total=total+1;
                        }

                        //double promLatitud = totalLatitud / total;
                        //double promLongitud = totalLongitud / total;

                        //if (servicioLocalizacion.latitud==0)
                        //if(servicioLocalizacion.)

                        LatLng promLatLng = new LatLng(servicioLocalizacion.latitud,servicioLocalizacion.longitud);

                        // Se crea una instancia del la camara y se agrega un zoom a las coordenadas agregadas
                        CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(promLatLng, 19);
                        mapa.MoveCamera(cameraUpdate);//Se mueve la camara del mapa
                    }
                    else
                        Console.WriteLine("MAPA NULL FRAGMENT MAP");
                }

            }
            catch (Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("MainActivity.AgregarUbicacionMapa " + ex.Message + " " + ex.StackTrace);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Esto fue lo que se agrego
        /// <summary>
        /// Se carga la lista de posiciones con color
        /// </summary>
        /// <param name="coordenadasList"></param>
        public void AgregarUbicacionMapaListaColor(List<KeyValuePair<LatLng, bool>> coordenadasList)
        {
            try
            {
                mapa = null;//Se limpia el mapa para colocar otra ubicación
                if (mapa == null)
                {
                    mapa = fragmentoMapa.Map;

                    if (mapa != null)
                    {
                        MarkerOptions mMarker;
                        Console.WriteLine("NOT NULL MAP");
                        //double totalLatitud = 0;
                        //double totalLongitud = 0;
                        //int total = 0;
                        foreach (var item in coordenadasList)
                        {
                            //totalLatitud = totalLatitud + item.Latitude;
                            //totalLongitud = totalLongitud + item.Longitude;
                            mMarker = new MarkerOptions();
                            mMarker.SetPosition(item.Key);
                            if (!item.Value)
                            {
                                mMarker.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueRed));
                            }
                            else
                            {
                                mMarker.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueBlue));
                            }

                            mapa.AddMarker(mMarker);
                            //total=total+1;
                        }

                        //double promLatitud = totalLatitud / total;
                        //double promLongitud = totalLongitud / total;

                        //if (servicioLocalizacion.latitud==0)
                        //if(servicioLocalizacion.)

                        LatLng promLatLng = new LatLng(servicioLocalizacion.latitud, servicioLocalizacion.longitud);
                        mMarker = new MarkerOptions();
                        mMarker.SetPosition(promLatLng);
                        mMarker.SetIcon(BitmapDescriptorFactory.DefaultMarker(BitmapDescriptorFactory.HueYellow));
                        mapa.AddMarker(mMarker);

                        // Se crea una instancia del la camara y se agrega un zoom a las coordenadas agregadas
                        CameraUpdate cameraUpdate = CameraUpdateFactory.NewLatLngZoom(promLatLng, 15);
                        mapa.MoveCamera(cameraUpdate);//Se mueve la camara del mapa
                    }
                    else
                        Console.WriteLine("MAPA NULL FRAGMENT MAP");
                }

            }
            catch (Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("MainActivity.AgregarUbicacionMapa " + ex.Message + " " + ex.StackTrace);
            }
        }

    }
}


