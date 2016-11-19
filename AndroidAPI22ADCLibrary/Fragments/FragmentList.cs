using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Android.Support.V7.Widget;
using Android.Content.Res;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Graphics;
using Android.Support.V4;
using Android.Runtime;
using Android.App;
using Android.Support.V4.View;
using AndroidAPI22ADCLibrary.Activities;
using Android.Database.Sqlite;
using Android.Database;
using System.Data;
using AndroidAPI22ADCLibrary.Fragments;
using static AndroidAPI22ADCLibrary.Helpers.servicioCheckDB;
using System.Net;
using System.IO;
using Newtonsoft.Json;


namespace AndroidAPI22ADCLibrary.Fragments
{   
    public class FragmentList : SupportFragment
    {
        private static List<bool> imageChecked = new List<bool>();    
        private List<string> notificaciones = new List<string>();
        private List<string> notificacionesVerbose = new List<string>();
        private List<string> detailActivityTitle = new List<string>();
        private List<string> codigoNotificacion = new List<string>();
        private List<string> codigoNotificador = new List<string>();
        private List<string> codigoReferencia = new List<string>();


        //private static List<string> numeroDeVisitas = new List<string>();
        //private static List<string> horarioDeEntrega = new List<string>();
        //private static List<string> urgenciaLista = new List<string>();

        private static Activity mActivity;
        private static bool enableMenuOption = false;
        private static bool debugMode = false   ;
        private RecyclerView recyclerView;
        public static string motivo = "";



        //public static string codNotificacion = "";
        //public static string info = "";


        private string supervisor = FragmentLogin.supervisor;
        
        //private static SimpleViewHolder simpleHolder;
        private static SimpleStringRecyclerViewAdapter SSRVA;
        private Android.Support.V7.Widget.SearchView _searchView;
        private int querySelect = 0;
        public static int estado = 1;
        private string query = "";
        //private SQLiteDatabase sqldb;
        

        public static FragmentList newInstance(int queryType)
        {
            FragmentList f = new FragmentList();
            Bundle args = new Bundle();
            args.PutInt("someInt", queryType);

            if (args != null)
            {
                f.Arguments = args;
            }
            else { }
            return f;
        }
    
        public override void OnCreate(Bundle savedInstanceState)
        {
            querySelect = Arguments.GetInt("someInt");
            estado = querySelect;
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            mActivity = Activity;       
        }

        public override void OnResume()
        {
            base.OnResume();
            refreshRecyclerView();
            //Toast.MakeText(this.Activity,"Called OnResume()",ToastLength.Short).Show();
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            Activity.InvalidateOptionsMenu();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            //base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == 1)
            { // 1 is an arbitrary number, can be any int
              // This is the return result of your DialogFragment
                if (resultCode == 1)
                { // 1 is an arbitrary number, can be any int
                  // Now do what you need to do after the dialog dismisses.
                    List<string> upload = new List<string>();
                    try
                    {
                        if (coneccionInternet.verificaConeccion(this.Context))
                        {
                           // int top = imageChecked.Count;
                            for (int k = 0; k < imageChecked.Count; k++)
                            {
                                //revisando todas las posiciones que se han marcado para asignar
                                if (imageChecked[k])
                                {
                                    //se actualiza el estado de las notificaciones a estado AsignarParaNotificar en caso de ser admin - o se auto-asignan en caso de los notificadores 
                                    //se genera la lista con las notificaciones que se van a utilizar para abrir sesion
                                    upload.Add(codigoNotificacion[k]);
                                }
                            }
                            //refrescando recyclerView despues de realizar los cambios en las listas
                            //refreshRecyclerView();
                            if (supervisor.Equals("true", StringComparison.Ordinal) || supervisor.Equals("True", StringComparison.Ordinal))
                            {

                                //se va a evitar el uso de servicios por la falta de control de escritura de datos.

                                //Activity.StartService(new Intent(this.Activity, typeof(Helpers.ServicioMaps)));
                                //Activity.StartService(new Intent(this.Activity, typeof(Helpers.ServicioAsignacion)));

                                //////bajar notificaciones en estado 4-6
                                //Activity.StartService(new Intent(this.Activity, typeof(Helpers.servicioCheckDB)));

                                ////se elabora una lista con todos los elementos a asignar  

                                List<string> notificacionesAAsignar = new List<string>();

                                for (int k = 0; k < imageChecked.Count; k++)
                                {
                                    if (imageChecked[k])
                                    {
                                        notificacionesAAsignar.Add(codigoNotificacion[k]);
                                    }
                                }

                                DateTime localDate = DateTime.Now;
                                string fechaAnnoMesDia = localDate.ToString("yyyyMMdd");
                                string fechaCompleja = localDate.ToString("o");
                                string json = JsonConvert.SerializeObject(notificacionesAAsignar);
                                Console.WriteLine("Json:" +json);
                                string request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/AsignarNotificacionAlOficialNotificador?PCodSupervisor=" + FragmentLogin.codNotificador + "&PCodlNotificador=" + EditNameDialogFragment.codigoNotificadorSeleccionado + "&PFecha="+fechaAnnoMesDia+"";
                                servicioCheckDB coneccion = new servicioCheckDB();

                                bool respuesta = coneccion.envioDatosWeb(request, json, this.Activity);
                                if (respuesta)
                                {                
                                    Toast.MakeText(this.Activity, "Asignación exitosa.", ToastLength.Short).Show();         
                                }
                                else
                                {
                                    Toast.MakeText(this.Activity, "Error al intentar cerar apertura.", ToastLength.Short).Show();
                                }

                                string tempQuery = "";
                                int top = imageChecked.Count;

                                for (int k = 0; k < top; k++)
                                {
                                    //revisando todas las posiciones que se han marcado para asignar
                                    if (imageChecked[k])
                                    {

                                        // se actualiza la informacion en la base de datos 
                                        ManejoBaseDatos.Abrir();
                                        tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Estado", "'AsignarParaNotificar'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                        tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "CodNotificador", "'" + EditNameDialogFragment.codigoNotificadorSeleccionado + "'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                        tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "CalcularPosicion", "'S'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                        ManejoBaseDatos.Cerrar();
                                        // se remueven los elementos del recycler view y se refresca

                                        notificaciones.RemoveAt(k);
                                        notificacionesVerbose.RemoveAt(k);
                                        detailActivityTitle.RemoveAt(k);
                                        codigoNotificacion.RemoveAt(k);
                                        codigoReferencia.RemoveAt(k);
                                        codigoNotificador.RemoveAt(k);
                                        imageChecked.RemoveAt(k);
                                        SSRVA.NotifyItemRemoved(k);
                                        k = k - 1;
                                        top = top - 1;
                                    }
                                }

                            }
                            else
                            {
                                DateTime localDate = DateTime.Now;
                                string output = localDate.ToString("o");

                                // Se va registrar una apertura de sesion
                                AperturaJornada jornada = new AperturaJornada
                                {
                                    codigo = Guid.NewGuid().ToString(),
                                    Apertura = output,
                                    oficialNotificador = new ClassOficialNotificador
                                    {
                                        CodNotificador = FragmentLogin.codNotificador,
                                        Oficinas = new List<ClassDespacho>()
                                        {
                                            new ClassDespacho()
                                            {
                                                Codigo =FragmentLogin.codOficina,
                                            }
                                        }
                                    },
                                    Notificaciones = new List<ClassNotificaciones>(),
                                    Justificacion = "",
                                };
                                foreach (var codigosNotificaciones in upload)
                                {
                                    int tempInt = Convert.ToInt32(codigosNotificaciones);
                                    jornada.Notificaciones.Add(new ClassNotificaciones() { CodNotificacion = tempInt });
                                }

                                string request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/AperturaJornadaOficialNotificador";
                                //serializacion de datos
                                string json = Newtonsoft.Json.JsonConvert.SerializeObject(jornada);
                                Console.WriteLine("Json: " + json);

                                try
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
                                            Toast.MakeText(this.Activity, "Asignación exitosa", ToastLength.Short).Show();

                                            string tempQuery = "";
                                            int top = imageChecked.Count;
                                            for (int k = 0; k < top; k++)
                                            {
                                                //revisando todas las posiciones que se han marcado para asignar

                                                if (imageChecked[k])
                                                {
                                                    // se actualiza la informacion en la base de datos 
                                                    ManejoBaseDatos.Abrir();
                                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Estado", "'Notificandose'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "CalcularPosicion", "'S'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                                    ManejoBaseDatos.Cerrar();

                                                    // se remueven los elementos del recycler view y se refresca

                                                    //numeroDeVisitas.RemoveAt(k);
                                                    //horarioDeEntrega.RemoveAt(k);
                                                    //urgenciaLista.RemoveAt(k);

                                                    notificaciones.RemoveAt(k);
                                                    notificacionesVerbose.RemoveAt(k);
                                                    detailActivityTitle.RemoveAt(k);
                                                    codigoNotificacion.RemoveAt(k);
                                                    codigoReferencia.RemoveAt(k);
                                                    codigoNotificador.RemoveAt(k);
                                                    imageChecked.RemoveAt(k);
                                                    SSRVA.NotifyItemRemoved(k);
                                                    k = k - 1;
                                                    top = top - 1;
                                                }
                                            }
                                            //se registra en la base de datos una sesion abierta y la hora de apertura
                                            ManejoBaseDatos.Abrir();
                                            ManejoBaseDatos.Insertar("Autenticacion", "Autoasignacion,FechaHoraApertura", " 'S','"+output+"'");
                                            ManejoBaseDatos.Cerrar();

                                            refreshRecyclerView();
                                        }
                                    }
                                    //
                                }
                                catch (WebException webEx)
                                {
                                    Console.WriteLine("ERROR AL ABRIR JORNADA: "+webEx.ToString());
                                    Toast.MakeText(this.Activity,"Error de conexión",ToastLength.Long).Show();
                                    return;
                                }
                            }
                            // Se refresca el recyclerView
                            refreshRecyclerView();
                            Activity.StartService(new Intent(this.Activity, typeof(Helpers.ServicioMaps)));
                            //SERVICIO DE DESCARGA DE NOTIFICACIONES
                            Activity.StartService(new Intent(this.Activity, typeof(Helpers.servicioCheckDB)));

                        }
                        else
                        {
                            AlertDialog.Builder alerta = new AlertDialog.Builder(this.Context);
                            alerta.SetTitle("Mensaje de alerta");
                            alerta.SetIcon(Resource.Drawable.alertaNuevo);
                            alerta.SetMessage("El servicio de Internet no se encuentra disponible, por favor revise su conexión e intente nuevamente");
                            alerta.SetNegativeButton("Salir", HandleButtonClick);
                            alerta.SetCancelable(false);
                            alerta.Create();
                            alerta.Show();
                        }

                    }
                    catch (Exception ex) { Console.WriteLine("Error while selecting item: "+ex.ToString()); }
                }
                if (resultCode==2)
                {
                    imageChecked.Clear();
                    enableMenuOption = false;
                    mActivity.InvalidateOptionsMenu();
                }
            }

            if (requestCode == 2)
            {
                if (resultCode==1)
                {
                    try
                    {
                        int top = imageChecked.Count;
                        for (int k = 0; k < top; k++)
                        {
                            if (imageChecked[k])
                            {
                                ManejoBaseDatos.Abrir();
                                string tempQuery = "";
                                //se actualiza el estado de las notificaciones a estado AsignarParaNotificar en caso de ser admin - o se auto-asignan en caso de los notificadores 
                                if (supervisor.Equals("true", StringComparison.Ordinal) || supervisor.Equals("True", StringComparison.Ordinal))
                                {
                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Estado", "'AsignarParaNotificar'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "CodNotificador", "'" + EditNameDialogFragment.codigoNotificadorSeleccionado + "'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Reasignar", "'S'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                    //tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "PendienteSubir", "'S'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                }
                                
                                if (!string.IsNullOrEmpty(tempQuery))
                                {
                                    Console.WriteLine("Error actualizando base de datos: " + tempQuery);
                                }

                                //numeroDeVisitas.RemoveAt(k);
                                //horarioDeEntrega.RemoveAt(k);
                                //urgenciaLista.RemoveAt(k);

                                notificaciones.RemoveAt(k);
                                notificacionesVerbose.RemoveAt(k);
                                detailActivityTitle.RemoveAt(k);
                                codigoNotificacion.RemoveAt(k);
                                codigoReferencia.RemoveAt(k);
                                codigoNotificador.RemoveAt(k);
                                imageChecked.RemoveAt(k);
                                SSRVA.NotifyItemRemoved(k);
                                k = k - 1;
                                top = top - 1;
                                ManejoBaseDatos.Cerrar();
                            }
                        }
                        Activity.StartService(new Intent(this.Activity, typeof(Helpers.ServicioReasignacion)));
                        refreshRecyclerView();
                    }
                    catch (Exception ex) { Console.WriteLine("Error actualizando Re-Asignacion"+ex.ToString()); }
                }
            }

            //Request codes para aceptacion o rechazo de notificaciones
            if (requestCode == 3)
            {
                //PENDIENTE DE DISTRIBUCION
                if (resultCode==1)
                {
                    try
                    {
                        int top = imageChecked.Count;
                        for (int k = 0; k < top; k++)
                        {
                            if (imageChecked[k])
                            {
                                ManejoBaseDatos.Abrir();
                                string tempQuery = "";
                                //se actualiza el estado de las notificaciones a estado AsignarParaNotificar en caso de ser admin - o se auto-asignan en caso de los notificadores 
                                if (supervisor.Equals("true", StringComparison.Ordinal) || supervisor.Equals("True", StringComparison.Ordinal))
                                {
                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Estado", "'PendienteDeDistribucion'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "PendienteSubir", "'S'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                }

                                if (!string.IsNullOrEmpty(tempQuery))
                                {
                                    Console.WriteLine("Error actualizando base de datos: " + tempQuery);
                                }

                                //numeroDeVisitas.RemoveAt(k);
                                //horarioDeEntrega.RemoveAt(k);
                                //urgenciaLista.RemoveAt(k);
                                notificaciones.RemoveAt(k);
                                notificacionesVerbose.RemoveAt(k);
                                detailActivityTitle.RemoveAt(k);
                                codigoNotificacion.RemoveAt(k);
                                codigoReferencia.RemoveAt(k);
                                codigoNotificador.RemoveAt(k);
                                imageChecked.RemoveAt(k);
                                SSRVA.NotifyItemRemoved(k);
                                k = k - 1;
                                top = top - 1;
                                ManejoBaseDatos.Cerrar();
                            }
                        }
                        refreshRecyclerView();

                        //
                        Activity.StartService(new Intent(this.Activity, typeof(Helpers.ServicioAceptacion)));
                    }
                    catch (Exception ex){ Console.WriteLine("ERROR APROBANDO NOTIFICACION: "+ex.ToString()); }
                }
                //EN CORRECCION 
                if (resultCode == 0)
                {
                    //    try
                    //    {
                    //        int top = imageChecked.Count;
                    //        for (int k = 0; k < top; k++)
                    //        {
                    //            if (imageChecked[k])
                    //            {
                    //                ManejoBaseDatos.Abrir();
                    //                string tempQuery = "";
                    //                //se actualiza el estado de las notificaciones a estado AsignarParaNotificar en caso de ser admin - o se auto-asignan en caso de los notificadores 
                    //                if (supervisor.Equals("true", StringComparison.Ordinal) || supervisor.Equals("True", StringComparison.Ordinal))
                    //                {
                    //                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Estado", "'ResultadoEnCorreccion'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                    //                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Motivo", "'" + FragmentList.motivo + "'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                    //                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "PendienteSubir", "'S'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                    //                }

                    //                if (!string.IsNullOrEmpty(tempQuery))
                    //                {
                    //                    Console.WriteLine("Error actualizando base de datos: " + tempQuery);
                    //                }

                    //                notificaciones.RemoveAt(k);
                    //                notificacionesVerbose.RemoveAt(k);
                    //                detailActivityTitle.RemoveAt(k);
                    //                codigoNotificacion.RemoveAt(k);
                    //                codigoNotificador.RemoveAt(k);
                    //                imageChecked.RemoveAt(k);
                    //                SSRVA.NotifyItemRemoved(k);
                    //                k = k - 1;
                    //                top = top - 1;
                    //                ManejoBaseDatos.Cerrar();
                    //            }
                    //        }
                    //        refreshRecyclerView();
                    //        //
                    //        Activity.StartService(new Intent(this.Activity, typeof(Helpers.ServicioRechazo)));
                    //    }
                    //    catch (Exception ex) { Console.WriteLine("ERROR APROBANDO NOTIFICACION: " + ex.ToString()); }
                    //

                    try
                    {
                        List<string> codNotificaciones = new List<string>();
                        List<string> codNotificadores = new List<string>();
                        List<string> codReferencia = new List<string>();
                        List<string> jsonString = new List<string>();

                        servicioCheckDB coneccion = new servicioCheckDB();

                        for (int k = 0; k < imageChecked.Count; k++)
                        {
                            if (imageChecked[k])
                            {
                                codNotificaciones.Add(codigoNotificacion[k]);
                                codNotificadores.Add(codigoNotificador[k]);
                                codReferencia.Add(codigoReferencia[k]);
                            }
                        }

                        int counter = 0;
                        int sentCounter = 0;
                        //todo 
                        foreach (var codigoNotif in codNotificaciones)
                        {
                            DateTime localDate = DateTime.Now;
                            //string fechaAnnoMesDia = localDate.ToString("yyyyMMdd");
                            string fechaCompleja = localDate.ToString("o");

                            RechazarActas actasRechazadas = new RechazarActas()
                            {

                                Fecha = fechaCompleja,
                                OficialSupervisor = FragmentLogin.codNotificador,
                                OficialNotificador = codNotificadores[counter],
                                CodOficina = FragmentLogin.codOficina,
                                Notificaciones = new List<ClassNotificaciones>()
                                        {
                                            new ClassNotificaciones()
                                            {
                                                CodNotificacion=Convert.ToInt32(codigoNotif),
                                                CodReferencia= Convert.ToInt32(codReferencia[counter])
                                            }
                                        },
                                Motivo = FragmentList.motivo
                            };
                            jsonString.Add(Newtonsoft.Json.JsonConvert.SerializeObject(actasRechazadas));
                            counter += 1;
                        }

                        string request = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ActaNotificacion/RechazarActasNotificacion";

                        foreach (var serial in jsonString)
                        {
                            if (coneccion.envioDatosWeb(request, serial, this.Activity))
                                sentCounter += 1;
                        }

                        if (codNotificaciones.Count == sentCounter)
                            Toast.MakeText(this.Activity, "Notificaciones rechazadas exitosamente", ToastLength.Short).Show();

                        int top = imageChecked.Count;

                        for (int k = 0; k < top; k++)
                        {
                            if (imageChecked[k])
                            {
                                ManejoBaseDatos.Abrir();
                                string tempQuery = "";
                                //se actualiza el estado de las notificaciones a estado AsignarParaNotificar en caso de ser admin - o se auto-asignan en caso de los notificadores 
                                if (supervisor.Equals("true", StringComparison.Ordinal) || supervisor.Equals("True", StringComparison.Ordinal))
                                {
                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Estado", "'ResultadoEnCorreccion'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "Motivo", "'" + FragmentList.motivo + "'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                    tempQuery = ManejoBaseDatos.Actualizar("Notificaciones", "PendienteSubir", "'S'", "CodigoNotificacion=" + codigoNotificacion[k].ToString());
                                }

                                if (!string.IsNullOrEmpty(tempQuery))
                                {
                                    Console.WriteLine("Error actualizando base de datos: " + tempQuery);
                                }

                                notificaciones.RemoveAt(k);
                                notificacionesVerbose.RemoveAt(k);
                                detailActivityTitle.RemoveAt(k);
                                codigoNotificacion.RemoveAt(k);
                                codigoNotificador.RemoveAt(k);
                                imageChecked.RemoveAt(k);
                                SSRVA.NotifyItemRemoved(k);
                                k = k - 1;
                                top = top - 1;
                                ManejoBaseDatos.Cerrar();
                            }
                        }
                        refreshRecyclerView();
                    }
                    catch (Exception ex ){ Console.WriteLine("ERROR AL RECHAZAR NOTIFICACIONES: "+ex.ToString()); }

                    //bool respuesta = coneccion.envioDatosWeb(requestCierre, json, this);
                    //if (respuesta)
                    //{
                    //    //observaciones.Text = "";
                    //    Toast.MakeText(this, "Cierre de apertura exitoso.", ToastLength.Short).Show();
                    //    //Se dirige al usuario a la pagina de buzones donde podra realizar el inicio de apertura.
                    //}
                    //else
                    //{
                    //    Toast.MakeText(this, "Error al intentar cerar apertura.", ToastLength.Short).Show();
                    //}

                }
            }
        }

        private void HandleButtonClick(object sender, DialogClickEventArgs e)
        {
            //throw new NotImplementedException();
            var dialog = (AlertDialog)sender;
            dialog.Dismiss();
        }

        /// <summary>
        /// Metodo donde se llama al detalle de las notificaciones.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="position"></param>
        void OnItemClick(object sender, int position)
        {
            if (!debugMode)
            {
                try
                {
                    if (SSRVA != null)
                    {
                        if (position>=0)
                        {
                            Intent intent = new Intent(this.Activity, typeof(MainActivity));
                            //intent.PutExtra(DetailActivity.EXTRA_NAME, detailActivityTitle[position]);
                            intent.PutExtra("EXTRA_INFO", "DETALLE");
                            intent.PutExtra("EXTRA_CODIGO_NOTIFICACION", codigoNotificacion[position]);

                            //intent.PutExtra(DetailActivity.EXTRA_NAME, detailActivityTitle[position]);
                            //intent.PutExtra(DetailActivity.EXTRA_INFO, notificacionesVerbose[position]);

                            StartActivity(intent);
                        }
                        
                    }
                }
                catch (Exception exB2F) { Console.WriteLine("Error when comming back to activity: "+exB2F.ToString()); }
                
            }
            else
            {
                Toast.MakeText(this.Activity, "Elemento Presionado del RecyclerView " + position.ToString(), ToastLength.Short).Show();
            }              
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
         
            //CODE FROM ACTIVITY
            //Console.WriteLine("-- fzeledon Called on create options menu on fragment "+querySelect.ToString());
            menu.Clear();
            inflater.Inflate(Resource.Menu.auxMenu, menu);

            var item = menu.FindItem(Resource.Id.search);

            // si se se encuentra des-habilitado el menu de opciones

            menu.FindItem(Resource.Id.change).SetVisible(false);
            menu.FindItem(Resource.Id.reasignar).SetVisible(false);
            menu.FindItem(Resource.Id.aceptar).SetVisible(false);
            menu.FindItem(Resource.Id.rechazar).SetVisible(false);

            //Habilita la asignacion de notificaciones
            if (enableMenuOption && MailBoxes.currentPage == 0)
            {
                menu.FindItem(Resource.Id.change).SetVisible(true);
            }

            //Habilita la reasignacion de notificaciones 
            if (enableMenuOption && MailBoxes.currentPage ==1)
            {
                menu.FindItem(Resource.Id.reasignar).SetVisible(true);
            }

            //Habilita el rechazo o aceptacion de las notificaciones
            if (enableMenuOption && MailBoxes.currentPage == 3)
            {
                menu.FindItem(Resource.Id.aceptar).SetVisible(true);
                menu.FindItem(Resource.Id.rechazar).SetVisible(true);
            }

            var searchView = MenuItemCompat.GetActionView(item);
            _searchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();
            _searchView.QueryTextChange += (s, e) =>
            {
                Console.WriteLine(e.NewText);
            };
            _searchView.QueryTextSubmit += (s, e) =>
            {

                //if (!String.IsNullOrEmpty(e.Query) && e.Query.Equals("login", StringComparison.Ordinal))
                //{
                //    try
                //    {
                //        imageChecked.Clear();
                //        int totalItems = notificaciones.Count;
                //        //Console.WriteLine("Numero original de notificaciones: "+totalItems);
                //        notificaciones.Clear();
                //        SSRVA.NotifyItemRangeRemoved(0,totalItems);
                //        recyclerView.RemoveAllViews();
                //        for (int k =0; k<10;k++)
                //        {
                //            notificaciones.Add("Login #"+k.ToString());
                //        }
                //        SSRVA.NotifyItemRangeInserted(0,9);
                //        recyclerView.SetItemViewCacheSize(notificaciones.Count);
                //    }
                //    catch (Exception exc1) { Console.WriteLine("Error en transicion de fragmentos: " + exc1.ToString()); }
                //}

                if (!String.IsNullOrEmpty(e.Query))
                {
                    try
                    {
                        imageChecked.Clear();
                        int totalItems = notificaciones.Count;
                        //Console.WriteLine("Numero original de notificaciones: "+totalItems);
                        notificaciones.Clear();
                        codigoNotificacion.Clear();
                        codigoNotificador.Clear();
                        codigoReferencia.Clear();
                        notificacionesVerbose.Clear();
                        SSRVA.NotifyItemRangeRemoved(0, totalItems);
                        recyclerView.RemoveAllViews();
                        codigoReferencia.Clear();

                        /*for (int k =0; k<10;k++)
                        {
                            notificaciones.Add("Login #"+k.ToString());
                        }*/

                        string consulta = "";
                        if (e.Query.Split(':').Length >= 2)
                        {
                            switch (e.Query.Split(':')[0].ToLower())
                            {
                                case "jornada":
                                //case "Jornada":
                                    {
                                        consulta = query;
                                        if (e.Query.Split(':')[1].Equals("diurna") || e.Query.Split(':')[1].Equals("Diurna"))
                                        {
                                            consulta += " and RolNocturno = 'True'";
                                        }
                                        else
                                        {
                                            consulta += " and RolNocturno = 'False'";
                                        }

                                        break;
                                    }
                                case "sector":
                                //case "Sector":
                                    {
                                        consulta = query;
                                        consulta += " and Sector = '" + e.Query.Split(':')[1].Trim() + "'";

                                        break;
                                    }
                                case "notificador":
                                //case "Notificador":
                                    {
                                        consulta = query;
                                        consulta += " and CodNotificador LIKE '%" + e.Query.Split(':')[1].Trim() + "%'";
                                        break;
                                    }
                                case "despacho":

                                    {
                                        consulta = query;
                                        consulta += " and DespachoCodigo = '" + e.Query.Split(':')[1].Trim() + "'";
                                    }
                                    break;


                                default: { break; }
                            }
                        }
                        try
                        {
                            if (consulta != "")
                            {
                                //sqldb = SQLiteDatabase.OpenDatabase(SplashActivity.dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                ManejoBaseDatos.Abrir();
                                Console.WriteLine("-- fzeledon Query a realizar: " + consulta);

                                ICursor resultSet;
                                resultSet = ManejoBaseDatos.Seleccionar(consulta);

                                if (resultSet.MoveToFirst())
                                {
                                    do
                                    {
                                        codigoNotificacion.Add(resultSet.GetString(resultSet.GetColumnIndex("CodigoNotificacion")));
                                        codigoNotificador.Add(resultSet.GetString(resultSet.GetColumnIndex("CodNotificador")));
                                        codigoReferencia.Add(resultSet.GetString(resultSet.GetColumnIndex("CodReferencia")));

                                        notificaciones.Add(
                                                           "Expediente: " + resultSet.GetString(resultSet.GetColumnIndex("Expediente")) + "\r\n" +
                                                           "Notificando: " + resultSet.GetString(resultSet.GetColumnIndex("Notificando")) + "\r\n" +
                                                           "Provincia: " + resultSet.GetString(resultSet.GetColumnIndex("Provincia")) + "\r\n" +
                                                           "Cantón: " + resultSet.GetString(resultSet.GetColumnIndex("Canton")) + "\r\n" +
                                                           "Distrito: " + resultSet.GetString(resultSet.GetColumnIndex("Distrito")) + "\r\n" +
                                                           "Código Notificación " + resultSet.GetString(resultSet.GetColumnIndex("CodigoNotificacion")) + "\r\n" +
                                                           "Sector " + resultSet.GetString(resultSet.GetColumnIndex("Sector"))
                                                          );

                                        notificacionesVerbose.Add(
                                                           "Expediente: " + resultSet.GetString(resultSet.GetColumnIndex("Expediente")) + "@" +
                                                           "Notificando: " + "\r\n" + resultSet.GetString(resultSet.GetColumnIndex("Notificando")) + "@" +
                                                           "Provincia: " + resultSet.GetString(resultSet.GetColumnIndex("Provincia")) + "@" +
                                                           "Cantón: " + resultSet.GetString(resultSet.GetColumnIndex("Canton")) + "@" +
                                                           "Distrito: " + resultSet.GetString(resultSet.GetColumnIndex("Distrito")) + "@" +
                                                           "Dirección: " + resultSet.GetString(resultSet.GetColumnIndex("Direccion")) + "@" +
                                                           "Despacho: " + resultSet.GetString(resultSet.GetColumnIndex("DespachoDescripcion")) + "@" +
                                                           "Fecha Documento: " + resultSet.GetString(resultSet.GetColumnIndex("FechaDocumento")) + "@" +
                                                           "Fecha Emisión: " + resultSet.GetString(resultSet.GetColumnIndex("FechaEmision")) + "@" +
                                                           "Sector: " + resultSet.GetString(resultSet.GetColumnIndex("Sector")));

                                        resultSet.MoveToNext();

                                    } while (!resultSet.IsAfterLast);
                                }
                                SSRVA.NotifyItemRangeInserted(0, notificaciones.Count);
                                resultSet.Close();
                                ManejoBaseDatos.Cerrar();
                            }
                            else
                            {
                                //sqldb = SQLiteDatabase.OpenDatabase(SplashActivity.dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                ManejoBaseDatos.Abrir();
                                Console.WriteLine("-- fzeledon Query a realizar: " + query);

                                ICursor resultSet;
                                resultSet = ManejoBaseDatos.Seleccionar(query);

                                if (resultSet.MoveToFirst())
                                {
                                    do
                                    {
                                        codigoNotificacion.Add(resultSet.GetString(resultSet.GetColumnIndex("CodigoNotificacion")));
                                        codigoReferencia.Add(resultSet.GetString(resultSet.GetColumnIndex("CodReferencia")));
                                        codigoNotificador.Add(resultSet.GetString(resultSet.GetColumnIndex("CodNotificador")));

                                        notificaciones.Add(
                                                           "Expediente: " + resultSet.GetString(resultSet.GetColumnIndex("Expediente")) + "\r\n" +
                                                           "Notificando: " + resultSet.GetString(resultSet.GetColumnIndex("Notificando")) + "\r\n" +
                                                           "Provincia: " + resultSet.GetString(resultSet.GetColumnIndex("Provincia")) + "\r\n" +
                                                           "Cantón: " + resultSet.GetString(resultSet.GetColumnIndex("Canton")) + "\r\n" +
                                                           "Distrito: " + resultSet.GetString(resultSet.GetColumnIndex("Distrito")) + "\r\n" +
                                                           "Código Notificación " + resultSet.GetString(resultSet.GetColumnIndex("CodigoNotificacion")) + "\r\n" +
                                                           "Sector " + resultSet.GetString(resultSet.GetColumnIndex("Sector"))
                                                          );

                                        notificacionesVerbose.Add(
                                                           "Expediente: " + resultSet.GetString(resultSet.GetColumnIndex("Expediente")) + "@" +
                                                           "Notificando: " + "\r\n" + resultSet.GetString(resultSet.GetColumnIndex("Notificando")) + "@" +
                                                           "Provincia: " + resultSet.GetString(resultSet.GetColumnIndex("Provincia")) + "@" +
                                                           "Cantón: " + resultSet.GetString(resultSet.GetColumnIndex("Canton")) + "@" +
                                                           "Distrito: " + resultSet.GetString(resultSet.GetColumnIndex("Distrito")) + "@" +
                                                           "Dirección: " + resultSet.GetString(resultSet.GetColumnIndex("Direccion")) + "@" +
                                                           "Despacho: " + resultSet.GetString(resultSet.GetColumnIndex("DespachoDescripcion")) + "@" +
                                                           "Fecha Documento: " + resultSet.GetString(resultSet.GetColumnIndex("FechaDocumento")) + "@" +
                                                           "Fecha Emisión: " + resultSet.GetString(resultSet.GetColumnIndex("FechaEmision")) + "@" +
                                                           "Sector: " + resultSet.GetString(resultSet.GetColumnIndex("Sector")));

                                        resultSet.MoveToNext();

                                    } while (!resultSet.IsAfterLast);
                                }
                                SSRVA.NotifyItemRangeInserted(0, notificaciones.Count);
                                resultSet.Close();
                                ManejoBaseDatos.Cerrar();
                            }
                        }
                        catch (Exception ex) { Console.WriteLine("Error while retrieving data from table: " + ex.ToString()); }



                        recyclerView.SetItemViewCacheSize(notificaciones.Count);
                    }
                    catch (Exception exc1) { Console.WriteLine("Error en transicion de fragmentos: " + exc1.ToString()); }
                }

            };

            _searchView.OnActionViewCollapsed();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.change:

                    if (!debugMode)
                    {
                        if (FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal) || FragmentLogin.supervisor.Equals("True", StringComparison.Ordinal))
                        {
                            try
                            {
                                EditNameDialogFragment dialog = EditNameDialogFragment.newInstance("Mensaje de confirmación");
                                dialog.SetTargetFragment(this, 1);
                                dialog.Show(FragmentManager, "dialog");
                            }
                            catch (Exception ex) { Console.WriteLine("Error en seleccion " + ex.ToString()); }
                        }
                        else
                        {
                            ManejoBaseDatos.Abrir();
                            string autenticacion = "";
                            ICursor mCursor= ManejoBaseDatos.Seleccionar("SELECT Autoasignacion FROM Autenticacion");
                            if (mCursor.MoveToFirst() && mCursor.Count==1)
                            {
                                do
                                {
                                    autenticacion = mCursor.GetString(0);
                                }
                                while (mCursor.MoveToNext());
                            }
                            mCursor.Close();
                            //creo que hacia falta cerrar la base de datos
                            ManejoBaseDatos.Cerrar();

                            if (autenticacion.Equals("S", StringComparison.Ordinal))
                            {
                                AlertDialog.Builder alerta = new AlertDialog.Builder(this.Context);
                                alerta.SetTitle("Mensaje de alerta");
                                alerta.SetIcon(Resource.Drawable.alertaNuevo);
                                alerta.SetMessage("La asignación de notificaciones ya ha sido realizada.");
                                alerta.SetNegativeButton("Salir", HandleButtonClick);
                                alerta.SetCancelable(false);
                                alerta.Create();
                                alerta.Show();
                            }
                            else
                            {
                                EditNameDialogFragment dialog = EditNameDialogFragment.newInstance("Mensaje de confirmación");
                                dialog.SetTargetFragment(this, 1);
                                dialog.Show(FragmentManager, "dialog");
                            }
                        }
                       
                    }
                             
                    enableMenuOption = false;
                    mActivity.InvalidateOptionsMenu();
                    recyclerView.SetItemViewCacheSize(notificaciones.Count);
                   
                    return true;

                case Resource.Id.reasignar:
                    //Toast.MakeText(this.Activity, "Re-Asignar presionado", ToastLength.Short).Show();
                    try
                    {
                        EditNameDialogFragment dialog = EditNameDialogFragment.newInstance("Mensaje de confirmación");
                        dialog.SetTargetFragment(this, 2);
                        dialog.Show(FragmentManager, "dialog");
                    }

                    catch (Exception ex) { Console.WriteLine("Error en seleccion " + ex.ToString()); }
                    enableMenuOption = false;
                    mActivity.InvalidateOptionsMenu();
                    recyclerView.SetItemViewCacheSize(notificaciones.Count);
                    return true;

                case Resource.Id.aceptar:
                    try
                    {

                        DialogFragment3 dialog = new DialogFragment3();
                        Bundle bun1 = new Bundle();
                        bun1.PutInt("someInt",0);
                        dialog.Arguments = bun1;
                        dialog.SetTargetFragment(this, 3);
                        dialog.Show(FragmentManager, "dialog");

                    }
                    catch (Exception ex) { Console.WriteLine("ERROR EN PROCESO DE ACEPTACION DE NOTIFICACIONES: "+ex.ToString());}
                    return true;

                case Resource.Id.rechazar:
                    try
                    {
                        DialogFragment3 dialog = new DialogFragment3();
                        Bundle bun1 = new Bundle();
                        bun1.PutInt("someInt", 1);
                        dialog.Arguments = bun1;
                        dialog.SetTargetFragment(this, 3);
                        dialog.Show(FragmentManager, "dialog");

                        //simpleDialog dialog = simpleDialog.newInstance("Aceptar Resultado", 1);
                        //dialog.SetTargetFragment(this, 3);
                        //dialog.Show(FragmentManager, "dialog");

                    }
                    catch (Exception ex) { Console.WriteLine("ERROR EN PROCESO DE ACEPTACION DE NOTIFICACIONES: " + ex.ToString()); }
                    return true;

            }
            return base.OnOptionsItemSelected(item);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {  
            recyclerView = inflater.Inflate(Resource.Layout.FragmentRecyclerView, container, false) as RecyclerView;
            SetUpRecyclerView(recyclerView);
            return recyclerView;
        }
            
        private void SetUpRecyclerView(RecyclerView recyclerView)
        {
            getData();       
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));
            SSRVA = new SimpleStringRecyclerViewAdapter(recyclerView.Context, notificaciones, Activity.Resources);
            SSRVA.ItemClick += OnItemClick;
            recyclerView.SetAdapter(SSRVA);
            recyclerView.SetItemViewCacheSize(notificaciones.Count);
            //Console.WriteLine("Total de elementos en notificaciones despues de setUpRecyclerView: "+notificaciones.Count.ToString());
        }

        public class SimpleStringRecyclerViewAdapter : RecyclerView.Adapter
        {
            private readonly TypedValue mTypedValue = new TypedValue();
            private int mBackground;
            private List<string> mValues;
            Resources mResource;
            private Dictionary<int, int> mCalculatedSizes;
            public event EventHandler<int> ItemClick;
            Context mContext;

            public SimpleStringRecyclerViewAdapter(Context context, List<string> items, Resources res)
            {
                mContext = context;
                context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
                mBackground = mTypedValue.ResourceId;
                mValues = items;
                mResource = res;
                mCalculatedSizes = new Dictionary<int, int>();
            }

            //This will fire any event handlers that are registered with our ItemClick
            //event.
            private void OnClick(int position)
            {
                if (ItemClick != null)
                {
                    ItemClick(this, position);
                    Console.WriteLine("Element Pressed: " + position.ToString());
                }
            }

            public override int ItemCount
            {
                get
                {
                    return mValues.Count;
                }
            }

            public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
 
                var simpleHolder = holder as SimpleViewHolder;          
                // En este punto se introducen los datos a mostrar en pantalla.
                simpleHolder.mBoundString = mValues[position];
                simpleHolder.mTxtView.Text = mValues[position];
                imageChecked.Add(false);

                int drawableID=0;

                //string visitas = numeroDeVisitas[position];
                //string urgencia = urgenciaLista[position];
                //string rolNocturno = "Diurna";
                //string visitas = "10";
                //string urgencia = "false";
                //string rolNocturno = "false";

                //estado = MailBoxes.currentPage;
                //estado += 1;


                try
                {
                    if (position >= 0 && (position < imageChecked.Count))
                    {
                        if (!imageChecked[position])
                        {
                            drawableID = Helper.ImagePicker(position);
                        }
                        else
                        {
                            Console.WriteLine("--fzeledon Drawing checked element " + position.ToString());
                            //drawableID = Resource.Drawable.check;
                        }
                    }
                }

                catch (Exception ex) { Console.WriteLine("Error when setting image for element " + position.ToString() + " " + ex.ToString()); }



                //JEFF
                //try
                //{
                //    if (position >= 0 && (position < imageChecked.Count))
                //    {
                //        if (!imageChecked[position])
                //        {
                //            drawableID = Helper.ImagePicker(estado);
                //            if (!(FragmentLogin.supervisor.Equals("True") || FragmentLogin.supervisor.Equals("true")))
                //            {
                //                if (estado == 2)
                //                {
                //                    drawableID = Resource.Drawable.ico_amarillo;
                //                }
                //            }
                //        }
                //        else
                //        {
                //            Console.WriteLine("--fzeledon Drawing checked element " + position.ToString());
                //            //drawableID = Resource.Drawable.check;
                //        }
                //    }
                //}

                //catch (Exception ex) { Console.WriteLine("Error when setting image for element " + position.ToString() + " " + ex.ToString()); }

                //if (rolNocturno.Equals("True") || rolNocturno.Equals("true"))
                //{
                //    drawableID = Resource.Drawable.ico_azul;
                //}

                //if (urgencia.Equals("True") || urgencia.Equals("true"))
                //{
                //    drawableID = Resource.Drawable.ico_urgente;
                //}


                BitmapFactory.Options options = new BitmapFactory.Options();
                
               var bitMap = await BitmapFactory.DecodeResourceAsync(mResource, drawableID, options);

               //var bitMap = Helper.drawTextToBitmap(mContext, drawableID, visitas);

                simpleHolder.mImageView.SetImageBitmap(bitMap);

                int counter = 0;
                //controlador de eventos para las vistas de imagenes circulares.
                simpleHolder.mImageView.Click += delegate
                {
                    //string visita = visitas;
                    //string urgenc = urgencia;
                    //string rolNocturn = rolNocturno;

                    //string visita = visitas;
                    //string urgenc = urgencia;
                    //string rolNocturn = rolNocturno;
                    //int estados = estado;

                    try
                    {
                        counter += 1;
                        Console.WriteLine("mImagview in position" + simpleHolder.Position.ToString() + " Counts " + counter);
                        if (!(counter % 2 == 0) && (simpleHolder.Position >= 0) && (simpleHolder.Position < imageChecked.Count))
                        {
                            simpleHolder.mImageView.SetImageResource(Resource.Drawable.check);
                            imageChecked[simpleHolder.Position] = true;
                        }

                        if ((counter % 2 == 0) && (simpleHolder.Position >= 0) && (simpleHolder.Position < imageChecked.Count))
                        {
                            simpleHolder.mImageView.SetImageResource(Helper.ImagePicker(position));
                            imageChecked[simpleHolder.Position] = false;
                        }


                        if (imageChecked.Contains(true))
                        {
                            enableMenuOption = true;
                            mActivity.InvalidateOptionsMenu();
                        }
                        else
                        {
                            enableMenuOption = false;
                            mActivity.InvalidateOptionsMenu();
                        }

                    }
                    catch (Exception ex) { Console.WriteLine("Error in click event description: " + ex.ToString()); }

                    //try
                    //{
                    //    counter += 1;
                    //    Console.WriteLine("mImagview in position" + simpleHolder.Position.ToString() + " Counts " + counter);
                    //    if (!(counter % 2 == 0) && (simpleHolder.Position >= 0) && (simpleHolder.Position < imageChecked.Count))
                    //    {
                    //        simpleHolder.mImageView.SetImageResource(Resource.Drawable.check);
                    //        imageChecked[simpleHolder.Position] = true;
                    //    }

                    //    if ((counter % 2 == 0) && (simpleHolder.Position >= 0) && (simpleHolder.Position < imageChecked.Count))
                    //    {
                    //        Bitmap bitMaps = null;

                    //        if (!(FragmentLogin.supervisor.Equals("True") || FragmentLogin.supervisor.Equals("true")))
                    //        {
                    //            if (estados == 2)
                    //            {
                    //                drawableID = Resource.Drawable.ico_amarillo;
                    //                bitMaps = Helper.drawTextToBitmap(mContext, drawableID, visita);
                    //            }
                    //            else
                    //            {
                    //                drawableID = Helper.ImagePicker(estados);
                    //                bitMaps = Helper.drawTextToBitmap(mContext, drawableID, visita);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            drawableID = Helper.ImagePicker(estados);
                    //            bitMaps = Helper.drawTextToBitmap(mContext, drawableID, visita);
                    //        }

                    //        if (rolNocturno.Equals("True") || rolNocturno.Equals("true"))
                    //        {
                    //            drawableID = Resource.Drawable.ico_azul;
                    //            bitMaps = Helper.drawTextToBitmap(mContext, drawableID, visita);
                    //        }

                    //        if (urgencia.Equals("True") || urgencia.Equals("true"))
                    //        {
                    //            drawableID = Resource.Drawable.ico_urgente;
                    //            bitMaps = Helper.drawTextToBitmap(mContext, drawableID, visita);
                    //        }

                    //        simpleHolder.mImageView.SetImageBitmap(bitMaps);
                    //        imageChecked[simpleHolder.Position] = false;
                    //    }


                    //    if (imageChecked.Contains(true))
                    //    {
                    //        enableMenuOption = true;
                    //        mActivity.InvalidateOptionsMenu();
                    //    }
                    //    else
                    //    {
                    //        enableMenuOption = false;
                    //        mActivity.InvalidateOptionsMenu();
                    //    }

                    //}
                    //catch (Exception ex) { Console.WriteLine("Error in click event description: " + ex.ToString()); }
                };
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.List_Item, parent, false);
                view.SetBackgroundResource(mBackground);
                return new SimpleViewHolder(view,OnClick);
            }
        }

        public class SimpleViewHolder : RecyclerView.ViewHolder
        {
            public string mBoundString;
            public readonly View mView;
            public readonly ImageView mImageView;
            public readonly TextView mTxtView;

            public SimpleViewHolder(View view, Action<int> listener) : base(view)
            {
                mView = view;
                mImageView = view.FindViewById<ImageView>(Resource.Id.avatar);
                mTxtView = view.FindViewById<TextView>(Resource.Id.text1);
                view.Click += (sender, e) => listener(Position);
            }

            public override string ToString()
            {
                return base.ToString() + " '" + mTxtView.Text;
            }

        }

        public class DetailOnPageChangeListener: ViewPager.SimpleOnPageChangeListener
        {
            private int currentPage;

            public void onPageSelected(int position)
            {
                currentPage = position;
            }
            public int getCurrentPage()
            {
                return currentPage;
            }
        }

        private void refreshRecyclerView()
        {
            try
            {
                imageChecked.Clear();
                enableMenuOption = false;
                mActivity.InvalidateOptionsMenu();
                recyclerView.RemoveAllViews();
                getData();
                SSRVA.NotifyDataSetChanged();
                recyclerView.SetItemViewCacheSize(notificaciones.Count);
            }
            catch (Exception ex) { Console.WriteLine("Error refrescando recyclerview: "+ex.ToString()); }
        }

        private void getData()
        {
            detailActivityTitle.Clear();
            notificaciones.Clear();
            notificacionesVerbose.Clear();
            codigoNotificacion.Clear();
            codigoNotificador.Clear();
            codigoReferencia.Clear();
            //relacionado a imageview
            //numeroDeVisitas.Clear();
            //horarioDeEntrega.Clear();
            //urgenciaLista.Clear();


            //Segun documentacion provista
            //NuevasAImprimir=1,
            //AsignarParaNotificar = 2,
            //Notificandose = 3,
            //NotificacionCompletada = 4,
            //ResultadoEnCorreccion = 5,
            //PendienteDeDistribucion = 6,
            //EntregadaAlDespacho = 7

            if (!debugMode)
            {
                switch (querySelect)
                {
                    case 1:
                        query = "SELECT * FROM Notificaciones WHERE Estado='NuevasAImprimir'";
                        Console.WriteLine("Se toma el valor querySelect: " + querySelect.ToString());
                        break;
                    case 2:
                        query = "SELECT * FROM Notificaciones WHERE Estado='AsignarParaNotificar'";
                        Console.WriteLine("Se toma el valor querySelect: " + querySelect.ToString());
                        break;

                    case 3:
                        query = "SELECT * FROM Notificaciones WHERE Estado='Notificandose'";
                        Console.WriteLine("Se toma el valor querySelect: " + querySelect.ToString());
                        break;
                    case 4:
                        query = "SELECT * FROM Notificaciones WHERE Estado='NotificacionCompletada'";
                        Console.WriteLine("Se toma el valor querySelect: " + querySelect.ToString());
                        break;
                    case 5:
                        query = "SELECT * FROM Notificaciones WHERE Estado='ResultadoEnCorreccion'";
                        Console.WriteLine("Se toma el valor querySelect: " + querySelect.ToString());
                        break;
                    case 6:
                        query = "SELECT * FROM Notificaciones WHERE Estado='PendienteDeDistribucion'";
                        Console.WriteLine("Se toma el valor querySelect: " + querySelect.ToString());
                        break;
                    default:
                        break;
                }

                try
                {
                    //sqldb = SQLiteDatabase.OpenDatabase(SplashActivity.dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                    ManejoBaseDatos.Abrir();
                    Console.WriteLine("-- fzeledon Query a realizar: " + query);
                    ICursor resultSet;
                    resultSet = ManejoBaseDatos.Seleccionar(query);
                    if (resultSet.MoveToFirst())
                    {
                        do
                        {

                            codigoNotificacion.Add(resultSet.GetString(resultSet.GetColumnIndex("CodigoNotificacion")));
                            codigoNotificador.Add(resultSet.GetString(resultSet.GetColumnIndex("CodNotificador")));
                            codigoReferencia.Add(resultSet.GetString(resultSet.GetColumnIndex("CodReferencia")));
                            //numeroDeVisitas.Add(resultSet.GetString(resultSet.GetColumnIndex("Visitas")));
                            //urgenciaLista.Add(resultSet.GetString(resultSet.GetColumnIndex("Urgente")));
                            //horarioDeEntrega.Add(resultSet.GetString(resultSet.GetColumnIndex("HorarioEntrega")));

                            detailActivityTitle.Add("Expediente " + "\r\n" +
                                                    resultSet.GetString(resultSet.GetColumnIndex("Expediente")));
                            notificaciones.Add(
                                               "Expediente: " + resultSet.GetString(resultSet.GetColumnIndex("Expediente")) + "\r\n" +
                                               "Notificando: " + resultSet.GetString(resultSet.GetColumnIndex("Notificando")) + "\r\n" +
                                               "Provincia: " + resultSet.GetString(resultSet.GetColumnIndex("Provincia")) + "\r\n" +
                                               "Cantón: " + resultSet.GetString(resultSet.GetColumnIndex("Canton")) + "\r\n" +
                                               "Distrito: " + resultSet.GetString(resultSet.GetColumnIndex("Distrito")) + "\r\n" +
                                               "Código Notificación " + resultSet.GetString(resultSet.GetColumnIndex("CodigoNotificacion")) + "\r\n" +
                                               "Sector " + resultSet.GetString(resultSet.GetColumnIndex("Sector"))
                                              );

                            notificacionesVerbose.Add(
                                               "Expediente: " + resultSet.GetString(resultSet.GetColumnIndex("Expediente")) + "@" +
                                               "Notificando: " + "\r\n" + resultSet.GetString(resultSet.GetColumnIndex("Notificando")) + "@" +
                                               "Provincia: " + resultSet.GetString(resultSet.GetColumnIndex("Provincia")) + "@" +
                                               "Cantón: " + resultSet.GetString(resultSet.GetColumnIndex("Canton")) + "@" +
                                               "Distrito: " + resultSet.GetString(resultSet.GetColumnIndex("Distrito")) + "@" +
                                               "Dirección: " + resultSet.GetString(resultSet.GetColumnIndex("Direccion")) + "@" +
                                               "Despacho: " + resultSet.GetString(resultSet.GetColumnIndex("DespachoDescripcion")) + "@" +
                                               "Fecha Documento: " + resultSet.GetString(resultSet.GetColumnIndex("FechaDocumento")) + "@" +
                                               "Fecha Emisión: " + resultSet.GetString(resultSet.GetColumnIndex("FechaEmision")) + "@" +
                                               "Sector: " + resultSet.GetString(resultSet.GetColumnIndex("Sector")));

                            resultSet.MoveToNext();

                        } while (!resultSet.IsAfterLast);
                    }

                    resultSet.Close();
                    ManejoBaseDatos.Cerrar();
                }
                catch (Exception ex) { Console.WriteLine("Error while retrieving data from table: " + ex.ToString()); }
            }
            else
            {
                for (int k = 0; k < 15; k++)
                {
                    notificaciones.Add("List Element #" + k.ToString());
                }
            }
        }
    }
}