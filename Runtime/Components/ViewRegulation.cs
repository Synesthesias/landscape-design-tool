using System.Collections.Generic;
using PlasticGui.Configuration.CloudEdition.Welcome;
using UnityEngine;
using UnityEngine.UIElements;


public class ViewRegulation : MonoBehaviour
{
    public Material highlightMaterial;
    public Material areaMaterial;

    public float screenWidth = 80.0f;
    public float screenHeight = 80.0f;
    
    /// <summary> 視線の向かう先 </summary>
    public Vector3 endPos;

    public float lineInterval = 4;
    public Color lineColorValid = new Color(0, 1, 0, 0.2f);
    public Color lineColorInvalid = new Color(1, 0, 0, 0.2f);
    [SerializeField] List<GameObject> ignoreObject = new List<GameObject>();

    public Vector3 StartPos => transform.position;

    public void UpdateParams(float screenWidthArg, float screenHeightArg, Vector3 endPosArg)
    {
        screenWidth = screenWidthArg;
        screenHeight = screenHeightArg;
        endPos = endPosArg;
    }

}

