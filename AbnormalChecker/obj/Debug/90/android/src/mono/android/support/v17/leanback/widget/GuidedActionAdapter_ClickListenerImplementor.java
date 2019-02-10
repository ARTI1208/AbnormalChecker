package mono.android.support.v17.leanback.widget;


public class GuidedActionAdapter_ClickListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.GuidedActionAdapter.ClickListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onGuidedActionClicked:(Landroid/support/v17/leanback/widget/GuidedAction;)V:GetOnGuidedActionClicked_Landroid_support_v17_leanback_widget_GuidedAction_Handler:Android.Support.V17.Leanback.Widget.GuidedActionAdapter/IClickListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.GuidedActionAdapter+IClickListenerImplementor, Xamarin.Android.Support.v17.Leanback", GuidedActionAdapter_ClickListenerImplementor.class, __md_methods);
	}


	public GuidedActionAdapter_ClickListenerImplementor ()
	{
		super ();
		if (getClass () == GuidedActionAdapter_ClickListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.GuidedActionAdapter+IClickListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public void onGuidedActionClicked (android.support.v17.leanback.widget.GuidedAction p0)
	{
		n_onGuidedActionClicked (p0);
	}

	private native void n_onGuidedActionClicked (android.support.v17.leanback.widget.GuidedAction p0);

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
