using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

/*
 * Implementation of the Date data type, which is comparable
 */
public class Date: IComparable<Date>
{
    private int year;
    private bool era; // false for BCE, true for CE

    // Instantiate a date from a year (string) and era (bool)
    public Date(string date, bool annoDom)
    {
        Int32.TryParse(date, out year); // only integers accepted
        era = annoDom;
    }

    // Instantiate a date from a string that includes year and era
    public Date(string date)
    {
        int space = date.IndexOf(" "); // should be positive
        if (space < 0) // should never occur, since I'm inputting it
            throw new FormatException("Problem with the date...");
        Int32.TryParse(date.Substring(0, space), out year);
        era = (date.Substring(space + 1).ToLower() == "ce");
    }

    // set BCE (false) or CE (true)
    public void SetEra(bool toggle)
    {
        era = toggle;
    }

    // Implements the IComparable interface by comparing two dates
    // A later date is considered "greater" than an earlier one
    public int CompareTo(Date other)
    {
        if (era == other.era) 
        {
            if (era) // both AD
            {
                return year - other.year;         
            }
            else // both BC
                return other.year - year;
        }
        else if (era) // this date is AD
            return 1;
        else
            return -1;
    }

    public override string ToString()
    {
        return year + " " + (era ? "CE" : "BCE");
    }
}

// Handles input on the date panel
public class DateFilter : MonoBehaviour {

    private Date start, end; // only data between start and end will be shown
    public TextMeshProUGUI startText, endText, errorText;
    private Color originalColor; // of the start and end text
    public string earliest, latest; // time span of this database
    public Color errorColor; // probably red
    public Toggle startAD, endAD; // radio buttons for era
    public Raycasting rayCastScript; // send dates to here

    private void Start()
    {
        originalColor = startText.color; // store the normal color of start and end text
    }

    // called when user finishes typing in the text box for start date
    public void SetStart(string entry)
    {
        if (entry.Length > 0)
            start = new Date(entry, startAD.isOn);
        else
            start = null;
    }

    // called when user finishes typing in the text box for end date
    public void SetEnd(string entry)
    {
        if (entry.Length > 0)
            end = new Date(entry, endAD.isOn);
        else
            end = null;
    }

    // called when user toggles radio button for BCE/CE
    public void ChangeEra(bool startButton)
    {
        if (startButton && start != null) // user toggled era for start date
            start.SetEra(startAD.isOn);
        else if (end != null) // toggle era for end date
            end.SetEra(endAD.isOn);
    }

    // sets/clears error message for invalid input
    private void SetErrorText(string message, string error)
    {
        errorText.text = message;
        switch (error)
        {
            // error in start input
            case "start": errorText.alignment = TextAlignmentOptions.MidlineLeft;
                startText.color = errorColor;
                break;
            // error in end input
            case "end": errorText.alignment = TextAlignmentOptions.MidlineRight;
                endText.color = errorColor;
                break;
            // start date later than end date
            case "both": errorText.alignment = TextAlignmentOptions.Midline;
                endText.color = errorColor;
                startText.color = errorColor;
                break;
           // no error
            case "neither": endText.color = originalColor;
                startText.color = originalColor;
                rayCastScript.FilterDates(start, end);
                break;
        }
    }

    // called when user presses submit button; checks for invalid input
    public void Submit()
    {
        // assuming all is correct
        SetErrorText("", "neither");

        if (start != null) // check start date for errors
        {
            CheckLimits(start, "start");
        }

        if (end != null) // check end date for errors
        {
            CheckLimits(end, "end");
        }

        // start date before end date
        if (start != null && end != null && start.CompareTo(end) > 0)
        {
            SetErrorText("Please select a start date that is earlier than the end date.", "both");
        }
    }

    // check that input is within the time span of this database
    private void CheckLimits(Date entry, string type)
    {
        if (entry.CompareTo(new Date(earliest)) < 0 || entry.CompareTo(new Date(latest)) > 0)
                SetErrorText(String.Format("Make sure that the {0} date is between {1} and {2}.", type, earliest, latest), type);
    }
}
