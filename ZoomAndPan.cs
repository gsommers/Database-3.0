namespace Mapbox.Unity.Map
{
    using System;
    using Mapbox.Unity.MeshGeneration;
    using Mapbox.Unity.Utilities;
    using Utils;
    using UnityEngine;
    using Mapbox.Map;
    using UnityEngine.UI;
    using Mapbox.Examples;
    using TMPro;

    // modified from AbstractMap
    public class ZoomAndPan : MonoBehaviour, IMap
    {
        [SerializeField]
        ReverseGeocodeInput rev; // used to convert from coordinates to place name

        [SerializeField]
        TextMeshProUGUI centerText; // displays current location

        [Geocode]
        [SerializeField]
        string _latitudeLongitudeString; // search location in longlat

        [SerializeField]
        Raycasting rayCastScript; // handles user clicks

        int _zoom; // zoom level (level of detail) of this map
        public int Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = value;
            }
        }

        public Slider zoomSlide; // shows the zoom level on a sliding scale

        // not exactly sure what this does
        [SerializeField]
        Transform _root;
        public Transform Root
        {
            get
            {
                return _root;
            }
        }

        [SerializeField]
        CameraBounds _tileProvider; // generates tiles

        [SerializeField]
        MapVisualizer _mapVisualizer; // renders the actual map style

        [SerializeField]
        float _unityTileSize;

        MapboxAccess _fileSouce;

        Vector2d _mapCenterLatitudeLongitude; // current center of map
        public Vector2d CenterLatitudeLongitude
        {
            get
            {
                return _mapCenterLatitudeLongitude;
            }
            set
            {
                _latitudeLongitudeString = string.Format("{0}, {1}", value.x, value.y);
                _mapCenterLatitudeLongitude = value;
                rev.InputCoords(_mapCenterLatitudeLongitude);
            }
        }

        Vector2d _mapCenterMercator; // center in Mercator projection (weird)
        public Vector2d CenterMercator
        {
            get
            {
                return _mapCenterMercator;
            }
        }

        // scale of map to real world size
        float _worldRelativeScale;
        public float WorldRelativeScale
        {
            get
            {
                return _worldRelativeScale;
            }
        }

        public event Action OnInitialized = delegate { };


        [SerializeField]
        Examples.ForwardGeocodeUserInput _searchLocation; // entered by user in "search location" field

        [SerializeField]
        Button zoomIn, zoomOut; // buttons for adjusting zoom level

        protected virtual void Awake()
        {
            // set up search location input system
            if (_searchLocation != null)
                _searchLocation.OnGeocoderResponse += SearchLocation_OnGeocoderResponse;

            // set up tile generation
            _fileSouce = MapboxAccess.Instance; // what's this?
            _tileProvider.OnTileAdded += TileProvider_OnTileAdded;
            _tileProvider.OnTileRemoved += TileProvider_OnTileRemoved;
            if (!_root)
            {
                _root = transform;
            }
        }

        // do I need this?
        void OnDestroy()
        {
            if (_searchLocation != null)
            {
                _searchLocation.OnGeocoderResponse -= SearchLocation_OnGeocoderResponse;
            }
        }

        /// <summary>
        /// New search location has become available, begin a new _map query.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void SearchLocation_OnGeocoderResponse(object sender, EventArgs e)
        {
            // you're already there
            if (_mapCenterLatitudeLongitude.Equals(_searchLocation.Coordinate))
            {
                return;
            }
            CenterLatitudeLongitude = _searchLocation.Coordinate;

            // kind of a hack, don't want the exact same zoom level
            zoomSlide.value = (zoomSlide.value == 12) ? 13 : 12;

            // able to zoom in and out
            zoomOut.interactable = true;
            zoomIn.interactable = true;
            SlideZoom();
        }

        /*protected virtual void OnDestroy()
        {
            if (_tileProvider != null)
            {
                _tileProvider.OnTileAdded -= TileProvider_OnTileAdded;
                _tileProvider.OnTileRemoved -= TileProvider_OnTileRemoved;
            }

            _mapVisualizer.Destroy();
        }*/

        // This is the part that is abstract?
        protected virtual void Start()
        {
            // initialize map
            var latLonSplit = _latitudeLongitudeString.Split(',');
            _mapCenterLatitudeLongitude = new Vector2d(double.Parse(latLonSplit[0]), double.Parse(latLonSplit[1]));
            Setup((int)zoomSlide.minValue);
        }

        void Setup(int zoom)
        {
            Zoom = zoom;
            // set center
            var referenceTileRect = Conversions.TileBounds(TileCover.CoordinateToTileId(_mapCenterLatitudeLongitude, _zoom));
            _mapCenterMercator = referenceTileRect.Center;

            // set scale
            _worldRelativeScale = (float)(_unityTileSize / referenceTileRect.Size.x);
            Root.localScale = Vector3.one * _worldRelativeScale;

            // where the tiles come from
            _mapVisualizer.Initialize(this, _fileSouce);
            _tileProvider.Initialize(this);

            // this is a delegate
            OnInitialized();
        }

        // called when user clicks + button
        public void ZoomIn()
        {
            zoomSlide.value++;
            zoomOut.interactable = true;
            SlideZoom();
            if (zoomSlide.value == zoomSlide.maxValue) // fully zoomed, can't zoom anymore
                zoomIn.interactable = false;
        }

        // called when user clicks - button
        public void ZoomOut()
        {
            zoomSlide.value--;
            zoomIn.interactable = true;
            SlideZoom();
            if (zoomSlide.value == zoomSlide.minValue) // fully zoomed out, can't zoom out anymore
                zoomOut.interactable = false;

        }

        // reinitialize map when zoom level changes
        public void SlideZoom()
        {
            // give map time to load before giving data
            rayCastScript.DelayLoad();
            
            _tileProvider.UpdateZoom(zoomSlide.value);
            // Debug.Log(_mapCenterLatitudeLongitude + " zoom");

            Setup((int)zoomSlide.value);
            // Debug.Log(_mapCenterLatitudeLongitude + " setup");

        }

        // shift center of map when user pans
        public void ShiftCenter(Vector2d shift)
        {
            CenterLatitudeLongitude = shift;
        }

        // add tile to map
        void TileProvider_OnTileAdded(UnwrappedTileId tileId)
        {
            _mapVisualizer.LoadTile(tileId);
        }

        // remove tile from map
        void TileProvider_OnTileRemoved(UnwrappedTileId tileId)
        {
            _mapVisualizer.DisposeTile(tileId);
        }
    }
}