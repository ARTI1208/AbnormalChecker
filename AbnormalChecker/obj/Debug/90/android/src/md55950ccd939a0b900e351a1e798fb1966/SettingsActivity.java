package md55950ccd939a0b900e351a1e798fb1966;


public class SettingsActivity
	extends android.support.v7.app.AppCompatActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onSupportNavigateUp:()Z:GetOnSupportNavigateUpHandler\n" +
			"";
		mono.android.Runtime.register ("AbnormalChecker.Activities.SettingsActivity, AbnormalChecker", SettingsActivity.class, __md_methods);
	}


	public SettingsActivity ()
	{
		super ();
		if (getClass () == SettingsActivity.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.Activities.SettingsActivity, AbnormalChecker", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public boolean onSupportNavigateUp ()
	{
		return n_onSupportNavigateUp ();
	}

	private native boolean n_onSupportNavigateUp ();

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
