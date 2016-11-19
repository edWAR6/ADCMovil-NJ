using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidAPI22ADCLibrary.Helpers;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class ReintentoNotificacion : Fragment
    {
        private static string codigoNotificacionReintento="";

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public static ReintentoNotificacion NewInstance(string codNotificacion)
        { 
            codigoNotificacionReintento = codNotificacion;
            var reintento = new ReintentoNotificacion { Arguments = new Bundle() };
            return reintento;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View reintento = inflater.Inflate(Resource.Layout.ReintentoNotificacion,container,false);

            return reintento;
        }

        public override void OnResume()
        {
            base.OnResume();
            cargarListaReintentos();
            Button agregar = Activity.FindViewById<Button>(Resource.Id.btnAgregar);
            agregar.Click += Agregar_Click;

        }

        private void Agregar_Click(object sender, EventArgs e)
        {
            try
            {
                servicioCheckDB coneccion = new servicioCheckDB();

                EditText observaciones = Activity.FindViewById<EditText>(Resource.Id.txtObservaciones);

                servicioCheckDB.ReintentoNotificacion reintento = new servicioCheckDB.ReintentoNotificacion();

                reintento.Descripcion = observaciones.Text;
                reintento.CodNotificacion = Convert.ToInt32(codigoNotificacionReintento);

                DateTime localDate = DateTime.Now;
                string output = localDate.ToString("o");
                Console.WriteLine("HORA de reintento" + output);

                reintento.Fecha = Convert.ToDateTime(output);
                //reintento.Fecha = Convert.ToDateTime("2016 - 09 - 26T17: 06:03.537");


                servicioCheckDB.OficialNotificador oficial = new servicioCheckDB.OficialNotificador();
                oficial.CodNotificador = FragmentLogin.codNotificador;
                reintento.OficialNotificador = oficial;

                servicioCheckDB.GeoLocalizacion ubicacion = new servicioCheckDB.GeoLocalizacion();
                ubicacion.Latitud = servicioLocalizacion.latitud;
                ubicacion.Longitud = servicioLocalizacion.longitud;
                ubicacion.Aproximado = true;
                reintento.Ubicacion = ubicacion;

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(reintento);
                bool respuesta = coneccion.envioDatosWeb("https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ReintentoNotificacion/GuardarReintentoNotificacion", json, Activity);

                if (respuesta)
                {
                    observaciones.Text = "";
                    Toast.MakeText(Activity,"Registro guardado correctamente.",ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(Activity, "Error al guardar el registro.", ToastLength.Short).Show();
                }

            }
            catch(Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("ReintentoNotificacion.Agregar_click " + ex.Message + " " + ex.StackTrace);
            }
        }

        public void cargarListaReintentos()
        {
            try
            {
                servicioCheckDB coneccion = new servicioCheckDB();

                var listaReintentos = coneccion.ObtenerListaStrings("https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/ReintentoNotificacion/ListarReintentosNotificacion?PCodNotificacion="+ codigoNotificacionReintento + "", Activity);

                if (listaReintentos != null)
                {
                    TableLayout tablaReintentos = Activity.FindViewById<TableLayout>(Resource.Id.tbNotificadores);

                    TableRow nuevaFila;
                    TableLayout.LayoutParams layoutParams = new TableLayout.LayoutParams(TableLayout.LayoutParams.MatchParent, TableLayout.LayoutParams.MatchParent);

                    for (int k = 0; k < listaReintentos.Count; k++)
                    {
                        nuevaFila = new TableRow(Activity);//Se instancia la nueva fila
                        nuevaFila.LayoutParameters = layoutParams;//Se agregan los tamaños del layout
                        nuevaFila.SetPadding(5, 5, 5, 5);//Se agregan los margenes de cada fila
                        nuevaFila.SetBackgroundColor(Android.Graphics.Color.Argb(2, 1, 0, 5));//Se cambia el color del fondo
                        nuevaFila.Clickable = true;

                        TextView descripcion = new TextView(Activity);
                        descripcion.Text = listaReintentos[k]["Descripcion"].ToString();
                        nuevaFila.AddView(descripcion);

                        TextView fecha = new TextView(Activity);
                        fecha.Text = listaReintentos[k]["Fecha"].ToString();
                        nuevaFila.AddView(fecha);

                        TextView nombre = new TextView(Activity);
                        nombre.Text = listaReintentos[k]["OficialNotificador"]["NombreCompleto"].ToString();
                        nuevaFila.AddView(nombre);

                        tablaReintentos.AddView(nuevaFila);
                    }
                }

                }catch(Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("ReintentoNotificacion.cargarListaReintentos" + ex.Message + " " + ex.StackTrace);
            }
        }
    }
}