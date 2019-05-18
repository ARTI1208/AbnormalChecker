using System.Collections.Generic;
using Android.Graphics;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace OnBoardingLib.Code
{
	public class OnBoardingAdapter : FragmentStatePagerAdapter, ViewScroller.ICardAdapter
	{
		private readonly string TAG = "OnBoardingAdapter";
		private readonly float mBaseElevation;
		private readonly List<OnBoardingFragment> mFragments = new List<OnBoardingFragment>();
		private readonly List<OnBoardingCard> pages;
		private readonly Typeface mTypeface;

		public OnBoardingAdapter(List<OnBoardingCard> pages, FragmentManager fm, float baseElevation,
			Typeface typeface) : base(fm)
		{
			this.pages = pages;
			mTypeface = typeface;
			mBaseElevation = baseElevation;

			foreach (var t in pages) AddCardFragment(t);
		}

		public override int Count => pages.Count;

		public int MaxElevationFactor()
		{
			return 6;
		}

		public float GetBaseElevation()
		{
			return mBaseElevation;
		}

		public CardView GetCardViewAt(int position)
		{
			SetTypeface(mTypeface, position);
			return mFragments[position].GetCardView();
		}

		public int GetCardsCount()
		{
			return pages.Count;
		}


		public override Fragment GetItem(int position)
		{
			return mFragments[position];
		}


		public override Object InstantiateItem(ViewGroup container, int position)
		{
			var fragment = base.InstantiateItem(container, position);
			mFragments[position] = (OnBoardingFragment) fragment;
			return fragment;
		}

		private void AddCardFragment(OnBoardingCard page)
		{
			mFragments.Add(OnBoardingFragment.NewInstance(page));
		}

		private void SetTypeface(Typeface typeface, int i)
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