using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using AndroidAPI22ADCLibrary.Reports;
using FragmentManager = Android.Support.V4.App.FragmentManager;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class Fragment1 : Fragment
    {
        Report[] reports;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public static Fragment1 NewInstance()
        {
            var frag1 = new Fragment1 { Arguments = new Bundle() };
            return frag1;
        }


        public override View OnCreateView(LayoutInflater inflater, 
                                          ViewGroup container, 
                                          Bundle savedInstanceState)
        {
            var ignored = base.OnCreateView(inflater, container, savedInstanceState);
            View self = inflater.Inflate(Resource.Layout.fragment1, null);

            ListView report_sel = self.FindViewById<ListView>(Resource.Id.report_sel_listview);

            // Array con los reportes. Simplemente debe de crearse una instancia
            // de cada reporde dentro de este array.
            // La logica del frame y la interna de cada reporte sera la que actualize
            // el frame segun corresponda
            Report[] reports = new Report[] {
                new NotificacionesPorNotificadorReport(),
                new NotificacionesEnviadasReport(),
                new NotificacionesDevueltas(),
                new NotificacionesRealizadasConDuracion(),
                new NotificacionesRealizadasSegunResultado(),
                new NotificacionesAsignadasPendientesPorJornada(),
            };

            report_sel.Adapter = new ArrayAdapter(
                report_sel.Context, 
                Android.Resource.Layout.SimpleListItem1, 
                reports);

            report_sel.ItemClick += (sender, e) => {
                Fragment frag = reports[e.Position].getInputReportFragment();
                FragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, frag).Commit();
            };

            return self;
        }
    }
}