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
using Android.Support.V4.App;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Database;
using AndroidAPI22ADCLibrary.Fragments;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class EditNameDialogFragment : Android.Support.V4.App.DialogFragment
    {
        //private EditText mEditText;
        public static string notificadorSeleccionado;
        public static string codigoNotificadorSeleccionado;

        private Spinner seleccionarNotificador;
        //private LinearLayout mLayout;
        private string title;
        private List<string> codNotificadores = new List<string>();

        //Helpers.SQLiteConeccion dbConeccion;

        public EditNameDialogFragment()
        {
            // Empty constructor required for DialogFragment
        }

        public static EditNameDialogFragment newInstance(string title)
        {
            EditNameDialogFragment frag = new EditNameDialogFragment();
            Bundle args = new Bundle();
            args.PutString("title", title);
            frag.Arguments = args;
            return frag;
        }

        public static EditNameDialogFragment newInstance(string title,List<string>codigoNotificaciones)
        {
            EditNameDialogFragment frag = new EditNameDialogFragment();
            Bundle args = new Bundle();
            args.PutString("title", title);
            frag.Arguments = args;
            return frag;
        }


        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            title = Arguments.GetString("title");

            var alertDialogBuilder = new AlertDialog.Builder(Activity);
            var inflater = Activity.LayoutInflater;
            var dialogView = inflater.Inflate(Resource.Layout.seleccionarNotificador, null);

            //Activity.StopService(new Intent(this.Activity, typeof(Helpers.servicioCheckDB)));

            alertDialogBuilder.SetTitle(title);
            alertDialogBuilder.SetMessage("Desea realizar la auto-asignación");
            alertDialogBuilder.SetPositiveButton("Continuar", HandlePositiveButtonClick);
            alertDialogBuilder.SetNegativeButton("Cancelar", HandleNegativeButtonClick);
            notificadorSeleccionado = "";
            codigoNotificadorSeleccionado = "";

            //Verificando si se tiene un usuario admin para establecer el layout especial
            if (FragmentLogin.supervisor.Equals("True",StringComparison.Ordinal)|| FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal))
            {
                if (dialogView != null)
                {
                    alertDialogBuilder.SetMessage("Por favor seleccione un notificador");
                    seleccionarNotificador = dialogView.FindViewById<Spinner>(Resource.Id.spinnerDialogoSeleccionNotificador);
                    cargarComboNotificador(this.Activity, dialogView);
                    alertDialogBuilder.SetView(dialogView);
                    seleccionarNotificador.ItemSelected += SeleccionarNotificador_ItemSelected;
                }
            }
         
            var dialog = alertDialogBuilder.Create();
            return dialog;
        }

        private void SeleccionarNotificador_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var spinner = (Spinner)sender;
            string temporal = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
            Console.WriteLine("Notificador seleccionado: " + temporal);
            notificadorSeleccionado = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
            if (e.Position>=0)
            {
                codigoNotificadorSeleccionado = codNotificadores[e.Position];
            }
            
        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            try
            {
                var dialog = (AlertDialog)sender;
                TargetFragment.OnActivityResult(TargetRequestCode, 1, this.Activity.Intent);
                //FIXME : ANTERIORMENTE ESTABA ENCENDIDO
                //Activity.StartService(new Intent(this.Activity, typeof(Helpers.servicioCheckDB)));
            }
            catch (Exception ex) { Console.WriteLine("ERROR HANDLE POSITIVE BUTTON: "+ex.ToString()); }
            
        }
        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            //dialog.Dismiss();
            TargetFragment.OnActivityResult(TargetRequestCode, 2, this.Activity.Intent);
        }


        public void cargarComboNotificador(Context context, View mView)
        {

            try
            {
                //Se crea la instancia del spinner para el combo de provincias
                Spinner mSpinner = mView.FindViewById<Spinner>(Resource.Id.spinnerDialogoSeleccionNotificador);
                ManejoBaseDatos.Abrir();
                ICursor cursor = ManejoBaseDatos.Seleccionar("SELECT NombreCompleto,CodigoNotificador FROM OficialesNotificadores");
                List<string> data = new List<string>();

                if (cursor.MoveToFirst())
                {
                    //agregando en lista data los nombres de los notificadores 
                    do
                    {
                        data.Add(cursor.GetString(0));
                        codNotificadores.Add(cursor.GetString(1));
                    }
                    while (cursor.MoveToNext());
                }

                cursor.Close(); 
                Android.Widget.ArrayAdapter<String> adapter;
                adapter = new Android.Widget.ArrayAdapter<String>(context, Android.Resource.Layout.SimpleSpinnerItem, data);
                mSpinner.Adapter = adapter;
                ManejoBaseDatos.Cerrar();

            }
            catch (Exception ex)
            {
                //Se guarda el error en el log de errores
                //Logs.saveLogError("FragmentMap.cargarComboNotificador " + e.Message + " " + e.StackTrace);
                //Se muestra un mensaje informando el error
                Console.WriteLine("Error cargando spinner: " + ex.ToString());
                Toast.MakeText(context, GetString(Resource.String.MensajeErrorCargaBaseDatos), ToastLength.Long).Show();
            }

        }


        //public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        //{

        //    // Use this to return your custom view for this Fragment
        //    return inflater.Inflate(Resource.Layout.seleccionarNotificador, container, false);

        //    //return base.OnCreateView(inflater, container, savedInstanceState);
        //}

        //public override void OnViewCreated(View view, Bundle savedInstanceState)
        //{
        //    base.OnViewCreated(view, savedInstanceState);

        //    //seleccionarNotificador = view.FindViewById<Spinner>(Resource.Id.spinnerDialogoSeleccionNotificador);

        //    //mEditText = (EditText)view.FindViewById(Resource.Id.txt_your_name);
        //    // Fetch arguments from bundle and set title

        //    //Dialog.SetTitle(title);
        //    // Show soft keyboard automatically and request focus to field
        //    //mEditText.RequestFocus();
        //    //Dialog.Window.SetSoftInputMode(WindowManagerLayoutParams.);
        //}
    }
}