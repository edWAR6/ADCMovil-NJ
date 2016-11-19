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
    public class DialogFragment2 : DialogFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Helpers.dbConeccion = new Helpers.SQLiteConeccion();//Se crea la instancia de la base de datos
        }
        public static DialogFragment2 NewInstance(Bundle bundle)
        {
            DialogFragment2 fragment = new DialogFragment2();
            fragment.Arguments = bundle;
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.fragmentDialog, container, false);
            Button button = view.FindViewById<Button>(Resource.Id.CloseButton);
            button.Click += delegate
            {
                Dismiss();
                Toast.MakeText(Activity, "Dialog fragment dismissed!", ToastLength.Short).Show();
            };

            return view;
        }
    }
}