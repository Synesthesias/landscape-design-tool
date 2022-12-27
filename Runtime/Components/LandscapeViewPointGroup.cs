using UnityEngine;
using UnityEngine.UI;

public class LandscapeViewPointGroup : MonoBehaviour
{
    [SerializeField] private Button viewpointButton;

    private const float Yoffset = 100;
    private const float Xoffset = 30;
    private const float Padding = 10;
    
    void Start()
    {
        GameObject ui = GameObject.Find("UI").gameObject;
        for (int i = 0; i < transform.childCount; i++)
        {
            Button btn = Instantiate(viewpointButton);
            btn.transform.SetParent(ui.transform);
            float x = Screen.width - Xoffset - btn.GetComponent<RectTransform>().rect.width / 2.0f;
            float y = Screen.height - Yoffset - (btn.GetComponent<RectTransform>().rect.height + Padding) * i;
            btn.transform.position = new Vector3(x, y);
            int n = i;
            btn.onClick.AddListener(() => OnViewpointButton(n));

            Text text = btn.transform.Find("Text").GetComponent<Text>();
            GameObject child = transform.GetChild(i).gameObject;
            text.text = child.name;
        }
    }

    public void OnViewpointButton(int n)
    {
        GameObject target = transform.GetChild(n).gameObject;
        LandscapeViewPoint viewpoint = target.GetComponent<LandscapeViewPoint>();
        Vector3 pos = viewpoint.transform.position;
        Quaternion rot = viewpoint.transform.localRotation;
        float fov = viewpoint.Fov;
        Camera camera = Camera.main;
        camera.transform.position = pos;
        camera.transform.localRotation = rot;
        camera.fieldOfView = fov;
    }
}
