using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

// handles multiple pages of text 
public class FlipPage : MonoBehaviour
{

    public TextMeshProUGUI listText; // text being displayed
    private int numPages; // how many pages does this list take up?
    public Button pageNext, pagePrevious; // buttons for flipping page

    private void Start()
    {
        pageNext.onClick.AddListener(NextPage);
        pagePrevious.onClick.AddListener(PreviousPage);
    }
    
    // called when panel with multiple pages of text is first opened
    public void SetPages()
    {
        // set page count occupied by this block of tect
        try
        {
            numPages = listText.GetTextInfo(listText.text).pageCount;
        }
        catch
        {
            numPages = 5; // default number, will handle most cases
            Debug.Log("Something went wrong"); // seems to occur the first time I open tree info
        }

        listText.pageToDisplay = 1; // start at first page
        pagePrevious.interactable = false; // set to first page, so can't go back

        if (numPages == 1)
            pageNext.interactable = false; // no second page
        else
            pageNext.interactable = true;
    }

    // go to next page when user clicks "next"
    void NextPage()
    {
        listText.pageToDisplay++;
        if (listText.pageToDisplay == numPages) // last page
            pageNext.interactable = false;
        pagePrevious.interactable = true;
    }

    // go to previous page when user clicks "back"
    void PreviousPage()
    {
        listText.pageToDisplay--;
        if (listText.pageToDisplay == 1) // first page
            pagePrevious.interactable = false;
        pageNext.interactable = true;

    }
}
