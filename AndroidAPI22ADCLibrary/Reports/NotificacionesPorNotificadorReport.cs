using Android.Support.V4.App;
using System;

namespace AndroidAPI22ADCLibrary.Reports
{
    public class NotificacionesPorNotificadorReport : Reports.Report
    {
        // Contructor, lo �nico que debe de hacer es costruir la parte base
        // del reporte con el nombre espec�fico y unico de este reporte
        public NotificacionesPorNotificadorReport() : base("Notificaciones por Notificador")
        {
            input_oficina = null;
            input_notificador = null;
            input_fecha_inicio = DateTime.Now;
            input_fecha_fin = DateTime.Now;
            output_error = null;
        }

        public override Fragment getInputReportFragment()
        {
            // Crea el reporte espec�fico, pasa la misma instancia del reporte
            // al segmento para que el mismo puede obtener/retornar de manera simple
            // los parametros
            return new CommonInputReport(this, false, true, false, false, true, true);
        }

        public override Fragment getOutputReportFragment()
        {
            // Crea el reporte espec�fico, pasa la misma instancia del reporte
            // al segmento para que el mismo puede obtener/retornar de manera simple
            // los parametros
            return new NotificacionesPorNotificadorOutputReport(this);
        }
    }
}