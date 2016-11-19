using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AndroidAPI22ADCLibrary.Fragments
{
    public class DatePickerDialogFragment : Android.Support.V4.App.DialogFragment
    {
        private readonly Context _context;
        private DateTime _date;
        private readonly Android.App.DatePickerDialog.IOnDateSetListener _listener;
        string _tag;

        public DatePickerDialogFragment(Context context, DateTime date, Android.App.DatePickerDialog.IOnDateSetListener listener, string tag)
        {
            _context = context;
            _date = date;
            _listener = listener;
            _tag = tag;
        }

        public override Dialog OnCreateDialog(Bundle savedState)
        {
            var dialog = new Android.App.DatePickerDialog(_context, _listener, _date.Year, _date.Month - 1, _date.Day);
            dialog.DatePicker.Tag = _tag;
            return dialog;
        }

    }
}