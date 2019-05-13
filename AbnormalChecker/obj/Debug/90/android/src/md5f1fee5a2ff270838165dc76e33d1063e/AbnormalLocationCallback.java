package md5f1fee5a2ff270838165dc76e33d1063e;


public class AbnormalLocationCallback
	extends md57dae306e9c511046bb3e5da82eb8f47a.LocationCallback
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onLocationResult:(Lcom/google/android/gms/location/LocationResult;)V:GetOnLocationResult_Lcom_google_android_gms_location_LocationResult_Handler\n" +
			"";
		mono.android.Runtime.register ("AbnormalChecker.Services.AbnormalLocationCallback, AbnormalChecker", AbnormalLocationCallback.class, __md_methods);
	}


	public AbnormalLocationCallback ()
	{
		super ();
		if (getClass () == AbnormalLocationCallback.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.Services.AbnormalLocationCallback, AbnormalChecker", "", this, new java.lang.Object[] {  });
	}


	public void onLocationResult (com.google.android.gms.location.LocationResult p0)
	{
		n_onLocationResult (p0);
	}

	private native void n_onLocationResult (com.google.android.gms.location.LocationResult p0);

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
