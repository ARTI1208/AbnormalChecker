using System.Diagnostics.CodeAnalysis;
using Android.Graphics.Drawables;
using Android.Support.Annotation;

namespace OnBoardingLib.Code
{
	public class OnBoardingCard
	{
		[ColorRes] private int backgroundColor;
		private readonly string description;
		[ColorRes] private int descriptionColor;
		[StringRes] private readonly int descriptionResourceId;
		private float descriptionTextSize;
		private int mIconWidth, mIconHeight, mMarginTop, mMarginLeft, mMarginRight, mMarginBottom;
		private Drawable imageResource;
		[DrawableRes] private int imageResourceId;
		private readonly string title;
		[ColorRes] private int titleColor;
		[StringRes] private readonly int titleResourceId;

		private float titleTextSize;

		public OnBoardingCard(string title, string description)
		{
			this.title = title;
			this.description = description;
		}

		public OnBoardingCard(int title, int description)
		{
			titleResourceId = title;
			descriptionResourceId = description;
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public OnBoardingCard(string title, string description, int imageResourceId)
		{
			this.title = title;
			this.description = description;
			this.imageResourceId = imageResourceId;
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public OnBoardingCard(string title, string description, Drawable imageResource)
		{
			this.title = title;
			this.description = description;
			this.imageResource = imageResource;
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public OnBoardingCard(int title, int description, int imageResourceId)
		{
			titleResourceId = title;
			descriptionResourceId = description;
			this.imageResourceId = imageResourceId;
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public OnBoardingCard(int title, int description, Drawable imageResource)
		{
			titleResourceId = title;
			descriptionResourceId = description;
			this.imageResource = imageResource;
		}

		public string GetTitle()
		{
			return title;
		}

		public int GetTitleResourceId()
		{
			return titleResourceId;
		}

		public string GetDescription()
		{
			return description;
		}

		public int GetDescriptionResourceId()
		{
			return descriptionResourceId;
		}

		public int GetTitleColor()
		{
			return titleColor;
		}

		public int GetDescriptionColor()
		{
			return descriptionColor;
		}

		public void SetTitleColor(int color)
		{
			titleColor = color;
		}

		public void SetDescriptionColor(int color)
		{
			descriptionColor = color;
		}

		public void SetImageResourceId(int imageResourceId)
		{
			this.imageResourceId = imageResourceId;
		}

		public int GetImageResourceId()
		{
			return imageResourceId;
		}

		public float GetTitleTextSize()
		{
			return titleTextSize;
		}

		public void SetTitleTextSize(float titleTextSize)
		{
			this.titleTextSize = titleTextSize;
		}

		public float GetDescriptionTextSize()
		{
			return descriptionTextSize;
		}

		public void SetDescriptionTextSize(float descriptionTextSize)
		{
			this.descriptionTextSize = descriptionTextSize;
		}

		public int GetBackgroundColor()
		{
			return backgroundColor;
		}

		public void SetBackgroundColor(int backgroundColor)
		{
			this.backgroundColor = backgroundColor;
		}

		public int GetIconWidth()
		{
			return mIconWidth;
		}

		public void SetIconLayoutParams(int iconWidth, int iconHeight, int marginTop, int marginLeft, int marginRight,
			int marginBottom)
		{
			mIconWidth = iconWidth;
			mIconHeight = iconHeight;
			mMarginLeft = marginLeft;
			mMarginRight = marginRight;
			mMarginTop = marginTop;
			mMarginBottom = marginBottom;
		}

		public int GetIconHeight()
		{
			return mIconHeight;
		}

		public int GetMarginTop()
		{
			return mMarginTop;
		}

		public int GetMarginLeft()
		{
			return mMarginLeft;
		}

		public int GetMarginRight()
		{
			return mMarginRight;
		}

		public int GetMarginBottom()
		{
			return mMarginBottom;
		}
	}
}