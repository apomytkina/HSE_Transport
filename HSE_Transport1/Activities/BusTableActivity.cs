using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.RecyclerView.Extensions;
using Android.Views;
using Android.Widget;
using FR.Ganfra.Materialspinner;
using HSE_Transport1.Adapters;
using HSE_Transport1.Helpers;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Newtonsoft.Json;

namespace HSE_Transport1.Activities
{
    [Activity(Label = "BusTableActivity", Theme = "@style/AppTheme", MainLauncher = false)]
    public class BusTableActivity : AppCompatActivity
    {
        //LinearLayout noteLayout;
        TextView noteTextView;

        ListView scheduleListView;
        int withoutTimeBuses;
        List<Bus> buses;

        List<Bus> arrivalBuses;
        List<Bus> departureBuses;

        ImageButton mapButton;
        ImageButton scheduleButton;
        ImageButton notificationButton;

        RelativeLayout notficationLayout;
        RelativeLayout mapLayout;
        RelativeLayout scheduleLayout;

        Android.Support.V7.Widget.Toolbar toolbar;
        
        MaterialSpinner daySpinner;
        string selectedDay;
        List<string> dayList;
        ArrayAdapter<string> spinnerAdapter;

        List<Bus> mon_fri_dub_odi;
        List<Bus> sat_dub_odi;
        List<Bus> mon_fri_odi_dub;
        List<Bus> sat_odi_dub;

        List<Bus> sun_dub;
        List<Bus> sun_odi;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.bus_table);

            //noteLayout = (LinearLayout)FindViewById(Resource.Id.noteLayout);
            noteTextView = (TextView)FindViewById(Resource.Id.noteTextView);
            scheduleListView = (ListView)FindViewById(Resource.Id.scheduleListView);

            daySpinner = (MaterialSpinner)FindViewById(Resource.Id.daySpinner);
            toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.tableToolbar);

            scheduleLayout = (RelativeLayout)FindViewById(Resource.Id.schedule_layout);
            notficationLayout = (RelativeLayout)FindViewById(Resource.Id.notification_layout);
            mapLayout = (RelativeLayout)FindViewById(Resource.Id.map_layout);

            scheduleButton = (ImageButton)FindViewById(Resource.Id.scheduleButton);
            notificationButton = (ImageButton)FindViewById(Resource.Id.notificationButton);
            mapButton = (ImageButton)FindViewById(Resource.Id.mapButton);

            scheduleButton.Click += ScheduleButton_Click;
            notificationButton.Click += NotificationButton_Click;

            notficationLayout.Click += NotficationLayout_Click;
            scheduleLayout.Click += ScheduleLayout_Click;

            ParseData();
            SortBuses();

            SetUpToolbar();
            SetUpSpinner();
            SetUpListView(withoutTimeBuses, mon_fri_dub_odi, mon_fri_odi_dub);
        }

        private void NotificationButton_Click(object sender, EventArgs e)
        {
            InitBusTableActivity();
        }

        private void ScheduleButton_Click(object sender, EventArgs e)
        {
            InitMainActivity();
        }

        private void ScheduleLayout_Click(object sender, EventArgs e)
        {
            InitBusTableActivity();
        }

        private void NotficationLayout_Click(object sender, EventArgs e)
        {
            InitMainActivity();
        }

        void InitMainActivity()
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
            Finish();
        }

        void InitBusTableActivity()
        {
            Intent intent = new Intent(this, typeof(BusTableActivity));
            StartActivity(intent);
            Finish();
        }

        void SetUpSpinner()
        {
            departureBuses = mon_fri_dub_odi;
            arrivalBuses = mon_fri_odi_dub;

            dayList = new List<string>();

            dayList.Add("Понедельник-Пятница");
            dayList.Add("Суббота");
            dayList.Add("Воскресенье");

            spinnerAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, dayList);
            spinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            daySpinner.Adapter = spinnerAdapter;
            daySpinner.ItemSelected += DaySpinner_ItemSelected;
        }

        private void DaySpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (e.Position != -1)
            {
                selectedDay = dayList[e.Position];
                Console.WriteLine(selectedDay);

                if (selectedDay == "Понедельник-Пятница")
                {
                    departureBuses = mon_fri_dub_odi;
                    arrivalBuses = mon_fri_odi_dub;

                    noteTextView.Visibility = ViewStates.Visible;

                    withoutTimeBuses = mon_fri_dub_odi
                        .Where(x => x.DepartureTime < DateTime.Parse("11:00"))
                        .Count();
                }
                else if (selectedDay == "Суббота")
                {
                    departureBuses = sat_dub_odi;
                    arrivalBuses = sat_odi_dub;

                    noteTextView.Visibility = ViewStates.Visible;

                    withoutTimeBuses = sat_dub_odi
                        .Where(x => x.DepartureTime < DateTime.Parse("11:00"))
                        .Count();
                }
                else if (selectedDay == "Воскресенье")
                {
                    departureBuses = sun_dub;
                    arrivalBuses = sun_odi;

                    noteTextView.Visibility = ViewStates.Invisible;
                    withoutTimeBuses = 0;
                }

                SetUpListView(withoutTimeBuses, departureBuses, arrivalBuses);
            }
        }

        void SetUpToolbar()
        {
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Расписание автобусов";
        }

        void ParseData()
        {
            buses = new List<Bus>();
            using (StreamReader sr = new StreamReader(Assets.Open("bus_info.csv")))
            {
                sr.ReadLine();
                string stringLine;
                while ((stringLine = sr.ReadLine()) != null && stringLine.Length > 0)
                {
                    var dataLine = stringLine.Split(',');

                    DateTime departureTime;
                    bool notify;

                    if (DateTime.TryParse(dataLine[0], out departureTime)
                        && RightDirection(dataLine[1], dataLine[2])
                        && (dataLine[4] == "extra-low"
                        || dataLine[4] == "low"
                        || dataLine[4] == "medium"
                        || dataLine[4] == "high"
                        || dataLine[4] == "extra-high")
                        && bool.TryParse(dataLine[5], out notify)
                        && (dataLine[6] == "Monday-Friday"
                        || dataLine[6] == "Saturday"
                        || dataLine[6] == "Sunday"))
                    {
                        Bus bus = new Bus
                        {
                            DepartureTime = departureTime,
                            DeparturePlace = dataLine[1],
                            ArrivalPlace = dataLine[2],
                            Occupancy = dataLine[4],
                            Notify = bool.Parse(dataLine[5]),
                            Day = dataLine[6]
                        };

                        buses.Add(bus);
                    }
                };
            }
        }

        static bool RightDirection(string departure, string arrival)
        {
            return (departure == "Dubki" && (arrival == "Slavyanski" || arrival == "Odintsovo"))
                || (departure == "Odintsovo" && (arrival == "Dubki"))
                || (departure == "Slavyanski" && arrival == "Dubki");
        }

        void SortBuses()
        {
            mon_fri_dub_odi = buses
                .Where(x => x.Day != "Sunday" && x.Day != "Saturday")
                .Where(x => x.DeparturePlace == "Dubki")
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            mon_fri_odi_dub = buses
                .Where(x => x.Day != "Sunday" && x.Day != "Saturday")
                .Where(x => x.ArrivalPlace == "Dubki")
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            sat_dub_odi = buses
                .Where(x => x.Day == "Saturday")
                .Where(x => x.DeparturePlace == "Dubki")
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            sat_odi_dub = buses
                .Where(x => x.Day == "Saturday")
                .Where(x => x.ArrivalPlace == "Dubki")
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            sun_dub = buses
                .Where(x => x.Day == "Sunday")
                .Where(x => x.DeparturePlace == "Dubki")
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            sun_odi = buses
               .Where(x => x.Day == "Sunday")
               .Where(x => x.DeparturePlace == "Odintsovo")
               //.OrderBy(x => x.DepartureTime)
               .ToList();

            withoutTimeBuses = mon_fri_dub_odi
                       .Where(x => x.DepartureTime < DateTime.Parse("11:00"))
                       .Count();
        }

        void SetUpListView(int withoutTimeBuses, List<Bus> departureBuses, List<Bus> arrivalBuses)
        {
            for (int i = 0; i < withoutTimeBuses; i++)
            {
                arrivalBuses.Insert(0, new Bus { DeparturePlace = "по прибытию" });
            }

            arrivalBuses.Insert(0, new Bus { DeparturePlace = "Одинцово" });
            departureBuses.Insert(0, new Bus { DeparturePlace = "Дубки" });

            scheduleListView.Adapter = new ScheduleAdapter(this, departureBuses, arrivalBuses);
        }
    }
}