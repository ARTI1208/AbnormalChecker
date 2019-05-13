using Android.Graphics.Drawables;
using Android.Support.Annotation;
using Android.Widget;

namespace OnBoardingLib.Code
{
	public class FlowingGradientUtils
	{
		private int drawableId;

		private int duration = 4000;
		private AnimationDrawable frameAnimation;
		private ImageView imageView;
		private LinearLayout linearLayout;
		private RelativeLayout relativeLayout;

		public FlowingGradientUtils SetTransitionDuration(int time)
		{
			duration = time;
			return this;
		}

		// ReSharper disable once UnusedMember.Global
		public FlowingGradientUtils OnLinearLayout(LinearLayout ll)
		{
			linearLayout = ll;
			return this;
		}

		// ReSharper disable once UnusedMember.Global
		public FlowingGradientUtils OnImageView(ImageView im)
		{
			imageView = im;
			return this;
		}

		public FlowingGradientUtils OnRelativeLayout(RelativeLayout rl)
		{
			relativeLayout = rl;
			return this;
		}

		public void Start()
		{
			if (linearLayout != null)
				linearLayout.SetBackgroundResource(drawableId);
			else if (relativeLayout != null)
				relativeLayout.SetBackgroundResource(drawableId);
			else
				imageView?.SetBackgroundResource(drawableId);

			if (linearLayout != null)
				frameAnimation = (AnimationDrawable) linearLayout.Background;
			else if (relativeLayout != null)
				frameAnimation = (AnimationDrawable) relativeLayout.Background;
			else if (imageView != null) frameAnimation = (AnimationDrawable) imageView.Background;
			frameAnimation.SetEnterFadeDuration(duration);
			frameAnimation.SetExitFadeDuration(duration);
			frameAnimation.Start();
		}

		public FlowingGradientUtils SetBackgroundResource([DrawableRes] int id)
		{
			drawableId = id;
			return this;
		}

		// ReSharper disable once UnusedMember.Global
		public FlowingGradientUtils SetAlpha(int alpha)
		{
			frameAnimation.SetAlpha(alpha);
			return this;
		}
	}
}