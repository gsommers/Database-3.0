using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// abstract class for making a dictionary that holds information on a particular material
public abstract class AbstractDictionaryCreator : ScriptableObject
{
    public TextAsset types; // text file holding the info, delimited by ";;;" and ":::"
    public SortedDictionary<string, string> materials; // dictionary keyed by name of material
    protected const int SEMICOLONS = 3; // "magic number" - length of the semicolon delimiter

    public abstract void OnEnable(); // makes the dictionary, implement in subclass
}
