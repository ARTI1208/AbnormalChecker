package mono.android.support.v17.leanback.widget;


public class ImeKeyMonitor_ImeKeyListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.ImeKeyMonitor.ImeKeyListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onKeyPreIme:(Landroid/widget/EditText;ILandroid/view/KeyEvent;)Z:GetOnKeyPreIme_Landroid_widget_EditText_ILandroid_view_KeyEvent_Handler:Android.Support.V17.Leanback.Widget.IImeKeyMonitorImeKeyListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.IImeKeyMonitorImeKeyListenerImplementor, Xamarin.Android.Support.v17.Leanback", ImeKeyMonitor_ImeKeyListenerImplementor.class, __md_methods);
	}


	public ImeKeyMonitor_ImeKeyListenerImplementor ()
	{
		super ();
		if (getClass () == ImeKeyMonitor_ImeKeyListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.IImeKeyMonitorImeKeyListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public boolean onKeyPreIme (android.widget.EditText p0, int p1, android.view.KeyEvent p2)
	{
		return n_onKeyPreIme (p0, p1, p2);
	}

	private native boolean n_onKeyPreIme (android.widget.EditText p0, int p1, android.view.KeyEvent p2);

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
