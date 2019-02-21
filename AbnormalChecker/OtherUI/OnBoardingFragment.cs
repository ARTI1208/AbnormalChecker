using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Annotation;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace AbnormalChecker.OtherUI
{
    public class OnBoardingFragment : Fragment
    {
        private const string AHOY_PAGE_TITLE = "ahoy_page_title";
        private const string AHOY_PAGE_TITLE_RES_ID = "ahoy_page_title_res_id";
        private const string AHOY_PAGE_TITLE_COLOR = "ahoy_page_title_color";
        private const string AHOY_PAGE_TITLE_TEXT_SIZE = "ahoy_page_title_text_size";
        private const string AHOY_PAGE_DESCRIPTION = "ahoy_page_description";
        private const string AHOY_PAGE_DESCRIPTION_RES_ID = "ahoy_page_description_res_id";
        private const string AHOY_PAGE_DESCRIPTION_COLOR = "ahoy_page_description_color";
        private const string AHOY_PAGE_DESCRIPTION_TEXT_SIZE = "ahoy_page_description_text_size";
        private const string AHOY_PAGE_IMAGE_RES_ID = "ahoy_page_image_res_id";
        private const string AHOY_PAGE_BACKGROUND_COLOR = "ahoy_page_background_color";
        private const string AHOY_PAGE_ICON_WIDTH = "ahoy_page_icon_width";
        private const string AHOY_PAGE_ICON_HEIGHT = "ahoy_page_icon_height";
        private const string AHOY_PAGE_MARGIN_LEFT = "ahoy_page_margin_left";
        private const string AHOY_PAGE_MARGIN_RIGHT = "ahoy_page_margin_right";
        private const string AHOY_PAGE_MARGIN_TOP = "ahoy_page_margin_top";
        private const string AHOY_PAGE_MARGIN_BOTTOM = "ahoy_page_margin_bottom";


        private string title;
        private string description;
        [StringRes] private int titleResId;
        [ColorRes] private int titleColor;
        [StringRes] private int descriptionResId;
        [ColorRes] private int backgroundColor;
        [ColorRes] private int descriptionColor;
        [DrawableRes] private int imageResId;
        private float titleTextSize;
        private float descriptionTextSize;

        private View view;
        private ImageView ivOnboarderImage;
        private TextView tvOnboarderTitle;
        private TextView tvOnboarderDescription;
        private CardView cardView;
        private int iconHeight, iconWidth;
        private int marginTop, marginBottom, marginLeft, marginRight;

        public static OnBoardingFragment newInstance(OnBoardingCard card)
        {
            Bundle args = new Bundle();
            args.PutString(AHOY_PAGE_TITLE, card.getTitle());
            args.PutString(AHOY_PAGE_DESCRIPTION, card.getDescription());
            args.PutInt(AHOY_PAGE_TITLE_RES_ID, card.getTitleResourceId());
            args.PutInt(AHOY_PAGE_DESCRIPTION_RES_ID, card.getDescriptionResourceId());
            args.PutInt(AHOY_PAGE_TITLE_COLOR, card.getTitleColor());
            args.PutInt(AHOY_PAGE_DESCRIPTION_COLOR, card.getDescriptionColor());
            args.PutInt(AHOY_PAGE_IMAGE_RES_ID, card.getImageResourceId());
            args.PutFloat(AHOY_PAGE_TITLE_TEXT_SIZE, card.getTitleTextSize());
            args.PutFloat(AHOY_PAGE_DESCRIPTION_TEXT_SIZE, card.getDescriptionTextSize());
            args.PutInt(AHOY_PAGE_BACKGROUND_COLOR, card.getBackgroundColor());
            args.PutInt(AHOY_PAGE_ICON_HEIGHT, card.getIconHeight());
            args.PutInt(AHOY_PAGE_ICON_WIDTH, card.getIconWidth());
            args.PutInt(AHOY_PAGE_MARGIN_LEFT, card.getMarginLeft());
            args.PutInt(AHOY_PAGE_MARGIN_RIGHT, card.getMarginRight());
            args.PutInt(AHOY_PAGE_MARGIN_TOP, card.getMarginTop());
            args.PutInt(AHOY_PAGE_MARGIN_BOTTOM, card.getMarginBottom());

            OnBoardingFragment fragment = new OnBoardingFragment();
            fragment.Arguments = args;
            return fragment;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Bundle bundle = Arguments;

            title = bundle.GetString(AHOY_PAGE_TITLE, null);
            titleResId = bundle.GetInt(AHOY_PAGE_TITLE_RES_ID, 0);
            titleColor = bundle.GetInt(AHOY_PAGE_TITLE_COLOR, 0);
            titleTextSize = bundle.GetFloat(AHOY_PAGE_TITLE_TEXT_SIZE, 0f);
            description = bundle.GetString(AHOY_PAGE_DESCRIPTION, null);
            descriptionResId = bundle.GetInt(AHOY_PAGE_DESCRIPTION_RES_ID, 0);
            descriptionColor = bundle.GetInt(AHOY_PAGE_DESCRIPTION_COLOR, 0);
            descriptionTextSize = bundle.GetFloat(AHOY_PAGE_DESCRIPTION_TEXT_SIZE, 0f);
            imageResId = bundle.GetInt(AHOY_PAGE_IMAGE_RES_ID, 0);
            backgroundColor = bundle.GetInt(AHOY_PAGE_BACKGROUND_COLOR, 0);
            iconWidth = bundle.GetInt(AHOY_PAGE_ICON_WIDTH, (int) dpToPixels(128, Activity));
            iconHeight = bundle.GetInt(AHOY_PAGE_ICON_HEIGHT, (int) dpToPixels(128, Activity));
            marginTop = bundle.GetInt(AHOY_PAGE_MARGIN_TOP, (int) dpToPixels(80, Activity));
            marginBottom = bundle.GetInt(AHOY_PAGE_MARGIN_BOTTOM, (int) dpToPixels(0, Activity));
            marginLeft = bundle.GetInt(AHOY_PAGE_MARGIN_LEFT, (int) dpToPixels(0, Activity));
            marginRight = bundle.GetInt(AHOY_PAGE_MARGIN_RIGHT, (int) dpToPixels(0, Activity));

            view = inflater.Inflate(Resource.Layout.boarding_fragment, container, false);
            ivOnboarderImage = (ImageView) view.FindViewById(Resource.Id.iv_image);
            tvOnboarderTitle = (TextView) view.FindViewById(Resource.Id.tv_title);
            tvOnboarderDescription = (TextView) view.FindViewById(Resource.Id.tv_description);
            cardView = (CardView) view.FindViewById(Resource.Id.cv_cardview);

            if (title != null)
            {
                tvOnboarderTitle.Text = title;
            }

            if (titleResId != 0)
            {
                tvOnboarderTitle.SetText(titleResId);
            }

            if (description != null)
            {
                tvOnboarderDescription.Text = description;
            }

            if (descriptionResId != 0)
            {
                tvOnboarderDescription.SetText(descriptionResId);
            }

            if (titleColor != 0)
            {
                tvOnboarderTitle.SetTextColor(new Color(ContextCompat.GetColor(Activity, titleColor)));
            }

            if (descriptionColor != 0)
            {
                tvOnboarderDescription.SetTextColor(new Color(ContextCompat.GetColor(Activity, descriptionColor)));
            }

            if (imageResId != 0)
            {
                ivOnboarderImage.SetImageDrawable(ContextCompat.GetDrawable(Activity, imageResId));
            }

            if (titleTextSize != 0f)
            {
                tvOnboarderTitle.TextSize = titleTextSize;
            }

            if (descriptionTextSize != 0f)
            {
                tvOnboarderDescription.TextSize = descriptionTextSize;
            }

            if (backgroundColor != 0)
            {
                cardView.SetCardBackgroundColor(ContextCompat.GetColor(Activity, backgroundColor));
            }

            if (iconWidth != 0 && iconHeight != 0)
            {
                FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(iconWidth, iconHeight);
                layoutParams.Gravity = GravityFlags.ClipHorizontal;
                layoutParams.SetMargins(marginLeft, marginTop, marginRight, marginBottom);
                ivOnboarderImage.LayoutParameters = layoutParams;
            }

            return view;
        }

        public float dpToPixels(int dp, Context context)
        {
            return dp * (context.Resources.DisplayMetrics.Density);
        }

        public CardView GetCardView()
        {
            return cardView;
        }

        public TextView GetTitleView()
        {
            return tvOnboarderTitle;
        }

        public TextView GetDescriptionView()
        {
            return tvOnboarderDescription;
        }
    }
}