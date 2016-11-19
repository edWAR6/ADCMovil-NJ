using Android.App;
using Android.OS;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using System;
using Android.Widget;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Views;
using Android.Content;
using AndroidAPI22ADCLibrary.Fragments;

namespace AndroidAPI22ADCLibrary.Activities
{
    [Activity(Label = "DetailActivity", Theme = "@style/Base.Theme.DesignDemo")]
    public class DetailActivity : AppCompatActivity
    {
        //Variable que contiene el detalle de la notificacion
        public const string EXTRA_NAME = "";
        public const string EXTRA_INFO = "";
        private string codigoNotificacionIn = "";
        //public const string EXTRA_CODIGO_NOTIFICACION = "";

        

        //Habilita la edicion de datos en el menu de la notificacion.
        private bool edicion = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Activity_Detail);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            //Se obtiene el nombre del expediente a mostrar en el detalle
            string title = Intent.GetStringExtra(EXTRA_NAME);
            string info = Intent.GetStringExtra(EXTRA_INFO);
            codigoNotificacionIn = Intent.GetStringExtra("EXTRA_CODIGO_NOTIFICACION");

            CollapsingToolbarLayout collapsingToolBar = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            TextView detailTextView = FindViewById<TextView>(Resource.Id.txtNotificando);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            detailTextView.Text = info.Split('@')[1];

            TextView sector = FindViewById<TextView>(Resource.Id.txtSector);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            sector.Text = info.Split('@')[9];

            TextView fechaDocumento = FindViewById<TextView>(Resource.Id.txtFechaDocumento);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            fechaDocumento.Text = info.Split('@')[7];

            TextView provincia = FindViewById<TextView>(Resource.Id.txtProvincia);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            provincia.Text = info.Split('@')[2];

            TextView canton = FindViewById<TextView>(Resource.Id.txtCanton);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            canton.Text = info.Split('@')[3];

            TextView distrito = FindViewById<TextView>(Resource.Id.txtDistrito);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            distrito.Text = info.Split('@')[4];

            TextView direccion = FindViewById<TextView>(Resource.Id.txtDireccion);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            direccion.Text = info.Split('@')[5];

            TextView despacho = FindViewById<TextView>(Resource.Id.txtBarrio);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            despacho.Text = info.Split('@')[6];

            TextView fechaEmision = FindViewById<TextView>(Resource.Id.txtFechaEmision);
            //Se le asigna el texto al TextView que se muestra en la pantalla de detalles
            fechaEmision.Text = info.Split('@')[8];

            //se le asigna el titulo a la barra que colapsa la informacion
            collapsingToolBar.SetTitle(info.Split('@')[0]);

            LoadBackDrop();

            FloatingActionButton editFab = FindViewById<FloatingActionButton>(Resource.Id.editFab);
            editFab.Click += (o, e) =>
            {
                //Toast.MakeText(this, "Edit", ToastLength.Short).Show();

                if (MailBoxes.currentPage == 0)
                {
                    Toast.MakeText(this, "El registro de datos debe ser realizado en el buzon notificándose", ToastLength.Long).Show();
                }
                if (MailBoxes.currentPage == 1 && !(FragmentLogin.supervisor.Equals("true", StringComparison.Ordinal) || FragmentLogin.supervisor.Equals("True", StringComparison.Ordinal)))
                {
                    Intent intent = new Intent(this, typeof(MainActivity));
                    //intent.PutExtra(DetailActivity.EXTRA_NAME, detailActivityTitle[position]);
                    intent.PutExtra("EXTRA_INFO", "EditarNotificacion");
                    intent.PutExtra("EXTRA_CODIGO_NOTIFICACION", codigoNotificacionIn);
                    StartActivity(intent);
                }
                if (MailBoxes.currentPage == 2)
                {
                    //var dialog = ServiceDialog.NewInstance();
                    //dialog.Show(FragmentManager, "dialog");
                }

            };

            FloatingActionButton reintentoFab = FindViewById<FloatingActionButton>(Resource.Id.reintentoFab);
            reintentoFab.Click += (s, args) => {

                Intent intent = new Intent(this, typeof(MainActivity));
                //intent.PutExtra(DetailActivity.EXTRA_NAME, detailActivityTitle[position]);
                intent.PutExtra("EXTRA_INFO", "ReintentoNotificacion");
                intent.PutExtra("EXTRA_CODIGO_NOTIFICACION", codigoNotificacionIn);
                StartActivity(intent);

            };

            if (MailBoxes.currentPage == 0)
            {
                editFab.Visibility = ViewStates.Invisible;
                reintentoFab.Visibility = ViewStates.Invisible;
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        //Se crea el menu de opciones con los datos del xml 
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (edicion)
            {
                MenuInflater.Inflate(Resource.Menu.sample_actions, menu);
                Console.WriteLine("On create options detail activity");
            }
            else
            {

            }
            return true;
        }

        private void LoadBackDrop()
        {

            //ImageView imageView = FindViewById<ImageView>(Resource.Id.backdrop);
            //Imagen a mostrar en el detalle de la notificacion
            //imageView.SetImageResource(Resource.Drawable.PJLogo2);
        }
    }
}