using System;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Java.Lang;

namespace AbnormalChecker.OtherUI
{
/**
 * Created by Rahul Juneja on 30-08-2016.
 */
    public class ViewScroller : Java.Lang.Object, ViewPager.IOnPageChangeListener, ViewPager.IPageTransformer
    {
        private ViewPager mViewPager;
        private ICardAdapter mAdapter;
        private float mLastOffset;
        private bool mScalingEnabled;

        public interface ICardAdapter
        {
            int maxElevationFactor();
            float getBaseElevation();
            CardView getCardViewAt(int position);
            
            int getCardsCount();
        }


        public ViewScroller(ViewPager viewPager, ICardAdapter adapter)
        {
            mViewPager = viewPager;
            viewPager.AddOnPageChangeListener(this);
            mAdapter = adapter;
        }

        public void enableScaling(bool enable)
        {
            if (mScalingEnabled && !enable)
            {
                // shrink main card
                CardView currentCard = mAdapter.getCardViewAt(mViewPager.CurrentItem);
                if (currentCard != null)
                {
                    currentCard.Animate().ScaleY(1);
                    currentCard.Animate().ScaleX(1);
                }
            }
            else if (!mScalingEnabled && enable)
            {
                // grow main card
                CardView currentCard = mAdapter.getCardViewAt(mViewPager.CurrentItem);
                if (currentCard != null)
                {
                    currentCard.Animate().ScaleY(1.1f);
                    currentCard.Animate().ScaleX(1.1f);
                }
            }

            mScalingEnabled = enable;
        }

        public void TransformPage(View page, float position)
        {
        }


        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            int realCurrentPosition;
            int nextPosition;
            float baseElevation = mAdapter.getBaseElevation();
            float realOffset;
            bool goingLeft = mLastOffset > positionOffset;

            // If we're going backwards, onPageScrolled receives the last position
            // instead of the current one
            if (goingLeft)
            {
                realCurrentPosition = position + 1;
                nextPosition = position;
                realOffset = 1 - positionOffset;
            }
            else
            {
                nextPosition = position + 1;
                realCurrentPosition = position;
                realOffset = positionOffset;
            }

            // Avoid crash on overscroll
            if (nextPosition > mAdapter.getCardsCount() - 1
                || realCurrentPosition > mAdapter.getCardsCount() - 1)
            {
                return;
            }

            CardView currentCard = mAdapter.getCardViewAt(realCurrentPosition);

            // This might be null if a fragment is being used
            // and the views weren't created yet
            if (currentCard != null)
            {
                if (mScalingEnabled)
                {
                    currentCard.ScaleX = (float) (1 + 0.1 * (1 - realOffset));
                    currentCard.ScaleY = (float) (1 + 0.1 * (1 - realOffset));
                }

                currentCard.Elevation = baseElevation + baseElevation
                                        * (mAdapter.maxElevationFactor() - 1) * (1 - realOffset);
            }

            CardView nextCard = mAdapter.getCardViewAt(nextPosition);

            // We might be scrolling fast enough so that the next (or previous) card
            // was already destroyed or a fragment might not have been created yet
            if (nextCard != null)
            {
                if (mScalingEnabled)
                {
                    nextCard.ScaleX = (float) (1 + 0.1 * realOffset);
                    nextCard.ScaleY = (float) (1 + 0.1 * realOffset);
                }

                nextCard.Elevation = baseElevation + baseElevation * (mAdapter.maxElevationFactor() - 1) * realOffset;
            }

            mLastOffset = positionOffset;
        }

        public void OnPageSelected(int position)
        {
        }

        public void OnPageScrollStateChanged(int state)
        {
        }
    }
}