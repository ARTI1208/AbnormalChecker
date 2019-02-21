using Android.Content;
using Android.Graphics;
using Android.Support.Annotation;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace AbnormalChecker.OtherUI
{
    public class CircleIndicatorView : View
    {
        private Context context;
        private Paint activeIndicatorPaint;
        private Paint inactiveIndicatorPaint;
        private int radius;
        private int size;
        private int position;
        private int indicatorsCount;

        public CircleIndicatorView(Context context) : base(context)
        {
            init(context);
        }

        public CircleIndicatorView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            init(context);
        }

        public CircleIndicatorView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            init(context);
        }

        private void init(Context context)
        {
            this.context = context;
            activeIndicatorPaint = new Paint();
            activeIndicatorPaint.Color = new Color(ContextCompat.GetColor(context, Resource.Color.active_indicator));
            activeIndicatorPaint.AntiAlias = true;
            inactiveIndicatorPaint = new Paint();
            inactiveIndicatorPaint.Color = new Color(ContextCompat.GetColor(context, Resource.Color.inactive_indicator));
            inactiveIndicatorPaint.AntiAlias = true;
            radius = Resources.GetDimensionPixelSize(Resource.Dimension.indicator_size);
            size = radius * 2;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            for (int i = 0; i < indicatorsCount; i++)
            {
                canvas.DrawCircle(radius + size * i, radius, radius / 2, inactiveIndicatorPaint);
            }
            canvas.DrawCircle(radius + size * position, radius, radius / 2, activeIndicatorPaint);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            SetMeasuredDimension(measureWidth(widthMeasureSpec), measureHeight(heightMeasureSpec));
        }

        public void setCurrentPage(int position)
        {
            this.position = position;
            Invalidate();
        }

        public void setPageIndicators(int size)
        {
            indicatorsCount = size;
            Invalidate();
        }

        private int measureWidth(int measureSpec)
        {
            int result = 0;
            MeasureSpecMode specMode = MeasureSpec.GetMode(measureSpec);
            int specSize = MeasureSpec.GetSize(measureSpec);

            if (specMode == MeasureSpecMode.Exactly)
            {
                result = specSize;
            }
            else
            {
                result = size * indicatorsCount;
                if (specMode == MeasureSpecMode.AtMost)
                {
                    result = Math.Min(result, specSize);
                }
            }

            return result;
        }

        private int measureHeight(int measureSpec)
        {
            int result = 0;
            MeasureSpecMode specMode = MeasureSpec.GetMode(measureSpec);
            int specSize = MeasureSpec.GetSize(measureSpec);

            if (specMode == MeasureSpecMode.Exactly)
            {
                result = specSize;
            }
            else
            {
                result = 2 * radius + PaddingTop + PaddingBottom;
                if (specMode == MeasureSpecMode.AtMost)
                {
                    result = Math.Min(result, specSize);
                }
            }

            return result;
        }

        public void setInactiveIndicatorColor([ColorRes] int color)
        {
            inactiveIndicatorPaint.Color = new Color(ContextCompat.GetColor(context, color));
            Invalidate();
        }

        public void setActiveIndicatorColor([ColorRes] int color)
        {
            activeIndicatorPaint.Color = new Color(ContextCompat.GetColor(context, color));
            Invalidate();
        }
    }
}