using Android.App;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.Design.Widget;
using AndroidAPI22ADCLibrary.Fragments;
using Android.Content.PM;
using Android.Views;
using System;
using Android.Runtime;
using Android.Widget;
using Android.Content;
using Android.Graphics.Drawables;

using SupportFragmentManager = Android.Support.V4.App.FragmentManager;
using Android.Graphics;
using AndroidAPI22ADCLibrary.Helpers;
using static AndroidAPI22ADCLibrary.Helpers.servicioCheckDB;
using System.Collections.Generic;
using Android.Database;

namespace AndroidAPI22ADCLibrary.Activities
{
    //[Activity(Label = "MailBoxes", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, Icon = "@drawable/Icon")]
    [Activity(Label = "Buzones")]
    public class MailBoxes : AppCompatActivity
    {
        private DrawerLayout mDrawerLayout;
        private static TabAdapter myTabAdpater;
        private static ViewPager viewPager;
        public static int currentPage=0;
        private bool supervisor;
        //private Dialog dialog ;

        public const string EXTRA_SUPERVISOR = "";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.mailboxes);

            //Extrayendo informacion de login para determinar el nivel del usuario
            if (Intent.GetStringExtra(EXTRA_SUPERVISOR).Equals("supervisor", StringComparison.Ordinal))
                supervisor = true;
            else
                supervisor = false;

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            //Comportamiento de actionBar a ToolBar
            SetSupportActionBar(toolBar);


            SupportActionBar ab = SupportActionBar;
            ab.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            ab.SetDisplayHomeAsUpEnabled(true);
            ab.SetDisplayShowHomeEnabled(true);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            TextView txtUser = FindViewById<TextView>(Resource.Id.userDescription);
            txtUser.Text = "Notificador: "+FragmentLogin.codNotificador + "\r\n" + "Codigo de Oficina: "+FragmentLogin.codOficina ;

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);

            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
                navigationView.Menu.Clear();
                if (FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal) || FragmentLogin.supervisor.Equals("True", StringComparison.Ordinal))
                {
                    navigationView.InflateMenu(Resource.Menu.adminMenu);
                }
                else
                {
                    navigationView.InflateMenu(Resource.Menu.notifMenu);
                }
                
            }


            navigationView.NavigationItemSelected += (sender, e) =>
            {
                e.MenuItem.SetChecked(true);

                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.notif_reportes:
                        Intent intent = new Intent(this, typeof(MainActivity));
                        intent.PutExtra("EXTRA_INFO", "REPORTES");
                        StartActivity(intent);
                        break;
                    case Resource.Id.notif_jornada:
                        {
                            Android.App.AlertDialog.Builder alerta = new Android.App.AlertDialog.Builder(this);
                            alerta.SetTitle("Mensaje de alerta");
                            alerta.SetIcon(Resource.Drawable.alertaNuevo);
                            alerta.SetMessage("¿Desea cerrar la jornada laboral?");
                            alerta.SetPositiveButton("Cerrar sesión", HandleButtonClick);
                            alerta.SetNegativeButton("Cerrar ventana", HandleNegativeClick);
                            alerta.SetCancelable(false);
                            alerta.Create();
                            alerta.Show();
                        }
                        //Toast.MakeText(this, "En construcción", ToastLength.Long).Show();
                        //ListItemClicked(3);
                        break;
                    case Resource.Id.notif_cierre:
                        this.Finish();
                        //Toast.MakeText(this, "En construcción", ToastLength.Long).Show();
                        break;

                    case Resource.Id.admin_reportes:
                        Intent intent1 = new Intent(this, typeof(MainActivity));
                        intent1.PutExtra("EXTRA_INFO", "REPORTES");
                        StartActivity(intent1);
                        break;
                    case Resource.Id.admin_rolnocturno:
                        //Toast.MakeText(this, "En construcción", ToastLength.Long).Show();
                        Intent intent2 = new Intent(this, typeof(MainActivity));
                        intent2.PutExtra("EXTRA_INFO", "ROLNOCTURNO");
                        StartActivity(intent2);
                        break;

                    case Resource.Id.admin_cierre:
                        //Toast.MakeText(this, "En construcción", ToastLength.Long).Show();
                        this.Finish();
                        break;

                }
                //Snackbar.Make(drawerLayout, "You selected: " + e.MenuItem.TitleFormatted, Snackbar.LengthLong).Show();
                //drawerLayout.CloseDrawers();
            };

            TabLayout tabs = FindViewById<TabLayout>(Resource.Id.tabs);
            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
            SetUpViewPager(viewPager);
            tabs.SetupWithViewPager(viewPager);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            fab.Click += (o, e) =>
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("EXTRA_INFO", "MAPS");
                StartActivity(intent);             
            };
        }

        private void HandleButtonClick(object sender, DialogClickEventArgs e)
        {
            try
            {
                //Se procede a cerrar la sesion, primero se procede a determinar la lista de las 
                //notificaciones que se encuentran en estado notificandose.
                string consulta = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Notificaciones/ListarNotificacionesPorNotificadorEstado?POficina=" + FragmentLogin.codOficina + "&PCodNotificador=" + FragmentLogin.codNotificador + "&PEstado=2&PJornada=3";
                servicioCheckDB coneccion = new servicioCheckDB();
                var resultados = coneccion.ObtenerListaStrings(consulta, this);

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

        private void HandleNegativeClick(object sender, DialogClickEventArgs e)
        {
            //throw new NotImplementedException();
            var dialog = (Android.App.AlertDialog)sender;
            dialog.Dismiss();

        }

        private void BtnDismiss_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            
        }

        private void SetUpViewPager(ViewPager viewPager)
        {
            myTabAdpater = new TabAdapter(SupportFragmentManager);

            // NuevasAImprimir = 1,
            // AsignarParaNotificar = 2,
            // Notificandose = 3,
            // NotificacionCompletada = 4,
            // ResultadoEnCorreccion = 5,
            // PendienteDeDistribucion = 6,
            // EntregadaAlDespacho = 7

            if (supervisor)
            {
                //Pagina 0
                FragmentList fragmentInstance1 = new FragmentList();
                Bundle bun1 = new Bundle();
                bun1.PutInt("someInt", 1);
                fragmentInstance1.Arguments = bun1;
                myTabAdpater.AddFragment(fragmentInstance1, "Nuevas a imprimir");

                //Pagina1
                FragmentList fragmentInstance2 = new FragmentList();//AsignarParaNotificar
                Bundle bun2 = new Bundle();
                bun2.PutInt("someInt", 2);
                fragmentInstance2.Arguments = bun2;
                myTabAdpater.AddFragment(fragmentInstance2, "A notificar"); //se registra el resultado

                //Pagina 2
                FragmentList fragmentInstance3 = new FragmentList();
                Bundle bun3 = new Bundle();
                bun3.PutInt("someInt", 3);
                fragmentInstance3.Arguments = bun3;
                myTabAdpater.AddFragment(fragmentInstance3, "Notificándose");// supervisor revisa el buzon filtrado por los notificadores.
                viewPager.Adapter = myTabAdpater;
                viewPager.AddOnPageChangeListener(new MyPageChangeListener(this));

                //Pagina 3
                FragmentList fragmentInstance4 = new FragmentList();
                Bundle bun4 = new Bundle();
                bun4.PutInt("someInt", 4);
                fragmentInstance4.Arguments = bun4;
                myTabAdpater.AddFragment(fragmentInstance4, "A entregar");// supervisor realiza la aprobacion o rechazo de notificaciones, se entiende como Notificaciones completadas
                viewPager.Adapter = myTabAdpater;
                viewPager.AddOnPageChangeListener(new MyPageChangeListener(this));

                //Pagina 4
                FragmentList fragmentInstance5 = new FragmentList();
                Bundle bun5 = new Bundle();
                bun5.PutInt("someInt", 6);
                fragmentInstance5.Arguments = bun5;
                myTabAdpater.AddFragment(fragmentInstance5, "Entregándose");// supervisor solicita correcion en una notificacion con observaciones incorrectas
                viewPager.Adapter = myTabAdpater;
                viewPager.AddOnPageChangeListener(new MyPageChangeListener(this));

                ////Pagina 4 
                //FragmentList fragmentInstance4 = new FragmentList();
                //Bundle bun4 = new Bundle();
                //bun4.PutInt("someInt", 6);
                //fragmentInstance4.Arguments = bun4;
                //myTabAdpater.AddFragment(fragmentInstance4, "Pendiente de distribución");// supervisor solicita correcion sobew notificacion
                //viewPager.Adapter = myTabAdpater;
                //viewPager.AddOnPageChangeListener(new MyPageChangeListener(this));
            }
            else
            {
                //Pagina 0
                FragmentList fragmentInstance1 = new FragmentList();
                Bundle bun1 = new Bundle();
                bun1.PutInt("someInt", 2);
                fragmentInstance1.Arguments = bun1;
                myTabAdpater.AddFragment(fragmentInstance1, "A notificar"); //se registra el resultado

                //Pagina 1
                FragmentList fragmentInstance2 = new FragmentList();
                Bundle bun2 = new Bundle();
                bun2.PutInt("someInt", 3);
                fragmentInstance2.Arguments = bun2;
                myTabAdpater.AddFragment(fragmentInstance2, "Notificándose");// supervisor solicita correcion sobew notificacion
                viewPager.Adapter = myTabAdpater;
                viewPager.AddOnPageChangeListener(new MyPageChangeListener(this));

                //Pagina 2
                FragmentList fragmentInstance3 = new FragmentList();
                Bundle bun3 = new Bundle();
                bun3.PutInt("someInt", 5);
                fragmentInstance3.Arguments = bun3;
                myTabAdpater.AddFragment(fragmentInstance3, "A corregir");// supervisor solicita correcion sobew notificacion
                viewPager.Adapter = myTabAdpater;
                viewPager.AddOnPageChangeListener(new MyPageChangeListener(this));

            }
        }

        private void SetUpDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {
                e.MenuItem.SetChecked(true);
                mDrawerLayout.CloseDrawers();
            };
        }

        public override void OnBackPressed()
        {
            
            base.OnBackPressed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {

            //Android.Support.V4.App.Fragment fragment = null;
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);

                //case Resource.Id.notif_reportes:
                //    //Toast.MakeText(this,"Presionado reportes",ToastLength.Short).Show();
                //    Console.WriteLine("Reportes");
                //    //fragment = Fragment1.NewInstance();
                //    //SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();
                //    return true;
            }

        }

        
        public class MyPageChangeListener : Java.Lang.Object, ViewPager.IOnPageChangeListener
        {
            Context _context;

            public MyPageChangeListener(Context context)
            {
                _context = context;
            }

            #region IOnPageChangeListener implementation
            public void OnPageScrollStateChanged(int p0)
            {
            }

            public void OnPageScrolled(int p0, float p1, int p2)
            {
            }
            
            public void OnPageSelected(int position)
            {
                //Toast.MakeText(_context, "Current page " + position, ToastLength.Short).Show();
                myTabAdpater.Refresh(position);
                currentPage = position;
            }
            #endregion
        }
    }
}