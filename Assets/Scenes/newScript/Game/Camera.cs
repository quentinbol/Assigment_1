using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("movement")]
    public float moveSpeed = 15f;

    public float fastSpeed = 2f;

    public float zoomSpeed = 10f;

    public float minHeight = 5f;

    public float maxHeight = 50f;

    public bool useBoundaries = false;
    public Vector3 minBounds = new Vector3(-50, 0, -50);
    public Vector3 maxBounds = new Vector3(50, 0, 100);
    
    void Update()
    {
        HandleMovement();
        HandleZoom();
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical);
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= fastSpeed;
        }
        transform.position += direction * speed * Time.deltaTime;
        if (useBoundaries)
        {
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.z = Mathf.Clamp(pos.z, minBounds.z, maxBounds.z);
            transform.position = pos;
        }
    }
    
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed;
            pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
            transform.position = pos;
        }
    }
}