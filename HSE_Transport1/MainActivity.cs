using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Support.V7.Widget;
using HSE_Transport1.Adapters;
using System.Collections.Generic;
using FR.Ganfra.Materialspinner;
using System;
using System.IO;
using System.Linq;
using Android.Content;
using HSE_Transport1.Activities;
using Android.Support.V4.App;
using Android.Graphics;
using HSE_Transport1.DataModels;
using Java.Util;
using Android.Media;

namespace HSE_Transport1
{
    [Activity(Label = "Ближайшие автобусы", Theme = "@style/AppTheme", MainLauncher = false, Icon = "@mipmap/ic_launcher")]
    public class MainActivity : AppCompatActivity
    {
        readonly string[] permissionsGroup = { };

        List<Bus> notificationBuses;

        public const int NOTI_SECONDARY1 = 1200;
        public const string SECONDARY_CHANNEL = "second";

        ImageButton mapButton;
        ImageButton scheduleButton;
        ImageButton notificationButton;

        RelativeLayout notficationLayout;
        RelativeLayout mapLayout;
        RelativeLayout scheduleLayout;

        Android.Support.V7.Widget.Toolbar toolbar;
        List<Bus> buses;

        string selectedRoute;
        MaterialSpinner routeSpinner;
        List<string> routeList;
        ArrayAdapter<string> spinnerAdapter;
        Dictionary<string, List<Bus>> routes;

        List<Bus> dubki_odintsovo;
        List<Bus> odintsovo_dubki;
        List<Bus> dubki_slavyanski;
        List<Bus> slavyanski_dubki;

        RecyclerView busesRecyclerView;
        BusAdapter busesAdapter;
        List<Bus> listOfBuses;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            //RequestPermissions(permissionsGroup, 0);

            notificationBuses = new List<Bus>();

            routeSpinner = (MaterialSpinner)FindViewById(Resource.Id.routeSpinner);
            busesRecyclerView = (RecyclerView)FindViewById(Resource.Id.busesRecyclerView);
            toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.busesToolbar);

            scheduleLayout = (RelativeLayout)FindViewById(Resource.Id.schedule_layout);
            notficationLayout = (RelativeLayout)FindViewById(Resource.Id.notification_layout);
            mapLayout = (RelativeLayout)FindViewById(Resource.Id.map_layout);

            scheduleButton = (ImageButton)FindViewById(Resource.Id.scheduleButton);
            notificationButton = (ImageButton)FindViewById(Resource.Id.notificationButton);
            mapButton = (ImageButton)FindViewById(Resource.Id.mapButton);

            scheduleButton.Click += ScheduleButton_Click;
            //notificationButton.Click += NotificationButton_Click;

            notficationLayout.Click += NotficationLayout_Click;
            scheduleLayout.Click += ScheduleLayout_Click;

            ParseData();
            SortBuses();

            SetUpToolbars();
            SetUpSpinner();
            SetUpRecyclerView();
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
            InitMainActivity();
        }

        private void NotficationLayout_Click(object sender, EventArgs e)
        {
            InitBusTableActivity();
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

        void SetUpRecyclerView()
        {
            busesRecyclerView.SetLayoutManager(new LinearLayoutManager(busesRecyclerView.Context));
            busesAdapter = new BusAdapter(listOfBuses);
            busesAdapter.NotificationClick += BusesAdapter_NotificationClick;
            busesRecyclerView.SetAdapter(busesAdapter);
        }

        private void BusesAdapter_NotificationClick(object sender, BusAdapterClickEventArgs e)
        {
            var bus = listOfBuses[e.Position];

            string departurePlace;

            if (bus.DeparturePlace == "Dubki")
            {
                departurePlace = "Дубки";
            }
            else if(bus.DeparturePlace == "Odintsovo")
            {
                departurePlace = "Одинцово";
            }
            else
            {
                departurePlace = "Славянский бульвар";
            }

            Android.Support.V7.App.AlertDialog.Builder NotificationAlert = new Android.Support.V7.App.AlertDialog.Builder(this);

            NotificationAlert.SetMessage("Напомнить про автобус за 10 минут до его отправления");
            NotificationAlert.SetTitle("Поставить уведомление");

            NotificationAlert.SetPositiveButton("Ok", (alert, args) =>
            {
                StartAlarm(bus.DepartureTime, departurePlace);
            });

            NotificationAlert.SetNegativeButton("Cancel", (alert, args) =>
            {
                NotificationAlert.Dispose();
            });

            NotificationAlert.Show();
        }

        void SetUpSpinner()
        {
            routes = new Dictionary<string, List<Bus>>();
            routeList = new List<string>();

            routes.Add("Дубки-Одинцово", dubki_odintsovo);
            routes.Add("Одинцово-Дубки", odintsovo_dubki);
            routes.Add("Дубки-Славянский бульвар", dubki_slavyanski);
            routes.Add("Славянский бульвар-Дубки", slavyanski_dubki);

            routeList.Add("Дубки-Одинцово");
            routeList.Add("Одинцово-Дубки");
            routeList.Add("Дубки-Славянский бульвар");
            routeList.Add("Славянский бульвар-Дубки");

            spinnerAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, routeList);
            spinnerAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            routeSpinner.Adapter = spinnerAdapter;
            routeSpinner.ItemSelected += RouteSpinner_ItemSelected;
        }

        private void RouteSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (e.Position != -1)
            {
                selectedRoute = routeList[e.Position];
                Console.WriteLine(selectedRoute);
                listOfBuses = routes[selectedRoute];

                SetUpRecyclerView();
            }
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
                        /*&& RightDuration(dataLine[3])*/
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
                            JourneyDuration = 0,
                            Occupancy = dataLine[4],
                            Notify = bool.Parse(dataLine[5]),
                            Day = dataLine[6]
                        };

                        buses.Add(bus);
                    }
                };
            }
        }

        void SortBuses()
        {
            dubki_odintsovo = buses
                .Where(x => x.DeparturePlace == "Dubki")
                .Where(x => x.ArrivalPlace == "Odintsovo")
                .Where(x => x.DepartureTime >= DateTime.Now || x.DepartureTime <= DateTime.Parse("2:00"))
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            odintsovo_dubki = buses
                .Where(x => x.DeparturePlace == "Odintsovo")
                .Where(x => x.ArrivalPlace == "Dubki")
                .Where(x => x.DepartureTime >= DateTime.Now || x.DepartureTime <= DateTime.Parse("2:00"))
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            slavyanski_dubki = buses
                .Where(x => x.DeparturePlace == "Slavyanski")
                .Where(x => x.ArrivalPlace == "Dubki")
                .Where(x => x.DepartureTime >= DateTime.Now || x.DepartureTime <= DateTime.Parse("2:00"))
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            dubki_slavyanski = buses
                .Where(x => x.DeparturePlace == "Dubki")
                .Where(x => x.ArrivalPlace == "Slavyanski")
                .Where(x => x.DepartureTime >= DateTime.Now || x.DepartureTime <= DateTime.Parse("2:00"))
                //.OrderBy(x => x.DepartureTime)
                .ToList();

            if (DateTime.Now.DayOfWeek.ToString() == "Saturday" || DateTime.Now.DayOfWeek.ToString() == "Sunday")
            {
                dubki_odintsovo = dubki_odintsovo
                    .Where(x => x.Day == DateTime.Now.DayOfWeek.ToString())
                    .ToList();

                odintsovo_dubki = odintsovo_dubki
                    .Where(x => x.Day == DateTime.Now.DayOfWeek.ToString())
                    .ToList();

                slavyanski_dubki = slavyanski_dubki
                    .Where(x => x.Day == DateTime.Now.DayOfWeek.ToString())
                    .ToList();

                dubki_slavyanski = dubki_slavyanski
                    .Where(x => x.Day == DateTime.Now.DayOfWeek.ToString())
                    .ToList();
            }
            else
            {
                dubki_odintsovo = dubki_odintsovo
                    .Where(x => x.Day != "Sunday" && x.Day != "Saturday")
                    .ToList();

                odintsovo_dubki = odintsovo_dubki
                    .Where(x => x.Day != "Sunday" && x.Day != "Saturday")
                    .ToList();

                dubki_slavyanski = dubki_slavyanski
                    .Where(x => x.Day != "Sunday" && x.Day != "Saturday")
                    .ToList();

                slavyanski_dubki = slavyanski_dubki
                    .Where(x => x.Day != "Sunday" && x.Day != "Saturday")
                    .ToList();
            }

            dubki_odintsovo
                .Where(x => x.DepartureTime <= DateTime.Parse("2:00"))
                .OrderBy(x => x.DepartureTime);

            odintsovo_dubki
                .Where(x => x.DepartureTime <= DateTime.Parse("2:00"))
                .OrderBy(x => x.DepartureTime);

            dubki_slavyanski
                .Where(x => x.DepartureTime <= DateTime.Parse("2:00"))
                .OrderBy(x => x.DepartureTime);

            slavyanski_dubki
                .Where(x => x.DepartureTime <= DateTime.Parse("2:00"))
                .OrderBy(x => x.DepartureTime);

            listOfBuses = dubki_odintsovo;
        }

        static bool RightDirection(string departure, string arrival)
        {
            return (departure == "Dubki" && (arrival == "Slavyanski" || arrival == "Odintsovo"))
                || (departure == "Odintsovo" && (arrival == "Dubki"))
                || (departure == "Slavyanski" && arrival == "Dubki");
        }

        static bool RightDuration(string str)
        {
            int duration;
            return int.TryParse(str, out duration) && duration > 0;
        }

        void SetUpToolbars()
        {
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Ближайшие автобусы сегодня";
        }

        void StartAlarm(DateTime time, string departurePlace)
        {
            var alarmIntent = new Intent(this, typeof(AlarmReceiver));
            alarmIntent.PutExtra("title", "Напоминание");
            alarmIntent.PutExtra("message", $"Автобус от станции {departurePlace} отправится в {time.ToString("HH:mm")}.");

            var pending = PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);

            var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();

            if (time.Ticks - DateTime.Now.AddMinutes(10).Ticks < 0 && time.Ticks - DateTime.Now.Ticks >= 0)
            {
                alarmManager.Set(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime(), pending);
            }
            else if (time.Ticks - DateTime.Now.AddMinutes(10).Ticks > 0)
            {
                long miliseconds = (time.Ticks - DateTime.Now.AddMinutes(10).Ticks) / 10000;
                alarmManager.Set(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + miliseconds, pending);
                //alarmManager.Cancel(pending);
            }
        }

        void SetNotifications()
        {
            if (notificationBuses.Count != 0)
            {
                foreach (Bus bus in notificationBuses)
                {
                    StartAlarm(bus.DepartureTime, bus.DeparturePlace);
                }
            }
        }
    }
}