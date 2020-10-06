using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mapbox.Json.Linq;

public class MarkersMenu : MonoBehaviour
{
    Dropdown m_Dropdown;
    public int m_DropdownValue;
    public string m_option;
    public JToken dropdownObjects;
    [SerializeField]
    string DefaultOption;

    CanvasGroup DrowpCanvas;

    public static MarkersMenu Instance { get; private set; }
    private readonly DBManager DB = new DBManager();

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

    public void ShowMenu()
    {
        DrowpCanvas = m_Dropdown.GetComponent<CanvasGroup>();
        DrowpCanvas.alpha = 1f;
        DrowpCanvas.blocksRaycasts = true;
        DrowpCanvas.interactable = true;
    }

    void Start()
    {
        //Fetch the DropDown component from the GameObject
        m_Dropdown = GetComponent<Dropdown>();
        StartCoroutine(DB.GetObjectsLists((returnedObjectsList) => {
            if (returnedObjectsList)
            {
                dropdownObjects = JToken.Parse(DB.objectsList);
                PopulateDropdown(m_Dropdown, dropdownObjects);
            }
        }));
    }

    void Update()
    {
        if (m_Dropdown != null)
        { 
           m_option = m_Dropdown.options[m_Dropdown.value].text;
        }
    }

    /*void DropdownValueChanged()
    {
        isChanged = true;
        Debug.Log("The onchange was called");
    }

    /*public int CurrentMenuIndex()
    {
        m_DropdownValue = m_Dropdown.value;
        return m_DropdownValue;
    }

    public string CurrentMenuText()
    {
        //m_DropdownValue = m_Dropdown.value;
        m_option = m_Dropdown.options[4].text;
        return m_option;
    }*/
}