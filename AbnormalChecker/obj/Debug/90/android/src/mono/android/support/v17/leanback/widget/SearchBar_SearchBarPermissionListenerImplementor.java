package mono.android.support.v17.leanback.widget;


public class SearchBar_SearchBarPermissionListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.SearchBar.SearchBarPermissionListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_requestAudioPermission:()V:GetRequestAudioPermissionHandler:Android.Support.V17.Leanback.Widget.SearchBar/ISearchBarPermissionListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.SearchBar+ISearchBarPermissionListenerImplementor, Xamarin.Android.Support.v17.Leanback", SearchBar_SearchBarPermissionListenerImplementor.class, __md_methods);
	}


	public SearchBar_SearchBarPermissionListenerImplementor ()
	{
		super ();
		if (getClass () == SearchBar_SearchBarPermissionListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.SearchBar+ISearchBarPermissionListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public void requestAudioPermission ()
	{
		n_requestAudioPermission ();
	}

	private native void n_requestAudioPermission ();

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
