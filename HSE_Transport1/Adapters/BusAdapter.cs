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

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Settin up layout 
            View itemView = null;
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.bus_row, parent, false);

            var vh = new BusAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var bus = BusesList[position];

            var holder = viewHolder as BusAdapterViewHolder;

            if (bus.Occupancy == "extra-low")
            {
                holder.occupancyLayout1.SetBackgroundColor(Color.ForestGreen);
                holder.occupancyLayout2.SetBackgroundColor(Color.White);
                holder.occupancyLayout3.SetBackgroundColor(Color.White);
                holder.occupancyLayout4.SetBackgroundColor(Color.White);
                holder.occupancyLayout5.SetBackgroundColor(Color.White);
            }
            if (bus.Occupancy == "low")
            {
                holder.occupancyLayout1.SetBackgroundColor(Color.Green);
                holder.occupancyLayout2.SetBackgroundColor(Color.Green);
                holder.occupancyLayout3.SetBackgroundColor(Color.White);
                holder.occupancyLayout4.SetBackgroundColor(Color.White);
                holder.occupancyLayout5.SetBackgroundColor(Color.White);
            }
            if (bus.Occupancy == "medium")
            {
                holder.occupancyLayout1.SetBackgroundColor(Color.Gold);
                holder.occupancyLayout2.SetBackgroundColor(Color.Gold);
                holder.occupancyLayout3.SetBackgroundColor(Color.Gold);
                holder.occupancyLayout4.SetBackgroundColor(Color.White);
                holder.occupancyLayout5.SetBackgroundColor(Color.White);
            }
            if (bus.Occupancy == "high")
            {
                holder.occupancyLayout1.SetBackgroundColor(Color.Orange);
                holder.occupancyLayout2.SetBackgroundColor(Color.Orange);
                holder.occupancyLayout3.SetBackgroundColor(Color.Orange);
                holder.occupancyLayout4.SetBackgroundColor(Color.Orange);
                holder.occupancyLayout5.SetBackgroundColor(Color.White);
            }
            if (bus.Occupancy == "extra-high")
            {
                holder.occupancyLayout1.SetBackgroundColor(Color.Red);
                holder.occupancyLayout2.SetBackgroundColor(Color.Red);
                holder.occupancyLayout3.SetBackgroundColor(Color.Red);
                holder.occupancyLayout4.SetBackgroundColor(Color.Red);
                holder.occupancyLayout5.SetBackgroundColor(Color.Red);
            }

            holder.timeTextView.Text = bus.DepartureTime.ToString("HH:mm");        }

        public override int ItemCount => BusesList.Count;

        void OnClick(BusAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(BusAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);
    }

    public class BusAdapterViewHolder : RecyclerView.ViewHolder
    {
        public RelativeLayout occupancyLayout1;
        public RelativeLayout occupancyLayout2;
        public RelativeLayout occupancyLayout3;
        public RelativeLayout occupancyLayout4;
        public RelativeLayout occupancyLayout5;

        public TextView timeTextView;

        public BusAdapterViewHolder(View itemView, Action<BusAdapterClickEventArgs> clickListener,
                            Action<BusAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            occupancyLayout1 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout1);
            occupancyLayout2 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout2);
            occupancyLayout3 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout3);
            occupancyLayout4 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout4);
            occupancyLayout5 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout5);

            timeTextView = (TextView)itemView.FindViewById(Resource.Id.timeTextView);

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