using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandscapeViewPointGroup : MonoBehaviour
{
    [SerializeField] private Button viewpointButton;

    private const float Yoffset = 100;
    private const float Xoffset = 30;
    private const float Padding = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0; i< transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            string description = child.GetComponent<LandscapeViewPoint>().GetDescription();
            GameObject ui = GameObject.Find("UI").gameObject;
            
            Button btn = Instantiate(viewpointButton);
            btn.transform.SetParent(ui.GetComponent<Canvas>().transform);
            float x = Screen.width - Xoffset - btn.GetComponent<RectTransform>().rect.width/2.0f;
            float y = Screen.height - Yoffset - (btn.GetComponent<RectTransform>().rect.height+Padding) * i;
            btn.transform.position = new Vector3(x, y);
            int n = i;
            btn.onClick.AddListener(() => OnViewpointButton(n));

            Text text = btn.transform.Find("Text").GetComponent<Text>();
            text.text = description;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnViewpointButton(int n)
    {
        GameObject target = transform.GetChild(n).gameObject;
        LandscapeViewPoint viewpoint = target.GetComponent<LandscapeViewPoint>();
        Vector3 pos = viewpoint.transform.position;
        Quaternion rot = viewpoint.transform.localRotation;
        float fov = viewpoint.GetFOV();
        float height=viewpoint.GetHeight();
        Camera camera = Camera.main;
        camera.transform.position = pos+new Vector3(0,height,0);
        camera.transform.localRotation = rot;
        camera.fieldOfView = fov;
    }
}
