using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// used on radio buttons
// ensures that exactly one option is selected
public class Lock : MonoBehaviour {

    public Toggle otherButton;
	public void TurnOn(bool toggle)
    {
        // you can't deselect a button that's turned on
        // unless you turn on the other button
        if (toggle)
        {
            GetComponent<Toggle>().interactable = false;
            otherButton.interactable = true;
        }
    }
}
