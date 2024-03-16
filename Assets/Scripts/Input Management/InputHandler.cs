using UnityEngine;
using UnityEngine.InputSystem;
using DRC.RTS.Player;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace DRC.RTS.InputManager
{
    public class InputHandler : MonoBehaviour
    {

        public static InputHandler Instance { get; private set; }
        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }
        public Ray ray { get; private set; }
        private void Update()
        {
            switch (ECurrentAction)
            {
                case EActions.Placing:
                    ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
                    break;
            }
            isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
        }
        RaycastHit hit;
        public RaycastHit Hit { get { return hit; } }

        public LayerMask interactableLayer = new();
        private PlayerInputActions inputActions;
        public enum EActions
        {
            None,
            Dragging,
            Placing
        }
        public EActions ECurrentAction { get; private set; } = EActions.None;
        public bool Multi { get; private set; } = false;

        private Vector3 mousePos;
        [SerializeField] Color colorRectangle;
        [SerializeField] Color colorRectangleBorder;


        public Transform mainCamera;
        [SerializeField] InputAction moveCameraAction;
        public float camMovementSpeed = 30;
        public float camRotSpeed = 50;
        public float mMarginX;
        public float mMarginY;

        public UnityEvent SelectionBoxEnded;
        private void OnEnable()
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable();
            moveCameraAction.Enable();
            inputActions.Player.LeftClick.started += OnLeftClickStarts;
            inputActions.Player.LeftClick.canceled += OnLeftClickEnds;
            inputActions.Player.RightClick.performed += OnRightClick;
            inputActions.Player.Multi.started += OnMultiKey;
            inputActions.Player.Multi.canceled += OnMultiKeyEnds;

            SelectionBoxEnded ??= new UnityEvent();
        }
        private void OnDisable()
        {
            moveCameraAction.Disable();
            inputActions.Player.LeftClick.started -= OnLeftClickStarts;
            inputActions.Player.LeftClick.canceled -= OnLeftClickEnds;
            inputActions.Player.RightClick.performed -= OnRightClick;
            inputActions.Player.Multi.started -= OnMultiKey;
            inputActions.Player.Multi.canceled -= OnMultiKeyEnds;
        }

        private void OnGUI()
        {
            switch (ECurrentAction)
            {
                case EActions.None:
                    break;
                case EActions.Dragging:
                    Rect rect = Selector.GetScreenRect(mousePos, Mouse.current.position.ReadValue());
                    Selector.DrawScreenRect(rect, colorRectangle);
                    Selector.DrawScreenRectBorder(rect, 3, colorRectangleBorder);
                    break;
                case EActions.Placing:
                    break;
            }
        }
        bool isPointerOverUI;
        public void OnPointerEnterUI()
        {
            isPointerOverUI = true;
        }

        public void OnPointerExitUI()
        {
            isPointerOverUI = false;
        }

        private void OnLeftClickStarts(InputAction.CallbackContext context)
        {
            if (isPointerOverUI)
                return;

            switch (ECurrentAction)
            {
                case EActions.None:
                    if (!Multi) PlayerManager.Instance.DeselectUnits();
                    ECurrentAction = EActions.Dragging;
                    mousePos = Mouse.current.position.ReadValue();
                    break;
                case EActions.Dragging:
                    if (!Multi) PlayerManager.Instance.DeselectUnits();
                    ECurrentAction= EActions.None;
                    break;
                case EActions.Placing:
                    PlayerManager.Instance.Construction(Multi);
                    break;
            }
        }

        public void OnLeftClickEnds(InputAction.CallbackContext context)
        {
            if (isPointerOverUI)
                return;

            switch (ECurrentAction)
            {
                case EActions.None:
                    break;
                case EActions.Dragging:
                    // Finaliza seleccion
                    ECurrentAction = EActions.None;
                    SelectionBoxEnded.Invoke();
                    break;
                case EActions.Placing:
                    if (!Multi)
                    {
                        PlayerManager.Instance.StopPlacingObject();
                        ECurrentAction = EActions.None;
                    }
                    break;
            }
        }

        private void OnRightClick(InputAction.CallbackContext context)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit) && Hit.transform.gameObject.layer != 5)
            {
                switch (ECurrentAction)
                {
                    case EActions.None:
                        PlayerManager.Instance.HandleRightClick(Hit.transform.gameObject.layer);
                        break;

                    case EActions.Dragging:
                        ECurrentAction = EActions.None;
                        break;

                    case EActions.Placing:
                        PlayerManager.Instance.StopPlacingObject();
                        ECurrentAction = EActions.None;
                        break;
                }
            }                      
        }

        

        private void OnMultiKey(InputAction.CallbackContext context)
        {
            Multi = true;
        }
        private void OnMultiKeyEnds(InputAction.CallbackContext context)
        {
            Multi = false;
        }

        // TODO ESTO DEBE CAMBIAR AL NUEVO INPUT ACTIONS CREADO ^^
        public void HandleCamera()
        {
            // Obtiene el vector de movimiento de la acción de entrada
            Vector2 movementInput = moveCameraAction.ReadValue<Vector2>();

            float verticalInput = movementInput.y;
            float horizontalInput = movementInput.x;

            if (Keyboard.current.qKey.isPressed)
            {
                mainCamera.Rotate(Vector3.up, Time.deltaTime * camRotSpeed);
            }
            else if (Keyboard.current.eKey.isPressed)
            {
                mainCamera.Rotate(Vector3.up, -Time.deltaTime * camRotSpeed);
            }
            // Verifica si el mouse está cerca del borde de la pantalla
            if (Mouse.current.position.x.value >= Screen.width - mMarginX ||
                Mouse.current.position.x.value <= mMarginX ||
                Mouse.current.position.y.value >= Screen.height - mMarginY ||
                Mouse.current.position.y.value <= mMarginY)
            {
#if !UNITY_EDITOR
                // Calcula la dirección en la que mover la cámara
                Vector3 movementDirection = Vector3.zero;

                if (Mouse.current.position.x.value >= Screen.width - mMarginX)
                    movementDirection += Vector3.right;
                else if (Mouse.current.position.x.value <= mMarginX)
                    movementDirection -= Vector3.right;

                if (Mouse.current.position.y.value >= Screen.height - mMarginY)
                    movementDirection += Vector3.forward;
                else if (Mouse.current.position.y.value <= mMarginY)
                    movementDirection -= Vector3.forward;

                // Mueve la cámara
                mainCamera.Translate(camMovementSpeed * Time.deltaTime * movementDirection.normalized);
#endif
            }
            else
            {
                // Mueve la cámara con las teclas W/A/S/D
                mainCamera.Translate(camMovementSpeed * Time.deltaTime * verticalInput * Vector3.forward);
                mainCamera.Translate(camMovementSpeed * Time.deltaTime * horizontalInput * Vector3.right);
            }
        }

        public bool IsWithinSelectionBounds(Transform tf)
        {
            Camera cam = Camera.main;
            Bounds vpBounds = Selector.GetViewportBounds(cam, mousePos, Mouse.current.position.ReadValue());
            return vpBounds.Contains(cam.WorldToViewportPoint(tf.position));
        }
        public void PlacingState()
        {
            ECurrentAction = EActions.Placing;
        }
        /*
        public void HandleGhost()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            placingObject.UpdateGhostStatus(ray);

            switch (eCurrentAction)
            {
                case EActions.Placing:

                    bool multiPlace = Keyboard.current.ctrlKey.isPressed;
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        // se mueve
                    }
                    if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        StopPlacingObject();
                    }
                    break;
            }
        }*/
    }
}
