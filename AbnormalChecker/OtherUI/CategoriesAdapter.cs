using System.Collections.Generic;
using AbnormalChecker.Activities;
using AbnormalChecker.BroadcastReceivers;
using AbnormalChecker.Services;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AbnormalChecker.OtherUI
{
    public class CategoriesAdapter : RecyclerView.Adapter
    {
        private readonly LayoutInflater mInflater;
        private readonly Context mContext;
        private readonly DataHolder mDataHolder;
        private List<DataHolder.CategoryData> list = new List<DataHolder.CategoryData>();
        private List<string> categories = new List<string>();

        public CategoriesAdapter(Context context)
        {
            mContext = context;
            mInflater = LayoutInflater.From(context);
            mDataHolder = new DataHolder(mContext);
            mDataHolder.Refresh();
            foreach (var pair in DataHolder.CategoriesDataDic)
            {
                list.Add(pair.Value);
                categories.Add(pair.Key);
            }
        }

        public void Refresh()
        {
            mDataHolder.Refresh();
            list.Clear();
            categories.Clear();
            foreach (var pair in DataHolder.CategoriesDataDic)
            {
                if (DataHolder.GetSelectedCategories().Contains(pair.Key))
                {
                    list.Add(pair.Value);
                    categories.Add(pair.Key);
                }
            }
            NotifyDataSetChanged();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ViewHolder myHolder = (ViewHolder) holder;
            DataHolder.CategoryData dataSet = list[position];
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

            switch (dataSet.Level)
            {
                case DataHolder.CheckStatus.Warning:
                    myHolder.Card.SetCardBackgroundColor(Color.ParseColor("#ff00ff00"));
                    break;
                case DataHolder.CheckStatus.Dangerous:
                    myHolder.Card.SetCardBackgroundColor(Color.ParseColor("#ffff0000"));
                    break;
                case DataHolder.CheckStatus.PermissionsRequired:
                    myHolder.Card.SetCardBackgroundColor(Color.ParseColor("#fffdd835"));
                    break;
                default:
                    myHolder.Card.SetCardBackgroundColor(Color.ParseColor("#ffffff"));
                    break;
            }
            if (!myHolder.Card.HasOnClickListeners)
            {
                myHolder.Card.Click += delegate
                {
                    if (dataSet.Level == DataHolder.CheckStatus.PermissionsRequired)
                    {
                        MainActivity.GrantPermissions(dataSet.RequiredPermissions);
                        return;
                    }
                    Intent intent = new Intent(mContext, typeof(MoreInfoActivity));
                    intent.PutExtra("title", dataSet.Title);
                    intent.PutExtra("category", categories[position]);
                    mContext.StartActivity(intent);
                };    
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = mInflater.Inflate(Resource.Layout.category_list_item, parent, false);
            return new ViewHolder(view);
        }

        public override int ItemCount => list.Count;

        private class ViewHolder : RecyclerView.ViewHolder
        {
            public readonly TextView TitleTextView;
            public readonly TextView StatusTextView;
            public readonly TextView DataTextView;
            public readonly CardView Card;

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