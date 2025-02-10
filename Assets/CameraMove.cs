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
        Quaternion rotation = Quaternion.Euler(-30, 45, 0);
        Vector3 forward = rotation * Vector3.forward;
        Vector3 right = rotation * Vector3.right;

        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDirection += forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= right;
        if (Input.GetKey(KeyCode.D)) moveDirection += right;

        moveDirection = moveDirection.normalized * moveSpeed * Time.deltaTime;
        transform.position = PixelPerfect(transform.position + moveDirection);
    }

    Vector3 PixelPerfect(Vector3 vector)
    {
        vector.x = Mathf.FloorToInt(vector.x * _scaledPixelsPerUnit) * _scaledInversePixelsPerUnit;
        vector.y = Mathf.FloorToInt(vector.y * _scaledPixelsPerUnit) * _scaledInversePixelsPerUnit;
        return vector;
    }

    //public float moveSpeed = 5f;

    //void Update()
    //{
    //    // Compute the camera's actual forward and right directions based on its rotation
    //    Quaternion rotation = Quaternion.Euler(-30, 45, 0);
    //    Vector3 forward = rotation * Vector3.forward;
    //    Vector3 right = rotation * Vector3.right;

    //    Vector3 moveDirection = Vector3.zero;

    //    if (Input.GetKey(KeyCode.W)) moveDirection += forward;
    //    if (Input.GetKey(KeyCode.S)) moveDirection -= forward;
    //    if (Input.GetKey(KeyCode.A)) moveDirection -= right;
    //    if (Input.GetKey(KeyCode.D)) moveDirection += right;

    //    transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;
    //}
}
