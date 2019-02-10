package mono.android.support.v17.leanback.widget;


public class BaseOnItemViewSelectedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.BaseOnItemViewSelectedListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onItemSelected:(Landroid/support/v17/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroid/support/v17/leanback/widget/RowPresenter$ViewHolder;Ljava/lang/Object;)V:GetOnItemSelected_Landroid_support_v17_leanback_widget_Presenter_ViewHolder_Ljava_lang_Object_Landroid_support_v17_leanback_widget_RowPresenter_ViewHolder_Ljava_lang_Object_Handler:Android.Support.V17.Leanback.Widget.IBaseOnItemViewSelectedListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.IBaseOnItemViewSelectedListenerImplementor, Xamarin.Android.Support.v17.Leanback", BaseOnItemViewSelectedListenerImplementor.class, __md_methods);
	}


	public BaseOnItemViewSelectedListenerImplementor ()
	{
		super ();
		if (getClass () == BaseOnItemViewSelectedListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.IBaseOnItemViewSelectedListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public void onItemSelected (android.support.v17.leanback.widget.Presenter.ViewHolder p0, java.lang.Object p1, android.support.v17.leanback.widget.RowPresenter.ViewHolder p2, java.lang.Object p3)
	{
		n_onItemSelected (p0, p1, p2, p3);
	}

	private native void n_onItemSelected (android.support.v17.leanback.widget.Presenter.ViewHolder p0, java.lang.Object p1, android.support.v17.leanback.widget.RowPresenter.ViewHolder p2, java.lang.Object p3);

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
