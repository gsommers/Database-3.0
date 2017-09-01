// adapted from an example in the Mapbox Unity SDK
namespace Mapbox.Examples.Playground
{
    using Mapbox.Json;
    using Mapbox.Utils.JsonConverters;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    // display info about the current center location of map
    public class ReverseGeo : MonoBehaviour
    {
        [SerializeField]
        ReverseGeocodeInput _searchLocation;

        [SerializeField]
        TextMeshProUGUI _resultsText;

        [SerializeField]
        string infoText;
        void Awake()
        {
            _searchLocation.OnGeocoderResponse += SearchLocation_OnGeocoderResponse;
        }

        void OnDestroy()
        {
            if (_searchLocation != null)
            {
                _searchLocation.OnGeocoderResponse -= SearchLocation_OnGeocoderResponse;
            }
        }

        void SearchLocation_OnGeocoderResponse(object sender, System.EventArgs e)
        {
            string results = JsonConvert.SerializeObject(_searchLocation.Response, Formatting.Indented, JsonConverters.Converters);
            int index = results.IndexOf("place_name\": \""); // I only want the place name
            if (index >= 0)
            {
                // index of end of place name treating start of name as index 0
                int endIndex = results.Substring(index + 14).IndexOf("\"");
                if (endIndex >= 0)
                    _resultsText.text = infoText + results.Substring(index + 14, endIndex);
            }
        }

    }
}