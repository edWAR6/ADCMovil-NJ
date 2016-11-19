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
using Android.Support.Design.Widget;
using SignaturePad;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Database;
using Android.Database.Sqlite;
using System.IO;
using AndroidAPI22ADCLibrary.Fragments;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class Impresion : SupportFragment 
    {
      
        private ListView listview;
        SQLiteDatabase db;
        string dbName = "PJNotificaciones.db";
        private static string codNotificacion = "";
        private static string notificandoImpresion = "";
        private string guidNotificando = "";
        private string guidTestigo ="";
        private string diligenciada = "";
        private string observaciones = "";
        private string nombreTestigo = "";

        private EditText txtObservaciones;
        private EditText txtNombreTestigo;
        private static SQLiteDatabase db1;
        string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "PJNotificaciones.db");

        //Helpers.SQLiteConeccion dbConeccion;
        public override void OnCreate(Bundle savedInstanceState)
        {
            //codNotificacion = Arguments.GetString("codigo");
            base.OnCreate(savedInstanceState);       
        }

        /// <summary>
        /// Instancia del Fragmento de Impresion. Es utilizado para registrar datos.
        /// </summary>
        /// <param name="codigoNotificacion"></param>
        /// <returns></returns>
        public static Impresion NewInstance(string codigoNotificacion,string notificando)
        {   
            //Impresion impresion = new Impresion();//Se crea la nueva instancia de la clase
            codNotificacion = codigoNotificacion;
            notificandoImpresion = notificando;
            var impresion = new Impresion { Arguments = new Bundle() };
            return impresion;//Se retorna la instancia creada

        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            View impresion = inflater.Inflate(Resource.Layout.Impresion, container, false);
            //Se habilita el menú lateral
            ((Activities.MainActivity)Activity).habilitarMenuLateral(true);
            ((Activities.MainActivity)Activity).cambiarTituloAplicacion("Registrar Datos Notificación");

            //Se crea la nueva instancia del botón de guardar y se le asigna el evento click
            Button btnGuardar = impresion.FindViewById<Button>(Resource.Id.btnGuardar);
            btnGuardar.Click += BtnGuardar_Click;//Se crea el evento click

            Button btnfirmaTestigo = impresion.FindViewById<Button>(Resource.Id.btnFirmarTestigo);
            btnfirmaTestigo.Click += BtnfirmaTestigo_Click;

            Button btnFirma = impresion.FindViewById<Button>(Resource.Id.btnFirmarUsuario);
            btnFirma.Click += BtnFirma_Click;

            txtObservaciones = impresion.FindViewById<EditText>(Resource.Id.editText1);
            txtObservaciones.TextChanged += TxtObservaciones_TextChanged;

            txtNombreTestigo = impresion.FindViewById<EditText>(Resource.Id.editText2);
            txtNombreTestigo.TextChanged += TxtNombreTestigo_TextChanged;


            //Button btnFirmaNotificando = impresion.FindViewById<Button>(Resource.Id.btnFirmarNotificando);

            //Se crea una instancia del contenedor del combo de justificaciones
            LinearLayout justificacion = impresion.FindViewById<LinearLayout>(Resource.Id.LayoutJustificacion);
            LinearLayout justificacion2 = impresion.FindViewById<LinearLayout>(Resource.Id.LayoutJustificacion2);
            //Se crea la instancia del combo que contendra las justificaciones de porque no firmo
            //Spinner justificaciones = impresion.FindViewById<Spinner>(Resource.Id.spnJustificacion);

            justificacion.Visibility = ViewStates.Invisible;
            justificacion2.Visibility = ViewStates.Invisible;

            //Se crea la instancia del checkBox
            CheckBox firma = impresion.FindViewById<CheckBox>(Resource.Id.chkNoFirma);

            LinearLayout razon = impresion.FindViewById<LinearLayout>(Resource.Id.linearLayout5);
            var param = razon.LayoutParameters;
            param.Height = 60;
            razon.Visibility = ViewStates.Invisible;

            LinearLayout layFirma = impresion.FindViewById<LinearLayout>(Resource.Id.linearLayout5b);
            layFirma.Visibility = ViewStates.Invisible;

            CheckBox rPositivo = impresion.FindViewById<CheckBox>(Resource.Id.chkResultadoPositivo);
            CheckBox rNegativo = impresion.FindViewById<CheckBox>(Resource.Id.chkResultadoNegativo);
            CheckBox rReintento = impresion.FindViewById<CheckBox>(Resource.Id.chkReintento);
            rReintento.Visibility = ViewStates.Gone;
            listview = impresion.FindViewById<ListView>(Resource.Id.listViewOpciones);
            Spinner spnOpciones = impresion.FindViewById<Spinner>(Resource.Id.spnOpciones);
            guidNotificando = Guid.NewGuid().ToString();
            guidTestigo = Guid.NewGuid().ToString();

            //firma positivo
            rPositivo.CheckedChange += (s, args) =>
            {
                if (rPositivo.Checked)
                {
                    diligenciada = "true";
                    rNegativo.Checked = false;
                    rReintento.Checked = false;
                    razon.Visibility = ViewStates.Visible;
                    layFirma.Visibility = ViewStates.Visible;

                    try
                    {
                        loadConnection();
                        string consulta = "SELECT Descripcion FROM RESULTADOS WHERE Diligenciada = 'True'";
                        List<string> data = new List<string>();
                        ICursor cursor = db.RawQuery(consulta, null);
                        if (cursor.MoveToFirst())
                        {
                            do
                            {
                                data.Add(cursor.GetString(0));
                            } while (cursor.MoveToNext());
                        }
                        cursor.Close();
                        db.Close();
                        int tempCounter = 0;
                        Console.WriteLine("TOTAL DE ELEMENTOS EN LISTA: " + data.Count.ToString());
                        var opciones = new string[data.Count];
                        foreach (var element in data)
                        {
                            if (data.Count >= 0)
                            {
                                Console.WriteLine("Resultado k" + tempCounter.ToString() + ": " + element);
                                opciones[tempCounter] = element;
                                tempCounter++;
                            }
                        }

                        if (opciones.Length > 0)
                        {
                            //listview.Adapter = new ArrayAdapter(listview.Context, Android.Resource.Layout.SimpleListItemMultipleChoice, opciones);
                            //listview.ChoiceMode = ChoiceMode.Single; 
                            spnOpciones.Adapter = new ArrayAdapter(spnOpciones.Context, Android.Resource.Layout.SimpleSpinnerItem, opciones);
                            //int opcionSeleccionada=listview.CheckedItemPosition;
                            //Console.WriteLine("opcion seleccionada: "+opcionSeleccionada.ToString());
                        }

                    }
                    catch (Exception ex1) { Console.WriteLine("ERROR en carga de listView: " + ex1.ToString()); }

                }
                else
                {
                    firma.Checked = false;
                    razon.Visibility = ViewStates.Invisible;
                    layFirma.Visibility = ViewStates.Invisible;
                    justificacion.Visibility = ViewStates.Invisible;
                    param.Height = 40;
                    diligenciada = "";
                }

            };
            rNegativo.CheckedChange += (s, args) =>
            {
                rPositivo.Checked = false;
                rReintento.Checked = false;
                razon.Visibility = ViewStates.Visible;
                justificacion.Visibility = ViewStates.Invisible;
                firma.Checked = false;
                if (rNegativo.Checked)
                {
                    try
                    {
                        diligenciada = "false";
                        loadConnection();
                        string consulta = "SELECT Descripcion FROM RESULTADOS WHERE Diligenciada = 'False'";
                        List<string> data = new List<string>();
                        ICursor cursor = db.RawQuery(consulta, null);
                        if (cursor.MoveToFirst())
                        {
                            do
                            {
                                data.Add(cursor.GetString(0));
                            } while (cursor.MoveToNext());
                        }
                        cursor.Close();
                        db.Close();

                        int tempCounter = 0;
                        var opciones = new string[data.Count];
                        foreach (var element in data)
                        {
                            if (data.Count >= 0)
                            {
                                opciones[tempCounter] = element;
                                tempCounter++;
                            }
                        }
                        if (opciones.Length > 0)
                        {
                            //listview.Adapter = new ArrayAdapter(listview.Context, Android.Resource.Layout.SimpleListItemMultipleChoice, opciones);
                            //listview.ChoiceMode = ChoiceMode.Single;
                            spnOpciones.Adapter = new ArrayAdapter(spnOpciones.Context, Android.Resource.Layout.SimpleSpinnerItem, opciones);
                        }
                    }
                    catch (Exception ex2) { Console.WriteLine("ERROR: " + ex2.ToString()); }
                }
                else
                {
                    diligenciada = "";
                    justificacion.Visibility = ViewStates.Invisible;
                    razon.Visibility = ViewStates.Invisible;
                    param.Height = 40;
                }

            };
            rReintento.CheckedChange += (s, args) =>
            {
                if (rReintento.Checked)
                {
                    rPositivo.Checked = false;
                    rNegativo.Checked = false;
                    razon.Visibility = ViewStates.Visible;
                }
                else { razon.Visibility = ViewStates.Invisible; }
            };

            //Si cambia la selección del check
            firma.CheckedChange += (s, args) =>
            {
                if (firma.Checked)//Se evalua si el checkBox esta checkeado o no
                {
                    //Se muestra visible el combo
                    justificacion.Visibility = ViewStates.Visible;
                    justificacion2.Visibility = ViewStates.Visible;

                    //btnFirma.Enabled = !args.IsChecked;//Se inhabilita el botón de firma
                }
                else
                {
                    //Se muestra oculto
                    justificacion.Visibility = ViewStates.Invisible;
                    justificacion2.Visibility = ViewStates.Invisible;
                    //btnFirma.Enabled = !args.IsChecked;//Se habilita el botón de firma
                }
            };
            Console.WriteLine("Codigo de entrada: " + codNotificacion.ToString());
            //Toast.MakeText(this.Activity,"Codigo de entrada: "+codNotificacion.ToString(),ToastLength.Short).Show();
            return impresion;
        }

        private void TxtNombreTestigo_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            //throw new NotImplementedException();
            nombreTestigo = txtNombreTestigo.Text;
            Console.WriteLine("Nombre de testigo: "+nombreTestigo);
        }

        private void TxtObservaciones_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            //throw new NotImplementedException();
            observaciones = txtObservaciones.Text;
            Console.WriteLine("Observaciones: " + observaciones);

        }

        private void BtnfirmaTestigo_Click(object sender, EventArgs e)
        {

            //throw new NotImplementedException();
            SignatureDialogFragment dialog = new SignatureDialogFragment();
            Bundle bun1 = new Bundle();
            bun1.PutString("codigo", codNotificacion);
            bun1.PutString("actor", "testigo");
            bun1.PutString("guid", guidTestigo);
            dialog.Arguments = bun1;
            dialog.Show(FragmentManager, "dialog");
        }

        /// <summary>
        /// Se navega a la pantalla de la firma
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFirma_Click(object sender, EventArgs e)
        {
            SignatureDialogFragment dialog = new SignatureDialogFragment();
            Bundle bun1 = new Bundle();
            bun1.PutString("codigo",codNotificacion);
            bun1.PutString("actor", "notificando");
            bun1.PutString("guid",guidNotificando);
            dialog.Arguments = bun1;
            dialog.Show(FragmentManager, "dialog");
        }

        /// <summary>
        /// Se crea el evento para el boton siguiente
        /// Se valida el formulario de la pantalla de Impresión
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            //verificando si hay un resultado para la notificacion
            if (!string.IsNullOrEmpty(diligenciada))
            {
                //verificando codigo de resultado
                if (listview.CheckedItemPosition >= 0)
                {
                    if (diligenciada.Equals("true", StringComparison.Ordinal))
                    {

                        ManejoBaseDatos.Abrir();
                        ICursor mCursor = ManejoBaseDatos.Seleccionar("SELECT * FROM Imagenes WHERE Nombre='" + guidNotificando + "'");

                        //verificando si existe firma
                        if (mCursor.Count > 0)
                        {
                            //ManejoBaseDatos.Cerrar();

                            //verificando sin hay observaciones
                            if (!string.IsNullOrEmpty(observaciones))
                            {
                                Console.WriteLine("Guardando datos");

                                try
                                {
                                    Console.WriteLine("Guardando datos para NotificacionCompletada");

                                    Console.WriteLine("Codigo notificacion: "+codNotificacion.ToString());
                                    
                                    db1 = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                    db1.ExecSQL("UPDATE Notificaciones SET Estado='NotificacionCompletada' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                    db1.ExecSQL("UPDATE Notificaciones SET PendienteSubir='S' WHERE CodigoNotificacion=" + codNotificacion + ";");


                                    db1.ExecSQL("UPDATE Notificaciones SET FirmaNotificando='" + guidNotificando + "' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                    db1.ExecSQL("UPDATE Notificaciones SET ResultadoCodigo="+ listview.CheckedItemPosition.ToString()+" WHERE CodigoNotificacion=" + codNotificacion + ";");
                                    db1.ExecSQL("UPDATE Notificaciones SET ResultadoDescripcion='" + listview.GetItemAtPosition(listview.CheckedItemPosition).ToString() + "' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                    db1.ExecSQL("UPDATE Notificaciones SET ResultadoDiligenciada='" + diligenciada + "' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                    db1.ExecSQL("UPDATE Notificaciones SET Observaciones='" + observaciones + "' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                    db1.ExecSQL("UPDATE Notificaciones SET FechaNotificacion='" + "2015-07-13T19:33:38.034Z" + "' WHERE CodigoNotificacion=" + codNotificacion + ";");


                                    //Console.WriteLine("Guid notificando: " + guidNotificando);
                                    //Console.WriteLine("Resultado Codigo: " + listview.CheckedItemPosition.ToString());
                                    //Console.WriteLine("Resultado Descripcion: " + listview.GetItemAtPosition(listview.CheckedItemPosition).ToString());
                                    //Console.WriteLine("Resultado diligenciada: " + diligenciada);
                                    //Console.WriteLine("observaciones: " + observaciones);
                                    //Console.WriteLine("Fecha de notificacion: " + "2015-07-13T19:33:38.034Z");
                                    //Console.WriteLine("Nombre de testigo: " + nombreTestigo);


                                    if (!string.IsNullOrEmpty(nombreTestigo))
                                    {
                                        db1.ExecSQL("UPDATE Notificaciones SET FirmaTestigo='" + guidTestigo + "' WHERE CodigoNotificacion='" + codNotificacion + "';");
                                        
                                    }
                                    db1.Close();
                                    Toast.MakeText(this.Activity, "Datos Guardados de forma exitosa", ToastLength.Short).Show();
                                }
                                catch (Exception ex) { Console.WriteLine("ERROR REGISTRANDO DATOS: " + ex.ToString()); }
                            }
                            else { Toast.MakeText(this.Activity, "Por favor agregue observaciones para poder registrar datos.", ToastLength.Long).Show(); }
                        }
                        else
                        {
                            //ManejoBaseDatos.Cerrar();
                            mCursor.Close();
                            Toast.MakeText(this.Activity, "Por favor solicite al notificando firmar el acta.", ToastLength.Long).Show();
                        }
                        mCursor.Close();
                        //
                    }


                    if (diligenciada.Equals("false", StringComparison.Ordinal))
                        {

                        if (listview.CheckedItemPosition >= 0)
                        {
                            if (!string.IsNullOrEmpty(observaciones))
                            {
                                //Guardando datos
                                db1 = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
                                db1.ExecSQL("UPDATE Notificaciones SET Estado='NotificacionCompletada' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                db1.ExecSQL("UPDATE Notificaciones SET PendienteSubir='S' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                db1.ExecSQL("UPDATE Notificaciones SET ResultadoCodigo=" + listview.CheckedItemPosition.ToString() + " WHERE CodigoNotificacion=" + codNotificacion + ";");
                                db1.ExecSQL("UPDATE Notificaciones SET ResultadoDescripcion='" + listview.GetItemAtPosition(listview.CheckedItemPosition).ToString() + "' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                db1.ExecSQL("UPDATE Notificaciones SET ResultadoDiligenciada='" + diligenciada + "' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                db1.ExecSQL("UPDATE Notificaciones SET Observaciones='" + observaciones + "' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                db1.ExecSQL("UPDATE Notificaciones SET FechaNotificacion='" + "2015-07-13T19:33:38.034Z" + "' WHERE CodigoNotificacion=" + codNotificacion + ";");
                                db1.Close();
                            }
                            else { Toast.MakeText(this.Activity, "Por favor agregue observaciones para poder registrar datos.", ToastLength.Long).Show(); }
                            //
                        }
                        else
                        {
                            Toast.MakeText(this.Activity, "Por favor seleccione la razón del resultado seleccionado.", ToastLength.Long).Show();
                        }
                    }
                }
                else
                {
                    Toast.MakeText(this.Activity, "Por favor seleccione la razón del resultado seleccionado.", ToastLength.Long).Show();
                }
            }
            else
            {
                Toast.MakeText(this.Activity, "Por favor seleccione un resultado para la notificación.", ToastLength.Long).Show(); 
            }
        }

        /// <summary>
        /// Se abre la conección a la base de datos
        /// </summary>
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