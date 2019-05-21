using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Annotation;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.Lang;

namespace OnBoardingLib.Code
{
	public abstract class OnBoardingActivity : AppCompatActivity, View.IOnClickListener,
		ViewPager.IOnPageChangeListener
	{
		private OnBoardingAdapter ahoyOnBoardingAdapter;
		private ImageView backgroundImage;
		private View backgroundImageOverlay;
		private List<OnBoardingCard> boardingCards;
		private TextView btnFinish;
		private FrameLayout buttonsLayout;
		private FrameLayout extraLayout;

		private TextView btnSkip;
		private TextView btnSettings;
		
		private CircleIndicatorView circleIndicatorView;
		private List<int> colorList;
		private ImageView ivNext, ivPrev;

		private ViewScroller mCardShadowTransformer;
		private FrameLayout navigationControls;
		private RelativeLayout parentLayout;
		private bool solidBackground;
		private Typeface textTypeface;
		private ViewPager vpOnBoardingPager;

		public void OnClick(View v)
		{
			var i = v.Id;
			var isInFirstPage = vpOnBoardingPager.CurrentItem == 0;
			var isInLastPage = vpOnBoardingPager.CurrentItem == ahoyOnBoardingAdapter.Count - 1;

			if (i == Resource.Id.btn_skip && isInLastPage)
				OnFinishButtonPressed();
			else if (i == Resource.Id.ivPrev && !isInFirstPage)
				vpOnBoardingPager.CurrentItem--;
			else if (i == Resource.Id.ivNext && !isInLastPage) vpOnBoardingPager.CurrentItem++;

			if (i == btnSkip.Id)
			{
				OnSkipButtonClicked();
			}
			
			if (i == btnSettings.Id)
			{
				OnSettingsButtonClicked();
			}
			
		}

		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
		}

		public void OnPageSelected(int position)
		{
			var firstPagePosition = 0;
			var lastPagePosition = ahoyOnBoardingAdapter.Count - 1;
			circleIndicatorView.SetCurrentPage(position);

			if (position == lastPagePosition)
			{
				fadeOut(circleIndicatorView);
				ShowFinish();
				fadeOut(ivNext);
				FadeIn(ivPrev);
			}
			else if (position == firstPagePosition)
			{
				fadeOut(ivPrev);
				FadeIn(ivNext);
				HideFinish();
				FadeIn(circleIndicatorView);
			}
			else
			{
				FadeIn(circleIndicatorView);
				HideFinish();
				FadeIn(ivPrev);
				FadeIn(ivNext);
			}

			if (solidBackground && boardingCards.Count == colorList.Count)
				backgroundImage.SetBackgroundColor(new Color(ContextCompat.GetColor(this, colorList[position])));
		}

		public void OnPageScrollStateChanged(int state)
		{
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.boarding_activity);
			HideActionBar();

			parentLayout = FindViewById<RelativeLayout>(Resource.Id.parent_layout);
			circleIndicatorView = FindViewById<CircleIndicatorView>(Resource.Id.circle_indicator_view);
			btnFinish = FindViewById<TextView>(Resource.Id.btn_skip);
			
			btnSkip = FindViewById<TextView>(Resource.Id.skip_button);
			btnSettings = FindViewById<TextView>(Resource.Id.settings_button);

			btnSettings.PaintFlags |= PaintFlags.UnderlineText;
			btnSkip.PaintFlags |= PaintFlags.UnderlineText;
			
			navigationControls = FindViewById<FrameLayout>(Resource.Id.navigation_layout);
			buttonsLayout = FindViewById<FrameLayout>(Resource.Id.buttons_layout);
			
			extraLayout = FindViewById<FrameLayout>(Resource.Id.extra_layout);
			
			ivNext = FindViewById<ImageView>(Resource.Id.ivNext);
			ivPrev = FindViewById<ImageView>(Resource.Id.ivPrev);
			backgroundImage = FindViewById<ImageView>(Resource.Id.background_image);
			backgroundImageOverlay = FindViewById<View>(Resource.Id.background_image_overlay);
			vpOnBoardingPager = FindViewById<ViewPager>(Resource.Id.vp_pager);
			vpOnBoardingPager.AddOnPageChangeListener(this);
			btnFinish.SetOnClickListener(this);
			
			btnSkip.SetOnClickListener(this);
			btnSettings.SetOnClickListener(this);
			
			ivPrev.SetOnClickListener(this);
			ivNext.SetOnClickListener(this);

			SetStatusBackgroundColor();
		}

		protected void SetOnBoardPages(List<OnBoardingCard> pages)
		{
			if (pages.Count > 1)
				ivNext.Visibility = ViewStates.Visible;
			else
				ShowFinish();
			boardingCards = pages;
			ahoyOnBoardingAdapter =
				new OnBoardingAdapter(pages, SupportFragmentManager, DpToPixels(0, this), textTypeface);
			mCardShadowTransformer = new ViewScroller(vpOnBoardingPager, ahoyOnBoardingAdapter);
			mCardShadowTransformer.EnableScaling(true);
			vpOnBoardingPager.Adapter = ahoyOnBoardingAdapter;
			vpOnBoardingPager.SetPageTransformer(false, mCardShadowTransformer);
			circleIndicatorView.SetPageIndicatorCount(pages.Count);
		}

		private static float DpToPixels(int dp, Context context)
		{
			return dp * context.Resources.DisplayMetrics.Density;
		}

		private void SetStatusBackgroundColor()
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				Window.DecorView.SystemUiVisibility =
					(StatusBarVisibility) (SystemUiFlags.LayoutStable | SystemUiFlags.LayoutFullscreen |
					                       SystemUiFlags.LayoutHideNavigation);
				Window.SetStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.black_transparent)));
				Window.SetNavigationBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.black_transparent)));
				ViewCompat.SetOnApplyWindowInsetsListener(buttonsLayout, new InsetsListener(InsetsListener.InsetSide.Bottom));
				ViewCompat.SetOnApplyWindowInsetsListener(extraLayout, new InsetsListener(InsetsListener.InsetSide.Top));
			}
		}

		private void fadeOut(View v, bool delay = true)
		{
			long duration = 0;
			if (delay) duration = 300;

			if (v.Visibility != ViewStates.Gone)
			{
				Animation fadeOut = new AlphaAnimation(1, 0);
				fadeOut.Interpolator = new AccelerateInterpolator();
				fadeOut.Duration = duration;
				fadeOut.SetAnimationListener(new FadeView(v));
				v.StartAnimation(fadeOut);
			}
		}

		public override void OnBackPressed()
		{
			vpOnBoardingPager.SetCurrentItem(vpOnBoardingPager.CurrentItem - 1, true);
		}

		private static void FadeIn(View v)
		{
			if (v.Visibility != ViewStates.Visible)
			{
				Animation fadeIn = new AlphaAnimation(0, 1);
				fadeIn.Interpolator = new DecelerateInterpolator();
				fadeIn.Duration = 300;
				fadeIn.SetAnimationListener(new FadeView(v, true));
				v.StartAnimation(fadeIn);
			}

			v.Visibility = ViewStates.Visible;
		}

		private void ShowFinish()
		{
			btnFinish.Visibility = ViewStates.Visible;
			btnFinish.Animate().TranslationY(0 - DpToPixels(5, this)).SetInterpolator(new DecelerateInterpolator())
				.SetDuration(500).Start();
		}

		private void HideFinish(bool delay = true)
		{
			long duration = 0;
			if (delay) duration = 250;

			btnFinish.Animate().TranslationY(btnFinish.Bottom + DpToPixels(100, this))
				.SetInterpolator(new AccelerateInterpolator()).SetDuration(duration)
				.SetListener(new FadeView(btnFinish, true)).Start();
		}

		private void HideActionBar()
		{
			SupportActionBar?.Hide();
		}

		protected abstract void OnFinishButtonPressed();

		protected virtual void OnSettingsButtonClicked()
		{
			Finish();
		}
		
		protected virtual void OnSkipButtonClicked()
		{
			Finish();
		}

		protected void ShowNavigationControls(bool navigation)
		{
			navigationControls.Visibility = navigation ? ViewStates.Visible : ViewStates.Gone;
		}

		protected void SetImageBackground(int resId)
		{
			backgroundImageOverlay.Visibility = ViewStates.Visible;
			backgroundImage.SetImageResource(resId);
		}

		protected void SetColorBackground([ColorRes] int color)
		{
			backgroundImage.SetBackgroundColor(new Color(ContextCompat.GetColor(this, color)));
		}

		protected void SetColorBackground(List<int> color)
		{
			colorList = color;
			solidBackground = true;
			backgroundImage.SetBackgroundColor(new Color(ContextCompat.GetColor(this, color[0])));
		}

		protected void SetGradientBackground()
		{
			var grad = new FlowingGradientUtils();
			grad.SetBackgroundResource(Resource.Drawable.translate)
				.OnRelativeLayout(parentLayout)
				.SetTransitionDuration(4000)
				.Start();
		}

		protected void SetGradientBackground(int drawable)
		{
			var grad = new FlowingGradientUtils();
			grad.SetBackgroundResource(drawable)
				.OnRelativeLayout(parentLayout)
				.SetTransitionDuration(4000)
				.Start();
		}

		protected void SetInactiveIndicatorColor(int color)
		{
			circleIndicatorView.SetInactiveIndicatorColor(color);
		}

		protected void SetActiveIndicatorColor(int color)
		{
			circleIndicatorView.SetActiveIndicatorColor(color);
		}

		protected void SetFinishButtonDrawableStyle(Drawable res)
		{
			btnFinish.Background = res;
		}

		protected void SetFinishButtonTitle(string title)
		{
			btnFinish.Text = title;
		}

		protected void SetFinishButtonTitle([StringRes] int titleResId)
		{
			btnFinish.SetText(titleResId);
		}
		
		protected void SetSettingsButtonTitle([StringRes] int titleResId)
		{
			btnSettings.SetText(titleResId);
		}
		
		protected void SetSkipButtonTitle([StringRes] int titleResId)
		{
			btnSkip.SetText(titleResId);
		}

		public void SetFont(Typeface typeface)
		{
			btnFinish.Typeface = typeface;
			textTypeface = typeface;
		}
	}

	internal class InsetsListener : Object, IOnApplyWindowInsetsListener
	{
		internal enum InsetSide
		{
			Top, Bottom
		}

		private readonly InsetSide Side;
		
		public InsetsListener(InsetSide side)
		{
			Side = side;
		}

		public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets)
		{
			Log.Debug(nameof(OnBoardingActivity), "0");
			if (v.LayoutParameters is ViewGroup.MarginLayoutParams layoutParams)
			{
				Log.Debug(nameof(OnBoardingActivity), "1");
				switch (Side)
				{
					case InsetSide.Top:
						Log.Debug(nameof(OnBoardingActivity), "2");
						layoutParams.TopMargin = insets.SystemWindowInsetTop;
						v.LayoutParameters = layoutParams;
						break;
					case InsetSide.Bottom:
						Log.Debug(nameof(OnBoardingActivity), "3");
						layoutParams.BottomMargin = insets.SystemWindowInsetBottom;
						v.LayoutParameters = layoutParams;
						Log.Debug(nameof(OnBoardingActivity), layoutParams.BottomMargin.ToString());
						break;
				}
			}
			return insets;
		}
	}
}