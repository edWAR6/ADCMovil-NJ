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
using System.Collections.Generic;

namespace AndroidAPI22ADCLibrary.Reports
{
    public class NotificacionesEnviadasOutputReport : Fragment
    {
        NotificacionesEnviadasReport report;

        public NotificacionesEnviadasOutputReport(NotificacionesEnviadasReport report)
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

            string query = @"https://pjgestionnotificacionmovilservicios.azurewebsites.net/api/Reportes/NotificacionesEnviadasADespachos" +
                "?PCodOficina=" + report.input_oficina + 
                "&PCodDespacho=" + // FIXME
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
                        if ((jsonParsed.Count % 2 )== 1) // Esperamos un número par de elementos del array
                        {
                            reportErrorGoBack("La información obtenida para le informe no puede ser procesada");
                            return self;
                        }
                        Dictionary<string, int> despacho_deligencia_true = new Dictionary<string, int>();
                        Dictionary<string, int> despacho_deligencia_false = new Dictionary<string, int>();
                        Dictionary<string, string> despacho_nombre        = new Dictionary<string, string>();
                        for (int i = 0; i < jsonParsed.Count; ++i)
                        {
                            string codigo = jsonParsed[i].Value<string>("Codigo");
                            despacho_nombre[codigo] = jsonParsed[i].Value<string>("Descripcion");

                            if (jsonParsed[i].Value<Boolean>("Diligenciada"))
                                despacho_deligencia_true[codigo] = jsonParsed[i].Value<int>("Total");
                            else
                                despacho_deligencia_false[codigo] = jsonParsed[i].Value<int>("Total");
                        }

                        // Por cada despacho encontrada
                        TableLayout.LayoutParams layoutParams = new TableLayout.LayoutParams(TableLayout.LayoutParams.MatchParent, TableLayout.LayoutParams.WrapContent);

                        foreach (string codigo in despacho_nombre.Keys )
                        {
                            // Cantidad
                            int total_cantidad = despacho_deligencia_true[codigo] + despacho_deligencia_false[codigo];

                            // Crea las columnas
                            TextView nombre_label = new TextView(Activity);
                            nombre_label.Text = "Nombre del despacho";
                            TextView text_nombre = new TextView(Activity);
                            text_nombre.Text = despacho_nombre[codigo];
                            // FIXME. Buscar una mejor manera de acomodar los nombres largos
                            if (despacho_nombre[codigo].Length > 20) text_nombre.Text = despacho_nombre[codigo].Substring(0,20);
                            TextView pad = new TextView(Activity);
                            pad.Text = "";
                            TextView pad2 = new TextView(Activity);
                            pad.Text = "";
                            TextView positivas = new TextView(Activity);
                            positivas.Text = "Positivas";
                            TextView negativas = new TextView(Activity);
                            negativas.Text = "Negativas";
                            TextView total = new TextView(Activity);
                            total.Text = "Total";
                            TextView cantidad = new TextView(Activity);
                            cantidad.Text = "Cantidad";
                            TextView porcentaje = new TextView(Activity);
                            porcentaje.Text = "Porcentaje";
                            TextView text_positivas_cantidad = new TextView(Activity);
                            text_positivas_cantidad.Text = despacho_deligencia_true[codigo].ToString();
                            TextView text_negativas_cantidad = new TextView(Activity);
                            text_negativas_cantidad.Text = despacho_deligencia_false[codigo].ToString();
                            TextView text_positivas_porcentaje = new TextView(Activity);
                            text_positivas_porcentaje.Text = (100.0 * despacho_deligencia_true[codigo] / total_cantidad).ToString("N3") + '%';
                            TextView text_negativas_porcentaje = new TextView(Activity);
                            text_negativas_porcentaje.Text = (100.0 * despacho_deligencia_false[codigo] / total_cantidad).ToString("N3") + '%';
                            TextView text_total_cantidad = new TextView(Activity);
                            text_total_cantidad.Text = total_cantidad.ToString();

                            // La Fila Nombre
                            TableRow filaNombre = new TableRow(Activity); //Se instancia la nueva fila
                            filaNombre.LayoutParameters = layoutParams; //Se agregan los tamaños del layout
                            filaNombre.AddView(nombre_label);
                            filaNombre.AddView(text_nombre);
                            filaNombre.SetPadding(15, 15, 15, 15);
                            filaNombre.SetBackgroundColor(Android.Graphics.Color.Argb(50, 1, 0, 5));

                            // La file de nombre de columnas
                            TableRow filaNombreColumnas = new TableRow(Activity); //Se instancia la nueva fila
                            filaNombreColumnas.LayoutParameters = layoutParams; //Se agregan los tamaños del layout
                            filaNombreColumnas.AddView(pad2);
                            filaNombreColumnas.AddView(cantidad);
                            filaNombreColumnas.AddView(porcentaje);
                            filaNombreColumnas.SetPadding(15, 15, 15, 15);
                            filaNombreColumnas.SetBackgroundColor(Android.Graphics.Color.Argb(30, 1, 0, 5));

                            // La Fila positivas
                            TableRow filaPositivas = new TableRow(Activity); //Se instancia la nueva fila
                            filaPositivas.LayoutParameters = layoutParams; //Se agregan los tamaños del layout
                            filaPositivas.AddView(positivas);
                            filaPositivas.AddView(text_positivas_cantidad);
                            filaPositivas.AddView(text_positivas_porcentaje);
                            filaPositivas.SetPadding(15, 15, 15, 15);

                            // La Fila negativas
                            TableRow filaNegativa = new TableRow(Activity); //Se instancia la nueva fila
                            filaNegativa.LayoutParameters = layoutParams; //Se agregan los tamaños del layout
                            filaNegativa.AddView(negativas);
                            filaNegativa.AddView(text_negativas_cantidad);
                            filaNegativa.AddView(text_negativas_porcentaje);
                            filaNegativa.SetPadding(15, 15, 15, 15);

                            // La Fila total
                            TableRow filatotal = new TableRow(Activity); //Se instancia la nueva fila
                            filatotal.LayoutParameters = layoutParams; //Se agregan los tamaños del layout
                            filatotal.AddView(total);
                            filatotal.AddView(text_total_cantidad);
                            filatotal.AddView(pad);
                            filatotal.SetPadding(15, 15, 15, 15);

                            // Agregar a la tabla
                            table.AddView(filaNombre);
                            table.AddView(filaNombreColumnas);
                            table.AddView(filaPositivas);
                            table.AddView(filaNegativa);
                            table.AddView(filatotal);
                        }
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