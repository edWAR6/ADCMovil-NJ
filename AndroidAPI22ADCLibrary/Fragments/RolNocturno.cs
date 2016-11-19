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
using Android.Database;
using System.Net;
using System.IO;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class RolNocturno : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public static RolNocturno NewInstance()
        {
            var rolNocturno = new RolNocturno { Arguments = new Bundle() };

            return rolNocturno;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rolNocturno = inflater.Inflate(Resource.Layout.asignarRolNocturno,container,false);

            return rolNocturno;
        }

        public override void OnResume()
        {
            base.OnResume();
            cargarNotificadores();

        }

        /// <summary>
        /// Se cargan la lista de notificadores
        /// </summary>
        public void cargarNotificadores()
        {
            try
            {
                ManejoBaseDatos.Abrir();
                //Se consulta en la base de datos la lista de los usuarios notificadores
                ICursor cursor = ManejoBaseDatos.Seleccionar("SELECT NombreCompleto, CodigoNotificador, RolNocturno FROM OficialesNotificadores");
                //Instancia de la tabla de notificadores
                TableLayout tablaNotificadores = Activity.FindViewById<TableLayout>(Resource.Id.tbNotificadores);
                //Se crea el objeto para las fila que se van a ingresar en la tabla
                TableRow nuevaFila;
                TableLayout.LayoutParams layoutParams = new TableLayout.LayoutParams(TableLayout.LayoutParams.MatchParent, TableLayout.LayoutParams.MatchParent);

                List<int> listaIndices = new List<int>();
                //Se guardan los indices que se van a eliminar de la tabla de notificadores
                for (int i = 1; i < tablaNotificadores.ChildCount; tablaNotificadores.RemoveViewAt(1));

                if (cursor.MoveToFirst())//Se posiciona el cursor en la primera posición
                {
                    do
                    {
                        nuevaFila = new TableRow(Activity);//Se instancia la nueva fila
                        nuevaFila.LayoutParameters = layoutParams;//Se agregan los tamaños del layout
                        nuevaFila.SetPadding(5, 5, 5, 5);//Se agregan los margenes de cada fila
                        nuevaFila.SetBackgroundColor(Android.Graphics.Color.Argb(2, 1, 0, 5));//Se cambia el color del fondo
                        nuevaFila.Clickable = true;
                        nuevaFila.Click += (s, args) => { };

                        //Se agrega el nombre del notificador
                        TextView nombreNotificador = new TextView(Activity);
                        nombreNotificador.Text = cursor.GetString(0);
                        //Se agrega el rol
                        CheckBox rolNocturno = new CheckBox(Activity);
                        rolNocturno.Gravity = GravityFlags.CenterHorizontal;
                        rolNocturno.Text = "";
                        //Se obtiene el valor del rol nocturno
                        string check = cursor.GetString(2);
                        //Si es verdadero el check se marca y si no, se deja sin marcar
                        rolNocturno.Checked = check == "True" || check == "true" ? true : false;
                        //Se guarda el codigo del notificador
                        string codigoNotificador = cursor.GetString(1);

                        rolNocturno.CheckedChange += (s, args) =>
                        {

                            ManejoBaseDatos.Abrir();
                            //Se actualiza el rol nocturno
                            string msj = ManejoBaseDatos.Actualizar("OficialesNotificadores", "RolNocturno", "'"+args.IsChecked+"'", "CodigoNotificador='" + codigoNotificador+"'");
                            ManejoBaseDatos.Cerrar();

                            try
                            {
                                if (args.IsChecked)
                                {
                                    string consulta = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/OficialNotificador/AsignarRolNocturno?PCodNotificador=" + codigoNotificador + "";
                                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(consulta);
                                    request.Method = "GET";
                                    request.ContentType = "application/json";
                                    HttpWebResponse myResp = (HttpWebResponse)request.GetResponse();
                                    string responseText;

                                    using (var response = request.GetResponse())
                                    {
                                        using (var reader = new StreamReader(response.GetResponseStream()))
                                        {
                                            responseText = reader.ReadToEnd();
                                            Console.WriteLine("Respuesta de web service: " + responseText);
                                            if (responseText.Equals("true") || responseText.Equals("True"))
                                            {
                                                Toast.MakeText(this.Activity, "Cambio a Rol Nocturno Exitoso", ToastLength.Short).Show();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (WebException webEx)
                            {
                                Toast.MakeText(this.Activity, "Error al actualizar datos", ToastLength.Short).Show();
                                Console.WriteLine("Error al asignar rol nocturno: "+webEx.ToString());
                            }

                            Toast.MakeText(Activity,msj,ToastLength.Short).Show();

                        };
                        //Se agregan las dos columnas a la fila
                        nuevaFila.AddView(nombreNotificador);
                        nuevaFila.AddView(rolNocturno);
                        //Se agrega la nueva fila a la tabla
                        tablaNotificadores.AddView(nuevaFila);

                    } while (cursor.MoveToNext());
                }
                cursor.Close();

                ManejoBaseDatos.Cerrar();

            }catch(Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("RolNocturno.cargarNotificadores " + ex.Message + " " + ex.StackTrace);
            }
        }

    }
}