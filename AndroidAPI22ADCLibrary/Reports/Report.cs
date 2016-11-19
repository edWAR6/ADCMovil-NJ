using Android.Support.V4.App;
using Android.Widget;
using Android.Views;
using System;

namespace AndroidAPI22ADCLibrary.Reports
{
    /* Abstraci�n gen�rica de un reporte.
     * Cada reporte debe de incluir/implementar:
     * 1- Un nombre �nico para ser mostrado en el list view
     * 2- Implemnetar un m�todo que retorne el fragmento que le permite al usuario ingresar los datos de
     *    las consulta
     * 3- Implmenetar un m�todos que procesa la informaci�n del fr�gmento anterior, y retorne un nuevo
     *    fragmento con el reporte requerido
     *    
     * Cada reporte espec�fico tiene la l�gica para recibir la informaci�n y generar la tabla de salida
     * Cuando se construyan los fragmentos de entrada/salida de los reportes (En la implementaci�n especifica
     * de getInputReportFragment y getOutputReportFragment) se pasara como contructor la instancia 
     * espec�fica del reporte. Con est� sera mucho m�s simple la comunicaci�n entre el input y el output
     * fragment de cada reporte
     */
    public abstract class Report
    {
        string reportName;

        /* Parametros de entrada para los reportes */
        public string loggin_usuario;
        public string loggin_officina;
        public bool loggin_supervisor;

        public string input_oficina;
        public string input_notificador;
        public string input_despacho;
        public DateTime input_fecha_inicio;
        public DateTime input_fecha_fin;
        public DateTime input_fecha_jornada;
        public string output_error;

        public Report(string name) {
            this.reportName = name;
            input_fecha_inicio = DateTime.Now;
            input_fecha_fin = DateTime.Now;
            input_fecha_jornada = DateTime.Now;
        }

        override
        public string ToString() {
            return reportName;
        }

        /* Obtiene la informacion general que toda consulta necesita y la carga en common output view pasado
         como parametro */
        public bool getFillCommonInfo(View self, bool rango_de_fechas, bool fecha_de_jornada, ref string error) {
            // Campos en la tabla que se van a actualizar
            TextView textView_reporte = self.FindViewById<TextView>(Resource.Id.textViewNombreDelReporte);
            TextView textView_user = self.FindViewById<TextView>(Resource.Id.textViewUsuario);
            TextView textView_office = self.FindViewById<TextView>(Resource.Id.textViewOficina);
            TextView textView_fecha_hora = self.FindViewById<TextView>(Resource.Id.textViewFechaHora);
            TextView textView_fecha_rango = self.FindViewById<TextView>(Resource.Id.textViewRangoFechas);
            TextView textView_fecha_label = self.FindViewById<TextView>(Resource.Id.textViewFechasLabel);

            textView_reporte.Text = reportName;

            try
            {
                Helpers.SQLiteConeccion dbConeccion;
                dbConeccion = new Helpers.SQLiteConeccion();
                dbConeccion.consultaDatos("SELECT CodigoNotificador FROM OficialNotificador", self.Context, ref loggin_usuario);

                dbConeccion = new Helpers.SQLiteConeccion();
                dbConeccion.consultaDatos("SELECT DespachoCodigo FROM OficialNotificador", self.Context, ref loggin_officina);

                string supervisor = "";
                dbConeccion = new Helpers.SQLiteConeccion();
                dbConeccion.consultaDatos("SELECT Supervisor FROM OficialNotificador", self.Context, ref supervisor);
                loggin_supervisor = String.Equals("True", supervisor);
            }
            catch (Exception)
            {
                error = "No fue posible obtener la informaci�n requerida para le reporte (ID)";
                return false;
            }

            textView_user.Text = loggin_usuario;
            textView_office.Text = loggin_officina;
            textView_fecha_hora.Text = DateTime.Now.ToString("yyyy-MMM-dd hh:mm");
            if (fecha_de_jornada)
            {
                textView_fecha_label.Text = "Fecha jornada";
                textView_fecha_rango.Text = input_fecha_jornada.ToString("yyyy-MMM-dd");
            }
            else if (rango_de_fechas)
            {
                textView_fecha_label.Text = "Rango de fechas";
                textView_fecha_rango.Text = "De " + input_fecha_inicio.ToString("yyyy-MMM-dd");
                textView_fecha_rango.Text += "\na " + input_fecha_fin.ToString("yyyy-MMM-dd");
            }
            else {
                textView_fecha_label.Text = "Fecha";
                textView_fecha_rango.Text = input_fecha_inicio.ToString("yyyy-MMM-dd");
            }

            // FIXME, Se permite generar informe de m�s de 3 meses para encontrar datos validos facilmente
            if (!ReportUtils.validarFechas(input_fecha_inicio, input_fecha_fin, false, ref error)) return false;
          
            return true;
        }

        /* Funciones virtual, las que todo reporte debe de implementar */
        public abstract Fragment getInputReportFragment();
        public abstract Fragment getOutputReportFragment();
    }
}