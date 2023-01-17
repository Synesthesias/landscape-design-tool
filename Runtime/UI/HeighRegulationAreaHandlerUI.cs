using LandscapeDesignTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LandScapeDesignTool
{

    public class HeighRegulationAreaHandlerUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        // [SerializeField] GameObject boxprefab;
        [SerializeField] Text areaName;
        [SerializeField] InputField heightField;
        [SerializeField] InputField diameterField;
        [SerializeField] InputField centerxField;
        [SerializeField] InputField centeryField;
        [SerializeField] InputField centerzField;

        GameObject _target = null;
        GameObject _box;
        HeightRegulationAreaHandler _targethandler;
        bool newflag = false;
        Vector3 newPosition;
        Color _areaColor = new Color(0, 1, 1, 0.5f);

        // Start is called before the first frame update
        void Start()
        {
            newflag = false;
            panel.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (newflag)
            {

                if (Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();

                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        LayerMask layer = LayerMask.NameToLayer("RegulationArea");
                        if( hit.collider.gameObject.layer != layer)
                        {
                            newPosition = hit.point;

                            _target = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            _target.layer = LayerMask.NameToLayer("RegulationArea");
                            _targethandler = _target.AddComponent<HeightRegulationAreaHandler>();
                            _target.transform.localScale = new Vector3(30, 15, 30);
                            _target.transform.position = new Vector3(newPosition.x, 30, newPosition.z);
                            _target.name = LDTTools.GetNumberWithTag("HeightRegulationArea", "高さ規制エリア");
                            _target.tag = "HeightRegulationArea";
                            Material mat = LDTTools.MakeMaterial(_areaColor);
                            _target.GetComponent<Renderer>().material = mat;


                            areaName.color = Color.green;
                            areaName.text = _target.name;

                            heightField.text = "30";
                            diameterField.text = "30";
                            _targethandler.SetHeight(30);
                            _targethandler.SetRadius(30);

                            centerxField.text = _target.transform.position.x.ToString();
                            centeryField.text = _target.transform.position.y.ToString();
                            centerzField.text = _target.transform.position.z.ToString();

                            ShowPanel();
                            newflag = false;
                        }
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();

                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider.gameObject.tag == "HeightRegulationArea")
                        {
                            _target = hit.collider.gameObject;
                            _target.GetComponent<Renderer>().enabled = false;
                            areaName.color = Color.green;
                            areaName.text = _target.name;

                            _targethandler = _target.GetComponent<HeightRegulationAreaHandler>();
                            ShowPanel();
                            heightField.text = _targethandler.GetHeight().ToString();
                            diameterField.text = _targethandler.GetRadius().ToString();
                            /*
                            centerxField.text = _targethandler.GetPoint().x.ToString();
                            centeryField.text = _targethandler.GetPoint().y.ToString();
                            centerzField.text = _targethandler.GetPoint().z.ToString();
                            */
                            centerxField.text = _target.transform.position.x.ToString();
                            centeryField.text = _target.transform.position.y.ToString();
                            centerzField.text = _target.transform.position.z.ToString();

                            // _box = Instantiate(boxprefab);
                            // _box.transform.position = _target.transform.position;
                        }
                    }
                }
            }
        }

        void ShowPanel()
        {
            panel.SetActive(true);

        }

        public void Apply()
        {
            if(_targethandler != null){
                float d = float.Parse(diameterField.text);
                float h = float.Parse(heightField.text);
                /*
                float x = float.Parse(centerxField.text);
                float y = float.Parse(centerxField.text);
                float z = float.Parse(centerxField.text);
                */
                float x = _target.transform.position.x;
                float y = _target.transform.position.y;
                float z = _target.transform.position.z;
                _targethandler.transform.localScale = new Vector3(d, h/2, d);
                _targethandler.transform.position = new Vector3(x, h, z);
                _target.GetComponent<Renderer>().enabled = true;
            }
        }

        public void NewArea()
        {
            newflag = true;
            areaName.color = Color.red;
            areaName.text = "配置する場所をクリックしてください";
        }

        public void MovePosition()
        {

        }
    }
}
