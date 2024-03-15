using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using DRC.RTS.Player;
using System.Linq;
using UnityEngine.EventSystems;
using DRC.RPG.Utils;
using DRC.RTS.UI.HUD;
using UnityEngine.AI;
using DRC.RTS.Interactables;
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
        private void Update()
        {
            isPointerOverUI = EventSystem.current.IsPointerOverGameObject();
            switch (eCurrentAction)
            {
                case EActions.Placing:
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
                    placingObject.UpdateGhostStatus(ray);
                    break;
            }
        }
        private RaycastHit hit;

        public HashSet<Transform> selectedUnits = new();
        public Transform selectedBuilding = null;
        public LayerMask interactableLayer = new();
        private PlayerInputActions inputActions;
        enum EActions
        {
            None,
            Dragging,
            Placing
        }
        EActions eCurrentAction = EActions.None;
        private bool multi = false;

        private Vector3 mousePos;
        [SerializeField] Color colorRectangle;
        [SerializeField] Color colorRectangleBorder;


        public Transform mainCamera;
        [SerializeField] InputAction moveCameraAction;
        public float camMovementSpeed = 30;
        public float camRotSpeed = 50;
        public float mMarginX;
        public float mMarginY;
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
            switch (eCurrentAction)
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

            switch (eCurrentAction)
            {
                case EActions.None:
                    // Inicia selección
                    eCurrentAction = EActions.Dragging;
                    mousePos = Mouse.current.position.ReadValue();
                    break;
                case EActions.Dragging:
                    if (!multi) DeselectUnits();
                    eCurrentAction= EActions.None;
                    break;
                case EActions.Placing:
                    GameObject building = placingObject.Place();
                    if (building)
                    {
                        foreach (var unit in selectedUnits)
                        {
                            if (unit.GetComponent<Interactables.IUnit>().unitType.type == Units.UnitData.EUnitType.Worker)
                            {
                                unit.GetComponent<Units.Player.PlayerUnit>().MoveTo(
                                    building.transform.position,
                                    unit.gameObject.GetComponent<NavMeshAgent>().stoppingDistance,
                                    () =>
                                    {
                                        building.GetComponent<IBuilding>().AddToConstructionWorkingQueue(unit);
                                    },
                                    multi
                                    );
                            }
                        }
                        if (!multi)
                        {
                            StopPlacingObject();
                        }
                    }
                    else if (!multi)
                    {
                        StopPlacingObject();
                    }
                    break;
            }
        }

        public UnityEvent SelectionBoxEnded;

        public void OnLeftClickEnds(InputAction.CallbackContext context)
        {
            if (isPointerOverUI)
                return;

            switch (eCurrentAction)
            {
                case EActions.None:
                    break;
                case EActions.Dragging:
                    Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                    // Finaliza seleccion
                    eCurrentAction = EActions.None;
                    SelectionBoxEnded.Invoke();
                    break;
                case EActions.Placing:
                    if (!multi)
                    {
                        StopPlacingObject();
                        eCurrentAction = EActions.None;
                    }
                    break;
            }
        }
        private void OnRightClick(InputAction.CallbackContext context)
        {
            if (isPointerOverUI)
                return;

            switch (eCurrentAction)
            {
                case EActions.None:
                    if (HasUnitsSelected())
                    {
                        HandleUnitsAction();
                    }
                    else if (selectedBuilding)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                        if (Physics.Raycast(ray, out hit))
                        {
                            selectedBuilding.gameObject.GetComponent<Interactables.IBuilding>().SetSpawnMarkerLocation(hit.point);

                        }
                    }
                    break;
                case EActions.Dragging:
                    eCurrentAction = EActions.None;
                    break;
                case EActions.Placing:
                    StopPlacingObject();
                    eCurrentAction = EActions.None;
                    break;
            }            
        }

        private void HandleUnitsAction()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit))
            {
                switch (hit.transform.gameObject.layer)
                {
                    case 8: //PlayerUnits Layer
                            // do something
                        break;
                    case 9: //EnemyUnits Layer
                            // attack or set target
                        break;
                    case 10: // Building Layer
                        foreach (var unit in selectedUnits)
                        {
                            if (unit.GetComponent<Interactables.IUnit>().unitType.type == Units.UnitData.EUnitType.Worker)
                            {
                                var collider = hit.transform.GetComponent<Collider>();
                                if (!collider)
                                {
                                    return; // nothing to do without a collider
                                }

                                Vector3 closestPoint = collider.ClosestPoint(unit.transform.position);
                                unit.GetComponent<Units.Player.PlayerUnit>().MoveTo(closestPoint,
                                    unit.gameObject.GetComponent<NavMeshAgent>().stoppingDistance,
                                    () =>
                                    {
                                        hit.transform.gameObject.GetComponent<IBuilding>().AddToConstructionWorkingQueue(unit);
                                    }
                                );
                            }
                        }
                        break;
                    case 11: // EnemyBuilding Layer
                        break;
                    case 12: // Node Layer
                        foreach (var unit in selectedUnits)
                        {
                            if (unit.GetComponent<Interactables.IUnit>().unitType.type == Units.UnitData.EUnitType.Worker)
                            {
                                var collider = hit.transform.GetComponent<Collider>();
                                if (!collider)
                                {
                                    return; // nothing to do without a collider
                                }
                                Vector3 closestPoint = collider.ClosestPoint(unit.transform.position);
                                unit.GetComponent<Units.Player.PlayerUnit>().MoveTo(closestPoint,
                                    unit.gameObject.GetComponent<NavMeshAgent>().stoppingDistance,
                                    () =>
                                    {
                                        StartCoroutine(unit.GetComponent<Interactables.IUnit>().Gather(hit.transform.parent.GetComponent<ResourceNode>()));
                                    }
                                );
                            }
                        }
                        break;
                    default:
                        // Ahora mismo todas las unidades forman.
                        // Buscar la lógica para que los trabajadores no se unan a la formación
                        // Ordenar unidades por rango en la lista
                        if (PlayerManager.instance.formationBase != null)
                        {
                            var forward = (hit.point - selectedUnits.ElementAt(0).transform.position).normalized;
                            var relativePositions = PlayerManager.instance.formationBase.EvaluatePoints(
                                forward,
                                selectedUnits.Count
                            );

                            for (int i = 0; i < selectedUnits.Count(); i++)
                            {
                                Units.Player.PlayerUnit pU = selectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                                pU.MoveTo(relativePositions.ElementAt(i) + hit.point, 0, () =>
                                {
                                    //set idle animation
                                });
                            }
                        }
                        else
                        {
                            for (int i = 0; i < selectedUnits.Count(); i++)
                            {
                                Units.Player.PlayerUnit pU = selectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                                pU.MoveTo(hit.point, 0f, () =>
                                {
                                    //set idle animation
                                });
                            }
                        }
                        break;
                }
            }
        }

        private void OnMultiKey(InputAction.CallbackContext context)
        {
            multi = true;
        }
        private void OnMultiKeyEnds(InputAction.CallbackContext context)
        {
            multi = false;
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
        
        public void HandleUnitMovement()
        {
            
        }

        private void TriggerSelectUnit(Transform unit)
        {
            if (!unit.Find("Highlight").gameObject.activeInHierarchy)
            {
                selectedUnits.Add(unit);
                // lets set an obj Highlight
                unit.Find("Highlight").gameObject.SetActive(true);
            }
            else
            {
                selectedUnits.Remove(unit);
                // lets set an obj Highlight
                unit.Find("Highlight").gameObject.SetActive(false);
            }
        }

        private void DeselectUnits()
        {
            if(selectedBuilding != null)
            {
                selectedBuilding.gameObject.GetComponent<Interactables.IBuilding>().OnInteractExit();
                selectedBuilding = null;
            }
            for (int i = 0; i < selectedUnits.Count(); i ++) 
            {
                selectedUnits.ElementAt(i).gameObject.GetComponent<Interactables.IUnit>().OnInteractExit();
            }
            selectedUnits.Clear();
            ActionFrame.instance.ClearActions();
        }

        public bool IsWithinSelectionBounds(Transform tf)
        {
            Camera cam = Camera.main;
            Bounds vpBounds = Selector.GetViewportBounds(cam, mousePos, Mouse.current.position.ReadValue());
            return vpBounds.Contains(cam.WorldToViewportPoint(tf.position));
        }
        private bool HasUnitsSelected()
        {
            return selectedUnits.Count() > 0;
        }

        public void AddToSelection(Transform tf)
        {
            if(AddedUnit(tf, multi)){

            }
            else if (AddedBuilding(tf))
            {

            }
            else { Debug.LogWarning("no es ni edificio ni unidad"); }
        }
        private Interactables.IUnit AddedUnit(Transform tf, bool canMultiselect = false)
        {
            Interactables.IUnit iUnit = tf.GetComponent<Interactables.IUnit>();
            if (iUnit)
            {
                if (!canMultiselect)
                {
                    DeselectUnits();
                }

                selectedUnits.Add(iUnit.gameObject.transform);

                iUnit.OnInteractEnter();

                return iUnit;
            }
            else
            {
                return null;
            }
        }

        private Interactables.IBuilding AddedBuilding(Transform tf)
        {
            Interactables.IBuilding iBuilding= tf.GetComponent<Interactables.IBuilding>();
            if (iBuilding)
            {
                DeselectUnits();

                selectedBuilding = iBuilding.gameObject.transform;

                iBuilding.OnInteractEnter();

                return iBuilding;
            }
            else
            {
                return null;
            }
        }
        public void BeginConstruction(Buildings.GhostPlaceable objectToPlace)
        {
            placingObject = objectToPlace;
            eCurrentAction = EActions.Placing;
        }
        public Buildings.GhostPlaceable placingObject;
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

        private void StopPlacingObject()
        {
            if (!placingObject) return; 
            ObjectPoolManager.ReturnObjectToPool(placingObject.gameObject);            
            PlayerManager.instance.playerState = PlayerManager.EPlayerState.selecting;
        }

        public void FormateToPoints()
        {
            if (PlayerManager.instance.formationBase != null)
            {
                var forward = (hit.point - selectedUnits.ElementAt(0).transform.position).normalized;
                var relativePositions = PlayerManager.instance.formationBase.EvaluatePoints(
                    forward,
                    selectedUnits.Count
                );

                for (int i = 0; i < selectedUnits.Count(); i++)
                {
                    Units.Player.PlayerUnit pU = selectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                    pU.MoveTo(relativePositions.ElementAt(i) + hit.point, 0, () =>
                    {
                        //set idle animation
                    });
                }
            }
            else
            {
                for (int i = 0; i < selectedUnits.Count(); i++)
                {
                    Units.Player.PlayerUnit pU = selectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                    pU.MoveTo(hit.point, 0f, () =>
                    {
                        //set idle animation
                    });
                }
            }
        }
    }
}
