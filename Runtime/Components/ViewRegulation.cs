using System.Collections.Generic;
using UnityEngine;


public class ViewRegulation : MonoBehaviour
{
    public Material highlightMaterial;
    public Material areaMaterial;

    public float screenWidth = 80.0f;
    public float screenHeight = 80.0f;
    [SerializeField] List<GameObject> ignoreObject = new List<GameObject>();

}

