package md50c1b22c621871810657ea4f4f2ca09e1;


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
		mono.android.Runtime.register ("AbnormalChecker.CategoriesAdapter+ViewHolder, AbnormalChecker", CategoriesAdapter_ViewHolder.class, __md_methods);
	}


	public CategoriesAdapter_ViewHolder (android.view.View p0)
	{
		super (p0);
		if (getClass () == CategoriesAdapter_ViewHolder.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.CategoriesAdapter+ViewHolder, AbnormalChecker", "Android.Views.View, Mono.Android", this, new java.lang.Object[] { p0 });
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
