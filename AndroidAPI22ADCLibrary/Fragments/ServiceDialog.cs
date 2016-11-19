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
using SignaturePad;


namespace AndroidAPI22ADCLibrary.Fragments
{
    public class ServiceDialog : DialogFragment
    {

        Helpers.SQLiteConeccion dbConeccion; //Se crea el objeto de la base de datos

        //Create class properties
        protected EditText NameEditText;
        protected EditText DescriptionEditText;
        protected EditText PriceEditText;
        protected EditText AddCategoryEditText;
        protected Spinner CategorySpinner;
        protected LinearLayout CategoryLayout;
        protected LinearLayout Container;
        protected CheckBox CategoryCheckBox;
        protected Button CategoryButton;
        private int codigoGen=0;

        //Create the string that will hold the value
        //Of the category drop down selected item
        protected string SelectedCategory = "";

        //static string descripcionFirma = "Firma del Notificando";//Descripción del pad de firmas
        //static string descripcionLateral = "X: ";//Descripción que aparece a la par de la linea de firma
        //static string descripcionBorrar = "Borrar";//Descripción del botón de borrar

        public static ServiceDialog NewInstance(int codigo)
        {
            var dialogFragment = new ServiceDialog();
            Bundle args = new Bundle();
            args.PutInt("someInt",codigo);          
            if (args != null)
            {
                dialogFragment.Arguments = args;
            }
            return dialogFragment;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            codigoGen = Arguments.GetInt("someInt");

            dbConeccion = new Helpers.SQLiteConeccion();

            // Begin building a new dialog.
            var builder = new AlertDialog.Builder(Activity);

            //Get the layout inflater
            var inflater = Activity.LayoutInflater;

            //Inflate the layout for this dialog
            var dialogView = inflater.Inflate(Resource.Layout.serviceDialog, null);

            if (dialogView != null)
            {
                //Initialize the properties
                NameEditText = dialogView.FindViewById<EditText>(Resource.Id.editTextName);

                //DescriptionEditText = dialogView.FindViewById<EditText>(Resource.Id.editTextDescription);
                //PriceEditText = dialogView.FindViewById<EditText>(Resource.Id.editTextPrice);

                //AddCategoryEditText = dialogView.FindViewById<EditText>(Resource.Id.editTextAddCategory);
                //CategorySpinner = dialogView.FindViewById<Spinner>(Resource.Id.spinnerCategory);

                CategoryLayout = dialogView.FindViewById<LinearLayout>(Resource.Id.linearLayoutCategorySection);

                //Container = dialogView.FindViewById<LinearLayout>(Resource.Id.linearLayoutContainer);


                //Hide this section for now

                if (codigoGen == 0)
                {
                    CategoryLayout.Visibility = ViewStates.Invisible;
                }
                else
                {
                    CategoryLayout.Visibility = ViewStates.Visible;
                }

                //CategoryButton = dialogView.FindViewById<Button>(Resource.Id.buttonCategory);

                //cargarComboNotificador(this.Activity,dialogView);

                //Set on item selected listener for the spinner
                //CategorySpinner.ItemSelected += spinner_ItemSelected;

                builder.SetView(dialogView);
                builder.SetPositiveButton("Aceptar", HandlePositiveButtonClick);
                builder.SetNegativeButton("Cancelar", HandleNegativeButtonClick);

                //AddSignaturePad(dialogView);
            }
            //Create the builder 
            var dialog = builder.Create();

            //Now return the constructed dialog to the calling activity
            return dialog;

        }

        //Handler for the drop down list
        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var spinner = (Spinner)sender;
            SelectedCategory = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
        }

        private void HandlePositiveButtonClick(object sender, DialogClickEventArgs e)
        {

        }

        private void HandleNegativeButtonClick(object sender, DialogClickEventArgs e)
        {
            var dialog = (AlertDialog)sender;
            dialog.Dismiss();
        }

        public void cargarComboNotificador(Context context, View mView)
        {

            try
            {
                //Se crea la instancia del spinner para el combo de provincias
                //Spinner mSpinner = mView.FindViewById<Spinner>(Resource.Id.spinnerCategory);
                //Se realiza la consulta a la base de datos y se carga el combo de provincias
                //dbConeccion.setAdaptadorCombo("SELECT NombreCompleto FROM OficialesNotificadores WHERE Activo='True' ", this.Activity, ref mSpinner);
            }
            catch (Exception e)
            {
                //Se guarda el error en el log de errores
                Logs.saveLogError("FragmentMap.cargarComboNotificador " + e.Message + " " + e.StackTrace);
                //Se muestra un mensaje informando el error
                Toast.MakeText(context, GetString(Resource.String.MensajeErrorCargaBaseDatos), ToastLength.Long).Show();
            }

        }

        //public void AddSignaturePad(View firma)
        //{
        //    //Se instancia la clase del SignaturePad
        //    var signature = new SignaturePadView(Activity);
        //    signature.Id = Resource.Id.componenteFirma;

        //    //Se cambia el color del fondo
        //    signature.BackgroundColor = Android.Graphics.Color.White;
        //    //Se cambia el color de la firma
        //    signature.StrokeColor = Android.Graphics.Color.Black;
        //    //Se agrega la descripción del texto que aparece al lado izquierdo de la firma
        //    signature.SignaturePromptText = descripcionLateral;
        //    //Se agrega la descripción que aparece al lado abajo de la firma
        //    signature.CaptionText = descripcionFirma;
        //    //Se agrega la descripción del botón de borrar
        //    signature.ClearLabelText = descripcionBorrar;

        //    //signature.VerticalScrollBarEnabled = false;

        //    //Se crea la instancia del layout que contendra el componente del SignaturePad
        //    LinearLayout contenedor = firma.FindViewById<LinearLayout>(Resource.Id.LayoutFirma);

        //    //Se agrega el componente SignaturePad al LinearLayout
        //    contenedor.AddView(signature, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        //}
    }
}