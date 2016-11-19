using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace AndroidAPI22ADCLibrary.Reports
{
    static class ReportUtils
    {
        /* 
         * Verifica que las fechas son valida
         * en caso de error, el mensaje de error es almacenado en el string error
         * Si verificar3meses es verdadero y las fechas definen un rango mayor a tres meses se reporta un error
         */
        public static bool validarFechas(DateTime inicio, DateTime fin, bool verificar3meses, ref string error) {
            if (null == inicio && null == fin)
            {
                error = "Las fechas de inicio y fin no deben de estar vacías";
                return false;
            }
            if (null == inicio)
            {
                error = "La fecha de inicio no debe de estar vacía";
                return false;
            }
            if (null == fin) {
                error = "La fecha de fin no debe de estar vacía";
                return false;
            }

            if (DateTime.Compare(inicio, fin) > 0) {
                error = "La fecha de inicio no debe de ser mayor que la fecha de fin";
                return false;
            }

            if (verificar3meses) {
                int total_mes_inicio = inicio.Year * 12 + inicio.Month;
                int total_mes_fin = fin.Year * 12 + fin.Month;
                int diff_meses = total_mes_fin - total_mes_inicio;
                int diff_dias = fin.Day - inicio.Day;

                if (diff_meses > 3 || (diff_meses == 3 && diff_dias > 0))
                {
                    error = "El rango de fechas no puede ser mayor de 3 meses";
                    return false;
                }
            }

            return true;
        }

        /* 
         * Agrega una fila con columnas en formato string a una tabla
         * Retorna el objecto fila que se agregó en la tabla
         */
        public static TableRow agregarFilaATabla(string[] columnas, FragmentActivity activity, TableLayout table)
        {
            TableRow fila = new TableRow(activity);
            foreach (string text_celda in columnas)
            {
                TextView celda = new TextView(activity);
                celda.Text = text_celda;
                fila.AddView(celda);
            }
            fila.SetPadding(15, 15, 15, 15); // Valor por default
            table.AddView(fila);

            TableLayout.LayoutParams layoutParams = new TableLayout.LayoutParams(TableLayout.LayoutParams.MatchParent, TableLayout.LayoutParams.WrapContent);
            fila.LayoutParameters = layoutParams;

            return fila;
        }

        public static void alertNoInternetMessage(Context contex) {
            AlertDialog.Builder alerta = new AlertDialog.Builder(contex);
            alerta.SetTitle("Mensaje de alerta");
            alerta.SetIcon(Resource.Drawable.alertaNuevo);
            alerta.SetMessage("El servicio de Internet no se encuentra disponible, por favor revise su conexión e intente ingresar nuevamente");
            alerta.SetNegativeButton("Salir", HandleButtonClick);
            alerta.SetCancelable(false);
            alerta.Create();
            alerta.Show();
        }

        private static void HandleButtonClick(object sender, DialogClickEventArgs e)
        {
            Java.Lang.JavaSystem.Exit(0);
        }
    }
}