using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]


public class CheckpointInternalLogic : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] float light_intensity = 1;
    [SerializeField] float light_range = 1;
    [SerializeField] float red = 1;
    [SerializeField] float green = 1;
    [SerializeField] float blue = 1;
    [SerializeField] float alpha = 1;


    [SerializeField] float lightRateOfChange = 1;

    [SerializeField] private bool lightIsOn = false;
    //DO NOT CHANGE, USED FOR TESTING
    [SerializeField] private bool disabledTrigger = false;
    //DO NOT CHANGE, USED FOR TESTING

    [SerializeField] GameObject haloLight;

    private float lightWarmUp = 0.0f;

    void Start()
    {
        haloLight.SetActive(false);
        lightIsOn = false;
        var light = GetComponent<Light>();
        light.range = light_range;
        light.color = new Color(red, green, blue, alpha) * 0;
    }

    // Update is called once per frame
    void Update()
    {
        HandleLightMote();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Collider boxCollider = this.GetComponent<Collider>();
            boxCollider.enabled = false;
            ///NEED TO TEST THIS

            disabledTrigger = true;

            haloLight.SetActive(false);
        }
        else if (other.tag == "LightOrb")
        { 
            lightIsOn = true;
        }
    }


    void HandleLightMote()
    {
        if (lightIsOn)
        {

            if (disabledTrigger) //start turning the light off
            {
                lightWarmUp -= Time.deltaTime * lightRateOfChange;
                if (lightWarmUp < 0.1f)
                {
                    lightIsOn = false;
                    lightWarmUp = 0.0f;
                }
            }
            else if (lightWarmUp < 1.0f)
            {
                lightWarmUp += Time.deltaTime * lightRateOfChange;
            }

            var light = GetComponent<Light>();
            light.range = light_range;
            light.color = new Color(red, green, blue, alpha) * light_intensity * lightWarmUp;
        }

    }

    void TurnOnHalo()
    { 
        haloLight.SetActive(true);
    }
}
