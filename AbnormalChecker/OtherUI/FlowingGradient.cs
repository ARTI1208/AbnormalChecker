

using System;
using AbnormalChecker;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Java.Lang;

public class FlowingGradient : View {

    int duration;
    int draw;
    public FlowingGradient(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) {
        init(context);
    }

    public FlowingGradient(Context context, IAttributeSet attrs) : base(context, attrs) {
        TypedArray a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.gradient,0, 0);

        draw = a.GetResourceId(Resource.Styleable.gradient_transition_drawable, Resource.Drawable.translate);
        duration = a.GetInt(Resource.Styleable.gradient_transition_duration,75);
        init(context);
    }

    public FlowingGradient(Context context) : base(context) {
        init(context);
    }

    private void init(Context context) {
        SetBackgroundResource(draw);
        AnimationDrawable frameAnimation = (AnimationDrawable) Background;
        frameAnimation.SetEnterFadeDuration(duration);
        frameAnimation.SetExitFadeDuration(duration);
        
        
        
        Post(new RunnableAnonymousInnerClassHelper(frameAnimation));
    }
    
    private class RunnableAnonymousInnerClassHelper : Java.Lang.Object, IRunnable
    {
        private AnimationDrawable animDrawable;

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