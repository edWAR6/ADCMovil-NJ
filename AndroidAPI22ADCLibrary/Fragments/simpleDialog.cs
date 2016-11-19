using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Support.V4.App;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class simpleDialog : Android.Support.V4.App.DialogFragment
    {
        private EditText mTextEdit;
        private string mTitle="";
        private int codigo = 0;


        public simpleDialog()
        {

        }

        public static simpleDialog newInstance(string title, int codigo)
        {
            simpleDialog frag = new simpleDialog();
            Bundle args = new Bundle();
            args.PutInt("someInt", codigo);
            args.PutString("someTitle",title);
            frag.Arguments=args;
            return frag;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
             return inflater.Inflate(Resource.Layout.simpleDialog, container, false);
            //return base.OnCreateView(inflater, container, savedInstanceState);
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            //base.OnViewCreated(view, savedInstanceState);
            mTitle = Arguments.GetString("someTitle");
            codigo = Arguments.GetInt("someInt");

            var alertDialogBuilder = new AlertDialog.Builder(Activity);
            var inflater = Activity.LayoutInflater;
            var dialogView = inflater.Inflate(Resource.Layout.seleccionarNotificador, null);


            mTextEdit = dialogView.FindViewById<EditText>(Resource.Id.txtEditSimpleDialog);

            if (codigo == 0)//con codigo cero se oculta el textEdit
            {
                mTextEdit.Visibility = ViewStates.Invisible;
            }
            else
            {
                mTextEdit.Visibility = ViewStates.Visible;
            }

            Activity.StopService(new Intent(this.Activity, typeof(Helpers.servicioCheckDB)));
            alertDialogBuilder.SetTitle(mTitle);

            if (codigo == 0)
            {
                alertDialogBuilder.SetMessage("Desea aceptar los resultados de las notificaciones?");
            }
            else
            {
                alertDialogBuilder.SetMessage("Desea rechazar los resultados de las notificaciones?");
            }
                      
            alertDialogBuilder.SetPositiveButton("Continuar", HandlePositiveButtonClick);
            alertDialogBuilder.SetNegativeButton("Cancelar", HandleNegativeButtonClick);

            var dialog = alertDialogBuilder.Create();
            return dialog;

        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            try
            {
                var dialog = (AlertDialog)sender;
                //Se retorna la respuesta al request code 3 dentro de activity results.
                //Se devuelve el codigo 1 que es con el cual se aceptan los resultados de las notificaciones.
                if (codigo == 0)
                {
                    TargetFragment.OnActivityResult(TargetRequestCode,1, this.Activity.Intent);
                }
                if (codigo==1)
                {
                    TargetFragment.OnActivityResult(TargetRequestCode, 0, this.Activity.Intent);
                }
            }
            catch (Exception ex) { Console.WriteLine("ERROR HANDLE POSITIVE BUTTON: " + ex.ToString()); }

        }
        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            dialog.Dismiss();
        }
    }
}