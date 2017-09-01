using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

// creates a dictionary of quarried stones
[CreateAssetMenu(menuName = "QuarryDictionary")]
public class QuarryDictionaryCreator : AbstractDictionaryCreator
{
    public bool corsi; // formatting for corsi dictionary is slightly different
    private const int NAME_OFFSET = 14; // length of "Stone Name::: "

    // initialize the dictionary of stones/quarries
    public override void OnEnable()
    {
        string[] lines = types.text.Split('\n');

        materials = new SortedDictionary<string, string>();
        foreach (string line in lines)
        {
            if (line.Length > 0) // this line contains data
            {
                // default
                string key = "";

               //3 = length of ";;;"
               int length = line.IndexOf("Alternate") - NAME_OFFSET - 3;
                    if (length >= 0)
                        key = line.Substring(NAME_OFFSET, length).ToLowerInvariant();
                    else if (corsi) // this is a corsi stone with no alternate names
                    {
                        length = line.IndexOf("Sample") - NAME_OFFSET - 3;
                        if (length >= 0)
                            key = line.Substring(NAME_OFFSET, length).ToLowerInvariant();
                    }

                if (!materials.ContainsKey(key))
                {
                    materials.Add(key, line); // add this stone to dictionary
                }

                else if (corsi && key.Trim().Length > 0)// append this info to the current dictionary entry
                {
                    String currentText = materials[key];
                    materials.Remove(key);
                    materials.Add(key, currentText + "<page>" + line.Substring(line.IndexOf("Sample")));
                }

            }
        }
         // System.IO.File.WriteAllLines(@"C:\Users\Grace Sommers\Desktop\pietri.txt", lines);
    }
}
