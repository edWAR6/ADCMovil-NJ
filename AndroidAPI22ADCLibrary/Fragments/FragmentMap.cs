using Android.Content;
using Android.Database;
using Android.Database.Sqlite;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using AndroidAPI22ADCLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class FragmentMap : Fragment
    {

        Helpers.SQLiteConeccion dbConeccion; //Se crea el objeto de la base de datos
        //private bool admin = false;

        SQLiteDatabase db;
        string dbName = "PJNotificaciones.db";
        public static List<LatLng> posiciones = new List<LatLng>();
        public static List<KeyValuePair<LatLng, bool>> listaPosiciones = new List<KeyValuePair<LatLng, bool>>();

        System.Globalization.CultureInfo EnglishCulture = new System.Globalization.CultureInfo("en-US");

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            dbConeccion = new SQLiteConeccion();//Se crea la instancia de la base de datos
        }

        public static FragmentMap NewInstance()
        {
            var fragmentMap = new FragmentMap { Arguments = new Bundle() };
            return fragmentMap;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment

            View ubicacion = inflater.Inflate(Resource.Layout.fragmentMap, container, false);
            ((Activities.MainActivity)Activity).habilitarMenuLateral(true);
            ((Activities.MainActivity)Activity).cambiarTituloAplicacion("Rutas Notificadores");

            TextView txt = ubicacion.FindViewById<TextView>(Resource.Id.textView9);

            Button btnSiguiente = ubicacion.FindViewById<Button>(Resource.Id.btnSiguiente);
            btnSiguiente.Click += BtnSiguiente_Click;//Se crea el evento click

            cargarComboNotificador(this.Activity, ubicacion);

            Spinner notificador = ubicacion.FindViewById<Spinner>(Resource.Id.spnNotificador);
            notificador.ItemSelected += Notificador_ItemSelected;

            if (!(FragmentLogin.supervisor.Equals("true") || FragmentLogin.supervisor.Equals("True")))
            {
                txt.Visibility = ViewStates.Invisible;
                notificador.Visibility = ViewStates.Invisible;
            }
            else
            {
                notificador.Visibility = ViewStates.Visible;
                txt.Visibility = ViewStates.Visible;
                dbConeccion.setAdaptadorCombo("SELECT NombreCompleto || '-' || CodigoNotificador FROM OficialesNotificadores",Activity,ref notificador);
                notificador.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(Notificador_ItemSelected1);
            }

            posiciones.Clear();
            listaPosiciones.Clear();
            ManejoBaseDatos.Abrir();
            if (FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal) || FragmentLogin.supervisor.Equals("True", StringComparison.Ordinal))
            {
                ICursor cursor = ManejoBaseDatos.Seleccionar("SELECT googleMapsX,googleMapsY,RolNocturno FROM Notificaciones WHERE Estado='AsignarParaNotificar'");
                if (cursor.MoveToFirst())
                {
                    listaPosiciones = new List<KeyValuePair<LatLng, bool>>();
                    KeyValuePair<LatLng, bool> posicion;

                    do
                    {
                        Console.WriteLine("Pares: "+ cursor.GetString(0)+" "+ cursor.GetString(1));

                        double latitudeM = double.Parse(cursor.GetString(0),EnglishCulture);
                        double longitudeM = double.Parse(cursor.GetString(1),EnglishCulture);
                        //Se obtiene el valor del rol nocturno
                        string rol = cursor.GetString(2);
                        //Si es verdadero el check se marca y si no, se deja sin marcar
                        bool rolNocturno = rol == "True" || rol == "true" ? true : false;

                        posicion = new KeyValuePair<LatLng, bool>(new LatLng(latitudeM, longitudeM), rolNocturno);
                        listaPosiciones.Add(posicion);

                        posiciones.Add(new LatLng(latitudeM, longitudeM));
                        // se agregan a la lista los diferentes elementos a los cuales se les va a calcular la posicion
                        //data.Add("Costa Rica," + cursor.GetString(0) + "," + cursor.GetString(1));
                        //codNotificacion.Add(cursor.GetString(2));

                    } while (cursor.MoveToNext());
                }
                cursor.Close();
                ManejoBaseDatos.Cerrar();
            }
            else
            {
                string codigoNotificador = FragmentLogin.codNotificador;
                ICursor cursor = ManejoBaseDatos.Seleccionar("SELECT googleMapsX,googleMapsY,RolNocturno FROM Notificaciones WHERE Estado='Notificandose' and CodNotificador = '"+codigoNotificador+"'");
                if (cursor.MoveToFirst())
                {
                    listaPosiciones = new List<KeyValuePair<LatLng, bool>>();
                    KeyValuePair<LatLng, bool> posicion;

                    do
                    {
                        NumberFormatInfo provider = new NumberFormatInfo();
                        provider.NumberDecimalSeparator = ".";

                        double latitudeM = Convert.ToDouble(cursor.GetString(0),provider);
                        double longitudeM = Convert.ToDouble(cursor.GetString(1),provider);
                        //Se obtiene el valor del rol nocturno
                        string rol = cursor.GetString(2);
                        //Si es verdadero el check se marca y si no, se deja sin marcar
                        bool rolNocturno = rol == "True" || rol == "true" ? true : false;

                        Console.WriteLine("latitudeM: "+latitudeM.ToString());
                        Console.WriteLine("longitudeM: " + longitudeM.ToString());

                        posicion = new KeyValuePair<LatLng, bool>(new LatLng(latitudeM, longitudeM), rolNocturno);
                        listaPosiciones.Add(posicion);

                        posiciones.Add(new LatLng(latitudeM, longitudeM));

                    } while (cursor.MoveToNext());
                }
                cursor.Close();
                ManejoBaseDatos.Cerrar();
            }

            ((Activities.MainActivity)Activity).IniciarFragmentoMapa(Resource.Id.map);//Se inicia el mapa en el fragmento
            return ubicacion;
        }

        private void Notificador_ItemSelected1(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                Spinner notificador = (Spinner)sender;
                ManejoBaseDatos.Abrir();
                ICursor cursor = ManejoBaseDatos.Seleccionar("SELECT googleMapsX,googleMapsY,RolNocturno FROM Notificaciones WHERE Estado='AsignarParaNotificar' and CodNotificador = '" + notificador.SelectedItem.ToString().Split('-')[1] + "'");
                listaPosiciones.Clear();
                if (cursor.MoveToFirst())
                {
                    listaPosiciones = new List<KeyValuePair<LatLng, bool>>();
                    KeyValuePair<LatLng, bool> posicion;

                    do
                    {
                        NumberFormatInfo provider = new NumberFormatInfo();
                        provider.NumberDecimalSeparator = ".";

                        double latitudeM = Convert.ToDouble(cursor.GetString(0), provider);
                        double longitudeM = Convert.ToDouble(cursor.GetString(1), provider);
                        //Se obtiene el valor del rol nocturno
                        string rol = cursor.GetString(2);
                        //Si es verdadero el check se marca y si no, se deja sin marcar
                        bool rolNocturno = rol == "True" || rol == "true" ? true : false;

                        Console.WriteLine("latitudeM: " + latitudeM.ToString());
                        Console.WriteLine("longitudeM: " + longitudeM.ToString());

                        posicion = new KeyValuePair<LatLng, bool>(new LatLng(latitudeM, longitudeM), rolNocturno);
                        listaPosiciones.Add(posicion);

                        posiciones.Add(new LatLng(latitudeM, longitudeM));

                    } while (cursor.MoveToNext());
                }
                cursor.Close();
                ManejoBaseDatos.Cerrar();

                ((Activities.MainActivity)Activity).AgregarUbicacionMapaListaColor(listaPosiciones);

            }
            catch (Exception ex)
            {
                //Se guarda el error
                Logs.saveLogError("FragmentMap.Notificador_ItemSelected1 " + ex.Message + " " + ex.StackTrace);
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            //
        }
        private void Notificador_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //throw new NotImplementedException();
            Console.WriteLine("Notificador seleccionado");
        }

        public override void OnStop()
        {
            base.OnStop();
        }

        private void BtnSiguiente_Click(object sender, EventArgs e)
        {
            try
            {

               
                //posiciones.Add(new LatLng(9.913914, -84.073037));
                //posiciones.Add(new LatLng(9.916155, -84.075612));
                //posiciones.Add(new LatLng(9.915155, -84.075617));
                //posiciones.Add(new LatLng(9.914155, -84.075614));
                
                //try
                //{
                //    loadConnection();
                //    string consulta = "SELECT Provincia,Canton,Distrito FROM Notificaciones WHERE Estado = 1";
                //    List<string> data = new List<string>();
                //    ICursor cursor = db.RawQuery(consulta, null);
                //    //se almacena en data los elmentos de la tabla Notificaciones cuando estado es 1 

                //    if (cursor.MoveToFirst())
                //    {
                //        do
                //        {
                //            data.Add("Costa Rica," + cursor.GetString(cursor.GetColumnIndex("Provincia")) + "," + cursor.GetString(cursor.GetColumnIndex("Canton")));
                //        } while (cursor.MoveToNext());
                //    }
                //    cursor.Close();
                //    db.Close();
                //    //se hace la busqueda de las direcciones para cada elemento
                //    foreach (var item in data)
                //    {
                //        Console.WriteLine("string buscar "+item);
                //        List<double> tempCoordenas = new List<double>();
                //        tempCoordenas = Helper.obtenerCoordenadas(item);
                //        if (tempCoordenas.Count >= 0)
                //        {
                //            posiciones.Add(new LatLng(tempCoordenas[0], tempCoordenas[1]));
                //            Console.WriteLine("Agregando posiciones: "+ tempCoordenas[0].ToString()+ " "+tempCoordenas[1].ToString());
                //        }         
                //    }
                //}
                //catch (Exception ex3) { Console.WriteLine("Error buscando coordenadas en fragment map: "+ex3.ToString()); }

                //try
                //{
                //    string buscar = "Costa Rica, San Jose, Desamparados";
                //    List<double> temp = new List<double>();
                //    temp = Helper.obtenerCoordenadas(buscar);
                //    if (temp.Count > 0)
                //    {
                //        Console.WriteLine("Coordenadas encontradas: " + temp[0].ToString() + " " + temp[1].ToString());
                //    }
                //}
                //catch (Exception exc) { Console.WriteLine("Excepcion Generada buscando Localizacion: "+exc.ToString()); }

                if (posiciones.Count > 0)
                {
                    Console.WriteLine("# de Posiciones encontradas:  "+posiciones.Count.ToString());
                    ((Activities.MainActivity)Activity).AgregarUbicacionMapaListaColor(listaPosiciones);
                }
                else { Toast.MakeText(this.Activity, "No existen datos en buzon", ToastLength.Short).Show(); }
           
            }
            catch (Exception ex) { Console.WriteLine("ERROR GENERANDO GOOGLE MAP datos: "+ex.ToString()); }
            
        }

        public void cargarComboNotificador(Context context, View ubicacion)
        {

            try
            {
                //Se crea la instancia del spinner para el combo de provincias
                Spinner notificador = ubicacion.FindViewById<Spinner>(Resource.Id.spnNotificador);
                //Se realiza la consulta a la base de datos y se carga el combo de provincias
                dbConeccion.setAdaptadorCombo("SELECT NombreCompleto FROM OficialesNotificadores WHERE Activo='True' ", this.Activity, ref notificador);
            }
            catch (Exception e)
            {
                //Se guarda el error en el log de errores
                Logs.saveLogError("FragmentMap.cargarComboNotificador " + e.Message + " " + e.StackTrace);
                //Se muestra un mensaje informando el error
                Toast.MakeText(context, GetString(Resource.String.MensajeErrorCargaBaseDatos), ToastLength.Long).Show();
            }

        }
        public override void OnResume()
        {
            base.OnResume();
            try {
                if (posiciones.Count > 0)
                {
                    Console.WriteLine("# de Posiciones encontradas:  " + posiciones.Count.ToString());
                    ((Activities.MainActivity)Activity).AgregarUbicacionMapaListaColor(listaPosiciones);
                }
                else { Toast.MakeText(this.Activity, "No existen datos en buzon", ToastLength.Short).Show(); }
            }catch(Exception ex)
            {
                //Se guarda el detalle del error
                Logs.saveLogError("FragmentMap.OnResume " + ex.Message + " " + ex.StackTrace);
            }
        }

        public void loadConnection()
        {
            //string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, dbName);

            bool exist = File.Exists(dbPath);
            try
            {
                if (exist)
                    db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                else
                {
                    FileStream writeStream = new FileStream(dbPath, FileMode.OpenOrCreate, FileAccess.Write);

                    ReadWriteStream(Android.App.Application.Context.Assets.Open(dbName), writeStream);

                    db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR ABRIENDO BASE DE DATOS: " + ex.ToString());
            }
        }

        private void ReadWriteStream(Stream readStream, Stream writeStream)
        {
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = readStream.Read(buffer, 0, Length);

            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = readStream.Read(buffer, 0, Length);
            }
            readStream.Close();
            writeStream.Close();
        }

    }
}