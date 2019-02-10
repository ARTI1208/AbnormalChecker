package mono.android.support.v17.leanback.widget;


public class SearchEditText_OnKeyboardDismissListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.SearchEditText.OnKeyboardDismissListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onKeyboardDismiss:()V:GetOnKeyboardDismissHandler:Android.Support.V17.Leanback.Widget.SearchEditText/IOnKeyboardDismissListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.SearchEditText+IOnKeyboardDismissListenerImplementor, Xamarin.Android.Support.v17.Leanback", SearchEditText_OnKeyboardDismissListenerImplementor.class, __md_methods);
	}


	public SearchEditText_OnKeyboardDismissListenerImplementor ()
	{
		super ();
		if (getClass () == SearchEditText_OnKeyboardDismissListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.SearchEditText+IOnKeyboardDismissListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public void onKeyboardDismiss ()
	{
		n_onKeyboardDismiss ();
	}

	private native void n_onKeyboardDismiss ();

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
