using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AbnormalChecker
{
    public class CategoriesAdapter : RecyclerView.Adapter
    {
        private readonly LayoutInflater mInflater;
        private readonly Context mContext;
        private readonly CategoriesData data;

        public CategoriesAdapter(Context context)
        {
            mContext = context;
            mInflater = LayoutInflater.From(context);
            data = new CategoriesData(mContext);
        }


        public void Refresh()
        {
            data.Refresh();
            NotifyDataSetChanged();
        }
        
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ViewHolder myHolder = (ViewHolder) holder;
            
            CategoriesData.CategoryStruct dataSet = data.categoriesList[position];
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

            Log.Debug("holderLoad", dataSet.Title);
            
            myHolder.Card.Click += delegate
            {
                Toast.MakeText(mContext, myHolder.TitleTextView.Text, ToastLength.Short).Show();
            };
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = mInflater.Inflate(Resource.Layout.category_list_item, parent, false);
            return new ViewHolder(view);
        }

        public override int ItemCount => data.categoriesList.Count;

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