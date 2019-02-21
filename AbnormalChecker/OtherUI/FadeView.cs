using System;
using Android.Animation;
using Android.Views;
using Android.Views.Animations;

namespace AbnormalChecker.OtherUI
{
    public class FadeView : Java.Lang.Object, Animation.IAnimationListener, Animator.IAnimatorListener
    {

        private View mView;
        private bool mShow;

        public FadeView(View view, bool show = false)
        {
            mView = view;
            mShow = show;
        }

        public void OnAnimationEnd(Animation animation)
        {
            if (mView != null)
            {
                mView.Visibility = mShow ? ViewStates.Visible : ViewStates.Gone;    
            }
            
        }

        public void OnAnimationRepeat(Animation animation)
        {
            
        }

        public void OnAnimationStart(Animation animation)
        {
            
        }
        
        public void OnAnimationCancel(Animator animation)
        {
            
        }

        public void OnAnimationEnd(Animator animation)
        {
            if (mView != null)
            {
                mView.Visibility = mShow ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        public void OnAnimationRepeat(Animator animation)
        {
            
        }

        public void OnAnimationStart(Animator animation)
        {
            
        }
    }
}