using UnityEngine;


public class LightHouseRotation : MonoBehaviour
{
    [SerializeField] private GameObject Flaps;
    [SerializeField] private float rotationSpeed = 20f;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Flaps.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
