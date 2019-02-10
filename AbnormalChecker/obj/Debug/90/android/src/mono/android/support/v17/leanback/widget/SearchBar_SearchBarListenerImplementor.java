package mono.android.support.v17.leanback.widget;


public class SearchBar_SearchBarListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.SearchBar.SearchBarListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onKeyboardDismiss:(Ljava/lang/String;)V:GetOnKeyboardDismiss_Ljava_lang_String_Handler:Android.Support.V17.Leanback.Widget.SearchBar/ISearchBarListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"n_onSearchQueryChange:(Ljava/lang/String;)V:GetOnSearchQueryChange_Ljava_lang_String_Handler:Android.Support.V17.Leanback.Widget.SearchBar/ISearchBarListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"n_onSearchQuerySubmit:(Ljava/lang/String;)V:GetOnSearchQuerySubmit_Ljava_lang_String_Handler:Android.Support.V17.Leanback.Widget.SearchBar/ISearchBarListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.SearchBar+ISearchBarListenerImplementor, Xamarin.Android.Support.v17.Leanback", SearchBar_SearchBarListenerImplementor.class, __md_methods);
	}


	public SearchBar_SearchBarListenerImplementor ()
	{
		super ();
		if (getClass () == SearchBar_SearchBarListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.SearchBar+ISearchBarListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public void onKeyboardDismiss (java.lang.String p0)
	{
		n_onKeyboardDismiss (p0);
	}

	private native void n_onKeyboardDismiss (java.lang.String p0);


	public void onSearchQueryChange (java.lang.String p0)
	{
		n_onSearchQueryChange (p0);
	}

	private native void n_onSearchQueryChange (java.lang.String p0);


	public void onSearchQuerySubmit (java.lang.String p0)
	{
		n_onSearchQuerySubmit (p0);
	}

	private native void n_onSearchQuerySubmit (java.lang.String p0);

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
