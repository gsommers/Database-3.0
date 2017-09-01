using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// handles panels that can be hidden and displayed
public class Window : MonoBehaviour {

    [SerializeField]
    ScrollRect scroll;

    [SerializeField]
    FlipPage pageFlip;
    // called by open and close buttons
    public void SetVisibility(bool button)
    {
        if (button) // open window
        {
            // reset scroll window to top
            if (scroll != null) 
                scroll.verticalNormalizedPosition = 1;

            // reset page to page 1
            if (pageFlip != null)
                pageFlip.SetPages();

        }

        gameObject.SetActive(button); // open/close window
    }
}
