using UnityEngine;

public class WalkThruHandler : MonoBehaviour
{
    private const float AngleDelta = 1.0f;
    private const float MoveDelta = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Quaternion q = gameObject.transform.rotation;
            float x = q.eulerAngles.x;
            float y = q.eulerAngles.y;
            Quaternion r = Quaternion.Euler(x,y-AngleDelta,0);
            gameObject.transform.rotation = r;

        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            Quaternion q = gameObject.transform.rotation;
            float x = q.eulerAngles.x;
            float y = q.eulerAngles.y;
            Quaternion r = Quaternion.Euler(x,y+AngleDelta,0);
            gameObject.transform.rotation = r;
            
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Quaternion q = gameObject.transform.rotation;
            float x = q.eulerAngles.x;
            float y = q.eulerAngles.y;
            Quaternion r = Quaternion.Euler(x-AngleDelta,y,0);
            gameObject.transform.rotation = r;
            
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            Quaternion q = gameObject.transform.rotation;
            float x = q.eulerAngles.x;
            float y = q.eulerAngles.y;
            Quaternion r = Quaternion.Euler(x+AngleDelta,y,0);
            gameObject.transform.rotation = r;
            
        }

        RaycastHit hit;
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 p = gameObject.transform.position;
            Vector3 ff = gameObject.transform.forward;
            Vector3 dst = p + ff * MoveDelta;
            dst.y += 100.0f;
            if (Physics.Raycast(dst, new Vector3(0, -1, 0), out hit, Mathf.Infinity))
            {
                Vector3 hitp = hit.point;
                hitp.y += 1.6f;
                gameObject.transform.position = hitp;
            }
        }
        
        if (Input.GetKey(KeyCode.Q))
        {
            Vector3 p = gameObject.transform.position;
            Vector3 bb = -gameObject.transform.forward;
            Vector3 dst = p + bb * MoveDelta;
            dst.y += 100.0f;
            if (Physics.Raycast(dst, new Vector3(0, -1, 0), out hit, Mathf.Infinity))
            {
                Vector3 hitp = hit.point;
                hitp.y += 1.6f;
                gameObject.transform.position = hitp;
            }
        }
    }
}
