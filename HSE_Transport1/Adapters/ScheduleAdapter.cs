using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace HSE_Transport1.Adapters
{
    class ScheduleAdapter : BaseAdapter<Bus>
    {
        TextView departureText;
        TextView arrivalText;

        IList<Bus> arrivalBuses;
        IList<Bus> departureBuses;
        Context context;

        public ScheduleAdapter(Context context, IList<Bus> departureBuses, IList<Bus> arrivalBuses)
        {
            this.arrivalBuses = arrivalBuses;
            this.departureBuses = departureBuses;
            this.context = context;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                var inflater = LayoutInflater.FromContext(context);
                view = inflater.Inflate(Resource.Layout.schedule_row, parent, false);
            }

            if (view != null)
            {
                departureText = (TextView)view.FindViewById(Resource.Id.dubkiTextView);
                arrivalText = (TextView)view.FindViewById(Resource.Id.odiTextView);

                if (position < departureBuses.Count)
                {
                    if (departureBuses[position].DeparturePlace == "Дубки")
                    {
                        departureText.Text = "Дубки";
                    }
                    else
                    {
                        departureText.Text = departureBuses[position].DepartureTime.ToString("HH:mm");

                        if (departureBuses[position].DeparturePlace == "Slavyanski" || departureBuses[position].ArrivalPlace == "Slavyanski")
                        {
                            departureText.Text += "**";
                        }
                    }
                }
                else
                {
                    departureText.Text = "-";
                }
                
                if (position < arrivalBuses.Count)
                {
                    if (arrivalBuses[position].DeparturePlace == "Одинцово")
                    {
                        arrivalText.Text = "Одинцово";
                    }
                    else if (arrivalBuses[position].DeparturePlace == "по прибытию")
                    {
                        arrivalText.Text = "По прибытию";
                    }
                    else
                    {
                        arrivalText.Text = arrivalBuses[position].DepartureTime.ToString("HH:mm");

                        if (arrivalBuses[position].DeparturePlace == "Slavyanski" || arrivalBuses[position].ArrivalPlace == "Slavyanski")
                        {
                            arrivalText.Text += "**";
                        }
                    }
                }
                else
                {
                    arrivalText.Text = "-";
                }
            }

            return view;
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get { return Math.Max(departureBuses.Count, arrivalBuses.Count); }
        }


        public override Bus this[int position]
        {
            get { return departureBuses.Count > arrivalBuses.Count ? departureBuses[position] : arrivalBuses[position]; }
        }
    }
}