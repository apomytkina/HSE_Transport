using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Android.Support.V7.Widget;
using HSE_Transport1.Adapters;
using System.Collections.Generic;
using FR.Ganfra.Materialspinner;
using System;
using System.IO;
using System.Linq;
using HSE_Transport1.Activities;
using System.Threading.Tasks;
using Android.Gms.Maps.Model;
using HSE_Transport1.Helpers;
using Newtonsoft.Json;
using static Android.Manifest;

namespace HSE_Transport1
{
    [Activity(Label = "Ближайшие автобусы", Theme = "@style/AppTheme", MainLauncher = false, Icon = "@mipmap/icon_launcher")]
    public class MainActivity : AppCompatActivity
    {
        static Dictionary<string, LatLng> latlngPairs;

        LatLng dubkiPosition;
        LatLng odintosovoPosition;
        LatLng slavyanskiPosition;

        readonly string[] permissionsGroup = { Permission.Internet };

        TextView durationTextView;

        MapHelper mapHepler = new MapHelper();

        ImageButton scheduleButton;
        ImageButton busesButton;

        RelativeLayout busesLayout;
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

            SetContentView(Resource.Layout.activity_main);

            slavyanskiPosition = new LatLng(55.728246, 37.473204);
            odintosovoPosition = new LatLng(55.672067, 37.279666);
            dubkiPosition = new LatLng(55.660864, 37.226496);

            latlngPairs = new Dictionary<string, LatLng>();

            latlngPairs.Add("Dubki", dubkiPosition);
            latlngPairs.Add("Odintsovo", odintosovoPosition);
            latlngPairs.Add("Slavyanski", slavyanskiPosition);

            RequestPermissions(permissionsGroup, 0);

            durationTextView = (TextView)FindViewById(Resource.Id.durationTextView);

            routeSpinner = (MaterialSpinner)FindViewById(Resource.Id.routeSpinner);
            busesRecyclerView = (RecyclerView)FindViewById(Resource.Id.busesRecyclerView);
            toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.busesToolbar);

            scheduleLayout = (RelativeLayout)FindViewById(Resource.Id.schedule_layout);
            busesLayout = (RelativeLayout)FindViewById(Resource.Id.buses_layout);

            scheduleButton = (ImageButton)FindViewById(Resource.Id.scheduleButton);
            busesButton = (ImageButton)FindViewById(Resource.Id.busesButton);

            scheduleButton.Click += Schedule_Click;
            busesButton.Click += Buses_Click;

            busesLayout.Click += Buses_Click;
            scheduleLayout.Click += Schedule_Click;

            ParseData();
            SortBusesByDay();
            SortBusesByTime();

            SetUpToolbars();
            SetUpSpinner();
            SetUpRecyclerViewAsync();
        }

        /// <summary>
        /// Method that starts MainActivity when clicking
        /// on the busesButton or busesLayout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Buses_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(BusTableActivity));
        }

        /// <summary>
        /// Method that starts BusTableActivity when clicking 
        /// on scheduleButton or scheduleLayout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Schedule_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MainActivity));
        }

        /// <summary>
        /// Method that starts RecyclerView of buses
        /// </summary>
        /// <returns></returns>
        async Task SetUpRecyclerViewAsync()
        {
            busesRecyclerView.SetLayoutManager(new LinearLayoutManager(busesRecyclerView.Context));
            busesAdapter = new BusAdapter(listOfBuses);
            busesRecyclerView.SetAdapter(busesAdapter);
            await GetDirectionAsync(latlngPairs[listOfBuses[0].DeparturePlace], latlngPairs[listOfBuses[0].ArrivalPlace]);
        }

        /// <summary>
        /// Method that sets up route spinner
        /// </summary>
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

        /// <summary>
        /// Method that invokes route spinner when choosing route 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RouteSpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (e.Position != -1)
            {
                selectedRoute = routeList[e.Position];
                Console.WriteLine(selectedRoute);
                listOfBuses = routes[selectedRoute];

                SetUpRecyclerViewAsync();
            }
        }

        /// <summary>
        /// Method that parses data about buses from the file "bus_info.csv"
        /// </summary>
        void ParseData()
        {
            buses = new List<Bus>();

            using (StreamReader sr = new StreamReader(Assets.Open("bus_info.csv")))
            {
                try
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
                                Occupancy = dataLine[4],
                                Notify = bool.Parse(dataLine[5]),
                                Day = dataLine[6]
                            };

                            buses.Add(bus);
                        }
                    };
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// Method that sorts buses by days of week
        /// </summary>
        void SortBusesByDay()
        {
            dubki_odintsovo = buses
                .Where(x => x.DeparturePlace == "Dubki")
                .Where(x => x.ArrivalPlace == "Odintsovo")
                .Where(x => x.DepartureTime >= DateTime.Now || x.DepartureTime <= DateTime.Parse("2:00"))
                .ToList();

            odintsovo_dubki = buses
                .Where(x => x.DeparturePlace == "Odintsovo")
                .Where(x => x.ArrivalPlace == "Dubki")
                .Where(x => x.DepartureTime >= DateTime.Now || x.DepartureTime <= DateTime.Parse("2:00"))
                .ToList();

            slavyanski_dubki = buses
                .Where(x => x.DeparturePlace == "Slavyanski")
                .Where(x => x.ArrivalPlace == "Dubki")
                .Where(x => x.DepartureTime >= DateTime.Now || x.DepartureTime <= DateTime.Parse("2:00"))
                .ToList();

            dubki_slavyanski = buses
                .Where(x => x.DeparturePlace == "Dubki")
                .Where(x => x.ArrivalPlace == "Slavyanski")
                .Where(x => x.DepartureTime >= DateTime.Now || x.DepartureTime <= DateTime.Parse("2:00"))
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

            listOfBuses = dubki_odintsovo;
        }

        /// <summary>
        /// Method that sorts buses by departureTime
        /// </summary>
        void SortBusesByTime()
        {
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

            dubki_odintsovo
                .Where(x => x.DepartureTime >= DateTime.Parse("2:00"))
                .OrderBy(x => x.DepartureTime);

            odintsovo_dubki
                .Where(x => x.DepartureTime >= DateTime.Parse("2:00"))
                .OrderBy(x => x.DepartureTime);

            dubki_slavyanski
                .Where(x => x.DepartureTime >= DateTime.Parse("2:00"))
                .OrderBy(x => x.DepartureTime);

            slavyanski_dubki
                .Where(x => x.DepartureTime >= DateTime.Parse("2:00"))
                .OrderBy(x => x.DepartureTime);
        }

        /// <summary>
        /// Method that checks wheather a bus has a right direction
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
        /// Method that sets up toolbar
        /// </summary>
        void SetUpToolbars()
        {
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = "Ближайшие автобусы сегодня";
        }

        /// <summary>
        /// Method that returns journey time depending on
        /// start location and destination location
        /// </summary>
        /// <param name="location"></param>
        /// <param name="destLocation"></param>
        /// <returns></returns>
        async Task GetDirectionAsync(LatLng location, LatLng destLocation)
        {
            string key = Resources.GetString(Resource.String.mapkey);

            try
            {
                string directionJson = await mapHepler.GetDirectionJsonAsync(location, destLocation, key);
                var directionData = JsonConvert.DeserializeObject<DirectionParser>(directionJson);
                double durationString = directionData.routes[0].legs[0].duration.value / 60;
                durationTextView.Text = "Время в пути по данному маршруту: " + durationString + " мин";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}