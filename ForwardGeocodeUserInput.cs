//-----------------------------------------------------------------------
// <copyright file="ForwardGeocodeUserInput.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// taken from the Mapbox Unity SDK
// translates a place name into coordinates
namespace Mapbox.Examples
{
    using Mapbox.Unity;
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using Mapbox.Geocoding;
    using Mapbox.Utils;

    [RequireComponent(typeof(InputField))]
    public class ForwardGeocodeUserInput : MonoBehaviour
    {
        InputField _inputField; // where user types a place name

        ForwardGeocodeResource _resource; // behind-the-scenes, autocompletes input

        Vector2d _coordinate; // long-lat associated with this place name
        public Vector2d Coordinate
        {
            get
            {
                return _coordinate;
            }
        }

        bool _hasResponse;
        public bool HasResponse
        {
            get
            {
                return _hasResponse;
            }
        }

        public ForwardGeocodeResponse Response { get; private set; }

        public event EventHandler<EventArgs> OnGeocoderResponse;

        void Awake()
        {
            _inputField = GetComponent<InputField>();
            _inputField.onEndEdit.AddListener(HandleUserInput);
            _resource = new ForwardGeocodeResource("");
        }

        // called when user searches for a location?
        void HandleUserInput(string searchString)
        {
            _hasResponse = false; // give it time to search?
            if (!string.IsNullOrEmpty(searchString))
            {
                _resource.Query = searchString;
                MapboxAccess.Instance.Geocoder.Geocode(_resource, HandleGeocoderResponse);
            }
        }

        // sets coordinates?
        void HandleGeocoderResponse(ForwardGeocodeResponse res)
        {
            _hasResponse = true;
            if (null != res.Features && res.Features.Count > 0)
            {
                _coordinate = res.Features[0].Center;
            }
            Response = res;
            if (OnGeocoderResponse != null)
            {
                OnGeocoderResponse(this, EventArgs.Empty);
            }
        }
    }
}
