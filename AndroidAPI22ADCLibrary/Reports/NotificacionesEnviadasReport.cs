using AndroidAPI22ADCLibrary.Fragments;
using Android.Support.V4.App;
using System;

namespace AndroidAPI22ADCLibrary.Reports
{
    public class NotificacionesEnviadasReport : Reports.Report
    {
        // Contructor, lo único que debe de hacer es costruir la parte base
        // del reporte con el nombre específico y unico de este reporte
        public NotificacionesEnviadasReport() : base("Notificaciones enviadas a Despachos")
        {
            input_oficina = null;
            input_despacho = null;
            input_fecha_inicio = DateTime.Now;
            input_fecha_fin = DateTime.Now;
            output_error = null;
        }

        public override Fragment getInputReportFragment()
        {
            // Crea el reporte específico, pasa la misma instancia del reporte
            // al segmento para que el mismo puede obtener/retornar de manera simple
            // los parametros
            return new CommonInputReport(this, false, false, false, false, true, true);
        }

        public override Fragment getOutputReportFragment()
        {
            // Crea el reporte específico, pasa la misma instancia del reporte
            // al segmento para que el mismo puede obtener/retornar de manera simple
            // los parametros
            return new NotificacionesEnviadasOutputReport(this);
        }
    }
}