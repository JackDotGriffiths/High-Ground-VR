using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class VersionNumber : MonoBehaviour
{
    /// <summary>
    /// Sets the text to the version number of the current build.
    /// </summary>
    void Start()
    {
        this.transform.GetComponent<TextMeshPro>().text = "Version " + Application.version;
    }
}
