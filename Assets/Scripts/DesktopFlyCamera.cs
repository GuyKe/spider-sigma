using UnityEngine;

// Desktop-only preview navigation (WASD + mouse look) so the room can be
// checked in Play mode before an XR rig is wired in. Not used once the
// real XR Origin / Quest camera rig replaces the Main Camera.
public class DesktopFlyCamera : MonoBehaviour
{
    public float moveSpeed = 2.2f;
    public float lookSpeed = 120f;

    float yaw, pitch;

    void Start()
    {
        var e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
        if (Input.GetMouseButtonDown(0)) Cursor.lockState = CursorLockMode.Locked;

        yaw += Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
        transform.eulerAngles = new Vector3(pitch, yaw, 0);

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? 2f : 1f);
        transform.position += transform.TransformDirection(move) * speed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q)) transform.position += Vector3.up * speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) transform.position -= Vector3.up * speed * Time.deltaTime;
    }
}
