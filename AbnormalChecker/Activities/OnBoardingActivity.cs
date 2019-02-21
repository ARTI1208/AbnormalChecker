using System;
using System.Collections.Generic;
using AbnormalChecker.OtherUI;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.Annotation;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace AbnormalChecker.Activities
{
    public abstract class OnBoardingActivity : AppCompatActivity, View.IOnClickListener, 
        ViewPager.IOnPageChangeListener {

    private CircleIndicatorView circleIndicatorView;
    private ViewPager vpOnboarderPager;
    private OnBoardingAdapter ahoyOnboarderAdapter;
    private TextView btnSkip;
    private ImageView ivNext, ivPrev;
    private FrameLayout navigationControls;
    private FrameLayout buttonsLayout;
    private RelativeLayout parentLayout;
    private ImageView backgroundImage;
    private View backgroundImageOverlay;

    private ViewScroller mCardShadowTransformer;
    private Typeface typeface;
    private List<int> colorList;
    private bool solidBackground;
    private List<OnBoardingCard> Pages;

    protected override void OnCreate(Bundle savedInstanceState) {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.boarding_activity);
        setStatusBackgroundColor();
        hideActionBar();

        parentLayout =  FindViewById<RelativeLayout>(Resource.Id.parent_layout);
        circleIndicatorView = FindViewById<CircleIndicatorView>(Resource.Id.circle_indicator_view);
        btnSkip = FindViewById<TextView>(Resource.Id.btn_skip);
        buttonsLayout = FindViewById<FrameLayout>(Resource.Id.buttons_layout);
        navigationControls = FindViewById<FrameLayout>(Resource.Id.navigation_layout);
        ivNext = FindViewById<ImageView>(Resource.Id.ivNext);
        ivPrev = FindViewById<ImageView>(Resource.Id.ivPrev);
        backgroundImage = FindViewById<ImageView>(Resource.Id.background_image);
        backgroundImageOverlay = FindViewById<View>(Resource.Id.background_image_overlay);
        vpOnboarderPager = FindViewById<ViewPager>(Resource.Id.vp_pager);
        vpOnboarderPager.AddOnPageChangeListener(this);
        btnSkip.SetOnClickListener(this);
        ivPrev.SetOnClickListener(this);
        ivNext.SetOnClickListener(this);

//        hideFinish(false);
//        fadeOut(ivPrev, false);
    }

    public void setOnboardPages(List<OnBoardingCard> pages) {
        if (pages.Count > 1) {
            ivNext.Visibility = ViewStates.Visible;
        }
        else
        {
            showFinish();
        }
        Pages = pages;
        ahoyOnboarderAdapter = new OnBoardingAdapter(pages, SupportFragmentManager, dpToPixels(0, this), typeface);
        mCardShadowTransformer = new ViewScroller(vpOnboarderPager, ahoyOnboarderAdapter);
        mCardShadowTransformer.enableScaling(true);
        vpOnboarderPager.Adapter = ahoyOnboarderAdapter;
        vpOnboarderPager.SetPageTransformer(false, mCardShadowTransformer);
        circleIndicatorView.setPageIndicators(pages.Count);
    }

    public float dpToPixels(int dp, Context context) {
        return dp * context.Resources.DisplayMetrics.Density;
    }

    private void setStatusBackgroundColor() {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility) (SystemUiFlags.LayoutStable | SystemUiFlags.LayoutFullscreen);
            Window.SetStatusBarColor(new Color(ContextCompat.GetColor(this, Resource.Color.black_transparent)));
        }
    }

    public void OnClick(View v) {
        int i = v.Id;
        bool isInFirstPage = vpOnboarderPager.CurrentItem == 0;
        bool isInLastPage = vpOnboarderPager.CurrentItem == ahoyOnboarderAdapter.Count - 1;

        if (i == Resource.Id.btn_skip && isInLastPage) {
            onFinishButtonPressed();
        } else if (i == Resource.Id.ivPrev && !isInFirstPage) {
            vpOnboarderPager.CurrentItem--;
        } else if (i == Resource.Id.ivNext && !isInLastPage) {
            vpOnboarderPager.CurrentItem++;
        }
    }

    public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
    {
        
    }

    public void OnPageSelected(int position) {

        int firstPagePosition = 0;
        int lastPagePosition = ahoyOnboarderAdapter.Count - 1;
        circleIndicatorView.setCurrentPage(position);

        if (position == lastPagePosition) {
            fadeOut(circleIndicatorView);
            showFinish();
            fadeOut(ivNext);
            fadeIn(ivPrev);
        } else if (position == firstPagePosition) {
            fadeOut(ivPrev);
            fadeIn(ivNext);
            hideFinish();
            fadeIn(circleIndicatorView);
        } else {
            fadeIn(circleIndicatorView);
            hideFinish();
            fadeIn(ivPrev);
            fadeIn(ivNext);
        }

        if (solidBackground && Pages.Count == colorList.Count) {
            backgroundImage.SetBackgroundColor(new Color(ContextCompat.GetColor(this, colorList[position])));
        }

    }

    public void OnPageScrollStateChanged(int state) {

    }

    private void fadeOut(View v, bool delay = true) {

        long duration = 0;
        if (delay) {
            duration = 300;
        }

        if (v.Visibility != ViewStates.Gone) {
            Animation fadeOut = new AlphaAnimation(1, 0);
            fadeOut.Interpolator = new AccelerateInterpolator(); //and this
            fadeOut.Duration = duration;
            fadeOut.SetAnimationListener(new FadeView(v));
            v.StartAnimation(fadeOut);
        }
    }

    private void fadeIn(View v) {

        if (v.Visibility != ViewStates.Visible) {
            Animation fadeIn = new AlphaAnimation(0, 1);
            Toast.MakeText(this, "fadein", ToastLength.Short).Show();
            fadeIn.Interpolator = new DecelerateInterpolator(); //add this
            fadeIn.Duration = 300;
            fadeIn.SetAnimationListener(new FadeView(v, true));
            v.StartAnimation(fadeIn);
        }
    }

    private void showFinish() {
        btnSkip.Visibility = ViewStates.Visible;
        btnSkip.Animate().TranslationY(0 - dpToPixels(5, this)).SetInterpolator(new DecelerateInterpolator())
            .SetDuration(500).Start();
    }

    private void hideFinish(bool delay) {

        long duration = 0;
        if (delay) {
            duration = 250;
        }

        btnSkip.Animate().TranslationY(btnSkip.Bottom + dpToPixels(100, this))
            .SetInterpolator(new AccelerateInterpolator()).SetDuration(duration).SetListener(new FadeView(btnSkip, true)).Start();
    }

    private void hideFinish() {
        hideFinish(true);
    }

    private void hideActionBar()
    {
        SupportActionBar?.Hide();
    }

    abstract public void onFinishButtonPressed();

    public void showNavigationControls(bool navigation) {
        if (navigation)
        {
            navigationControls.Visibility = ViewStates.Visible;
        } 
        else
        {
            navigationControls.Visibility = ViewStates.Gone;
        }
    }

    public void setImageBackground(int resId)
    {
        backgroundImageOverlay.Visibility = ViewStates.Visible;
        backgroundImage.SetImageResource(resId);
    }

    public void setColorBackground([ColorRes] int color) {
        backgroundImage.SetBackgroundColor(new Color(ContextCompat.GetColor(this, color)));
    }

    public void setColorBackground(List<int> color) {
        colorList = color;
        solidBackground = true;
        backgroundImage.SetBackgroundColor(new Color(ContextCompat.GetColor(this, color[0])));
    }

    public void setGradientBackground() {

        FlowingGradientClass grad = new FlowingGradientClass();
        grad.setBackgroundResource(Resource.Drawable.translate)
                .onRelativeLayout(parentLayout)
                .setTransitionDuration(4000)
                .start();
    }

    public void setGradientBackground(int drawable) {
        FlowingGradientClass grad = new FlowingGradientClass();
        grad.setBackgroundResource(drawable)
                .onRelativeLayout(parentLayout)
                .setTransitionDuration(4000)
                .start();
    }

    public void setInactiveIndicatorColor(int color) {
        circleIndicatorView.setInactiveIndicatorColor(color);
    }

    public void setActiveIndicatorColor(int color) {
        circleIndicatorView.setActiveIndicatorColor(color);
    }

    /**
     * <br/><br/>
     * <b>N.B. Builds before JELLY_BEAN will use the default style</b>
     * <br/><br/>
     * Set the xml drawable style of the skip/done button, <br/>
     * using for example: ContextCompat.getDrawable(this, R.drawable.rectangle_button);
     *
     * @param res A drawable xml file representing your desired button style
     */
//    OnBoardingActivity(api = Build.VERSION_CODES.JELLY_BEAN)
    public void setFinishButtonDrawableStyle(Drawable res) {
        btnSkip.Background = res;
    }

    public void setFinishButtonTitle(string title) {
        btnSkip.Text = title;
    }

    public void setFinishButtonTitle([StringRes] int titleResId) {
        btnSkip.SetText(titleResId);
    }

    public void setFont(Typeface typeface) {
        btnSkip.Typeface = typeface;
        this.typeface = typeface;
    }

    
    
    }
}