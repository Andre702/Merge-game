using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] int _pixelsPerUnit = 31;
    [SerializeField] int _baseResolutionHeight = 180;

    int _scaledPixelsPerUnit;
    float _scaledInversePixelsPerUnit;
    public float moveSpeed = 5f;

    void Awake()
    {
        _scaledPixelsPerUnit = _pixelsPerUnit * Screen.currentResolution.height / _baseResolutionHeight;
        _scaledInversePixelsPerUnit = 1f / _scaledPixelsPerUnit;
    }

    void Update()
    {
        //Vector2 panPosition = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector2 panPosition = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // GetAxisRaw accelerates to 1 and decelerates to 0 instantly

        transform.position += Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) * new Vector3(panPosition.x, 0, panPosition.y * 1.5f) * moveSpeed * Time.deltaTime;

        //Quaternion rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        //Vector3 forward = rotation * Vector3.forward;
        //Vector3 right = rotation * Vector3.right;

        //Vector3 moveDirection = Vector3.zero;

        //if (Input.GetKey(KeyCode.W)) moveDirection += forward;
        //if (Input.GetKey(KeyCode.S)) moveDirection -= forward;
        //if (Input.GetKey(KeyCode.A)) moveDirection -= right;
        //if (Input.GetKey(KeyCode.D)) moveDirection += right;

        //moveDirection = moveDirection.normalized * moveSpeed * Time.deltaTime;
        //transform.position = PixelPerfect(transform.position + moveDirection);
    }

    Vector3 PixelPerfect(Vector3 vector)
    {
        // due to the floating values on pixels in the unit the camera drifts down every 9th update or so
        vector.x = Mathf.FloorToInt(vector.x * _scaledPixelsPerUnit) * _scaledInversePixelsPerUnit;
        vector.y = Mathf.FloorToInt(vector.y * _scaledPixelsPerUnit) * _scaledInversePixelsPerUnit;
        return vector;
    }
}
