package md5b43e60ce216ac851cd1ebd1beb1f36b7;


public class ServiceDialog
	extends android.app.DialogFragment
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreateDialog:(Landroid/os/Bundle;)Landroid/app/Dialog;:GetOnCreateDialog_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("AndroidAPI22ADCLibrary.Fragments.ServiceDialog, AndroidAPI22ADCLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ServiceDialog.class, __md_methods);
	}


	public ServiceDialog () throws java.lang.Throwable
	{
		super ();
		if (getClass () == ServiceDialog.class)
			mono.android.TypeManager.Activate ("AndroidAPI22ADCLibrary.Fragments.ServiceDialog, AndroidAPI22ADCLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public android.app.Dialog onCreateDialog (android.os.Bundle p0)
	{
		return n_onCreateDialog (p0);
	}

	private native android.app.Dialog n_onCreateDialog (android.os.Bundle p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
