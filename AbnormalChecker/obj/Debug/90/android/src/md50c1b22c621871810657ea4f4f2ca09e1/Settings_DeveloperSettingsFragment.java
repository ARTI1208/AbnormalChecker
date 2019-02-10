package md50c1b22c621871810657ea4f4f2ca09e1;


public class Settings_DeveloperSettingsFragment
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
		mono.android.Runtime.register ("AbnormalChecker.Settings+DeveloperSettingsFragment, AbnormalChecker", Settings_DeveloperSettingsFragment.class, __md_methods);
	}


	public Settings_DeveloperSettingsFragment ()
	{
		super ();
		if (getClass () == Settings_DeveloperSettingsFragment.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.Settings+DeveloperSettingsFragment, AbnormalChecker", "", this, new java.lang.Object[] {  });
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
