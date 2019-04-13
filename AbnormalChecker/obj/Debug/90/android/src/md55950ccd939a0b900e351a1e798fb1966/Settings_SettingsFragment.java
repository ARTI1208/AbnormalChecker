package md55950ccd939a0b900e351a1e798fb1966;


public class Settings_SettingsFragment
	extends android.support.v7.preference.PreferenceFragmentCompat
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreatePreferences:(Landroid/os/Bundle;Ljava/lang/String;)V:GetOnCreatePreferences_Landroid_os_Bundle_Ljava_lang_String_Handler\n" +
			"";
		mono.android.Runtime.register ("AbnormalChecker.Activities.Settings+SettingsFragment, AbnormalChecker", Settings_SettingsFragment.class, __md_methods);
	}


	public Settings_SettingsFragment ()
	{
		super ();
		if (getClass () == Settings_SettingsFragment.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.Activities.Settings+SettingsFragment, AbnormalChecker", "", this, new java.lang.Object[] {  });
	}


	public void onCreatePreferences (android.os.Bundle p0, java.lang.String p1)
	{
		n_onCreatePreferences (p0, p1);
	}

	private native void n_onCreatePreferences (android.os.Bundle p0, java.lang.String p1);

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
