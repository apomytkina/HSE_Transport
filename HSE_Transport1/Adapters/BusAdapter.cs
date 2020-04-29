using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Graphics;

namespace HSE_Transport1.Adapters
{
    class BusAdapter : RecyclerView.Adapter
    {
        public event EventHandler<BusAdapterClickEventArgs> NotificationClick;
        public event EventHandler<BusAdapterClickEventArgs> ItemClick;
        public event EventHandler<BusAdapterClickEventArgs> ItemLongClick;

        List<Bus> BusesList; 

        public BusAdapter(List<Bus> data)
        {
            BusesList = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.bus_row, parent, false);

            var vh = new BusAdapterViewHolder(itemView, OnClick, OnLongClick, OnNotificationClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var bus = BusesList[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as BusAdapterViewHolder;

            if (bus.Notify)
            {
                holder.okImageView.Visibility = ViewStates.Visible;
            }

            //if (bus.JourneyDuration < 7)
            //{
            //    holder.journeyDurationTextView.SetTextColor(Color.DarkGreen);
            //}
            //if (bus.JourneyDuration >= 7 && bus.JourneyDuration <= 10)
            //{
            //    holder.journeyDurationTextView.SetTextColor(Color.LightGreen);
            //}
            //if (bus.JourneyDuration > 10 && bus.JourneyDuration <= 15)
            //{
            //    holder.journeyDurationTextView.SetTextColor(Color.Gold);
            //}
            //if (bus.JourneyDuration > 15 && bus.JourneyDuration <= 20)
            //{
            //    holder.journeyDurationTextView.SetTextColor(Color.Orange);
            //}
            //if (bus.JourneyDuration > 20)
            //{
            //    holder.journeyDurationTextView.SetTextColor(Color.Red);
            //}

            if (bus.Occupancy == "extra-low")
            {
                holder.loadLayout.SetBackgroundColor(Color.DarkGreen);
            }
            if (bus.Occupancy == "low")
            {
                holder.loadLayout.SetBackgroundColor(Color.Green);
            }
            if (bus.Occupancy == "medium")
            {
                holder.loadLayout.SetBackgroundColor(Color.Gold);
            }
            if (bus.Occupancy == "high")
            {
                holder.loadLayout.SetBackgroundColor(Color.Orange);
            }
            if (bus.Occupancy == "extra-high")
            {
                holder.loadLayout.SetBackgroundColor(Color.Red);
            }

            holder.timeTextView.Text = bus.DepartureTime.ToString("HH:mm");
            //holder.journeyDurationTextView.Text = bus.JourneyDuration.ToString() + "мин";
        }

        public override int ItemCount => BusesList.Count;

        void OnClick(BusAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(BusAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
        void OnNotificationClick(BusAdapterClickEventArgs args) => NotificationClick?.Invoke(this, args);
    }

    public class BusAdapterViewHolder : RecyclerView.ViewHolder
    {
        //public TextView TextView { get; set; }
        public TextView timeTextView;
        public RelativeLayout loadLayout;
        public TextView journeyDurationTextView;
        public LinearLayout setNotificationLayout;
        public ImageView okImageView;

        public BusAdapterViewHolder(View itemView, Action<BusAdapterClickEventArgs> clickListener,
                            Action<BusAdapterClickEventArgs> longClickListener, Action<BusAdapterClickEventArgs> notificationClickListener) : base(itemView)
        {
            //TextView = v;
            okImageView = (ImageView)itemView.FindViewById(Resource.Id.okImageView);
            timeTextView = (TextView)itemView.FindViewById(Resource.Id.timeTextView);
            loadLayout = (RelativeLayout)itemView.FindViewById(Resource.Id.loadLayout);
            journeyDurationTextView = (TextView)itemView.FindViewById(Resource.Id.journeyDurationTextView);
            setNotificationLayout = (LinearLayout)itemView.FindViewById(Resource.Id.setNotificationLayout);

            itemView.Click += (sender, e) => clickListener(new BusAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new BusAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            setNotificationLayout.Click += (sender, e) => notificationClickListener(new BusAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class BusAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}