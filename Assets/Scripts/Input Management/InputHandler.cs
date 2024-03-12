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
using static UnityEditor.FilePathAttribute;

namespace DRC.RTS.InputManager
{
    public class InputHandler : MonoBehaviour
    {
        public static InputHandler instance;

        private RaycastHit hit;

        public HashSet<Transform> selectedUnits = new();
        public Transform selectedBuilding = null;
        public LayerMask interactableLayer = new();

        [SerializeField]private bool isDragging;
        [SerializeField]private bool isPlacing;

        private Vector3 mousePos;
        [SerializeField] Color colorRectangle;
        [SerializeField] Color colorRectangleBorder;


        public Transform mainCamera;
        [SerializeField] InputAction moveCameraAction;
        public float camMovementSpeed = 30;
        public float camRotSpeed = 50;
        public float mMarginX;
        public float mMarginY;

        private void Awake()
        {
            instance = this;
        }
        private void OnEnable()
        {
            moveCameraAction.Enable();
        }
        private void OnDisable()
        {
            moveCameraAction.Disable();
        }
        private void OnGUI()
        {
            if (isPlacing) return;
            if (isDragging)
            {
                Rect rect = Selector.GetScreenRect(mousePos, Mouse.current.position.ReadValue());
                Selector.DrawScreenRect(rect, colorRectangle);
                Selector.DrawScreenRectBorder(rect, 3, colorRectangleBorder);
            }
        }

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
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                mousePos = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                //if(NavMesh.Raycast(ray.origin, ray.direction, out hit, interactableLayer.value))
                if(Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayer))
                {
                    if (AddedUnit(hit.transform, Keyboard.current.leftShiftKey.isPressed || Keyboard.current.leftCtrlKey.isPressed))
                    {
                        //be able to do stuff with units
                    }
                    else if (AddedBuilding(hit.transform))
                    {
                         //be able to do stuff with building
                    }
                }
                else if(!isPlacing)
                {
                    isDragging = true;
                    if (!Keyboard.current.leftShiftKey.isPressed && !Keyboard.current.leftCtrlKey.isPressed) DeselectUnits();
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                foreach(Transform child in Player.PlayerManager.instance.playerUnits)
                {
                    foreach(Transform unit in child)
                    {
                        if(IsWithinSelectionBounds(unit))
                            AddedUnit(unit, true);
                    }
                }
                isDragging = false;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame && HasUnitsSelected())
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
                                        () => {
                                            hit.transform.gameObject.GetComponent<IBuilding>().AddToConstructionWorkingQueue(unit);                                                
                                        }
                                    );
                                }
                            }
                            break;
                        case 11: // EnemyBuilding Layer
                            break;
                        case 12: // Node Layer
                            print("gathering");
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
                                        () => {
                                            print("gathering");
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

                                for(int i =0; i < selectedUnits.Count(); i++)
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
            else if (Mouse.current.rightButton.wasPressedThisFrame && selectedBuilding)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out hit))
                {
                    selectedBuilding.gameObject.GetComponent<Interactables.IBuilding>().SetSpawnMarkerLocation(hit.point);

                }
            }
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

        private bool IsWithinSelectionBounds(Transform tf)
        {
            if(!isDragging)
            {
                return false;
            }

            Camera cam = Camera.main;
            Bounds vpBounds = Selector.GetViewportBounds(cam, mousePos, Mouse.current.position.ReadValue());
            return vpBounds.Contains(cam.WorldToViewportPoint(tf.position));
        }
        private bool HasUnitsSelected()
        {
            return selectedUnits.Count() > 0;
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
            PlayerManager.instance.playerState = PlayerManager.EPlayerState.placing;
        }
        public Buildings.GhostPlaceable placingObject;
        public void HandleGhost()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
            placingObject.UpdateGhostStatus(ray);
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                isPlacing = true;
                return;
            }
            if (!isPlacing) return;

            bool multiPlace = Keyboard.current.ctrlKey.isPressed;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
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
                                multiPlace
                                );
                        }
                    }
                    if (!multiPlace)
                    {
                        StopPlacingObject();
                    }
                }
                else if(!multiPlace)
                {
                    StopPlacingObject();
                }
            }
            if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                StopPlacingObject();
            }
        }

        private void StopPlacingObject()
        {
            if(placingObject) ObjectPoolManager.ReturnObjectToPool(placingObject.gameObject);
            PlayerManager.instance.playerState = PlayerManager.EPlayerState.selecting;
            isPlacing = false;
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
