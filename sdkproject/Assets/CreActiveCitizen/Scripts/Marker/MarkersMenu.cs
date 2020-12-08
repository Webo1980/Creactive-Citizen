using Mapbox.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Json.Linq;

public class MarkersMenu : MonoBehaviour
{
    public static Dropdown m_Dropdown;
    public int m_DropdownValue;
    public string m_option;
    public JToken dropdownObjects;
    [SerializeField]
    string DefaultOption;

    public static MarkersMenu Instance { get; private set; }
    
    void PopulateDropdown(Dropdown dropdown, JToken optionsArray)
    {
        List<string> options = new List<string>();
        options.Add(DefaultOption); // Or whatever you want for a label
        foreach (var option in optionsArray)
        {
            //dropdown.value = option["id"].ToObject<int>();
            options.Add(option["german_name"].ToObject<string>());
        }
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
    }

    void Awake() // to store dropdown data 
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //Fetch the DropDown component from the GameObject
        m_Dropdown = GetComponent<Dropdown>();
        IEnumerator cuurentObjectsList = DBManager.GetObjectsLists((returnedObjectsList) => {
            if (returnedObjectsList)
            {
                string DBObjectsList = DBManager.objectsList;
                dropdownObjects = JToken.Parse(DBObjectsList);
                PopulateDropdown(m_Dropdown, dropdownObjects);
            }
        });
        StartCoroutine(cuurentObjectsList);
    }

    void Update()
    {
        if (m_Dropdown != null)
        { 
           m_option = m_Dropdown.options[m_Dropdown.value].text;
        }

        if (DrawOnScreen.isDrawing == true)
        {
            m_Dropdown.value = 0;
        }
        
    }

    public void IsDropdownValueChanged()
    {
        if(m_Dropdown.value != 0)  // if the user selected a marker
        {
            DrawOnScreen.isDrawing = false;  // stop painting
            // hide drawing colors
            AddCustomMarker.ChangeVisibility("SetRed", 0.0f, false, false);
            AddCustomMarker.ChangeVisibility("SetGreen", 0.0f, false, false);
            AddCustomMarker.ChangeVisibility("SetBlue", 0.0f, false, false);
        }
    }
}