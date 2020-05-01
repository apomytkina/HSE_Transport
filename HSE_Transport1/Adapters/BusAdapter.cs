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

            var vh = new BusAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var bus = BusesList[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as BusAdapterViewHolder;

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

            holder.timeTextView.Text = bus.DepartureTime.ToString("HH:mm");        }

        public override int ItemCount => BusesList.Count;

        void OnClick(BusAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(BusAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    }

    public class BusAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView timeTextView;
        public RelativeLayout loadLayout;

        public BusAdapterViewHolder(View itemView, Action<BusAdapterClickEventArgs> clickListener,
                            Action<BusAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            timeTextView = (TextView)itemView.FindViewById(Resource.Id.timeTextView);
            loadLayout = (RelativeLayout)itemView.FindViewById(Resource.Id.loadLayout);

            itemView.Click += (sender, e) => clickListener(new BusAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new BusAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class BusAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}