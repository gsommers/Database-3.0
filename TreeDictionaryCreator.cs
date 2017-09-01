using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "TreeDictionary")]
public class TreeDictionaryCreator : AbstractDictionaryCreator
{
    private const int NAME_OFFSET = 19; // length of "Scientific Name::: "

    public override void OnEnable()
    {
        string[] lines = types.text.Split('\n');

        // initialize the dictionary of tree species
        materials = new SortedDictionary<string, string>();
        foreach (string line in lines)
        {
            if (line.Length > 0)
            {
                int index = line.IndexOf("Scientific Name:::");
                if (index >= 0)
                {
                    int length = line.IndexOf("Distribution") - index - NAME_OFFSET - SEMICOLONS;
                    string key = "";
                    if (length > 0)
                        key = line.Substring(index + NAME_OFFSET, length); // scientific name
                    if (!materials.ContainsKey(key))
                        materials.Add(key.ToLowerInvariant(), line); // add this species to dictionary
                }

            }
        }
    }
}
