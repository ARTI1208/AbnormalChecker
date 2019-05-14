using AbnormalChecker.Activities;
using Android.Content;
using Android.Util;
using Android.Views;

namespace AbnormalChecker.OtherUI
{
	public class CategoryClickListener : Java.Lang.Object, View.IOnClickListener
	{

		private string mCategory;
		private Context mContext;
		private DataHolder.CategoryData dataSet;
		
		public CategoryClickListener(Context context, string category, DataHolder.CategoryData data)
		{
			mContext = context;
			mCategory = category;
			dataSet = data;
		}

		public void OnClick(View v)
		{
			Log.Debug(nameof(CategoriesAdapter), "click click listener");
			if (dataSet.Level == DataHolder.CheckStatus.PermissionsRequired)
			{
				MainActivity.GrantPermissions(dataSet.RequiredPermissions);
				return;
			}
			
			Intent intent = new Intent(mContext, typeof(CategoryInfoActivity));
			intent.PutExtra("title", dataSet.Title);
			intent.PutExtra("category", mCategory);
			mContext.StartActivity(intent);
		}
	}
}