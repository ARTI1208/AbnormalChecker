package md5374a0c9fac0666b1b75f32511b1f785d;


public class OnBoardingAdapter
	extends android.support.v4.app.FragmentStatePagerAdapter
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_getItem:(I)Landroid/support/v4/app/Fragment;:GetGetItem_IHandler\n" +
			"n_instantiateItem:(Landroid/view/ViewGroup;I)Ljava/lang/Object;:GetInstantiateItem_Landroid_view_ViewGroup_IHandler\n" +
			"n_getCount:()I:GetGetCountHandler\n" +
			"";
		mono.android.Runtime.register ("AbnormalChecker.OtherUI.OnBoardingAdapter, AbnormalChecker", OnBoardingAdapter.class, __md_methods);
	}


	public OnBoardingAdapter (android.support.v4.app.FragmentManager p0)
	{
		super (p0);
		if (getClass () == OnBoardingAdapter.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.OtherUI.OnBoardingAdapter, AbnormalChecker", "Android.Support.V4.App.FragmentManager, Xamarin.Android.Support.Fragment", this, new java.lang.Object[] { p0 });
	}


	public android.support.v4.app.Fragment getItem (int p0)
	{
		return n_getItem (p0);
	}

	private native android.support.v4.app.Fragment n_getItem (int p0);


	public java.lang.Object instantiateItem (android.view.ViewGroup p0, int p1)
	{
		return n_instantiateItem (p0, p1);
	}

	private native java.lang.Object n_instantiateItem (android.view.ViewGroup p0, int p1);


	public int getCount ()
	{
		return n_getCount ();
	}

	private native int n_getCount ();

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
