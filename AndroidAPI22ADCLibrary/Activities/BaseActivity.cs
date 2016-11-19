
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;

namespace AndroidAPI22ADCLibrary.Activities
{
    public abstract class BaseActivity : AppCompatActivity
    {
        public Toolbar Toolbar
        {
            get;
            set;
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            try
            {
                SetContentView(LayoutResource);
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                }
            }
            catch (Exception ex) { Console.WriteLine("Exception in baseActivity: "+ex.ToString()); }
        }

        protected abstract int LayoutResource
        {
            get;
        }

        protected int ActionBarIcon
        {
            set { Toolbar.SetNavigationIcon(value); }
        }
    }


}

