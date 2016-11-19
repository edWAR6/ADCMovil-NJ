using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using AndroidAPI22ADCLibrary.Helpers;
using AndroidAPI22ADCLibrary.Fragments;

namespace AndroidAPI22ADCLibrary.Reports
{
    public class CommonInputReport : Fragment, Android.App.DatePickerDialog.IOnDateSetListener
    {
        /* Reporte al cual se le asignarán los parametros de entrada */
        Report report;

        /* Cuales parametros son seleccionables en este reporte */
        bool sel_oficina;
        bool sel_notificador;
        bool sel_despacho;
        bool sel_fecha_jornada;
        bool sel_fecha_inicio;
        bool sel_fecha_fin;

        public CommonInputReport(Report report, bool sel_oficina, bool sel_notificador, bool sel_despacho, bool sel_fecha_jornada, bool sel_fecha_inicio, bool sel_fecha_fin)
        {
            this.report = report;
            this.sel_oficina = sel_oficina;
            this.sel_notificador = sel_notificador;
            this.sel_despacho = sel_despacho;
            this.sel_fecha_jornada = sel_fecha_jornada;
            this.sel_fecha_inicio = sel_fecha_inicio;
            this.sel_fecha_fin = sel_fecha_fin;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            View self = inflater.Inflate(Resource.Layout.CommonReportInput, null);
            TextView info = self.FindViewById<TextView>(Resource.Id.textViewReportInputInfo);
            Spinner spinner_oficina = self.FindViewById<Spinner>(Resource.Id.spinnerInputOficina);
            Spinner spinner_despacho = self.FindViewById<Spinner>(Resource.Id.spinnerInputDespacho);
            Spinner spinner_notificador = self.FindViewById<Spinner>(Resource.Id.spinnerInputNotificador);
            TextView fecha_inicio = self.FindViewById<TextView>(Resource.Id.textViewInputFechaInicio);
            TextView fecha_fin = self.FindViewById<TextView>(Resource.Id.textViewInputFechaFin);
            TextView fecha_jornada = self.FindViewById<TextView>(Resource.Id.textViewInputFechaJornada);

            // Oculta/Muestra los parametros de entrada que no corresponden al
            // reporte/perfil de usuario
            bool es_supervisor = false;
            try
            {
                string es_supervisor_str = "False";
                Helpers.SQLiteConeccion dbConeccion;
                dbConeccion = new Helpers.SQLiteConeccion();
                dbConeccion.consultaDatos("SELECT Supervisor FROM OficialNotificador", this.Activity, ref es_supervisor_str);
                es_supervisor = (String.Compare("True", es_supervisor_str, true) == 0);
            }
            catch (Exception e)
            {
                //Se guarda el error en el log de errores
                Logs.saveLogError("NotificacionesEnviadasADespachoInputReport.OnCreateView " + e.Message + " " + e.StackTrace);
                //Se muestra un mensaje informando el error
                Toast.MakeText(this.Context, GetString(Resource.String.MensajeErrorCargaBaseDatos), ToastLength.Long).Show();
            }

            self.FindViewById<LinearLayout>(Resource.Id.linearLayoutInputOficina).Visibility = sel_oficina ? ViewStates.Visible : ViewStates.Gone;
            self.FindViewById<LinearLayout>(Resource.Id.linearLayoutInputNotificador).Visibility = (sel_notificador && es_supervisor) ? ViewStates.Visible : ViewStates.Gone;
            self.FindViewById<LinearLayout>(Resource.Id.linearLayoutInputDespacho).Visibility = sel_despacho ? ViewStates.Visible : ViewStates.Gone;
            self.FindViewById<LinearLayout>(Resource.Id.linearLayoutInputFechaInicio).Visibility = sel_fecha_inicio ? ViewStates.Visible : ViewStates.Gone;
            self.FindViewById<LinearLayout>(Resource.Id.linearLayoutInputFechaFin).Visibility = sel_fecha_fin ? ViewStates.Visible : ViewStates.Gone;
            self.FindViewById<LinearLayout>(Resource.Id.linearLayoutInputFechaJornada).Visibility = sel_fecha_jornada ? ViewStates.Visible : ViewStates.Gone;

            // Cargar los valores que se tengan del ultimo reporte
            fecha_inicio.Text = report.input_fecha_inicio.ToString("yyyy-MMM-dd");
            fecha_fin.Text = report.input_fecha_fin.ToString("yyyy-MMM-dd");
            fecha_jornada.Text = report.input_fecha_jornada.ToString("yyyy-MMM-dd");

            // FIXME: Poner el mensaje de error en un mejor lugar
            if (String.IsNullOrEmpty(report.output_error))
            {
                info.Text = "Datos de entrada para Reporte: " + report.ToString();
            }
            else
            {
                info.Text = report.output_error;
                report.output_error = null;
            }

            // Accion para el botón de generar reporte
            Button btnGen = self.FindViewById<Button>(Resource.Id.btnGenerarReporte);
            btnGen.Click += (sender, e) => {
                Fragment frag = report.getOutputReportFragment();
                // Asigna los parámetros de entrada (las fechas se assignan en OnDateSet)
                report.input_oficina = "0534"; // FIXME. El valor esta harc
                report.input_despacho = (string)spinner_despacho.SelectedItem;
                if (sel_notificador)
                {
                    if (es_supervisor) report.input_notificador = (string)spinner_notificador.SelectedItem;
                    else {
                        try
                        {
                            Helpers.SQLiteConeccion dbConeccion;
                            dbConeccion = new Helpers.SQLiteConeccion();
                            dbConeccion.consultaDatos("SELECT NombreCompleto FROM OficialNotificador", this.Activity, ref report.input_notificador);
                        }
                        catch (Exception ex)
                        {
                            //Se guarda el error en el log de errores
                            Logs.saveLogError("NotificacionesEnviadasADespachoInputReport.OnCreateView " + ex.Message + " " + ex.StackTrace);
                            //Se muestra un mensaje informando el error
                            Toast.MakeText(this.Context, GetString(Resource.String.MensajeErrorCargaBaseDatos), ToastLength.Long).Show();
                            report.input_notificador = "";
                        }
                    }
                }

                FragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, frag).Commit();
            };

            // Spinner con oficinas
            if (sel_oficina)
            {
                Android.Widget.ArrayAdapter<String> ad;
                ad = new Android.Widget.ArrayAdapter<String>(
                    this.Context, Android.Resource.Layout.SimpleSpinnerItem,
                    new string[] { Fragments.FragmentLogin.codOficina, "0534" }); // FIXME El valor de la oficina esta hardcoded
                spinner_oficina.Adapter = ad;
            }

            // Spinner con notificadores
            if (sel_notificador && es_supervisor)
            {
                try
                {
                    Helpers.SQLiteConeccion dbConeccion;
                    dbConeccion = new Helpers.SQLiteConeccion();
                    dbConeccion.setAdaptadorCombo("SELECT NombreCompleto FROM OficialesNotificadores", this.Activity, ref spinner_notificador);
                }
                catch (Exception e)
                {
                    //Se guarda el error en el log de errores
                    Logs.saveLogError("FragmentMap.cargarComboNotificador " + e.Message + " " + e.StackTrace);
                    //Se muestra un mensaje informando el error
                    Toast.MakeText(this.Context, GetString(Resource.String.MensajeErrorCargaBaseDatos), ToastLength.Long).Show();
                }
            }

            // Spinner con despachos
            if (sel_despacho)
            {
                Android.Widget.ArrayAdapter<String> adapter;
                adapter = new Android.Widget.ArrayAdapter<String>(
                    this.Context, Android.Resource.Layout.SimpleSpinnerItem,
                    new string[] { Fragments.FragmentLogin.codOficina, "0163" }); // FIXME El valor del despacho esta hardcoded
                spinner_despacho.Adapter = adapter;
            }

            // Fecha inicio
            if (sel_fecha_inicio)
            {
                DatePickerDialogFragment fecha_inicio_picker = new DatePickerDialogFragment(Activity, DateTime.Now, this, "inicio");
                fecha_inicio.Click += (sender, args) =>
                {
                    fecha_inicio_picker.Show(FragmentManager, null);
                };
            }

            // Fecha fin
            if (sel_fecha_fin)
            {
                DatePickerDialogFragment fecha_fin_picker = new DatePickerDialogFragment(Activity, DateTime.Now, this, "fin");
                fecha_fin.Click += (sender, args) =>
                {
                    fecha_fin_picker.Show(FragmentManager, null);
                };
            }

            // Fecha jornada
            if (sel_fecha_jornada)
            {
                DatePickerDialogFragment fecha_jornada_picker = new DatePickerDialogFragment(Activity, DateTime.Now, this, "jornada");
                fecha_jornada.Click += (sender, args) =>
                {
                    fecha_jornada_picker.Show(FragmentManager, null);
                };
            }

            return self;
        }

        /* Necesario para la selección de las fechas desde el DatePickerDialog */
        public void OnDateSet(DatePicker picker, int year, int monthOfYear, int dayOfMonth)
        {
            var date = new DateTime(year, monthOfYear + 1, dayOfMonth);
            string tag = (string)(picker.Tag);

            switch (tag)
            {
                case "inicio":
                    View.FindViewById<TextView>(Resource.Id.textViewInputFechaInicio).Text = date.ToString("yyyy-MMM-dd");
                    report.input_fecha_inicio = date;
                    break;
                case "fin":
                    View.FindViewById<TextView>(Resource.Id.textViewInputFechaFin).Text = date.ToString("yyyy-MMM-dd");
                    report.input_fecha_fin = date;
                    break;
                case "jornada":
                    View.FindViewById<TextView>(Resource.Id.textViewInputFechaJornada).Text = date.ToString("yyyy-MMM-dd");
                    report.input_fecha_jornada = date;
                    break;
                default:
                    Console.WriteLine("Elemento seleccionado '" + tag + "'");
                    break;
            }
        }
    }
}