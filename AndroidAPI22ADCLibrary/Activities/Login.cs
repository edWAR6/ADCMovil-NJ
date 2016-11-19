using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.Design.Widget;
using Android.Content.PM;
using AndroidAPI22ADCLibrary.Fragments;
using Android.Support.V4.Widget;

namespace AndroidAPI22ADCLibrary.Activities
{
    //[Activity(Label = "Login", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, Icon = "@drawable/Icon")]
    [Activity(Label = "Login")]
    public class Login : BaseActivity
    {
        NavigationView navigationView;
        DrawerLayout drawerLayout;

        protected override int LayoutResource
        {
            get
            {
                return Resource.Layout.main;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Login);


            drawerLayout = this.FindViewById<DrawerLayout>(Resource.Id.drawer_layout);

            //setup navigation view
            navigationView = FindViewById<NavigationView>(Resource.Id.nav_view_login);

            //Fragment loginFragment = new Fragment();
            Android.Support.V4.App.Fragment fragment = null;
            fragment = FragmentLogin.NewInstance();

            //if (savedInstanceState == null)
            //{
            //    currentFragment = new DefaultFragment();
            //    getFragmentManager().beginTransaction()
            //            .add(R.id.frame, currentFragment).commit();
            //}

            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, fragment).Commit();

        }
    }
}