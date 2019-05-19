using AbnormalChecker.Activities;
using Android.Content;
using Android.Views;

namespace AbnormalChecker.OtherUI
{
	public class CategoryClickListener : Java.Lang.Object, View.IOnClickListener
	{
		private readonly string mCategory;
		private readonly Context mContext;
		private readonly DataHolder.CategoryData dataSet;
		
		public CategoryClickListener(Context context, string category, DataHolder.CategoryData data)
		{
			mContext = context;
			mCategory = category;
			dataSet = data;
		}

		public void OnClick(View v)
		{
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