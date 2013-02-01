using System;
using System.Web;

namespace ColorduinoMaster
{
    public class Weather
    {
        private readonly Uri _requestUri;

        public Weather(string lat, string lon, string apiKey)
        {
			_requestUri = new Uri(string.Format("http://free.worldweatheronline.com/feed/weather.ashx?q={0},{1}&format=csv&num_of_days=2&key={2}", lat, lon, apiKey));
        }

        public void Refresh()
        {
//            new System.Web.HttpRequest(
        }
    }
}

