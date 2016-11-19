using System;
using System.Collections.Generic;
using Android.Database.Sqlite;
using System.IO;
using Android.Database;
using Android.App;
using Android.Content.Res;

namespace AndroidAPI22ADCLibrary.Helpers
{
    public static class ManejoBaseDatos
    { 
        private static string dbName = "PJNotificaciones.db";
        private static string dbPath = Path.Combine(global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, dbName);


        private static SQLiteDatabase db;

        public static bool DataBaseOpen()
        {
            if (db.IsOpen)
                return true;
            else return false;
        }


        public static string Abrir()
        {

            string abrirError = "";
            try
            {
                db = SQLiteDatabase.OpenDatabase(dbPath, null, DatabaseOpenFlags.OpenReadwrite);
            }
            catch (Exception ex1) { abrirError = ex1.ToString(); }
            return abrirError;
        }
        public static void  Cerrar()
        {
            try
            {
                db.Close();
            }
            catch (Exception ex) { Console.WriteLine("ERROR CERRANDO BASE DE DATOS: "+ex.ToString()); }
        }

        
        public static string Insertar(string tabla, string columnas,string valores)
        {
            string resultadoQuery = "";    
            try
            {              
                db.ExecSQL(@"INSERT INTO " + tabla + " (" + columnas + ") VALUES(" + valores + ")");
            }
            catch (Exception exQuery){ resultadoQuery = exQuery.ToString(); }
            return resultadoQuery;
        }

        public static string InsertarMultiples(string query)
        {
            string resultadoQuery = "";
            try
            {
                db.ExecSQL(@""+query);
            }
            catch (Exception exQuery) { resultadoQuery = exQuery.ToString(); }
            return resultadoQuery;
        }




        public static string Actualizar(string tabla, string columna, string valor,string condicion)
        {
            string resultado = "";
            try
            {
                //@"UPDATE Notificaciones SET Estado='AsignadoParaNotificar' WHERE CodigoNotificacion=" + codigoNotificacion[k].ToString() + ""
                db.ExecSQL(@"UPDATE " + tabla + " SET  " + columna + "=" + valor + " WHERE "+condicion);
            }
            catch (Exception ex) { resultado = "Error ejecutando actualización en base de datos: "+ ex.ToString(); }
            return resultado;
        }

        public static string ActualizarMultiples(string query)
        {
            string resultado = "";
            try
            {
                //@"UPDATE Notificaciones SET Estado='AsignadoParaNotificar' WHERE CodigoNotificacion=" + codigoNotificacion[k].ToString() + ""
                db.ExecSQL(@""+query);
            }
            catch (Exception ex) { resultado = "Error ejecutando actualización en base de datos: " + ex.ToString(); }
            return resultado;
        }

        public static string BorrarTablas(List<string>tablas)
        {
            string resultadoBorrar = "";
            try
            {
                foreach (var item in tablas)
                {
                    db.ExecSQL("DELETE FROM " + item + ";");
                    db.ExecSQL("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='" + item + "';");
                }
            }
            catch (Exception exc1) { resultadoBorrar = exc1.ToString(); }
            return resultadoBorrar;
        }

        public static ICursor Seleccionar(string query)
        {
            //string resultado = "";
            ICursor newCursor;
            newCursor = db.RawQuery(query, null);
            return newCursor; 
        }


        public static void CrearDB()
        {

            if (!File.Exists(dbPath))
            {
                using (BinaryReader br = new BinaryReader(Application.Context.Assets.Open(dbName)))
                {
                    using (BinaryWriter bw = new BinaryWriter(new FileStream(dbPath, FileMode.Create)))
                    {
                        byte[] buffer = new byte[2048];
                        int len = 0;
                        while ((len = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, len);
                        }
                    }
                }
            }
        }



    }

    
}