using System.Collections.Generic;
using AbnormalChecker.Activities;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace AbnormalChecker.OtherUI
{
	public class CategoriesAdapter : RecyclerView.Adapter
	{
		private readonly Context mContext;
		private readonly LayoutInflater mInflater;
		public readonly List<string> categories = new List<string>();
		private readonly List<DataHolder.CategoryData> list = new List<DataHolder.CategoryData>();

		public CategoriesAdapter(Context context)
		{
			mContext = context;
			mInflater = LayoutInflater.From(context);
			DataHolder.Refresh();
			list.Clear();
			categories.Clear();
			foreach (var pair in DataHolder.CategoriesDictionary)
			{
				list.Add(pair.Value);
				categories.Add(pair.Key);
			}
		}

		public override int ItemCount => list.Count;

		public static void Refresh(string category)
		{

			if (MainActivity.Adapter != null)
			{
				int pos = MainActivity.Adapter.categories.FindIndex(s => s == category);
				if (pos < 0)
				{
					return;
				}

				Log.Debug("CateUpdater", "upd");
				MainActivity.Adapter.Refresh(pos);
			}
			
		}
		
		public void Refresh(int position = -1)
		{
			DataHolder.Refresh();
			list.Clear();
			categories.Clear();
			foreach (var pair in DataHolder.CategoriesDictionary)
				if (DataHolder.GetSelectedCategories().Contains(pair.Key))
				{
					list.Add(pair.Value);
					categories.Add(pair.Key);
				}

			if (position < 0)
			{
				NotifyDataSetChanged();	
			}
			else
			{
				Log.Debug("CateUpdater", "upd2");
				NotifyItemChanged(position);	
			}
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var myHolder = (ViewHolder) holder;
			var dataSet = list[position];
//			var dataSet = DataHolder.CategoriesDictionary[categories[position]];
			myHolder.TitleTextView.Text = dataSet.Title;
			myHolder.StatusTextView.Text = dataSet.Status;
			
			if (dataSet.Data != null)
			{
				myHolder.DataTextView.Text = dataSet.Data;
				myHolder.DataTextView.Visibility = ViewStates.Visible;
			}
			else
			{
				myHolder.DataTextView.Visibility = ViewStates.Gone;
			}

			if (categories[position] == DataHolder.ScreenCategory)
			{
				Log.Debug("ScreenBindAb",dataSet.Data ?? "null");
			}
			
			switch (dataSet.Level)
			{
				case DataHolder.CheckStatus.Warning:
					myHolder.Card.SetCardBackgroundColor(Color.ParseColor("#fffdd835"));
					break;
				case DataHolder.CheckStatus.Dangerous:
					myHolder.Card.SetCardBackgroundColor(Color.ParseColor("#ffff0000"));
					break;
				case DataHolder.CheckStatus.PermissionsRequired:
					myHolder.Card.SetCardBackgroundColor(Color.ParseColor("#ff00ff00"));
					break;
				default:
					myHolder.Card.SetCardBackgroundColor(Color.ParseColor("#ffffff"));
					break;
			}
			
			myHolder.Card.SetOnClickListener(new CategoryClickListener(mContext, categories[position], dataSet));
		}
		
		public override void OnViewRecycled(Object holder)
		{
			if (holder is ViewHolder viewHolder)
			{
				viewHolder.Card.SetOnClickListener(null);
			}
			base.OnViewRecycled(holder);
		}
		
		

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			View view = mInflater.Inflate(Resource.Layout.category_list_item, parent, false);
			return new ViewHolder(view);
		}

		private class ViewHolder : RecyclerView.ViewHolder
		{
			public readonly CardView Card;
			public readonly TextView DataTextView;
			public readonly TextView StatusTextView;
			public readonly TextView TitleTextView;

			public ViewHolder(View itemView) : base(itemView)
			{
				TitleTextView = itemView.FindViewById<TextView>(Resource.Id.title);
				StatusTextView = itemView.FindViewById<TextView>(Resource.Id.status);
				DataTextView = itemView.FindViewById<TextView>(Resource.Id.data);
				Card = itemView.FindViewById<CardView>(Resource.Id.cardView);
			}
		}
	}
}