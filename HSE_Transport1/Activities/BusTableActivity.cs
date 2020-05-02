using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using FR.Ganfra.Materialspinner;
using HSE_Transport1.Adapters;

namespace HSE_Transport1.Activities
{
    [Activity(Label = "BusTableActivity", Theme = "@style/AppTheme", MainLauncher = false)]
    public class BusTableActivity : AppCompatActivity
    {
        TextView noteTextView;

        ListView scheduleListView;
        int withoutTimeBuses;
        List<Bus> buses;

        List<Bus> arrivalBuses;
        List<Bus> departureBuses;

        ImageButton scheduleButton;
        ImageButton busesButton;

        RelativeLayout busesLayout;
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

            noteTextView = (TextView)FindViewById(Resource.Id.noteTextView);
            scheduleListView = (ListView)FindViewById(Resource.Id.scheduleListView);

            daySpinner = (MaterialSpinner)FindViewById(Resource.Id.daySpinner);
            toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.tableToolbar);

            scheduleLayout = (RelativeLayout)FindViewById(Resource.Id.schedule_layout);
            busesLayout = (RelativeLayout)FindViewById(Resource.Id.buses_layout);

            scheduleButton = (ImageButton)FindViewById(Resource.Id.scheduleButton);
            busesButton = (ImageButton)FindViewById(Resource.Id.busesButton);

            scheduleButton.Click += Schedule_Click;
            busesButton.Click += Buses_Click;

            busesLayout.Click += Buses_Click;
            scheduleLayout.Click += Schedule_Click;

            ParseData();
            SortBuses();

            SetUpToolbar();
            SetUpSpinner();
            SetUpListView(withoutTimeBuses, mon_fri_dub_odi, mon_fri_odi_dub);
        }

        /// <summary>
        /// Method that starts BusTableActivity when clicking 
        /// on busButton or busLayout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Buses_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(BusTableActivity));
        }

        /// <summary>
        /// Method that starts MainActivity when clicking
        /// on scheduleButton or scheduleLayout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Schedule_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }

        /// <summary>
        /// Method that sets up day spinner
        /// </summary>
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

        /// <summary>
        /// Method that invokes day spinner
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Method that sets up toolbar
        /// </summary>
        void SetUpToolbar()
        {
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Расписание автобусов";
        }

        /// <summary>
        /// Method that parses data abput buses from "bus_info.csv"
        /// </summary>
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

                    if (DateTime.TryParse(dataLine[0], out departureTime)
                        && RightDirection(dataLine[1], dataLine[2])
                        && (dataLine[3] == "extra-low"
                        || dataLine[3] == "low"
                        || dataLine[3] == "medium"
                        || dataLine[3] == "high"
                        || dataLine[3] == "extra-high")
                        && (dataLine[4] == "Monday-Friday"
                        || dataLine[4] == "Saturday"
                        || dataLine[4] == "Sunday"))
                    {
                        Bus bus = new Bus
                        {
                            DepartureTime = departureTime,
                            DeparturePlace = dataLine[1],
                            ArrivalPlace = dataLine[2],
                            Occupancy = dataLine[3],
                            Day = dataLine[4]
                        };

                        buses.Add(bus);
                    }
                };
            }
        }

        /// <summary>
        /// Method that checks weather a bus has a right direction
        /// </summary>
        /// <param name="departure"></param>
        /// <param name="arrival"></param>
        /// <returns></returns>
        static bool RightDirection(string departure, string arrival)
        {
            return (departure == "Dubki" && (arrival == "Slavyanski" || arrival == "Odintsovo"))
                || (departure == "Odintsovo" && (arrival == "Dubki"))
                || (departure == "Slavyanski" && arrival == "Dubki");
        }

        /// <summary>
        /// Method that sorts buses by day of week
        /// </summary>
        void SortBuses()
        {
            mon_fri_dub_odi = buses
                .Where(x => x.Day != "Sunday" && x.Day != "Saturday")
                .Where(x => x.DeparturePlace == "Dubki")
                .ToList();

            mon_fri_odi_dub = buses
                .Where(x => x.Day != "Sunday" && x.Day != "Saturday")
                .Where(x => x.ArrivalPlace == "Dubki")
                .ToList();

            sat_dub_odi = buses
                .Where(x => x.Day == "Saturday")
                .Where(x => x.DeparturePlace == "Dubki")
                .ToList();

            sat_odi_dub = buses
                .Where(x => x.Day == "Saturday")
                .Where(x => x.ArrivalPlace == "Dubki")
                .ToList();

            sun_dub = buses
                .Where(x => x.Day == "Sunday")
                .Where(x => x.DeparturePlace == "Dubki")
                .ToList();

            sun_odi = buses
               .Where(x => x.Day == "Sunday")
               .Where(x => x.DeparturePlace == "Odintsovo")
               .ToList();

            withoutTimeBuses = mon_fri_dub_odi
                       .Where(x => x.DepartureTime < DateTime.Parse("11:00"))
                       .Count();
        }

        /// <summary>
        /// Method that sets up scheduleListView
        /// </summary>
        /// <param name="withoutTimeBuses"></param>
        /// <param name="departureBuses"></param>
        /// <param name="arrivalBuses"></param>
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