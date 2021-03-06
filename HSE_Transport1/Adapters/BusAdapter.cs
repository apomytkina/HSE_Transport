﻿using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Graphics;

namespace HSE_Transport1.Adapters
{
    class BusAdapter : RecyclerView.Adapter
    {
        List<Bus> busesList; 

        public BusAdapter(List<Bus> data)
        {
            busesList = data;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            //Setting up layout 
            View itemView = null;
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.bus_row, parent, false);

            var vh = new BusAdapterViewHolder(itemView);
            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var bus = busesList[position];

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

        public override int ItemCount => busesList.Count;
    }

    public class BusAdapterViewHolder : RecyclerView.ViewHolder
    {
        public RelativeLayout occupancyLayout1;
        public RelativeLayout occupancyLayout2;
        public RelativeLayout occupancyLayout3;
        public RelativeLayout occupancyLayout4;
        public RelativeLayout occupancyLayout5;

        public TextView timeTextView;

        public BusAdapterViewHolder(View itemView) : base(itemView)
        {
            occupancyLayout1 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout1);
            occupancyLayout2 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout2);
            occupancyLayout3 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout3);
            occupancyLayout4 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout4);
            occupancyLayout5 = (RelativeLayout)itemView.FindViewById(Resource.Id.occupancy_layout5);

            timeTextView = (TextView)itemView.FindViewById(Resource.Id.timeTextView);
        }
    }
}