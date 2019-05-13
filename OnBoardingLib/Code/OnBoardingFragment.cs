using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Annotation;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace OnBoardingLib.Code
{
	public class OnBoardingFragment : Fragment
	{
		private const string PageTitle = "ahoy_page_title";
		private const string PageTitleResId = "ahoy_page_title_res_id";
		private const string PageTitleColor = "ahoy_page_title_color";
		private const string PageTitleTextSize = "ahoy_page_title_text_size";
		private const string PageDescription = "ahoy_page_description";
		private const string PageDescriptionResId = "ahoy_page_description_res_id";
		private const string PageDescriptionColor = "ahoy_page_description_color";
		private const string PageDescriptionTextSize = "ahoy_page_description_text_size";
		private const string PageImageResId = "ahoy_page_image_res_id";
		private const string PageBackgroundColor = "ahoy_page_background_color";
		private const string PageIconWidth = "ahoy_page_icon_width";
		private const string PageIconHeight = "ahoy_page_icon_height";
		private const string PageMarginLeft = "ahoy_page_margin_left";
		private const string PageMarginRight = "ahoy_page_margin_right";
		private const string PageMarginTop = "ahoy_page_margin_top";
		private const string PageMarginBottom = "ahoy_page_margin_bottom";
		[ColorRes] private int backgroundColor;
		private CardView cardView;
		private string description;
		[ColorRes] private int descriptionColor;
		[StringRes] private int descriptionResId;
		private float descriptionTextSize;
		private int iconHeight, iconWidth;
		[DrawableRes] private int imageResId;
		private ImageView ivOnBoardingImage;
		private int marginTop, marginBottom, marginLeft, marginRight;
		
		private string title;
		[ColorRes] private int titleColor;
		[StringRes] private int titleResId;
		private float titleTextSize;
		private TextView tvOnBoardingDescription;
		private TextView tvOnBoardingTitle;

		private View view;

		public static OnBoardingFragment NewInstance(OnBoardingCard card)
		{
			var args = new Bundle();
			args.PutString(PageTitle, card.GetTitle());
			args.PutString(PageDescription, card.GetDescription());
			args.PutInt(PageTitleResId, card.GetTitleResourceId());
			args.PutInt(PageDescriptionResId, card.GetDescriptionResourceId());
			args.PutInt(PageTitleColor, card.GetTitleColor());
			args.PutInt(PageDescriptionColor, card.GetDescriptionColor());
			args.PutInt(PageImageResId, card.GetImageResourceId());
			args.PutFloat(PageTitleTextSize, card.GetTitleTextSize());
			args.PutFloat(PageDescriptionTextSize, card.GetDescriptionTextSize());
			args.PutInt(PageBackgroundColor, card.GetBackgroundColor());
			args.PutInt(PageIconHeight, card.GetIconHeight());
			args.PutInt(PageIconWidth, card.GetIconWidth());
			args.PutInt(PageMarginLeft, card.GetMarginLeft());
			args.PutInt(PageMarginRight, card.GetMarginRight());
			args.PutInt(PageMarginTop, card.GetMarginTop());
			args.PutInt(PageMarginBottom, card.GetMarginBottom());

			var fragment = new OnBoardingFragment {Arguments = args};
			return fragment;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var bundle = Arguments;

			title = bundle.GetString(PageTitle, null);
			titleResId = bundle.GetInt(PageTitleResId, 0);
			titleColor = bundle.GetInt(PageTitleColor, 0);
			titleTextSize = bundle.GetFloat(PageTitleTextSize, 0f);
			description = bundle.GetString(PageDescription, null);
			descriptionResId = bundle.GetInt(PageDescriptionResId, 0);
			descriptionColor = bundle.GetInt(PageDescriptionColor, 0);
			descriptionTextSize = bundle.GetFloat(PageDescriptionTextSize, 0f);
			imageResId = bundle.GetInt(PageImageResId, 0);
			backgroundColor = bundle.GetInt(PageBackgroundColor, 0);
			iconWidth = bundle.GetInt(PageIconWidth, (int) DpToPixels(128, Activity));
			iconHeight = bundle.GetInt(PageIconHeight, (int) DpToPixels(128, Activity));
			marginTop = bundle.GetInt(PageMarginTop, (int) DpToPixels(80, Activity));
			marginBottom = bundle.GetInt(PageMarginBottom, (int) DpToPixels(0, Activity));
			marginLeft = bundle.GetInt(PageMarginLeft, (int) DpToPixels(0, Activity));
			marginRight = bundle.GetInt(PageMarginRight, (int) DpToPixels(0, Activity));

			view = inflater.Inflate(Resource.Layout.boarding_fragment, container, false);
			ivOnBoardingImage = (ImageView) view.FindViewById(Resource.Id.iv_image);
			tvOnBoardingTitle = (TextView) view.FindViewById(Resource.Id.tv_title);
			tvOnBoardingDescription = (TextView) view.FindViewById(Resource.Id.tv_description);
			cardView = (CardView) view.FindViewById(Resource.Id.cv_cardview);

			if (title != null) tvOnBoardingTitle.Text = title;

			if (titleResId != 0) tvOnBoardingTitle.SetText(titleResId);

			if (description != null) tvOnBoardingDescription.Text = description;

			if (descriptionResId != 0) tvOnBoardingDescription.SetText(descriptionResId);

			if (titleColor != 0) tvOnBoardingTitle.SetTextColor(new Color(ContextCompat.GetColor(Activity, titleColor)));

			if (descriptionColor != 0)
				tvOnBoardingDescription.SetTextColor(new Color(ContextCompat.GetColor(Activity, descriptionColor)));

			if (imageResId != 0) ivOnBoardingImage.SetImageDrawable(ContextCompat.GetDrawable(Activity, imageResId));

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (titleTextSize != 0f) tvOnBoardingTitle.TextSize = titleTextSize;

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (descriptionTextSize != 0f) tvOnBoardingDescription.TextSize = descriptionTextSize;

			if (backgroundColor != 0)
				cardView.SetCardBackgroundColor(ContextCompat.GetColor(Activity, backgroundColor));

			if (iconWidth != 0 && iconHeight != 0)
			{
				var layoutParams = new FrameLayout.LayoutParams(iconWidth, iconHeight)
				{
					Gravity = GravityFlags.ClipHorizontal
				};
				layoutParams.SetMargins(marginLeft, marginTop, marginRight, marginBottom);
				ivOnBoardingImage.LayoutParameters = layoutParams;
			}

			return view;
		}

		private static float DpToPixels(int dp, Context context)
		{
			return dp * context.Resources.DisplayMetrics.Density;
		}

		public CardView GetCardView()
		{
			return cardView;
		}

		public TextView GetTitleView()
		{
			return tvOnBoardingTitle;
		}

		public TextView GetDescriptionView()
		{
			return tvOnBoardingDescription;
		}
	}
}