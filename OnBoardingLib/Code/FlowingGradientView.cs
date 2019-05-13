using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace OnBoardingLib.Code
{
	// ReSharper disable once UnusedMember.Global
	public class FlowingGradientView : View
	{
		private readonly int draw;

		private readonly int duration;

		public FlowingGradientView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			Init();
		}

		public FlowingGradientView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.gradient, 0, 0);

			draw = a.GetResourceId(Resource.Styleable.gradient_transition_drawable, Resource.Drawable.translate);
			duration = a.GetInt(Resource.Styleable.gradient_transition_duration, 75);
			Init();
		}

		public FlowingGradientView(Context context) : base(context)
		{
			Init();
		}

		private void Init()
		{
			SetBackgroundResource(draw);
			var frameAnimation = (AnimationDrawable) Background;
			frameAnimation.SetEnterFadeDuration(duration);
			frameAnimation.SetExitFadeDuration(duration);
			
			Post(new RunnableAnonymousInnerClassHelper(frameAnimation));
		}

		private class RunnableAnonymousInnerClassHelper : Object, IRunnable
		{
			private readonly AnimationDrawable animDrawable;

			public RunnableAnonymousInnerClassHelper(AnimationDrawable animDrawable)
			{
				this.animDrawable = animDrawable;
			}

			public void Run()
			{
				animDrawable.Start();
			}
		}
	}
}