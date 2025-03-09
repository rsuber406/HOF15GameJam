using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] int sensitivity;
    [SerializeField] private int lockVertMin, lockVertMax;
    [SerializeField] private bool invYAxis;


    private Quaternion initialAngle;
    private Vector3 cameraInitPos;

    private float rotX;
    private float rotZ;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        initialAngle = transform.localRotation;
        cameraInitPos = transform.localPosition;

    }

    void Update()
    {
        CameraMovement();
    }

    void CameraMovement()
    {
        ChangeLookView();
    }

    void ChangeLookView()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        if (invYAxis)
        {
            rotX += mouseY;

        }
        else rotX -= mouseY;
        
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(rotX, lockVertMin, rotZ);
        transform.parent.Rotate(Vector3.up * mouseX);
    }
    


}
