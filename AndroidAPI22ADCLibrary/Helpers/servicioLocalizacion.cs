using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Android.Util;
using System.Threading;

namespace AndroidAPI22ADCLibrary.Helpers
{
    [Service]
    public class servicioLocalizacion : Service
    {
        private static int tiempoReLocalizacion = 10000;
        public LocationManager locationManager;
        public MyLocationListener listener;
        public Location mejorlocalizacionAnterior = null;

        public static double latitud = 0;
        public static double longitud = 0;
        public static DateTime fecha;
        public static TimeSpan hora;

        public override void OnCreate()
        {
            base.OnCreate();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            locationManager = (LocationManager)GetSystemService(Context.LocationService);
            listener = new MyLocationListener();

            Criteria criteria = new Criteria();
            criteria.Accuracy = Accuracy.Fine;
            String provider = locationManager.GetBestProvider(criteria, true);

            locationManager.RequestLocationUpdates(provider, 4000 , 0 , listener);

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        protected bool isBetterLocation(Location location, Location currentBestLocation)
        {
            if (currentBestLocation == null)
            {
                return true;
            }

            // Se verifica que la nueva localización no tenga un tiempo menor al que ya se tenia
            long timeDelta = location.Time - currentBestLocation.Time;
            bool isSignificantlyNewer = timeDelta > tiempoReLocalizacion;
            bool isSignificantlyOlder = timeDelta < -tiempoReLocalizacion;
            bool isNewer = timeDelta > 0;

            //Si han pasado mas de dos minutos desde la ultima localizaciónn, se agrega la nueva localización
            //porque el usuario se esta moviendo
            if (isSignificantlyNewer)
            {
                return true;
            }
            else if (isSignificantlyOlder)
            {
                return false;
            }

            //Se verifica si la localización es más precisa
            int accuracyDelta = (int)(location.Accuracy - currentBestLocation.Accuracy);
            bool isLessAccurate = accuracyDelta > 0;
            bool isMoreAccurate = accuracyDelta < 0;
            bool isSignificantlyLessAccurate = accuracyDelta > 200;

            bool isFromSameProvider = isSameProvider(location.Provider,
                    currentBestLocation.Provider);

            if (isMoreAccurate)
            {
                return true;
            }
            else if (isNewer && !isLessAccurate)
            {
                return true;
            }
            else if (isNewer && !isSignificantlyLessAccurate && isFromSameProvider)
            {
                return true;
            }
            return false;

        }

        private bool isSameProvider(String provider1, String provider2)
        {
            if (provider1 == null)
            {
                return provider2 == null;
            }
            return provider1.Equals(provider2);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Verbose("STOP_SERVICE", "DONE");
            locationManager.RemoveUpdates(listener);
        }

        public static Thread performOnBackgroundThread(Java.Lang.Runnable runnable)
        {
            Thread t = new Thread(new ThreadStart(delegate
            {
                runnable.Run();
            }));

            t.Start();

            return t;
        }

        public class MyLocationListener : servicioLocalizacion, ILocationListener
        {
            /// <summary>
            /// Evento que se dispara cuando cambia la localización
            /// </summary>
            /// <param name="location"></param>
            public void OnLocationChanged(Location location)
            {
                //Se valida que la localización anterior sea mejor a la nueva
                if (isBetterLocation(location, mejorlocalizacionAnterior))
                {
                    latitud = location.Latitude;//Se obtiene la latitud
                    longitud = location.Longitude;//Se obtiene la longitud

                    //Console.WriteLine("Servicio de localizacion Coordenadas latitud: "+latitud.ToString()+ " longitud: "+longitud.ToString());
                    long tiempo = location.Time;
                    DateTime fechahora = new DateTime(tiempo);

                    fecha = fechahora.Date;//La fecha no se obtiene
                    //Se obtiene la hora
                    hora = new TimeSpan(fechahora.Hour, fechahora.Minute, fechahora.Second);

                }
            }

            public void OnProviderDisabled(string provider)
            {
            }

            public void OnProviderEnabled(string provider)
            {
            }

            public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
            {
            }
        }

    }
}
