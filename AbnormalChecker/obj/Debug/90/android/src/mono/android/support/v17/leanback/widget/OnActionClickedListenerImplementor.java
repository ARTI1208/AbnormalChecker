package mono.android.support.v17.leanback.widget;


public class OnActionClickedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.OnActionClickedListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onActionClicked:(Landroid/support/v17/leanback/widget/Action;)V:GetOnActionClicked_Landroid_support_v17_leanback_widget_Action_Handler:Android.Support.V17.Leanback.Widget.IOnActionClickedListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.IOnActionClickedListenerImplementor, Xamarin.Android.Support.v17.Leanback", OnActionClickedListenerImplementor.class, __md_methods);
	}


	public OnActionClickedListenerImplementor ()
	{
		super ();
		if (getClass () == OnActionClickedListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.IOnActionClickedListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public void onActionClicked (android.support.v17.leanback.widget.Action p0)
	{
		n_onActionClicked (p0);
	}

	private native void n_onActionClicked (android.support.v17.leanback.widget.Action p0);

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
