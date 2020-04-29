﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Maps.Model;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace HSE_Transport1.Helpers
{
    public class MapHelper
    {
        public async Task<string> GetDirectionJsonAsync(LatLng location, LatLng destLocation, string mapkey)
        {
            // Origin of route
            string str_origin = "origin=" + location.Latitude.ToString() + "," + location.Longitude.ToString();

            // Destination of route
            string str_destination = "destination=" + destLocation.Latitude.ToString() + "," + destLocation.Longitude.ToString();

            // Mode
            string mode = "mode=driving";

            string parameters = str_origin + "&" + str_destination + "&" + mode + "&key=" + mapkey;

            // Output
            string output = "json";

            string url = "https://maps.googleapis.com/maps/api/directions/" + output + "?" + parameters;

            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string jsonString = await client.GetStringAsync(url);

            return jsonString;
        }
    }
}