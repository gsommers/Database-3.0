using UnityEngine;
using TMPro;

// displays the current search radius
public class Radius : MonoBehaviour
{
    private TextMeshProUGUI radiusText;

    void Start()
    {
        radiusText = gameObject.GetComponent<TextMeshProUGUI>();
    }

    // called when user changes search radius using slider
    public void SetRadius(float radius)
    {
        radiusText.text = radius.ToString();
    }
}
