using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using DRC.RTS.Player;
using System.Linq;
using UnityEngine.EventSystems;
using DRC.RTS.Buildings;
using DRC.RTS.Interactables;

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

        private Vector3 mousePos;
        [SerializeField] Color colorRectangle;
        [SerializeField] Color colorRectangleBorder;

        private void Awake()
        {
            instance = this;
        }
        
        private void OnGUI()
        {
            if (isDragging)
            {
                Rect rect = Selector.GetScreenRect(mousePos, Mouse.current.position.ReadValue());
                Selector.DrawScreenRect(rect, colorRectangle);
                Selector.DrawScreenRectBorder(rect, 3, colorRectangleBorder);
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
                if(Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayer))
                {
                    if (addedUnit(hit.transform, Keyboard.current.leftShiftKey.isPressed || Keyboard.current.leftCtrlKey.isPressed))
                    {
                        //be able to do stuff with units
                    }
                    else if (addedBuilding(hit.transform))
                    {
                         //be able to do stuff with building
                    }
                }
                else
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
                            addedUnit(unit, true);
                    }
                }
                isDragging = false;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame && HasUnitsSelected())
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(ray, out hit))
                {
                    LayerMask layerHit = hit.transform.gameObject.layer;

                    switch (layerHit.value)
                    {
                        case 8: //PlayerUnits Layer
                            // do something
                            break;
                        case 9: //EnemyUnits Layer
                            // attack or set target
                            break;
                        default:
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
                                    pU.MoveUnit(relativePositions.ElementAt(i) + hit.point);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < selectedUnits.Count(); i++)
                                {
                                    Units.Player.PlayerUnit pU = selectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                                    pU.MoveUnit(hit.point);
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

        private Interactables.IUnit addedUnit(Transform tf, bool canMultiselect = false)
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

        private Interactables.IBuilding addedBuilding(Transform tf)
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
    }
}
