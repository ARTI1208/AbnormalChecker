using System.Collections.Generic;
using Android.Graphics;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace AbnormalChecker.OtherUI
{
    
    public class OnBoardingAdapter : FragmentStatePagerAdapter, ViewScroller.ICardAdapter
    {
        private string TAG = "OnBoardingAdapter";
        List<OnBoardingCard> pages = new List<OnBoardingCard>();
        private List<OnBoardingFragment> mFragments = new List<OnBoardingFragment>();
        private float mBaseElevation;
        private Typeface typeface;

        public OnBoardingAdapter(List<OnBoardingCard> pages, FragmentManager fm, float baseElevation,
            Typeface typeface) : base(fm)
        {
            this.pages = pages;
            this.typeface = typeface;
            mBaseElevation = baseElevation;

            foreach (OnBoardingCard t in pages)
            {
                addCardFragment(t);
            }

            //setTypeface(typeface);
        }


        public override Fragment GetItem(int position)
        {
            //return OnBoardingFragment.newInstance(pages.get(position));
            return mFragments[position];
        }


        public override Object InstantiateItem(ViewGroup container, int position)
        {
            Object fragment = base.InstantiateItem(container, position);
            mFragments[position] = (OnBoardingFragment) fragment;
            return fragment;
        }

        public override int Count => pages.Count;

        public int maxElevationFactor()
        {
            return 6;
        }

        public float getBaseElevation()
        {
            return mBaseElevation;
        }

        public CardView getCardViewAt(int position)
        {
            setTypeface(typeface, position);
            return mFragments[position].GetCardView();
        }

        public int getCardsCount()
        {
            return pages.Count;
        }

        public void addCardFragment(OnBoardingCard page)
        {
            mFragments.Add(OnBoardingFragment.newInstance(page));
        }

        public void setTypeface(Typeface typeface, int i)
        {
            if (typeface != null)
            {
                if (mFragments[i] == null)
                {
                    Log.Info(TAG, "Fragment is null");
                    return;
                }

                if (mFragments[i].GetTitleView() == null)
                {
                    Log.Info(TAG, "TitleView is null");
                    return;
                }

                if (mFragments[i].GetTitleView() == null)
                {
                    Log.Info(TAG, "DescriptionView is null");
                    return;
                }

                mFragments[i].GetTitleView().Typeface = typeface;
                mFragments[i].GetDescriptionView().Typeface = typeface;
            }
        }
    }
}