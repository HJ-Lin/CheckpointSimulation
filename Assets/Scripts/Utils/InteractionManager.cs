using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask interactableLayers = -1; // Everything by default

    private Camera mainCamera;
    private InputAction clickAction;
    private SimulationController controller;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null) mainCamera = Camera.main;

        // Alternative: use InputSystem without generated code:
        clickAction = new InputAction("Click", binding: "<Mouse>/leftButton");
        clickAction.performed += OnClickPerformed;
        clickAction.Enable();
    }

    void Start()
    {
        controller = SimulationController.Instance;
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayers))
        {
            // Check for lane waypoint holder first (because both lane and counter may have colliders)
            LaneWaypointHolder lane = hit.collider.GetComponent<LaneWaypointHolder>();
            if (lane != null)
            {
                lane.OnClicked();
                return;
            }

            SecurityCounter sec = hit.collider.GetComponent<SecurityCounter>();
            if (sec != null)
            {
                sec.OnClicked();
                return;
            }

            ImmigrationCounter imm = hit.collider.GetComponent<ImmigrationCounter>();
            if (imm != null)
            {
                imm.OnClicked();
                return;
            }

            controller.SelectedCounter = null;
        }
    }

    private void OnDestroy()
    {
        clickAction?.Dispose();
    }
}