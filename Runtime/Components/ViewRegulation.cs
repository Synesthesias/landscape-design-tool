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
    public Vector3 endPos;
    [SerializeField] List<GameObject> ignoreObject = new List<GameObject>();

    public Vector3 StartPos
    {
        get
        {
            return transform.position;
        }
    }

    public void UpdateParams(float screenWidthArg, float screenHeightArg, Vector3 endPosArg)
    {
        screenWidth = screenWidthArg;
        screenHeight = screenHeightArg;
        endPos = endPosArg;
    }

    // public const string NameOfScreenWidth = nameof(screenWidth);
    // public const string NameOfScreenHeight = nameof(screenHeight);
}

