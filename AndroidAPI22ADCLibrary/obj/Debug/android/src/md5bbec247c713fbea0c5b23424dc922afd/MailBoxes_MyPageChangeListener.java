package md5bbec247c713fbea0c5b23424dc922afd;


public class MailBoxes_MyPageChangeListener
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v4.view.ViewPager.OnPageChangeListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onPageScrollStateChanged:(I)V:GetOnPageScrollStateChanged_IHandler:Android.Support.V4.View.ViewPager/IOnPageChangeListenerInvoker, Xamarin.Android.Support.v4\n" +
			"n_onPageScrolled:(IFI)V:GetOnPageScrolled_IFIHandler:Android.Support.V4.View.ViewPager/IOnPageChangeListenerInvoker, Xamarin.Android.Support.v4\n" +
			"n_onPageSelected:(I)V:GetOnPageSelected_IHandler:Android.Support.V4.View.ViewPager/IOnPageChangeListenerInvoker, Xamarin.Android.Support.v4\n" +
			"";
		mono.android.Runtime.register ("AndroidAPI22ADCLibrary.Activities.MailBoxes+MyPageChangeListener, AndroidAPI22ADCLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", MailBoxes_MyPageChangeListener.class, __md_methods);
	}


	public MailBoxes_MyPageChangeListener () throws java.lang.Throwable
	{
		super ();
		if (getClass () == MailBoxes_MyPageChangeListener.class)
			mono.android.TypeManager.Activate ("AndroidAPI22ADCLibrary.Activities.MailBoxes+MyPageChangeListener, AndroidAPI22ADCLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public MailBoxes_MyPageChangeListener (android.content.Context p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == MailBoxes_MyPageChangeListener.class)
			mono.android.TypeManager.Activate ("AndroidAPI22ADCLibrary.Activities.MailBoxes+MyPageChangeListener, AndroidAPI22ADCLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.Content.Context, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065", this, new java.lang.Object[] { p0 });
	}


	public void onPageScrollStateChanged (int p0)
	{
		n_onPageScrollStateChanged (p0);
	}

	private native void n_onPageScrollStateChanged (int p0);


	public void onPageScrolled (int p0, float p1, int p2)
	{
		n_onPageScrolled (p0, p1, p2);
	}

	private native void n_onPageScrolled (int p0, float p1, int p2);


	public void onPageSelected (int p0)
	{
		n_onPageSelected (p0);
	}

	private native void n_onPageSelected (int p0);

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
