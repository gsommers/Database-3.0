namespace Mapbox.Unity.Map
{
	using UnityEngine;
	using Mapbox.Map;
	using Mapbox.Unity.Utilities;
	using Mapbox.Utils;

    public class CameraBounds : AbstractTileProvider
	{
		[SerializeField]
		Camera _camera;

		[SerializeField]
		int _visibleBuffer; // how much to render beforehand

        [SerializeField]
        ZoomAndPan zoompan; // other script attached to the map

		[SerializeField]
		int _disposeBuffer; // where to remove tiles

        [SerializeField]
        float _updateInterval; // how often to try to generate tiles

        float _timeElapsed;

        // used to figure out center tile
		Plane _groundPlane;
		Ray _ray;
		float _hitDistance;
		Vector3 _viewportTarget;

		// used to determined whether to generate new tiles
		UnwrappedTileId _cachedTile;
		UnwrappedTileId _currentTile;

		Vector2d _currentLatitudeLongitude; // current center of map
        Vector2d _lastLatitudeLongitude; // center at start of drag

        // overrides AbstractTileProvider
        // sets up references
		internal override void OnInitialized()
		{
			_groundPlane = new Plane(Vector3.up, Mapbox.Unity.Constants.Math.Vector3Zero);
			_viewportTarget = new Vector3(0.5f, 0.5f, 0);
            _timeElapsed = 0;

		}

        // called when user presses down to start pan
        public void StartMove()
        {
            _lastLatitudeLongitude = _currentLatitudeLongitude;
        }

        // called when user releases mouse button to end pan
        public void EndMove()
        {
            // if position changed, shift center of map
            // otherwise user just clicked somewhere
            if (!_currentLatitudeLongitude.Equals(_lastLatitudeLongitude))
            {
                zoompan.ShiftCenter(_currentLatitudeLongitude);
            }
        }

        // updates tile rendering at regular intervals
		void Update()
		{
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed >= _updateInterval)
            {
                _timeElapsed = 0; // reset time
                // what the camera is looking at, center of screen
                _ray = _camera.ViewportPointToRay(_viewportTarget);
               
                if (_groundPlane.Raycast(_ray, out _hitDistance))
                {
                    // geographical coordinates that the camera is looking at
                    _currentLatitudeLongitude = _ray.GetPoint(_hitDistance).GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
                    // Debug.Log(_currentLatitudeLongitude);
                    // tile that the camera is looking at
                    _currentTile = TileCover.CoordinateToTileId(_currentLatitudeLongitude, _map.Zoom);

                    // camera has moved
                    if (!_currentTile.Equals(_cachedTile))
                    {
                        
                        // FIXME: this results in bugs at world boundaries! Does not cleanly wrap. Negative tileIds are bad.
                        // I just make the screen go dark, not a great fix but shouldn't occur anyway
                        for (int x = Mathd.Max(_currentTile.X - _visibleBuffer, 0); x <= (_currentTile.X + _visibleBuffer); x++)
                        {
                            for (int y = Mathd.Max(_currentTile.Y - _visibleBuffer, 0); y <= (_currentTile.Y + _visibleBuffer); y++)
                            {
                                AddTile(new UnwrappedTileId(_map.Zoom, x, y));
                            }
                        }
                        _cachedTile = _currentTile;
                        Cleanup(_currentTile);
                    }
                    
                }
            }
		}

        // remove old tiles when zoom level changes
        public void UpdateZoom(float zoom)
        {
            _map.Zoom = (int)zoom;
            var count = _activeTiles.Count;

            // iterate over all tiles, removing each one
            for (int i = count - 1; i >= 0; i--)
            {
                var tile = _activeTiles[i];
                RemoveTile(tile);
            }
        }

        // remove the tiles outside buffer region
        void Cleanup(UnwrappedTileId currentTile)
		{
			var count = _activeTiles.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				var tile = _activeTiles[i];
				bool dispose = false;
				dispose = tile.X > currentTile.X + _disposeBuffer || tile.X < _currentTile.X - _disposeBuffer;
				dispose = dispose || tile.Y > _currentTile.Y + _disposeBuffer || tile.Y < _currentTile.Y - _disposeBuffer;

				if (dispose)
				{
					RemoveTile(tile);
				}
			}
		}
	}
}
