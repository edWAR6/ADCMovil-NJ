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

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class FragmentYesNo : DialogFragment
    {
        //Create class properties
        protected TextView txtTitulo;
        protected TextView txtDescripcion;



        public static FragmentYesNo NewInstance()
        {
            var mFragment = new FragmentYesNo();
            return mFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here

            // Begin building a new dialog.
            var builder = new AlertDialog.Builder(Activity);

            //Get the layout inflater
            var inflater = Activity.LayoutInflater;

            //Inflate the layout for this dialog
            var dialogView = inflater.Inflate(Resource.Layout.fragmentYesNo, null);


            if (dialogView != null)
            {
                //Initialize the properties

                txtTitulo = dialogView.FindViewById<TextView>(Resource.Id.textTitleYN);
                txtDescripcion = dialogView.FindViewById<TextView>(Resource.Id.textViewNameYN);

                txtTitulo.Text = "Alerta";
                txtDescripcion.Text = "¿Desea continuar con la operación actual? ";
                builder.SetView(dialogView);
                builder.SetPositiveButton("Aceptar", HandlePositiveButtonClick);
                builder.SetNegativeButton("Cancelar", HandleNegativeButtonClick);
            }
            //Create the builder 
            var dialog = builder.Create();

            //Now return the constructed dialog to the calling activity
            return dialog;
        }


        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {

        }

        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            dialog.Dismiss();
        }
    }
}