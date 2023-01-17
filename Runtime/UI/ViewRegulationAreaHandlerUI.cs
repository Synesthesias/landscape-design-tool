using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LandScapeDesignTool
{
    public class ViewRegulationAreaHandlerUI : MonoBehaviour
    {
        GameObject[] objects;
        // Start is called before the first frame update
        void Start()
        {

            objects = GameObject.FindGameObjectsWithTag("ViewRegulationArea");
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void ShowViewRegulaitonArea()
        {

            foreach (var obj in objects)
            {
                obj.SetActive(true);
            }

        }
        public void HideViewRegulationarea()
        {

            foreach( var obj in objects)
            {
                obj.SetActive(false);
            }
        }
    }
}
