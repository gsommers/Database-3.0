using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// not just for trees, also parses data for stone
public class TreeDictionary : MonoBehaviour
{
    // holds info about trees, keyed by their scientific names
    private SortedDictionary<string, string> species;
    public TreeDictionaryCreator treeDictionary;
    private TextMeshProUGUI treeInfo; // displays text held in species

    // holds info about stone types, keyed by common names
    private SortedDictionary<string, string> rocks;
    public QuarryDictionaryCreator quarryDictionary;

    // holds info about stone types in Corsi's collection
    private SortedDictionary<string, string> corsi;
    public QuarryDictionaryCreator corsiDictionary;

    // panel for displaying text
    public GameObject treeDisplay;
    private Window treeWindow;

    // display shapefile
    public GameObject treeMapPanel;
    public Image treeMap;
    private AssetBundle mapBundle;

    // panel for displaying info from Corsi's collection
    public GameObject corsiPanel;
    private Window corsiWindow;
    private TextMeshProUGUI corsiInfo;

    // texture of this material
    [SerializeField] // so I can see it in the inspector
    Texture2D texture;
    private AssetBundle woodBundle; // for wood
    private AssetBundle stoneBundle; // for stone
    private TextAsset textureAsset; // loaded from asset bundle

    // for parsing sample number
    private int OFFSET = "Sample Number::: ".Length;

    private void Awake()
    {
        // assign dictionaries from scriptable objects
        species = treeDictionary.materials;
        rocks = quarryDictionary.materials;
        corsi = corsiDictionary.materials;

        // text and window script of info panel
        treeInfo = treeDisplay.GetComponentInChildren<TextMeshProUGUI>();
        treeWindow = treeDisplay.GetComponent<Window>();

        // text and window script of corsi panel
        corsiInfo = corsiPanel.GetComponentInChildren<TextMeshProUGUI>();
        corsiWindow = corsiPanel.GetComponent<Window>();

        // load bundle that holds map images
        LoadBundle("tree maps");

        // load bundle that holds wood textures
        LoadBundle("wood textures");

        // load bundle that holds stone textures
        LoadBundle("stone textures");

        // default texture
        texture = new Texture2D(2, 2);
    }

    // loads an asset bundle from streaming assets
    private void LoadBundle(String path)
    {
        switch(path)
        {
            case "tree maps": mapBundle = AssetBundle.LoadFromFile(
                Path.Combine(Application.streamingAssetsPath, path));
                break;
            case "wood textures": woodBundle = AssetBundle.LoadFromFile(
                Path.Combine(Application.streamingAssetsPath, path));
                break;
            case "stone textures": stoneBundle = AssetBundle.LoadFromFile(
                Path.Combine(Application.streamingAssetsPath, path));
                break;
        }
    }

    // called when user clicks on a tree species name
    public void Read(string name, bool wood)
    {
        string lowerCaseName = name.ToLowerInvariant();
        string value;
        if (wood) // looking for a tree species
            species.TryGetValue(lowerCaseName, out value); // try to get this species from dictionary
        else // looking for a rock type
            rocks.TryGetValue(lowerCaseName, out value);
 
        if (value == null) // no species/stone with this name is in dictionary (shouldn't happen)
            treeInfo.text = "Material not in database";
        else // found species/rock type!
        {
            // print title and properties
            SetInfoText(treeInfo, name, value);

            if (wood) // user clicked on a type of wood
            {
                // map
                treeMapPanel.SetActive(true);
                treeMap.sprite = mapBundle.LoadAsset<Sprite>(name);
                corsiWindow.SetVisibility(false); // make sure Corsi panel doesdn't cover up map

                // texture - this is just a test
                textureAsset = woodBundle.LoadAsset<TextAsset>(name);
            }
            else // user clicked on stone
            {
                textureAsset = stoneBundle.LoadAsset<TextAsset>(name); // only have English stones right now

                // display panel for Corsi's Collection
                string corsiText;
                corsi.TryGetValue(lowerCaseName, out corsiText);

                if (corsiText == null) // no rock with this name in Corsi's collection
                    corsiInfo.text = "Rock type not in Corsi's collection";
                else // found rock!
                {
                    SetInfoText(corsiInfo, name, corsiText);

                    // find sample number
                    int index = corsiText.IndexOf("Sample Number:::");
                    if (index >= 0) // should always be true
                    {
                        int endIndex = corsiText.Substring(index + OFFSET).IndexOf(";;;"); // should always be >= 0
                        String sampleNumber = corsiText.Substring(index + OFFSET, endIndex);

                        // load sample image into texture
                        textureAsset = stoneBundle.LoadAsset<TextAsset>(sampleNumber);
                    }
                }

                // show Corsi panel, not tree map
                corsiWindow.SetVisibility(true);
                treeMapPanel.SetActive(false);
            }

            if (textureAsset != null)
                texture.LoadImage(textureAsset.bytes);
            //test: treeMap.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        treeWindow.SetVisibility(true); // show the information panel
    }

    // lists properties in a formatted list
    private void SetInfoText(TextMeshProUGUI textDisplay, string name, string info)
    {
        // title
        textDisplay.text = "<color=#336666FF><size=130%><smallcaps><b>" + name + "</size></smallcaps></color></b>\n";

        // split up properties
        string[] properties = info.Split(new string[] { ";;;" }, System.StringSplitOptions.None);
        foreach (string prop in properties)
        {
            // split between property type and value
            string[] split = prop.Split(new string[] { "::" }, 2, System.StringSplitOptions.None);
            textDisplay.text += "<b><size=120%>" + split[0] + "</b></size>";
            if (split.Length > 1) // there actually is a value
                textDisplay.text += split[1] + "\n";
            else
                textDisplay.text += "\n";
        }
    }
}


