using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] string[] weathertypes;
    [SerializeField] Material[] weatherMats;
    [SerializeField] TMP_Text text;
    enum locations
    {
        Orlando,
        Paris,
        Naxos,
        Nara,
        Whistler,
    }

    string[] locStrings = { "Orlando, us", "Paris, fr", "Naxos, gr", "Nara, jp", "Whistler, ca"};
    locations location = locations.Whistler;

    private void Start()
    {
        NextCity();
    }

    public void NextCity()
    {
        if (location == locations.Whistler)
            location = locations.Orlando;
        else
            location++;
        text.text = "Current City: " + location.ToString();
        StartCoroutine(GetWeatherJSON(OnJSONDataLoaded));

    }
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

    public IEnumerator GetWeatherJSON(Action<string> callback)
    {
        return CallAPI("http://api.openweathermap.org/data/2.5/weather?q=" + locStrings[(int) location] + "&mode=json&appid=553960262dfabc41c7b7baa6e8023ed0", callback);
    }

    public void OnJSONDataLoaded(string data)
    {
        JSONReturn results = JsonUtility.FromJson<JSONReturn>(data);
        SetScene(results);
    }

    private void SetScene(JSONReturn json)
    {
        switch (json.weather[0].main) {
            case "Clear": // Day
                RenderSettings.skybox = weatherMats[0];
                break;

            case "Thunderstorm": // Rain
            case "Drizzle":
            case "Rain":
            case "Tornado":
                RenderSettings.skybox = weatherMats[1];
                break;

            case "Snow": // Snow
                RenderSettings.skybox = weatherMats[2];
                break;

            default: // Day Sunless
                RenderSettings.skybox = weatherMats[3];
                break;
    }
    }

    [Serializable] public class JSONReturn
    {
        public Coord coord;
        public Weather[] weather;
        public Main main;
        public int visibility;
        public Wind wind;
        public Rain rain;
        public Clouds clouds;
        public int dt, timezone, id, cod;
        public Sys sys;
        public string name;
    }

    [Serializable] public class Sys
    {
        public int type, id, sunrise, sunset;
        public string country { get; set; }
    }

    [Serializable] public class Coord
    {
        public float lon, lat;
    }

    [Serializable] public class Main
    {
        public float temp, feels_like, temp_min, temp_max, pressure, humidity, sea_level, grnd_level;
    }

    [Serializable] public class Wind
    {
        public float speed, deg, gust;
    }

    [Serializable] public class Rain
    {
        public float _1h;
    }

    [Serializable] public class Clouds
    {
        public int all;
    }

    [Serializable] public class Weather
    {
        public int id;
        public string main, description, icon;
    }

}
