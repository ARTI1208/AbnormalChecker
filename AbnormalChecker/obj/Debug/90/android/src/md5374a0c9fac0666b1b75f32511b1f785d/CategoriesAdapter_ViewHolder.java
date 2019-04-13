package md5374a0c9fac0666b1b75f32511b1f785d;


public class CategoriesAdapter_ViewHolder
	extends android.support.v7.widget.RecyclerView.ViewHolder
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("AbnormalChecker.OtherUI.CategoriesAdapter+ViewHolder, AbnormalChecker", CategoriesAdapter_ViewHolder.class, __md_methods);
	}


	public CategoriesAdapter_ViewHolder (android.view.View p0)
	{
		super (p0);
		if (getClass () == CategoriesAdapter_ViewHolder.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.OtherUI.CategoriesAdapter+ViewHolder, AbnormalChecker", "Android.Views.View, Mono.Android", this, new java.lang.Object[] { p0 });
	}

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
