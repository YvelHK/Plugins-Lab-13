using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using System.IO;

public class WeatherManager : MonoBehaviour
{
    XmlSerializer serializer = new XmlSerializer(typeof(Current));
    enum locations
    {
        Orlando,
        Paris,
        Naxos,
        Nara,
        Vancouver,
    }

    string[] locStrings = { "Orlando, us", "Paris, fr", "Naxos, gr", "Nara, jp", "Whistler, ca"};
    [SerializeField] locations location;

    private IEnumerator CallAPI(string url, Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogError($"network problem: {request.error}");
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"response error: {request.responseCode}");
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }
    }

    public IEnumerator GetWeatherXML(Action<string> callback)
    {
        return CallAPI("http://api.openweathermap.org/data/2.5/weather?q=" + locStrings[(int) location] + "&mode=xml&appid=553960262dfabc41c7b7baa6e8023ed0", callback);
    }

    private void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            location = (locations) i;
            StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
        }
    }

    public void OnXMLDataLoaded(string data)
    {
        Current current;
        using (TextReader reader = new StringReader(data))
        {
            current = (Current)serializer.Deserialize(reader);
        }
        Debug.Log(current.clouds.name);
    }

    [XmlRoot(ElementName = "current")] public class Current
    {
        [XmlElement("city")] public City city;
        [XmlElement("temperature")] public Temperature temperature;
        [XmlElement("feels_like")] public Measurement feelsLike;
        [XmlElement("humidity")] public Measurement humidity;
        [XmlElement("pressure")] public Measurement pressure;
        [XmlElement("wind")] public Wind wind;
        [XmlElement("clouds")] public Clouds clouds;
        [XmlElement("visibility")] public SingleValue visibility;
        [XmlElement("precipitation")] public Precipitation precipitation;
        [XmlElement("weather")] public Weather weather;
        [XmlElement("lastupdate")] public SingleValue lastUpdate;
    }

    public class City
    {
        [XmlElement("id")]public int id;
        [XmlElement("name")] public string name;
        [XmlElement("coord")] public Coord coord;
        [XmlElement("country")] public string country;
        [XmlElement("timezone")] public int timezone;
        [XmlElement("sun")] public Sun sun;
    }

    public class Coord
    {
        [XmlElement("lon")] public int lon;
        [XmlElement("lat")] public int lat;
    }

    public class Sun
    {
        [XmlElement("rise")] public string rise;
        [XmlElement("set")] public string set;
    }

    public class Temperature
    {
        [XmlElement("value")] public float value;
        [XmlElement("min")] public float min;
        [XmlElement("max")] public float max;
        [XmlElement("unit")] public string unit;
    }

    public class Measurement // Used for FeelsLike, Humidity, and Pressure
    {
        [XmlElement("value")] public float value;
        [XmlElement("unit")] public string unit;
    }

    public class Wind
    {
        [XmlElement("speed")] public WindSpeed speed;
        [XmlElement("gusts")] public SingleValue gusts;
        [XmlElement("direction")] public WindDirection direction;
    }

    public class WindSpeed
    {
        [XmlElement("value")] public float value;
        [XmlElement("unit")] public string unit;
        [XmlElement("name")] public string name;
    }
    public class WindDirection
    {
        [XmlElement("value")] public float value;
        [XmlElement("code")] public string code;
        [XmlElement("name")] public string name;
    }
    public class Clouds
    {
        [XmlElement("value")] public float value;
        [XmlElement("name")] public string name;
    }
    public class Precipitation
    {
        [XmlElement("value")] public float value;
        [XmlElement("mode")] public string mode;
        [XmlElement("unit")] public string unit;
    }
    public class Weather
    {
        [XmlElement("number")] public int number;
        [XmlElement("value")] public string value;
        [XmlElement("icon")] public string icon;
    }
    public class SingleValue // Used for gusts and lastupdate
    {
        [XmlElement("value")] public string value;
    }

}
