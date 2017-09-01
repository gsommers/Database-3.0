using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Components;
using Mapbox.Unity.Utilities;
using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// detects when user clicks on or around a tileset
// doesn't always get the shortest distance, though - spherecast not very accurate

public class Raycasting : MonoBehaviour
{
    public Slider radiusSlide; // determines radius of search area

    public Camera cam; // renders the whole screen

    private Ray ray; // imaginary line directed from mouse position

    public TextMeshProUGUI materialsList; // displays which materials are
    public GameObject materialsPanel; // at this location
    public FlipPage pageSetter; // controls what page of the list is shown
    public GameObject infoPanel; // profile of selected species/stone
    public GameObject mapPanel; // map of selected species
    public GameObject corsiPanel; // profile of selected stone in Corsi's collection

    // what to search for
    public Toggle timber;
    public Toggle stone;

    // handles double clicks
    private bool clicked = false;
    private float firstClickTime;
    public float interval; // how long to wait between clicks

    public float loadTime; // how long to wait for tilesets
    private WaitForSeconds delay;
    private bool loading = false; // am I waiting for tilesets to load after zooming?
    private bool foundTilesets = true; // did I find tilesets, even though map was loading?

    public ZoomAndPan map; // interactive map of world

    private Vector2d click; // where user clicked, in meters
    private SortedDictionary<int, List<string>> display;
    private Dictionary<string, double> found;
    Vector2d point; // where the collision is

    private Date start, end; // used to filter out stones

    private void Start()
    {
        display = new SortedDictionary<int, List<string>>(); // sorts species names by categories of distance from click
        found = new Dictionary<string, double>(); // contains the name of and distance to all of the species within range
        delay = new WaitForSeconds(loadTime);
    }

    // if user just zoomed in/out, they can't immediately look for species
    IEnumerator DelayForLoad()
    {
        loading = true;
        yield return delay;
        loading = false;

        // close window displaying loading message - unless you've already found tilesets
        materialsPanel.SetActive(foundTilesets);
    }

    // called when user zooms
    public void DelayLoad()
    {
        StartCoroutine(DelayForLoad());
    }

    private void Update()
    {
        // only if user double clicks on map
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            // first click
            if (!clicked)
            {
                clicked = true;
                firstClickTime = Time.time;
            }

            // second click
            else
            {
                clicked = false; // reset clicks

                // show materials panel, nothing else
                materialsPanel.SetActive(true);
                infoPanel.SetActive(false);
                mapPanel.SetActive(false);
                corsiPanel.SetActive(false);

                // detects if mouse clicks intersects with any tileset
                ray = cam.ScreenPointToRay(Input.mousePosition);
                // Debug.Log(Input.mousePosition);

                Vector2d clickCoords = (cam.ScreenToWorldPoint(Input.mousePosition) - new Vector3(0, cam.transform.position.y, 0))
                       .GetGeoPosition(map.CenterMercator, map.WorldRelativeScale); // geographic location of point click (in long lat)
                // Debug.Log(clickCoords.ToString());
                click = Conversions.LatLonToMeters(clickCoords); // in meters

                // assuming no materials selected
                materialsList.text = "\n<size=25pt>No materials selected. Please select stone and/or timber to retrieve data.</size>";

                // layermask - which materials to display? 
                if (timber.isOn || stone.isOn)
                {
                    materialsList.text = ""; // reset text
                    if (timber.isOn)
                        ShowList(8); // timber layer
                    if (stone.isOn)
                    {
                        ShowList(11); // quarry layer
                    }
                }

                pageSetter.SetPages(); // initialize page formatting of materials panel

            }
        }

        // user didn't click, time for double click has passed
        else if (clicked && Time.time - firstClickTime > interval)
        {
            clicked = false;
        }
    }

    // called when user submits valid filter dates
    // used to filter out stones that were quarried during different time periods
    public void FilterDates(Date startYear, Date endYear)
    {
        start = startYear;
        end = endYear;
        // Debug.Log("Dates set");
    }

    /* 
     * checks whether this material was used during the filter dates
     * @return true if recorded use falls outside these dates
     */
    private bool OutsideDates(FeatureBehaviour fb)
    {
        return (fb.endDate != null && start != null && start.CompareTo(fb.endDate) > 0) // material use predates filtered era
            || (fb.startDate != null && end != null && end.CompareTo(fb.startDate) < 0); // material use began after filtered era
    }

    // does a spherecast to detect materials in surrounding regions and displays them on panel
    private void ShowList(int layerMask)
    {
        RaycastHit[] hits = Physics.SphereCastAll(ray, radiusSlide.value * 1000 * map.WorldRelativeScale, 120, 1 << layerMask);

        //reset dictionaries for new search
        found.Clear();
        display.Clear();

        // no tilesets found
        if (hits.Length == 0)
        {
            if (loading) // something might be here, it just hasn't loaded
            {
                foundTilesets = false;
                materialsList.text = "\n<size=25pt>Map is loading. Please click again when this panel closes.</size>";
            }

            else // nothing is here
                SetMissingText(LayerMask.LayerToName(layerMask));
            // Debug.Log("Nothing here...");
        }

        else // found tilesets!
        {
            foundTilesets = true;
            double d;
            foreach (RaycastHit hit in hits)
            {
                // geographic location of collision with shapefile (in meters)
                point = Conversions.LatLonToMeters(hit.point.GetGeoPosition(map.CenterMercator, map.WorldRelativeScale));

                d = Vector2d.Distance(point / 1000, click / 1000); // distance from click location to collision in km

                // within range (I think it should always be, but just in case, here's another check)
                if (d < radiusSlide.value)
                {
                    // GetComponent is expensive. Better way?
                    FeatureBehaviour fb = hit.transform.gameObject.GetComponent<FeatureBehaviour>();
                    string name = fb.DataString;
                    if (name == null)
                        Debug.Log(hit.transform.name);
                    // Debug.Log(name + ": " + d);
                    if (layerMask == 11) // quarry
                    {
                        if (OutsideDates(fb)) // don't include this material in the list
                        {
                            continue;
                        }
                    }

                    // already found a collision with this shapefile; keep the closer one
                    if (found.ContainsKey(name))
                    {
                        double distance = found[name];
                        if (d < distance)
                        {
                            found.Remove(name);
                            found.Add(name, d);
                        }
                    }

                    else
                        found.Add(name, d);
                }
            }

            if (found.Count == 0) // no tilesets within range
            {
                SetMissingText(LayerMask.LayerToName(layerMask));
            }

            else
            {
                // go through kv pairs and assign to categories
                foreach (KeyValuePair<string, double> pair in found)
                {
                    int category = Category(pair.Value);

                    // other tilesets are in the same distance range
                    if (display.ContainsKey(category))
                    {
                        display[category].Add(pair.Key);
                    }

                    else // this is the first hit at this distance
                    {
                        List<string> thisList = new List<string>();
                        thisList.Add(pair.Key);
                        display.Add(category, thisList);
                    }
                }

                // display by distance categories
                StringBuilder text = new StringBuilder();
                foreach (KeyValuePair<int, List<string>> pair in display)
                {
                    // category label
                    text.Append(string.Format("\n<i>Within a {0}-km radius:</i>\n", pair.Key));

                    switch (layerMask) // text differs depending on type of material
                    {
                        case 8: // species list for wood
                        foreach (string species in pair.Value)
                        {

                            text.Append("<size=90%><link=\"id_tree\"><color=\"blue\"><u>" + species + "</color></u></link></size>\n");
                        }
                            break;

                        case 11: // stone quarries
                        foreach (string type in pair.Value)
                            {
                                text.Append("<size=90%><link=\"id_quarry\"><color=\"blue\"><u>" + type + "</color></u></link></size>\n");
                            }
                            break;
                    }

                }
                SetListText(layerMask, text.ToString()); // display text
            }

        }
    }

    // sets the text listing materials found in region
    private void SetListText(int layerMask, string list)
    {
        if (layerMask == 8) // timber
            materialsList.text += "\n<size=120%><b>Click on a species of wood for more info.</b></size> \n" + list;
        else // stone
            materialsList.text += "\n<size=120%><b>Click on one of the links below to learn about stone in nearby regions.</b></size> \n" + list;
    }

    // this displays when there are no species in the region
    private void SetMissingText(string layerName)
    {
        materialsList.text += String.Format("<size=25pt>\nNo {0} found here. Click somewhere else or broaden your search.</size>\n", layerName);
    }

    // groups distances into intervals for display
    // double gets rounded up to nearest interval mark
    private int Category(double distance)
    {
        if (distance < 5)
            return 5;
        else if (distance < 50)
            return 50;
        else if (distance < 100)
            return 100;
        else if (distance < 200)
            return 200;
        else if (distance < 300)
            return 300;
        else if (distance < 400)
            return 400;
        else
            return 500;
    }

}