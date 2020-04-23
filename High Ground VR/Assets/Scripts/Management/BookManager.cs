using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class BookManager : MonoBehaviour
{

    private static BookManager s_instance;


    [SerializeField, Tooltip("Length of time double speed should be active for.")] private float m_doubleSpeedTime = 5.0f;

    [SerializeField, Tooltip("Object within which the colliders, buttons and displays for the actions is held")] private GameObject m_actionsMenu;
    [Tooltip("All available buildings from the book menu")] public BuildingOption[] buildingOptions; //A list of all available buildings from the player's menu.


    [Header("Button Controls")]
    [SerializeField, Tooltip("Button Material")] private Material m_buttonMaterial;
    [SerializeField, Tooltip("Selected Material")] private Material m_selectedMaterial;
    [SerializeField, Tooltip("Buildings Buttons")] private List<GameObject> m_buildingButtons = new List<GameObject>();
    [SerializeField, Tooltip("Spell Buttons")] private List<GameObject> m_spellButtons = new List<GameObject>();


    [Header ("Book Text Objects")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI moneyText2;
    public TextMeshProUGUI timerText2;
    public TextMeshProUGUI roundText2;
    [SerializeField,Tooltip("Text to show the cost of a wall")] private TextMeshPro m_wallCost;
    [SerializeField, Tooltip("Text to show the cost of a barracks")] private TextMeshPro m_barracksCost;
    [SerializeField, Tooltip("Text to show the cost of a mine")] private TextMeshPro m_mineCost;



    [Header("Time Skip Text Objects"), Space(5)]
    [SerializeField] private TextMeshPro m_timeButton1;
    [SerializeField] private TextMeshPro m_timeButton2;



    private bool m_isShowingSpells;


    #region Accessors
    public static BookManager Instance { get => s_instance; set => s_instance = value; }
    #endregion
    private void Awake()
    {
        //Singleton Implementation
        if (s_instance == null)
            s_instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

    }
    void Start()
    {
        Invoke("delayedStart", 0.1f);
    }

    void delayedStart()
    {
        m_wallCost.text = GameManager.Instance.wallsCost.ToString();
        m_barracksCost.text = GameManager.Instance.barracksCost.ToString();
        m_mineCost.text = GameManager.Instance.mineCost.ToString();
    }

    void Update()
    {
        //Changes text based on the phase the game is in.
        if(GameManager.Instance.CurrentPhase == Phases.Building)
        {
            m_timeButton1.text = "Skip Building";
            m_timeButton2.text = "Skip Building";
        }
        else
        {
            m_timeButton1.text = "Double Speed";
            m_timeButton2.text = "Double Speed";
        }
    }

    /// <summary>
    /// Sets the current building through a button press.
    /// </summary>
    /// <param name="_building">Sets the current building to this option.</param>
    public void SetBuilding(int _index)
    {
        try
        {
            InputManager.Instance.CurrentlySelectedBuilding = buildingOptions[_index];
            InputManager.Instance.CurrentlySelectedSpell = (spellTypes)0;
        }
        catch
        {
            Debug.LogWarning("Error trying to find the InputManager or error with the chosen index.");
        }
    }

    /// <summary>
    /// Sets the current spell through a button press.
    /// </summary>
    /// <param name="_spell"></param>
    public void SetSpell(int _spellIndex)
    {
        try
        {
            InputManager.Instance.CurrentlySelectedBuilding = null;
            InputManager.Instance.CurrentlySelectedSpell = (spellTypes)_spellIndex;
        }
        catch
        {
            Debug.LogWarning("Error trying to find the InputManager.");
        }
    }

    /// <summary>
    /// Turns the book to show the spells
    /// </summary>
    public void TurnToSpells()
    {
        //Make sure all of the buttons are unlocked.
        foreach (GameObject _button in m_buildingButtons)
        {
            _button.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            _button.GetComponent<PhysicalButton>().isLocked = false;
            _button.GetComponent<MeshRenderer>().material = m_buttonMaterial;
        }
        foreach (GameObject _button in m_spellButtons)
        {
            _button.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            _button.GetComponent<PhysicalButton>().isLocked = false;
            _button.GetComponent<MeshRenderer>().material = m_buttonMaterial;
        }

        gameObject.GetComponent<Animator>().Play("TurnToSpells");
        InputManager.Instance.CurrentlySelectedBuilding = null;
        InputManager.Instance.CurrentlySelectedSpell = (spellTypes)0;
        m_isShowingSpells = true;
    }

    /// <summary>
    /// Turns the book to show the buildings
    /// </summary>
    public void TurnToBuildings()
    {
        //Make sure all of the buttons are unlocked.
        foreach (GameObject _button in m_buildingButtons)
        {
            _button.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            _button.GetComponent<PhysicalButton>().isLocked = false;
            _button.GetComponent<MeshRenderer>().material = m_buttonMaterial;
        }
        foreach (GameObject _button in m_spellButtons)
        {
            _button.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            _button.GetComponent<PhysicalButton>().isLocked = false;
            _button.GetComponent<MeshRenderer>().material = m_buttonMaterial;
        }

        gameObject.GetComponent<Animator>().Play("TurnToBuildings");
        InputManager.Instance.CurrentlySelectedBuilding = null;
        InputManager.Instance.CurrentlySelectedSpell = (spellTypes)0;
        m_isShowingSpells = false;
    }
    
    /// <summary>
    /// Starts the coroutine that runs the game at double speed for a set amount of time.
    /// </summary>
    public void pressTimeButton()
    {
        if(GameManager.Instance.GameOver == false)
        {
            if (GameManager.Instance.CurrentPhase == Phases.Building)
            {
                GameManager.Instance.SkipBuildingPhase();
            }
            else
            {
                StartCoroutine(doubleSpeed());
            }
        }
    }

    IEnumerator doubleSpeed()
    {
        GameManager.Instance.GameSpeed = 2.0f;
        yield return new WaitForSeconds(m_doubleSpeedTime);
        GameManager.Instance.GameSpeed = 1.0f;
    }

    /// <summary>
    /// Unlocks all of the rigidbodies of the selection buttons so that 
    /// </summary>
    public void lockButtons(GameObject _selectedButton)
    {
        foreach(GameObject _button in m_buildingButtons)
        {
            if(_button != _selectedButton)
            {
                //Unlock
                _button.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
                _button.GetComponent<PhysicalButton>().isLocked = false;
                _button.GetComponent<MeshRenderer>().material = m_buttonMaterial;
            }
            else
            {
                //Lock
                _button.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                _button.GetComponent<PhysicalButton>().isLocked = true;
                _button.GetComponent<MeshRenderer>().material = m_selectedMaterial;
            }
        }
        foreach (GameObject _button in m_spellButtons)
        {
            if (_button != _selectedButton)
            {
                //Unlock
                _button.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
                _button.GetComponent<PhysicalButton>().isLocked = false;
                _button.GetComponent<MeshRenderer>().material = m_buttonMaterial;
            }
            else
            {
                //Lock
                _button.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                _button.GetComponent<PhysicalButton>().isLocked = true;
                _button.GetComponent<MeshRenderer>().material = m_selectedMaterial;
            }
        }
    }
}
