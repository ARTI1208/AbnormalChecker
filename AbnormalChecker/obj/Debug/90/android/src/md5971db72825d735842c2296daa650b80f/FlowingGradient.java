package md5971db72825d735842c2296daa650b80f;


public class FlowingGradient
	extends android.view.View
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("FlowingGradient, AbnormalChecker", FlowingGradient.class, __md_methods);
	}


	public FlowingGradient (android.content.Context p0)
	{
		super (p0);
		if (getClass () == FlowingGradient.class)
			mono.android.TypeManager.Activate ("FlowingGradient, AbnormalChecker", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public FlowingGradient (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == FlowingGradient.class)
			mono.android.TypeManager.Activate ("FlowingGradient, AbnormalChecker", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public FlowingGradient (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == FlowingGradient.class)
			mono.android.TypeManager.Activate ("FlowingGradient, AbnormalChecker", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}


	public FlowingGradient (android.content.Context p0, android.util.AttributeSet p1, int p2, int p3)
	{
		super (p0, p1, p2, p3);
		if (getClass () == FlowingGradient.class)
			mono.android.TypeManager.Activate ("FlowingGradient, AbnormalChecker", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2, p3 });
	}

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
