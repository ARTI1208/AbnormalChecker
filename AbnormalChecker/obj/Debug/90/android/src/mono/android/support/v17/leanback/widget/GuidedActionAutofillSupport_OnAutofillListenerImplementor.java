package mono.android.support.v17.leanback.widget;


public class GuidedActionAutofillSupport_OnAutofillListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.GuidedActionAutofillSupport.OnAutofillListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onAutofill:(Landroid/view/View;)V:GetOnAutofill_Landroid_view_View_Handler:Android.Support.V17.Leanback.Widget.IGuidedActionAutofillSupportOnAutofillListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.IGuidedActionAutofillSupportOnAutofillListenerImplementor, Xamarin.Android.Support.v17.Leanback", GuidedActionAutofillSupport_OnAutofillListenerImplementor.class, __md_methods);
	}


	public GuidedActionAutofillSupport_OnAutofillListenerImplementor ()
	{
		super ();
		if (getClass () == GuidedActionAutofillSupport_OnAutofillListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.IGuidedActionAutofillSupportOnAutofillListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public void onAutofill (android.view.View p0)
	{
		n_onAutofill (p0);
	}

	private native void n_onAutofill (android.view.View p0);

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
