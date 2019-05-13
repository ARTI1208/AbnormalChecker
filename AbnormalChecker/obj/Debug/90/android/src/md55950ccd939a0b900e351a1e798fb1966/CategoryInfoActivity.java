package md55950ccd939a0b900e351a1e798fb1966;


public class CategoryInfoActivity
	extends android.support.v7.app.AppCompatActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onSupportNavigateUp:()Z:GetOnSupportNavigateUpHandler\n" +
			"n_onNavigateUp:()Z:GetOnNavigateUpHandler\n" +
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("AbnormalChecker.Activities.CategoryInfoActivity, AbnormalChecker", CategoryInfoActivity.class, __md_methods);
	}


	public CategoryInfoActivity ()
	{
		super ();
		if (getClass () == CategoryInfoActivity.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.Activities.CategoryInfoActivity, AbnormalChecker", "", this, new java.lang.Object[] {  });
	}


	public boolean onSupportNavigateUp ()
	{
		return n_onSupportNavigateUp ();
	}

	private native boolean n_onSupportNavigateUp ();


	public boolean onNavigateUp ()
	{
		return n_onNavigateUp ();
	}

	private native boolean n_onNavigateUp ();


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);

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
