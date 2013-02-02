using System;
using System.Web;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ColorduinoMaster
{
    public class Weather
    {
        private readonly Uri _requestUri;

		public string CurrentWeatherCondition { get; private set; }

        public Weather(string lat, string lon, string apiKey)
        {
			_requestUri = new Uri(string.Format("http://free.worldweatheronline.com/feed/weather.ashx?q={0},{1}&format=xml&num_of_days=2&key={2}", lat, lon, apiKey));
        }

        public void Refresh()
        {
			var client = new WebClient();
			var content = client.DownloadString(_requestUri);
			XDocument xdoc = XDocument.Parse(content);
			var weatherCode = xdoc.XPathSelectElement("/data/current_condition/weatherCode").Value;
			CurrentWeatherCondition = TranslateWeatherCode(weatherCode);
        }

		private string TranslateWeatherCode(string weatherCode)
		{
			switch (weatherCode) {
			case "113":
				return "sun";
			case "116":
			case "119":
			case "122":
				return "cloud";
			case "143":
			case "248":
			case "260":
				return "fog";
			case "176":
			case "263":
			case "266":
			case "293":
			case "296":
			case "299":
			case "302":
			case "305":
			case "308":
			case "353":
			case "356":
			case "359":
			case "362":
				return "rain";
			case "179":
			case "182":
			case "185":
			case "227":
			case "230":
			case "281":
			case "284":
			case "311":
			case "314":
			case "317":
			case "320":
			case "323":
			case "326":
			case "329":
			case "332":
			case "335":
			case "338":
			case "350":
			case "365":
			case "368":
			case "371":
			case "374":
			case "377":
				return "snow";
			case "200":
			case "386":
			case "389":
			case "392":
			case "395":
				return "thunderstorm";
			default:
				return null;
			}
		}
    }
}

