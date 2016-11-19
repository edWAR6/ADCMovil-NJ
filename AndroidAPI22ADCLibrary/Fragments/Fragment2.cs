using Android.Animation;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Database;
using Android.Support.V7.App;
using Android.Content;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class Fragment2 : Fragment
    {
        private LinearLayout expandable;
        private static string codNotificacion = "";

        private TextView mExpediente;
        private TextView mSector;
        private TextView mFormaNotificacion;
        private TextView mProvincia;
        private TextView mCanton;
        private TextView mDistrito;
        private TextView mDireccion;
        private TextView mCopias;
        private TextView mOficial;
        private TextView mFechaNotificacion;
        private TextView mresultadoNotificacion;



        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public static Fragment2 NewInstance(string codigoNotificacion)
        {
            codNotificacion = codigoNotificacion;
            var fragment2 = new Fragment2 { Arguments = new Bundle() };
            return fragment2; //Se retorna la instancia creada
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
           
            View view = inflater.Inflate(Resource.Layout.detalle, container, false);

            Button btnRegistroDatos = view.FindViewById<Button>(Resource.Id.btnRegistrarDatos);
            expandable = view.FindViewById<LinearLayout>(Resource.Id.linearLayoutFillData);
            
            //Definicion de los diferentes espacios de texto a mostrar
            mExpediente = view.FindViewById<TextView>(Resource.Id.txtExpediente);
            mSector = view.FindViewById<TextView>(Resource.Id.txtSector);
            mFormaNotificacion = view.FindViewById<TextView>(Resource.Id.txtFormaNotificacion);
            mProvincia = view.FindViewById<TextView>(Resource.Id.txtProvincia);
            mCanton = view.FindViewById<TextView>(Resource.Id.txtCanton);
            mDistrito = view.FindViewById<TextView>(Resource.Id.txtDistrito);
            mDireccion = view.FindViewById<TextView>(Resource.Id.txtDistrito);
            mCopias = view.FindViewById<TextView>(Resource.Id.txtCopias);
            mOficial = view.FindViewById<TextView>(Resource.Id.txtOficialNombreDatos);
            mFechaNotificacion = view.FindViewById<TextView>(Resource.Id.txtFechaNotificacion);
            mresultadoNotificacion = view.FindViewById<TextView>(Resource.Id.txtResultadoNotificacion);

            Switch mSwitch = view.FindViewById<Switch>(Resource.Id.monitored_switch);

            mSwitch.CheckedChange += delegate (object sender, CompoundButton.CheckedChangeEventArgs e) 
            {
                mSwitch.Text = "Resultado "+ (e.IsChecked ? "Positivo" : "Negativo");
            };
            expandable.Visibility = ViewStates.Gone;
            btnRegistroDatos.Click += (s, e) =>
            {
                if (expandable.Visibility.Equals(ViewStates.Gone))
                {
                    expandable.Visibility = ViewStates.Visible;
                    int widthSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                    int heightSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
                    expandable.Measure(widthSpec, heightSpec);
                    ValueAnimator mAnimator = slideAnimator(0, expandable.MeasuredHeight);
                    mAnimator.Start();
                }
                else
                {
                    int finalHeight = expandable.Height;
                    ValueAnimator mAnimator = slideAnimator(finalHeight, 0);
                    mAnimator.Start();
                    mAnimator.AnimationEnd += (object IntentSender, EventArgs arg) => {
                        expandable.Visibility = ViewStates.Gone;
                    };
                }
            };
            //
            if (!string.IsNullOrEmpty(codNotificacion))
            {
                string query = "SELECT * FROM Notificaciones WHERE CodigoNotificacion=" + codNotificacion + "";
                ManejoBaseDatos.Abrir();
                ICursor mCursor = ManejoBaseDatos.Seleccionar(query);
                if (mCursor.MoveToFirst())
                {
                    do
                    {
                        mExpediente.Text = (!string.IsNullOrEmpty(mCursor.GetString(mCursor.GetColumnIndex("Expediente"))) ? mCursor.GetString(mCursor.GetColumnIndex("Expediente")) : ""); 
                        mSector.Text = (!string.IsNullOrEmpty(mCursor.GetString(mCursor.GetColumnIndex("Sector"))) ? mCursor.GetString(mCursor.GetColumnIndex("Sector")) : ""); 

                        mProvincia.Text = (!string.IsNullOrEmpty(mCursor.GetString(mCursor.GetColumnIndex("Provincia"))) ? mCursor.GetString(mCursor.GetColumnIndex("Provincia")) : "");
                        mCanton.Text = (!string.IsNullOrEmpty(mCursor.GetString(mCursor.GetColumnIndex("Canton"))) ? mCursor.GetString(mCursor.GetColumnIndex("Canton")) : ""); ;
                        mDistrito.Text = (!string.IsNullOrEmpty(mCursor.GetString(mCursor.GetColumnIndex("Distrito"))) ? mCursor.GetString(mCursor.GetColumnIndex("Distrito")) : ""); ;




                    } while (mCursor.MoveToNext());

                }
                mCursor.Close();
                ManejoBaseDatos.Cerrar();
            }
            else
            {
                AlertDialog.Builder alerta = new AlertDialog.Builder(this.Context);
                alerta.SetTitle("Mensaje de alerta");
                alerta.SetIcon(Resource.Drawable.alertaNuevo);
                alerta.SetMessage("El usuario tiene una jornada abierta.");
                alerta.SetPositiveButton("Regresar", HandleButtonClick);
                //alerta.SetNegativeButton("Continuar", HandleButonContinuar);
                alerta.SetCancelable(false);
                alerta.Create();
                alerta.Show();
            }
            return view;
        }

        private void HandleButtonClick(object sender, DialogClickEventArgs e)
        {
            //Java.Lang.JavaSystem.Exit(0);
            this.Activity.Finish();
        }

        /// <summary>
        /// Se encarga de realizar la animacion de expansion de layout "expandable"
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private ValueAnimator slideAnimator(int start, int end)
        {

            ValueAnimator animator = ValueAnimator.OfInt(start, end);

            animator.Update +=
                (object sender, ValueAnimator.AnimatorUpdateEventArgs e) => {

                    var value = (int)animator.AnimatedValue;
                    ViewGroup.LayoutParams layoutParams = expandable.LayoutParameters;
                    layoutParams.Height = value;
                    expandable.LayoutParameters = layoutParams;

                };

            return animator;
        }

        
    }
}