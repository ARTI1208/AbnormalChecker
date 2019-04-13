package md5971db72825d735842c2296daa650b80f;


public class FlowingGradient_RunnableAnonymousInnerClassHelper
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		java.lang.Runnable
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_run:()V:GetRunHandler:Java.Lang.IRunnableInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("FlowingGradient+RunnableAnonymousInnerClassHelper, AbnormalChecker", FlowingGradient_RunnableAnonymousInnerClassHelper.class, __md_methods);
	}


	public FlowingGradient_RunnableAnonymousInnerClassHelper ()
	{
		super ();
		if (getClass () == FlowingGradient_RunnableAnonymousInnerClassHelper.class)
			mono.android.TypeManager.Activate ("FlowingGradient+RunnableAnonymousInnerClassHelper, AbnormalChecker", "", this, new java.lang.Object[] {  });
	}

	public FlowingGradient_RunnableAnonymousInnerClassHelper (android.graphics.drawable.AnimationDrawable p0)
	{
		super ();
		if (getClass () == FlowingGradient_RunnableAnonymousInnerClassHelper.class)
			mono.android.TypeManager.Activate ("FlowingGradient+RunnableAnonymousInnerClassHelper, AbnormalChecker", "Android.Graphics.Drawables.AnimationDrawable, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public void run ()
	{
		n_run ();
	}

	private native void n_run ();

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
