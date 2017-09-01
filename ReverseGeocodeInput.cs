// from the Mapbox Unity SDK
//-----------------------------------------------------------------------
// <copyright file="ReverseGeocodeUserInput.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Examples
{
    using Mapbox.Unity;
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Mapbox.Geocoding;
    using Mapbox.Utils;

    /// <summary>
    /// Peforms a reverse geocoder request (search by latitude, longitude) whenever the InputField on *this*
    /// gameObject is finished with an edit. 
    /// Expects input in the form of "latitude, longitude"
    /// </summary>

    public class ReverseGeocodeInput : MonoBehaviour
    {

        ReverseGeocodeResource _resource;

        Geocoder _geocoder;

        Vector2d _coordinate;

        bool _hasResponse;
        public bool HasResponse
        {
            get
            {
                return _hasResponse;
            }
        }

        public ReverseGeocodeResponse Response { get; private set; }

        public event EventHandler<EventArgs> OnGeocoderResponse;

        void Awake()
        {
            _resource = new ReverseGeocodeResource(_coordinate);
        }

        void Start()
        {
            _geocoder = MapboxAccess.Instance.Geocoder;
        }

        /// <summary>
        /// An edit was made to the InputField.
        /// Unity will send the string from _inputField.
        /// Make geocoder query.
        /// </summary>
        /// <param name="searchString">Search string.</param>
        public void InputCoords(Vector2d coords)
        {
            _coordinate = coords;
            _resource.Query = _coordinate;
            _geocoder.Geocode(_resource, HandleGeocoderResponse);
        }

        /// <summary>
        /// Handles the geocoder response by updating coordinates and notifying observers.
        /// </summary>
        /// <param name="res">Res.</param>
        void HandleGeocoderResponse(ReverseGeocodeResponse res)
        {
            _hasResponse = true;
            Response = res;
            if (OnGeocoderResponse != null)
            {
                OnGeocoderResponse(this, EventArgs.Empty);
            }
        }
    }
}