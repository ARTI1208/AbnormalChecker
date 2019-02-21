using Android.Graphics.Drawables;
using Android.Support.Annotation;

namespace AbnormalChecker.OtherUI
{
    public class OnBoardingCard
    {
         public string title;
    public string description;
    public Drawable imageResource;
    [StringRes]
    public int titleResourceId;
    [StringRes]
    public int descriptionResourceId;
    [DrawableRes]
    public int imageResourceId;
    [ColorRes]
    public int titleColor;
    [ColorRes]
    public int descriptionColor;
    [ColorRes]
    public int backgroundColor;

    public float titleTextSize;
    public float descriptionTextSize;
    public int iconWidth, iconHeight, marginTop, marginLeft, marginRight, marginBottom;

    public OnBoardingCard(string title, string description) {
        this.title = title;
        this.description = description;
    }

    public OnBoardingCard(int title, int description) {
        this.titleResourceId = title;
        this.descriptionResourceId = description;
    }

    public OnBoardingCard(string title, string description, int imageResourceId) {
        this.title = title;
        this.description = description;
        this.imageResourceId = imageResourceId;
    }

    public OnBoardingCard(string title, string description, Drawable imageResource) {
        this.title = title;
        this.description = description;
        this.imageResource = imageResource;
    }

    public OnBoardingCard(int title, int description, int imageResourceId) {
        this.titleResourceId = title;
        this.descriptionResourceId = description;
        this.imageResourceId = imageResourceId;
    }

    public OnBoardingCard(int title, int description, Drawable imageResource) {
        this.titleResourceId = title;
        this.descriptionResourceId = description;
        this.imageResource = imageResource;
    }

    public string getTitle() {
        return title;
    }

    public int getTitleResourceId() {
        return titleResourceId;
    }

    public string getDescription() {
        return description;
    }

    public int getDescriptionResourceId() {
        return descriptionResourceId;
    }

    public int getTitleColor() {
        return titleColor;
    }

    public int getDescriptionColor() {
        return descriptionColor;
    }

    public void setTitleColor(int color) {
        this.titleColor = color;
    }

    public void setDescriptionColor(int color) {
        this.descriptionColor = color;
    }

    public void setImageResourceId(int imageResourceId) {
        this.imageResourceId = imageResourceId;
    }

    public int getImageResourceId() {
        return imageResourceId;
    }

    public float getTitleTextSize() {
        return titleTextSize;
    }

    public void setTitleTextSize(float titleTextSize) {
        this.titleTextSize = titleTextSize;
    }

    public float getDescriptionTextSize() {
        return descriptionTextSize;
    }

    public void setDescriptionTextSize(float descriptionTextSize) {
        this.descriptionTextSize = descriptionTextSize;
    }

    public int getBackgroundColor() {
        return backgroundColor;
    }

    public void setBackgroundColor(int backgroundColor) {
        this.backgroundColor = backgroundColor;
    }

    public int getIconWidth() {
        return iconWidth;
    }

    public void setIconLayoutParams(int iconWidth, int iconHeight, int marginTop, int marginLeft, int marginRight, int marginBottom) {
        this.iconWidth = iconWidth;
        this.iconHeight = iconHeight;
        this.marginLeft = marginLeft;
        this.marginRight = marginRight;
        this.marginTop = marginTop;
        this.marginBottom = marginBottom;
    }

    public int getIconHeight() {
        return iconHeight;
    }

    public int getMarginTop() {
        return marginTop;
    }

    public int getMarginLeft() {
        return marginLeft;
    }

    public int getMarginRight() {
        return marginRight;
    }

    public int getMarginBottom() {
        return marginBottom;
    }
    }
}