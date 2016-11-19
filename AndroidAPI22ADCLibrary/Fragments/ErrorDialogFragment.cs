using Android.App;
using Android.OS;

namespace AndroidAPI22ADCLibrary.Fragments
{
   

    public class ErrorDialogFragment : DialogFragment
    {
        public ErrorDialogFragment(Dialog dialog)
        {
            Dialog = dialog;
        }

        public new Dialog Dialog { get; private set; }


        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            return Dialog;
        }
    }
}