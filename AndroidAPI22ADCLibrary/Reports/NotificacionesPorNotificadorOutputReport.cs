using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Content;
using System.Net;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace AndroidAPI22ADCLibrary.Reports
{
    public class NotificacionesPorNotificadorOutputReport : Fragment
    {
        NotificacionesPorNotificadorReport report;

        public NotificacionesPorNotificadorOutputReport(NotificacionesPorNotificadorReport report)
        {
            this.report = report;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            report.output_error = null;

            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            View self = inflater.Inflate(Resource.Layout.CommonReportOutput, null);

            if (!report.getFillCommonInfo(self, true, false, ref report.output_error))
            {
                reportErrorGoBack(report.output_error);
                return self;
            }

            TableLayout table = self.FindViewById<TableLayout>(Resource.Id.tbReporte);

            // Asegura que ningun parametro este vacío
            if (String.IsNullOrEmpty(report.input_oficina))
            {
                reportErrorGoBack("El campo de oficina no puede ser vacío");
                return self;
            }

            // Consulta
            // Cambiar las fechas al formato admitido por la BD
            string fecha_inicio = report.input_fecha_inicio.ToString("yyyyMMdd");
            string fecha_fin = report.input_fecha_fin.ToString("yyyyMMdd");

            string codigoNotificador = "";
            if(report.loggin_supervisor) try
            {
                Helpers.SQLiteConeccion dbConeccion;
                dbConeccion = new Helpers.SQLiteConeccion();
                dbConeccion.consultaDatos(
                    "SELECT CodigoNotificador FROM OficialesNotificadores WHERE NombreCompleto = '" + report.input_notificador + "'",
                    this.Context, ref codigoNotificador);
            }
            catch (Exception)
            {
                reportErrorGoBack("No fue posible obtener la información requerida para le reporte (ID)");
                return self;
            }

            string query = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Reportes/NotificacionesCompletadasPorNotificador" +
                "?PCodOficina=" + report.input_oficina +
                "&PCodNotificador=" + (report.loggin_supervisor ? codigoNotificador : report.loggin_usuario) +
                "&PFecha1=" + fecha_inicio +
                "&PFecha2=" + fecha_fin;

            // Verificar si la conección a internet esta disponible
            if (!coneccionInternet.verificaConeccion(this.Context))
                ReportUtils.alertNoInternetMessage(this.Context);

            WebRequest request = HttpWebRequest.Create(query);
            request.ContentType = "application/json";
            request.Method = "GET";
            string content = "";

            Console.Out.WriteLine("--XDEBUG " + query);
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);
                    reportErrorGoBack("No fue posible obtener la información requerida para le reporte");
                    return self;
                }

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    content = reader.ReadToEnd();

                    if (string.IsNullOrWhiteSpace(content))
                    {
                        Console.Out.WriteLine("fzeledon -- Response contained empty body...");
                        reportErrorGoBack("Los valores de entrada de la consulta no generaron resultado");
                        return self;
                    }

                    try
                    {
                        var jsonParsed = JArray.Parse(content);
                        if (jsonParsed.Count != 2) // Esperamos 2 resultados unicamente
                        {
                            reportErrorGoBack("La información obtenida para le informe no puede ser procesada");
                            return self;
                        }

                        ReportUtils.agregarFilaATabla(new string[] { "Notificador", report.input_notificador, "" }, Activity, table);
                        ReportUtils.agregarFilaATabla(new string[] { "", "Cantidad", "Porcentaje" }, Activity, table);

                        int total_positivas = 0;
                        int total_negativas = 0;
                        int i = 0;
                        for (i = 0; i < jsonParsed.Count; ++i)
                        {
                            int total_diligencias = jsonParsed[i].Value<int>("Total");
                            bool diligenciada = jsonParsed[i].Value<bool>("Diligenciada");

                            if (diligenciada) total_positivas += total_diligencias;
                            else total_negativas += total_diligencias;
                        }
                        int gran_total = total_positivas + total_negativas;

                        ReportUtils.agregarFilaATabla(new string[] { "Positivas:", total_positivas.ToString(), (100.0 * total_positivas / gran_total).ToString("N3") + "%" }, Activity, table);
                        ReportUtils.agregarFilaATabla(new string[] { "Negativas", total_negativas.ToString(), (100.0 * total_negativas / gran_total).ToString("N3") + "%" }, Activity, table);
                        ReportUtils.agregarFilaATabla(new string[] { "Total:", (gran_total).ToString() }, Activity, table);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error descargando datos de usuario: " + ex.ToString());
                        reportErrorGoBack("Error descargando datos de usuario");
                        return self;
                    }
                }

            }

            Button btnGen = self.FindViewById<Button>(Resource.Id.btnRegresarNotificacionesEnviadas);
            btnGen.Click += (sender, e) => {
                report.output_error = null;
                Fragment frag = report.getInputReportFragment();
                FragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, frag).Commit();
            };

            return self;
        }

        /* Wrapper para retornar un error al fragmento de entrada */
        private void reportErrorGoBack(string error)
        {
            report.output_error = error;
            Fragment frag = report.getInputReportFragment();
            FragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, frag).Commit();
        }
    }
}