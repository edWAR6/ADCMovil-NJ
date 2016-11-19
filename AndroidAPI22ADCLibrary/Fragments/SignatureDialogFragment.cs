using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using SignaturePad;
using System.IO;
using Android.Graphics;
using Java.IO;
using Java.Sql;
using Android.Database.Sqlite;
using AndroidAPI22ADCLibrary.Helpers;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class SignatureDialogFragment : Android.Support.V4.App.DialogFragment
    {
        SQLiteDatabase db;
        string dbName = "PJNotificaciones.db";
        private string codigo = "";
        private string actor = "";
        private string guid="";
        private SignaturePadView signature;
        static string descripcionFirma = "Firma de la persona";//Descripción del pad de firmas
        static string descripcionLateral = "X: ";//Descripción que aparece a la par de la linea de firma
        static string descripcionBorrar = "Borrar";//Descripción del botón de borrar


        public static SignatureDialogFragment NewInstance(string codigoNotificacion,string actor,string guid)
        {
            var dialogFragment = new SignatureDialogFragment();
            Bundle args = new Bundle();
            args.PutString("codigo", codigoNotificacion);
            args.PutString("actor",actor);
            args.PutString("guid",guid);
            dialogFragment.Arguments = args;
            return dialogFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            codigo = Arguments.GetString("codigo");
            actor = Arguments.GetString("actor");
            guid = Arguments.GetString("guid");

            base.OnCreate(savedInstanceState);
            //codigo = Arguments.GetString("codigo");
            //actor = Arguments.GetString("actor");
            
            var builder = new AlertDialog.Builder(Activity);
            var inflater = Activity.LayoutInflater;
            var dialogView = inflater.Inflate(Resource.Layout.SignatureDialogFragment, null);


            if (dialogView != null)
            {
                AddSignaturePad(dialogView);
                if (actor.Equals("notificando", StringComparison.Ordinal))
                {
                    builder.SetTitle("Firma notificando");
                }
                if (actor.Equals("testigo", StringComparison.Ordinal))
                {
                    builder.SetTitle("Firma testigo");
                }

                builder.SetView(dialogView);
                builder.SetPositiveButton("Aceptar", HandlePositiveButtonClick);
                builder.SetNegativeButton("Cancelar", HandleNegativeButtonClick);

            }

            //Create the builder 
            var dialog = builder.Create();

            //Now return the constructed dialog to the calling activity
            return dialog;
        }


        public void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            Toast.MakeText(this.Activity, "Presionado No", ToastLength.Short).Show();
        }

        public void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            
            try
            {
                //signature = Activity.FindViewById<SignaturePadView>(Resource.Id.signatureFrame);
                Bitmap imagen = signature.GetImage();
                MemoryStream ms = new MemoryStream();
                //ByteArrayOutputStream bos = new ByteArrayOutputStream();
                imagen.Compress(Bitmap.CompressFormat.Png,100, ms);
                byte[] bArray = ms.ToArray();

                //string id = Guid.NewGuid().ToString();
                System.Console.WriteLine("GUID: "+guid);
                loadConnection();
                db.BeginTransaction();
                //Se almacena en base de datos el BLOB con su respectivo GUID
                try
                {
                    string sql = "INSERT INTO IMAGENES (Nombre,Imagen) VALUES(?,?)";
                    SQLiteStatement insertStmt = db.CompileStatement(sql);
                    insertStmt.ClearBindings();
                    insertStmt.BindString(1,guid);
                    insertStmt.BindBlob(2,bArray);
                    insertStmt.ExecuteInsert();
                    db.SetTransactionSuccessful();
                    db.EndTransaction();
                    db.Close();
                    try
                    {
                        if (actor.Equals("notificando", StringComparison.Ordinal))
                        {
                            ManejoBaseDatos.Abrir();
                            ManejoBaseDatos.Actualizar("Notificaciones", "ValidacionNotificando", "S", "CodigoNotificacion=" + codigo + "");
                            ManejoBaseDatos.Cerrar();
                        }
                        if (actor.Equals("testigo", StringComparison.Ordinal))
                        {
                            ManejoBaseDatos.Abrir();
                            ManejoBaseDatos.Actualizar("Notificaciones", "ValidacionTestigo", "S", "CodigoNotificacion=" + codigo + "");
                            ManejoBaseDatos.Cerrar();
                        }


                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("Error almacenando confirmacion de firma: " + ex.ToString());
                        Toast.MakeText(this.Activity, "Error guardando confirmacion", ToastLength.Short).Show();
                    }

                }
                catch (Exception ex) { System.Console.WriteLine("Error guardando imagen en db: "+ex.ToString()); }

                Toast.MakeText(this.Activity, "Firma capturada de forma exitosa", ToastLength.Short).Show();
            }
            catch (Exception ex) { System.Console.WriteLine("ERROR guardando la imagen: "+ex.ToString()); }
        }
        /// <summary>
        /// Se crea el componente de signature pad y se agrega a la vista
        /// </summary>
        /// <param name="impresion"></param>
        public void AddSignaturePad(View firma)
        {
            //Se instancia la clase del SignaturePad
            signature = new SignaturePadView(Activity);

            signature.Id = Resource.Id.signatureFrame;
            //Se cambia el color del fondo
            signature.BackgroundColor = Android.Graphics.Color.White;
            //Se cambia el color de la firma
            signature.StrokeColor = Android.Graphics.Color.Black;
            //Se agrega la descripción del texto que aparece al lado izquierdo de la firma
            signature.SignaturePromptText = descripcionLateral;
            //Se agrega la descripción que aparece al lado abajo de la firma
            signature.CaptionText = descripcionFirma;
            //Se agrega la descripción del botón de borrar
            signature.ClearLabelText = descripcionBorrar;

            //Se crea la instancia del layout que contendra el componente del SignaturePad
            LinearLayout contenedor = firma.FindViewById<LinearLayout>(Resource.Id.LayoutFirma2);

            //Se agrega el componente SignaturePad al LinearLayout
            contenedor.AddView(signature, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

        }


        private void loadConnection()
        {
            //string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string dbPath = System.IO.Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, dbName);

            bool exist = System.IO.File.Exists(dbPath);
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
                System.Console.WriteLine("ERROR ABRIENDO BASE DE DATOS: " + ex.ToString());
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