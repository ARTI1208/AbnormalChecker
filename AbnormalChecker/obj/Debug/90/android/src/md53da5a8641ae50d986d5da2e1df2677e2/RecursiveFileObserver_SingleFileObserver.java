package md53da5a8641ae50d986d5da2e1df2677e2;


public class RecursiveFileObserver_SingleFileObserver
	extends android.os.FileObserver
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onEvent:(ILjava/lang/String;)V:GetOnEvent_ILjava_lang_String_Handler\n" +
			"";
		mono.android.Runtime.register ("AbnormalChecker.Extensions.RecursiveFileObserver+SingleFileObserver, AbnormalChecker", RecursiveFileObserver_SingleFileObserver.class, __md_methods);
	}


	public RecursiveFileObserver_SingleFileObserver (java.lang.String p0)
	{
		super (p0);
		if (getClass () == RecursiveFileObserver_SingleFileObserver.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.Extensions.RecursiveFileObserver+SingleFileObserver, AbnormalChecker", "System.String, mscorlib", this, new java.lang.Object[] { p0 });
	}


	public RecursiveFileObserver_SingleFileObserver (java.lang.String p0, int p1)
	{
		super (p0, p1);
		if (getClass () == RecursiveFileObserver_SingleFileObserver.class)
			mono.android.TypeManager.Activate ("AbnormalChecker.Extensions.RecursiveFileObserver+SingleFileObserver, AbnormalChecker", "System.String, mscorlib:Android.OS.FileObserverEvents, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public void onEvent (int p0, java.lang.String p1)
	{
		n_onEvent (p0, p1);
	}

	private native void n_onEvent (int p0, java.lang.String p1);

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
