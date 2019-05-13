package md55950ccd939a0b900e351a1e798fb1966;


public class WelcomeActivity
	extends md5ad598d7af5f6e4c2df2e85b48258a249.OnBoardingActivity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("AbnormalChecker.Activities.WelcomeActivity, AbnormalChecker", WelcomeActivity.class, __md_methods);
	}


	public WelcomeActivity ()
	{
		super ();
		if (getClass () == WelcomeActivity.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.Activities.WelcomeActivity, AbnormalChecker", "", this, new java.lang.Object[] {  });
	}


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
