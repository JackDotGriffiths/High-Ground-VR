using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class MenuManagement : MonoBehaviour
{

    [SerializeField] private Color m_optionOn, m_optionOff;

    [Header("Handedness Menu Items"),Space(10)]
    [SerializeField] private TextMeshProUGUI m_handednessText;
    [SerializeField] private GameObject m_leftHandIcon, m_rightHandIcon;

    [Header("Helper Board Items"), Space(10)]
    [SerializeField] private Image m_helperImage;
    [SerializeField] private Animator m_buildingBoard;
    [SerializeField] private Animator m_spellsBoard;


    [Header("Audio Items"), Space(10)]
    [SerializeField] private AudioMixer m_mixer;
    [SerializeField] private Image m_musicImage;
    [Space(5)]
    [SerializeField] private Image m_effectsImage;




    void Start()
    {
        setDefaultHandedness();
        loadStatusInfoPanels();
        loadMusicStatus();
        loadEffectStatus();
    }

    #region Handedness Control
    private void setDefaultHandedness()
    {
        m_handednessText.text = "Right Handed";
        m_leftHandIcon.SetActive(false);
        m_rightHandIcon.SetActive(true);
        InputManager.Instance.Handedness = HandTypes.right;
    }

    public void toggleHandedness()
    {
        if(InputManager.Instance.Handedness == HandTypes.right)
        {
            m_handednessText.text = "Left Handed";
            m_leftHandIcon.SetActive(true);
            m_rightHandIcon.SetActive(false);
            InputManager.Instance.Handedness = HandTypes.left;
        }
        else
        {
            m_handednessText.text = "Right Handed";
            m_leftHandIcon.SetActive(false);
            m_rightHandIcon.SetActive(true);
            InputManager.Instance.Handedness = HandTypes.right;
        }
    }
    #endregion

    #region Info Panels Control
    private void loadStatusInfoPanels()
    {
       if(PlayerPrefs.GetInt("InfoPanelsStatus") == 0)
       {
            //Info Panels Off
            m_buildingBoard.Play("SlideDown");
            m_spellsBoard.Play("SlideDown");
            m_helperImage.color = m_optionOff;
        }
       else
       {
            //Create PlayerPrefs & Set Defaults
            PlayerPrefs.SetInt("InfoPanelsStatus", 1);
            m_helperImage.color = m_optionOn;

        }
    }
    public void toggleInfoPanels()
    {
        if (PlayerPrefs.GetInt("InfoPanelsStatus") == 1)
        {
            //Info Panels On
            PlayerPrefs.SetInt("InfoPanelsStatus", 0);
            m_buildingBoard.Play("SlideUp");
            m_spellsBoard.Play("SlideUp");
            m_helperImage.color = m_optionOn;
        }
        else
        {
            //Info Panels Off
            PlayerPrefs.SetInt("InfoPanelsStatus", 1);
            m_buildingBoard.Play("SlideDown");
            m_spellsBoard.Play("SlideDown");
            m_helperImage.color = m_optionOff;
        }
    }
    #endregion

    #region Audio Control
    private void loadMusicStatus()
    {
        if (PlayerPrefs.GetInt("MusicStatus") == 0)
        {
            //Music Off
            m_mixer.SetFloat("musicVolume", -80); //Mute the music
            m_musicImage.color = m_optionOff;
        }
        else
        {
            //Create PlayerPrefs & Set Defaults
            PlayerPrefs.SetInt("MusicStatus", 1);
            m_mixer.SetFloat("musicVolume", 0); //Unmute the music
            m_musicImage.color = m_optionOn;

        }
    }
    public void toggleMusicStatus()
    {
        if (PlayerPrefs.GetInt("MusicStatus") == 1)
        {
            //Info Panels On
            PlayerPrefs.SetInt("MusicStatus", 0);
            m_mixer.SetFloat("musicVolume", -80); //Mute the music
            m_musicImage.color = m_optionOn;
        }
        else
        {
            //Info Panels Off
            PlayerPrefs.SetInt("MusicStatus", 1);
            m_mixer.SetFloat("musicVolume", 0); //Unmute the music
            m_musicImage.color = m_optionOff;
        }
    }

    private void loadEffectStatus()
    {
        if (PlayerPrefs.GetInt("EffectsStatus") == 0)
        {
            //Effects Off
            m_mixer.SetFloat("effectsVolume", -80); //Mute the effects
            m_mixer.SetFloat("userinterfaceVolume", -80);
            m_effectsImage.color = m_optionOff;
        }
        else
        {
            //Create PlayerPrefs & Set Defaults
            PlayerPrefs.SetInt("EffectsStatus", 1);
            m_mixer.SetFloat("effectsVolume", 0); //Unmute the effects
            m_mixer.SetFloat("userinterfaceVolume", 0);
            m_effectsImage.color = m_optionOn;

        }
    }
    public void toggleEffectStatus()
    {
        if (PlayerPrefs.GetInt("EffectsStatus") == 1)
        {
            PlayerPrefs.SetInt("EffectsStatus", 0);
            m_mixer.SetFloat("effectsVolume", -80); //Unmute the effects
            m_mixer.SetFloat("userinterfaceVolume", -80);
            m_effectsImage.color = m_optionOn;
        }
        else
        {
            PlayerPrefs.SetInt("EffectsStatus", 1);
            //Effects Off
            m_mixer.SetFloat("effectsVolume", 0); //Mute the effects
            m_mixer.SetFloat("userinterfaceVolume", 0);
            m_effectsImage.color = m_optionOff;
        }
    }


    #endregion


}
