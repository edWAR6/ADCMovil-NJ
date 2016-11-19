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

using AndroidAPI22ADCLibrary.Helpers;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class DialogFragment1 : Android.Support.V4.App.DialogFragment
    {
        //private int codigo = 0;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Helpers.dbConeccion = new Helpers.SQLiteConeccion();//Se crea la instancia de la base de datos
        }
        public static DialogFragment1 NewInstance(int codigo)
        {
            DialogFragment1 fragment = new DialogFragment1();
            Bundle args = new Bundle();
            args.PutInt("someInt",codigo);
            if (args != null)
            {
                fragment.Arguments = args;
            }
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