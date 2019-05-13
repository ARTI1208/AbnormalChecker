package md5ad598d7af5f6e4c2df2e85b48258a249;


public class FlowingGradientView_RunnableAnonymousInnerClassHelper
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
		mono.android.Runtime.register ("OnBoardingLib.Code.FlowingGradientView+RunnableAnonymousInnerClassHelper, OnBoardingLib", FlowingGradientView_RunnableAnonymousInnerClassHelper.class, __md_methods);
	}


	public FlowingGradientView_RunnableAnonymousInnerClassHelper ()
	{
		super ();
		if (getClass () == FlowingGradientView_RunnableAnonymousInnerClassHelper.class)
			mono.android.TypeManager.Activate ("OnBoardingLib.Code.FlowingGradientView+RunnableAnonymousInnerClassHelper, OnBoardingLib", "", this, new java.lang.Object[] {  });
	}

	public FlowingGradientView_RunnableAnonymousInnerClassHelper (android.graphics.drawable.AnimationDrawable p0)
	{
		super ();
		if (getClass () == FlowingGradientView_RunnableAnonymousInnerClassHelper.class)
			mono.android.TypeManager.Activate ("OnBoardingLib.Code.FlowingGradientView+RunnableAnonymousInnerClassHelper, OnBoardingLib", "Android.Graphics.Drawables.AnimationDrawable, Mono.Android", this, new java.lang.Object[] { p0 });
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
