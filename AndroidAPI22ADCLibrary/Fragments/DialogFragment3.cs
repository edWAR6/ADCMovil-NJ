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
    public class DialogFragment3 : Android.Support.V4.App.DialogFragment
    {

        private int codigo=0;
        protected EditText NameEditText;
        protected TextView mtextView;

        public static DialogFragment3 NewInstance(int codigo)
        {
            var dialogFragment = new DialogFragment3();
            Bundle args = new Bundle();
            args.PutInt("someInt", codigo);
            dialogFragment.Arguments = args;
            return dialogFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            codigo = Arguments.GetInt("someInt");
            var builder = new AlertDialog.Builder(Activity);
            var inflater = Activity.LayoutInflater;
            var dialogView = inflater.Inflate(Resource.Layout.addServiceDialog, null);


            if (dialogView != null)
            {
                FragmentList.motivo = "";
                NameEditText = dialogView.FindViewById<EditText>(Resource.Id.editTextNameASD);
                mtextView = dialogView.FindViewById<TextView>(Resource.Id.textTitleASD);
                if (codigo == 0)
                    mtextView.Text = "Aceptar resultado de notifiacion";
                if (codigo == 1)
                    mtextView.Text = "Rechazar resultado de notificacion";

                builder.SetView(dialogView);
                builder.SetPositiveButton("Aceptar", HandlePositiveButtonClick);
                builder.SetNegativeButton("Cancelar", HandleNegativeButtonClick);
                NameEditText.TextChanged += NameEditText_TextChanged;
            }


                //Create the builder 
                var dialog = builder.Create();

            //Now return the constructed dialog to the calling activity
            return dialog;
        }

        private void NameEditText_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            //throw new NotImplementedException();
            Console.WriteLine("Editando: "+NameEditText.Text);
            FragmentList.motivo = NameEditText.Text;
        }

        public void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {
            //Toast.MakeText(this.Activity,"Presionado si",ToastLength.Short).Show();
            try
            {
                if (codigo==0)
                {
                    //se aprueba
                    var dialog = (AlertDialog)sender;
                    TargetFragment.OnActivityResult(TargetRequestCode, 1, this.Activity.Intent);
                }
                if (codigo==1)
                {
                    if (!string.IsNullOrEmpty(FragmentList.motivo))
                    {
                        //en correcion
                        var dialog = (AlertDialog)sender;
                        TargetFragment.OnActivityResult(TargetRequestCode, 0, this.Activity.Intent);
                    }
                    else
                        Toast.MakeText(this.Activity, "Por favor incluya las observaciones", ToastLength.Long).Show();
                }
                
                //Activity.StartService(new Intent(this.Activity, typeof(Helpers.servicioCheckDB)));
            }
            catch (Exception ex) { Console.WriteLine("ERROR HANDLE POSITIVE BUTTON: " + ex.ToString()); }

        }

        public void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            Toast.MakeText(this.Activity, "Presionado No", ToastLength.Short).Show();
        }


    }
}