using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]


public class LightIntensity : MonoBehaviour
{
    [SerializeField] float light_intensity = 1;
    [SerializeField] float light_range = 1;
    [SerializeField] float red = 1;
    [SerializeField] float green = 1;
    [SerializeField] float blue = 1;
    [SerializeField] float alpha = 1;

    //public Vector4 manualColor;
    
    void Update()
    {
        var light = GetComponent<Light>();
        light.range = light_range;
        light.color = new Color(red, green, blue, alpha) * light_intensity;
    }
}