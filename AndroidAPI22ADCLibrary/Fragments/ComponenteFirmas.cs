using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using SignaturePad;
using Android.Graphics;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class ComponenteFirmas : Fragment
    {

        static string descripcionFirma = "Firma de la persona";//Descripción del pad de firmas
        static string descripcionLateral = "X: ";//Descripción que aparece a la par de la linea de firma
        static string descripcionBorrar = "Borrar";//Descripción del botón de borrar

        static Fragment fragmentoRetornar;//Fragmento al que se devuelve cuando se realiza el guardado

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Activity.Title = "Firma";//Se cambia el titulo de la pantalla            
        }

        /// <summary>
        /// Se crea la instancia del fragmento de componente de firmas
        /// </summary>
        /// <param name="descripcion">Descripción del pad de firmas</param>
        /// <returns></returns>
        public static ComponenteFirmas NewInstance(string descripcion)
        {
            //Se pasa la descripción que se utiliza debajo de la línea de firma
            descripcionFirma = descripcion;
            //Se crea la instancia de la clase ComponenteFirmas
            return new ComponenteFirmas { Arguments = new Bundle() };
        }

        /// <summary>
        /// Se crea la instancia del fragmento de componente de firmas
        /// </summary>
        /// <param name="descripcion">Descripción del pad de firmas</param>
        /// <returns></returns>
        public static ComponenteFirmas NewInstance(string descripcion, Fragment fragmento)
        {
            //Se pasa la descripción que se utiliza debajo de la línea de firma

            descripcionFirma = descripcion;
            fragmentoRetornar = fragmento;

            //Se crea la instancia de la clase ComponenteFirmas
            return new ComponenteFirmas { Arguments = new Bundle() };
        }

        /// <summary>
        /// Se crea la vista del fragmento de firmas
        /// </summary>
        /// <param name="inflater"></param>
        /// <param name="container"></param>
        /// <param name="savedInstanceState"></param>
        /// <returns></returns>
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //Se infla la vista del fragmento del componente de firmas
            View firmas = inflater.Inflate(Resource.Layout.Firmas, container, false);
            //Se crea el componente de la firma
            AddSignaturePad(firmas);

            //Se crea la instancia del botón de guardar y se le crea el evento que se ejecuta al presionar el botón
            Button guardar = firmas.FindViewById<Button>(Resource.Id.btnGuardar);
            guardar.Click += Guardar_Click;

            //Se crea la instancia del botón de cancelar y se le crea el evento que se ejecuta al presionar el botón
            Button cancelar = firmas.FindViewById<Button>(Resource.Id.btnCancelar);
            cancelar.Click += Cancelar_Click;

            ((Activities.MainActivity)Activity).habilitarMenuLateral(false);

            return firmas;
        }

        /// <summary>
        /// No se guarda la firma
        /// Se retorna a la vista de donde fue llamada la pantalla de firmas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancelar_Click(object sender, EventArgs e)
        {
            if (fragmentoRetornar != null)
            {
                ((Activities.MainActivity)Activity).navegacionFragment(fragmentoRetornar);
            }
        }

        /// <summary>
        /// Se guarda la firma
        /// Se devuelve a la vista de donde fue llamada la pantalla de firmas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Guardar_Click(object sender, EventArgs e)
        {
            if(fragmentoRetornar != null)
            {
                //fzeledon
                SignaturePadView signature = Activity.FindViewById<SignaturePadView>(Resource.Id.signatureFrame);
                Bitmap imagen = signature.GetImage();
                var t = signature.BackgroundImageView;
                //guardarFirma(imagen);//Se guarda la firma

                //Se retorna a la pantalla anterior
                ((Activities.MainActivity)Activity).navegacionFragment(fragmentoRetornar);
            }
        }

        /// <summary>
        /// Se obtiene la imagen de la firma guardada
        /// </summary>
        /// <returns></returns>

        //public Bitmap getFirma()
        //{
        //    return  ((Activities.MainActivity)Activity).firmaUsuario.firma;
        //}

        /// <summary>
        /// Se guarda la imagen con la firma
        /// </summary>
        /// <param name="firma"></param>
        public void guardarFirma(Bitmap firma)
        {
        //    Models.Firma mFirma = new Models.Firma(firma);
        //    ((Activities.MainActivity)Activity).firmaUsuario = mFirma;
        }


        /// <summary>
        /// Se crea el componente de signature pad y se agrega a la vista
        /// </summary>
        /// <param name="impresion"></param>
        public void AddSignaturePad(View firma)
        {
            //Se instancia la clase del SignaturePad
            var signature = new SignaturePadView(Activity);

            signature.Id = Resource.Id.signatureFrame;
            //Se cambia el color del fondo
            signature.BackgroundColor = Android.Graphics.Color.White;
            //Se cambia el color de la firma
            signature.StrokeColor = Android.Graphics.Color.Black;
            //Se agrega la descripción del texto que aparece al lado izquierdo de la firma
            signature.SignaturePromptText = descripcionLateral;
            //Se agrega la descripción que aparece al lado abajo de la firma
            signature.CaptionText = descripcionFirma;
            //Se agrega la descripción del botón de borrar
            signature.ClearLabelText = descripcionBorrar;

            //Se crea la instancia del layout que contendra el componente del SignaturePad
            LinearLayout contenedor = firma.FindViewById<LinearLayout>(Resource.Id.LayoutFirma);

            //Se agrega el componente SignaturePad al LinearLayout
            contenedor.AddView(signature, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

        }

    }
}