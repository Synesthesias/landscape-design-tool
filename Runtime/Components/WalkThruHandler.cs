using UnityEngine;

public class WalkThruHandler : MonoBehaviour
{
    private const float AngleDelta = 1.0f;
    private const float MoveDelta = 1f;
    private const float RotateDelta = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.W))
        {
            Vector3 p = gameObject.transform.position;
            Vector3 ff = gameObject.transform.forward;
            Vector3 dst = p + ff * MoveDelta;
            transform.position = dst;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Vector3 p = gameObject.transform.position;
            Vector3 bb = -gameObject.transform.forward;
            Vector3 dst = p + bb * MoveDelta;
            transform.position = dst;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Vector3 p = gameObject.transform.position;
            Vector3 bb = -gameObject.transform.right;
            Vector3 dst = p + bb * MoveDelta;
            transform.position = dst;

        }

        if (Input.GetKey(KeyCode.D))
        {
            Vector3 p = gameObject.transform.position;
            Vector3 bb = gameObject.transform.right;
            Vector3 dst = p + bb * MoveDelta;
            transform.position = dst;
        }

        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            Vector3 p = transform.position;
            Vector3 bb = -transform.up;
            Vector3 dst = p + bb * MoveDelta;
            transform.position = dst;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 p = transform.position;
            Vector3 bb = transform.up;
            Vector3 dst = p + bb * MoveDelta;
            transform.position = dst;
        }

        // Right-click + mouse to rotate
        if (Input.GetMouseButton(1))
        {
            transform.eulerAngles += RotateDelta * new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        }
    }
}
