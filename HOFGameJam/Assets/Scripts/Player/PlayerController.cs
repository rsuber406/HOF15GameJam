using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Color debugTextColor = Color.white;
    private GUIStyle debugTextStyle;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Transform cameraTransform;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private int maxJumps = 1;
    [SerializeField] private float gravityStrength = 9.81f;
    [SerializeField] private float cameraInversionSpeed = 2f;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private bool showGroundCheckDebug = true;

    [Header("Death Settings")]
    [SerializeField] private Vector3 initialCheckpoint = Vector3.zero;
    [SerializeField] private float fallDeathY = 100f;

    [Header("Other Settings")]
    [SerializeField] private KeyCode gravityInvertKey = KeyCode.G;
    [SerializeField] private float gravityTransitionDuration = 1.0f;
    [SerializeField] private float gravityInversionCooldown = 1.5f;
    [SerializeField] private float timeSlowFactor = 0.5f;
    
    private PlayerStateMachine stateMachine;
    
    private GroundedState groundedState;
    private AirborneState airborneState;
    private GravityTransitionState gravityTransitionState;
    private DeathState deathState;
    
    public Vector3 Velocity { get; set; }
    public Vector3 LastValidCheckpoint { get; set; }
    public int JumpCount { get; set; }
    public bool IsGrounded { get; set; }
    public bool IsGravityInverted { get; set; }
    public Quaternion TargetCameraRotation { get; set; }
    public float JumpCooldown { get; set; }
    public float GravityTransitionTimer { get; set; }
    public float GravityInversionCooldownTimer { get; set; }
    public float CurrentGravityFactor { get; set; } = 1f;
    public bool IsTransitioning { get; set; }
    public float DefaultTimeScale { get; private set; }
    public Quaternion InitialCameraRotation { get; set; }
    public bool CanInvert { get; set; }

    
    public CharacterController Controller => controller;
    public Transform PlayerModel => playerModel;
    public Transform CameraTransform => cameraTransform;
    public float MoveSpeed => moveSpeed;
    public float JumpHeight => jumpHeight;
    public int MaxJumps => maxJumps;
    public float GravityStrength => gravityStrength;
    public float CameraInversionSpeed => cameraInversionSpeed;
    public float GroundCheckRadius => groundCheckRadius;
    public float GroundCheckDistance => groundCheckDistance;
    public float FallDeathY => fallDeathY;
    public KeyCode GravityInvertKey => gravityInvertKey;
    public float GravityTransitionDuration => gravityTransitionDuration;
    public float GravityInversionCooldown => gravityInversionCooldown;
    public float TimeSlowFactor => timeSlowFactor;

    public GroundedState GetGroundedState() => groundedState;
    public AirborneState GetAirborneState() => airborneState;
    public GravityTransitionState GetGravityTransitionState() => gravityTransitionState;
    public DeathState GetDeathState() => deathState;

    private void Awake()
    {
        stateMachine = new PlayerStateMachine();
        groundedState = new GroundedState(this, stateMachine);
        airborneState = new AirborneState(this, stateMachine);
        gravityTransitionState = new GravityTransitionState(this, stateMachine);
        deathState = new DeathState(this, stateMachine);
    }

    private void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
            
        LastValidCheckpoint = initialCheckpoint;
        TargetCameraRotation = cameraTransform.localRotation;
        DefaultTimeScale = Time.timeScale;
        CurrentGravityFactor = 1f;
        
        debugTextStyle = new GUIStyle();
        debugTextStyle.fontSize = 18;
        debugTextStyle.fontStyle = FontStyle.Bold;
        debugTextStyle.normal.textColor = debugTextColor;
        
        stateMachine.Initialize(groundedState);
    }

    private void Update()
    {
        if (JumpCooldown > 0)
            JumpCooldown -= Time.deltaTime;
            
        if (GravityInversionCooldownTimer > 0)
            GravityInversionCooldownTimer -= Time.deltaTime;
        
        stateMachine.CurrentState.HandleInput();
        stateMachine.CurrentState.Update();
        
        UpdateCameraRotation();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            LastValidCheckpoint = transform.position;
        }
        else if (other.CompareTag("AbilityTrigger"))
        {
            CanInvert = true;
            GameManager.instance.toolTip.SetActive(true);
            StartCoroutine(wait());
        }
        else if (other.CompareTag("GameWin"))
        { 
            GameManager.instance.Win();
        }
    }

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(5.0f);
        GameManager.instance.toolTip.SetActive(false);
    }

    public bool GetFlip()
    {
        return IsGravityInverted;
    }

    public void UpdateCameraRotation()
    {
    
        if (!IsTransitioning)
            return;
        
        float progress = GravityTransitionTimer / GravityTransitionDuration;
        float transitionCurve = progress * progress * (3f - 2f * progress); //smooth step transition
    
        float targetAngle = IsGravityInverted ? 0f : 180f;
        Quaternion targetRotation = Quaternion.Euler(targetAngle, PlayerModel.localEulerAngles.y, 0f);
        PlayerModel.localRotation = Quaternion.Slerp(InitialCameraRotation, targetRotation, transitionCurve);
    }

    public bool CheckGrounded()
    {
        float offsetFromCenter = Controller.height / 2;
        Vector3 origin = transform.position;

        int gravityDirection = CurrentGravityFactor < 0 ? -1 : 1;

        if (gravityDirection < 0)
        {
            origin.y += offsetFromCenter - GroundCheckDistance * 0.2f;
        }
        else
        {
            origin.y -= offsetFromCenter - GroundCheckDistance * 0.2f;
        }

        Vector3 direction = gravityDirection < 0 ? Vector3.up : Vector3.down;
        
        float actualCheckDistance = GroundCheckDistance + 0.05f;

        Collider[] hitColliders = Physics.OverlapSphere(origin + direction * actualCheckDistance, GroundCheckRadius);

        bool foundGround = false;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject)
                continue;

            if (hitCollider.CompareTag("Ground"))
            {
                foundGround = true;
                break;
            }
        }
        
        if (!foundGround)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 rayOrigin = origin;
                if (i > 0)
                {
                    float angle = i * 90f;
                    rayOrigin += new Vector3(Mathf.Cos(angle) * GroundCheckRadius * 0.75f, 0, Mathf.Sin(angle) * GroundCheckRadius * 0.75f);
                }
                
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, direction, out hit, actualCheckDistance + GroundCheckRadius, 
                                   Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        foundGround = true;
                        break;
                    }
                }
            }
        }
        
        return foundGround;
    }

    public void RespawnAtCheckpoint()
    {
        Controller.enabled = false;
        transform.position = LastValidCheckpoint;
        Velocity = Vector3.zero;
        
        if (IsGravityInverted)
        {
            IsGravityInverted = false;
            CurrentGravityFactor = 1f;
            PlayerModel.localRotation = Quaternion.identity;
            TargetCameraRotation = Quaternion.identity;
        }
        
        Controller.enabled = true;
        IsTransitioning = false;
        Time.timeScale = DefaultTimeScale;
    }

    public bool IsPlayerGrounded()
    {
        return IsGrounded;
    }

    public bool IsGravityInvertedState()
    {
        return IsGravityInverted;
    }

    public bool IsTransitioningGravityState()
    {
        return IsTransitioning;
    }

    public void SetCheckpoint(Vector3 position)
    {
        LastValidCheckpoint = position;
    }

    private void OnDisable()
    {
        Time.timeScale = DefaultTimeScale;
    }
    
    private void OnGUI()
    {
        if (showDebugInfo && stateMachine != null && stateMachine.CurrentState != null)
        {
            string stateName = stateMachine.CurrentState.GetType().Name;
            string gravityInfo = IsGravityInverted ? "Inverted" : "Normal";
            string groundedInfo = IsGrounded ? "Grounded" : "Airborne";
            
            GUI.Label(new Rect(10, 10, 300, 20), $"State: {stateName}", debugTextStyle);
            GUI.Label(new Rect(10, 30, 300, 20), $"Gravity: {gravityInfo} ({CurrentGravityFactor})", debugTextStyle);
            GUI.Label(new Rect(10, 50, 300, 20), $"Grounded: {groundedInfo}", debugTextStyle);
            GUI.Label(new Rect(10, 70, 300, 20), $"Jump Count: {JumpCount}/{MaxJumps}", debugTextStyle);
            
            if (IsTransitioning)
            {
                GUI.Label(new Rect(10, 90, 300, 20), $"Transition: {GravityTransitionTimer}/{GravityTransitionDuration}", debugTextStyle);
            }
            
            if (GravityInversionCooldownTimer > 0)
            {
                GUI.Label(new Rect(10, 110, 300, 20), $"Cooldown: {GravityInversionCooldownTimer:F2}", debugTextStyle);
            }
        }
    }
}