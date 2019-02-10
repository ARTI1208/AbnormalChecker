package mono.android.support.v17.leanback.widget.picker;


public class Picker_PickerValueListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.support.v17.leanback.widget.picker.Picker.PickerValueListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onValueChanged:(Landroid/support/v17/leanback/widget/picker/Picker;I)V:GetOnValueChanged_Landroid_support_v17_leanback_widget_picker_Picker_IHandler:Android.Support.V17.Leanback.Widget.Picker.Picker/IPickerValueListenerInvoker, Xamarin.Android.Support.v17.Leanback\n" +
			"";
		mono.android.Runtime.register ("Android.Support.V17.Leanback.Widget.Picker.Picker+IPickerValueListenerImplementor, Xamarin.Android.Support.v17.Leanback", Picker_PickerValueListenerImplementor.class, __md_methods);
	}


	public Picker_PickerValueListenerImplementor ()
	{
		super ();
		if (getClass () == Picker_PickerValueListenerImplementor.class)
			mono.android.TypeManager.Activate ("Android.Support.V17.Leanback.Widget.Picker.Picker+IPickerValueListenerImplementor, Xamarin.Android.Support.v17.Leanback", "", this, new java.lang.Object[] {  });
	}


	public void onValueChanged (android.support.v17.leanback.widget.picker.Picker p0, int p1)
	{
		n_onValueChanged (p0, p1);
	}

	private native void n_onValueChanged (android.support.v17.leanback.widget.picker.Picker p0, int p1);

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
