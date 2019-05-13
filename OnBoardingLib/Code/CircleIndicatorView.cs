using System.Diagnostics.CodeAnalysis;
using Android.Content;
using Android.Graphics;
using Android.Support.Annotation;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace OnBoardingLib.Code
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public class CircleIndicatorView : View
	{
		private Paint activeIndicatorPaint;
		private Paint inactiveIndicatorPaint;
		private int indicatorsCount;
		private Context mContext;
		private int mPosition;
		private int radius;
		private int size;

		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public CircleIndicatorView(Context context) : base(context)
		{
			Init(context);
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public CircleIndicatorView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Init(context);
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global")]
		public CircleIndicatorView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			Init(context);
		}

		private void Init(Context context)
		{
			mContext = context;
			activeIndicatorPaint = new Paint
			{
				Color = new Color(ContextCompat.GetColor(context, Resource.Color.active_indicator)),
				AntiAlias = true
			};
			inactiveIndicatorPaint = new Paint
			{
				Color = new Color(ContextCompat.GetColor(context, Resource.Color.inactive_indicator)),
				AntiAlias = true
			};
			radius = Resources.GetDimensionPixelSize(Resource.Dimension.indicator_size);
			size = radius * 2;
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			for (var i = 0; i < indicatorsCount; i++)
				canvas.DrawCircle(radius + size * i, radius, radius / 2f, inactiveIndicatorPaint);
			canvas.DrawCircle(radius + size * mPosition, radius, radius / 2f, activeIndicatorPaint);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			SetMeasuredDimension(MeasureWidth(widthMeasureSpec), MeasureHeight(heightMeasureSpec));
		}

		public void SetCurrentPage(int position)
		{
			mPosition = position;
			Invalidate();
		}

		public void SetPageIndicatorCount(int count)
		{
			indicatorsCount = count;
			Invalidate();
		}

		private int MeasureWidth(int measureSpec)
		{
			int result;
			var specMode = MeasureSpec.GetMode(measureSpec);
			var specSize = MeasureSpec.GetSize(measureSpec);

			if (specMode == MeasureSpecMode.Exactly)
			{
				result = specSize;
			}
			else
			{
				result = size * indicatorsCount;
				if (specMode == MeasureSpecMode.AtMost) result = Math.Min(result, specSize);
			}

			return result;
		}

		private int MeasureHeight(int measureSpec)
		{
			int result;
			var specMode = MeasureSpec.GetMode(measureSpec);
			var specSize = MeasureSpec.GetSize(measureSpec);

			if (specMode == MeasureSpecMode.Exactly)
			{
				result = specSize;
			}
			else
			{
				result = 2 * radius + PaddingTop + PaddingBottom;
				if (specMode == MeasureSpecMode.AtMost) result = Math.Min(result, specSize);
			}

			return result;
		}

		public void SetInactiveIndicatorColor([ColorRes] int color)
		{
			inactiveIndicatorPaint.Color = new Color(ContextCompat.GetColor(mContext, color));
			Invalidate();
		}

		public void SetActiveIndicatorColor([ColorRes] int color)
		{
			activeIndicatorPaint.Color = new Color(ContextCompat.GetColor(mContext, color));
			Invalidate();
		}
	}
}