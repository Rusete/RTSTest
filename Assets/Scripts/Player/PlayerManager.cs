using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DRC.RTS.InputManager;
using DRC.RTS.Formations;
using DRC.RTS.Units;
using System.Linq;
using UnityEngine.AI;
using DRC.RPG.Utils;
using DRC.RTS.UI.HUD;

namespace DRC.RTS.Player
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Scene Data")]
        public static PlayerManager Instance;
        public Transform playerUnits;
        public Transform enemyUnits;
        public Transform playerBuildings;

        public FormationBase formationBase = null;

        public Transform SelectedBuilding {  get; private set; } = null;
        public HashSetListener<Transform> SelectedUnits { get; private set; } = new();
        public Buildings.GhostPlaceable placingObject;


        public enum EPlayerState
        {
            Placing,
            Selecting
        }

        public EPlayerState playerState = EPlayerState.Selecting;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetBasicStats(playerUnits);
            SetBasicStats(enemyUnits);
            SetBasicStats(playerBuildings);
        }

        private void Update()
        {
            
            switch (playerState)
            {
                case EPlayerState.Placing:
                    placingObject.UpdateGhostStatus(InputHandler.Instance.ray);
                    break;
                case EPlayerState.Selecting:
                    break;

            }
            InputHandler.Instance.HandleCamera();
        }

        public void SetBasicStats(Transform type)
        {

            foreach (Transform child in type)
            {
                foreach (Transform entity in child)
                {
                    string entityName = child.name.Substring(0, child.name.Length - 1).ToLower();
                    //var stats = Units.UnitHandler.instance.GetBasicUnitStats(unitName);

                    if (type == playerUnits)
                    {
                        Units.Player.PlayerUnit pU = entity.GetComponent<Units.Player.PlayerUnit>();
                        pU.baseStats = Units.UnitHandler.instance.GetBasicUnitStats(entityName);
                        pU.GetComponent<Interactables.Health>().SetupHealth();
                    }
                    else if (type == enemyUnits)
                    {
                        Units.Enemy.EnemyUnit eU = entity.GetComponent<Units.Enemy.EnemyUnit>();
                        eU.baseStats = Units.UnitHandler.instance.GetBasicUnitStats(entityName);
                        eU.GetComponent<Interactables.Health>().SetupHealth();
                    }
                    else if (type == playerBuildings)
                    {
                        Buildings.Player.PlayerBuilding pB = entity.GetComponent<Buildings.Player.PlayerBuilding>();
                        pB.baseStats = Buildings.BuildingsHandler.instance.GetBasicBuildingStats(entityName);
                        pB.GetComponent<Interactables.Health>().SetupHealth();
                    }
                }
            }
        }
        public class HashSetListener<T> : HashSet<Transform>
        {
            public new bool Remove(Transform item)
            {
                item.GetComponent<Interactables.Health>().Killed.RemoveListener(() =>
                {
                    base.Remove(item);
                });;
                return base.Remove(item);
            }
        }
        public void RemoveFromSelection(Transform tf)
        {
            SelectedUnits.Remove(tf);
        }

        public void AddToSelection(Transform tf)
        {
            if (AddedUnit(tf, InputHandler.Instance.Multi))
            {
                tf.GetComponent<Interactables.Health>().Killed.AddListener(() =>
                {
                    SelectedUnits.Remove(tf);
                });
            }
            else if (AddedBuilding(tf))
            {
                tf.GetComponent<Interactables.Health>().Killed.AddListener(() =>
                {
                    SelectedBuilding = null;
                    
                });
            }
            else { Debug.LogWarning("no es ni edificio ni unidad"); }
        }

        public void Construction(bool multi)
        {
            GameObject building = placingObject.Place();
            if (building)
            {
                foreach (var unit in SelectedUnits)
                {
                    if (unit.GetComponent<Interactables.IUnit>().unitType.type == Units.UnitData.EUnitType.Worker)
                    {
                        unit.GetComponent<Units.Player.PlayerUnit>().MoveTo(
                            building.transform.position,
                            unit.gameObject.GetComponent<NavMeshAgent>().stoppingDistance,
                            () =>
                            {
                                building.GetComponent<Interactables.IBuilding>().AddToConstructionWorkingQueue(unit);
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
        }

        public void StopPlacingObject()
        {
            if (!placingObject) return;
            ObjectPoolManager.ReturnObjectToPool(placingObject.gameObject);
            playerState = EPlayerState.Selecting;
        }

        public void HandleRightClick(int layer)
        {
            if (SelectedUnits.Count() > 0)
            {
                HandleUnitsAction(layer);
            }
            else if (SelectedBuilding)
            {
                SelectedBuilding.gameObject.GetComponent<Interactables.IBuilding>().SetSpawnMarkerLocation(InputHandler.Instance.Hit.point);

            }
        }

        private void HandleUnitsAction(int layer)
        {
            switch (layer)
            {
                case 8: //PlayerUnits Layer
                        // do something
                    break;
                case 9: //EnemyUnits Layer
                        // attack or set target
                    break;
                case 10: // Building Layer
                    foreach (var unit in SelectedUnits)
                    {
                        if (unit.GetComponent<Interactables.IUnit>().unitType.type == Units.UnitData.EUnitType.Worker)
                        {
                            var collider = InputHandler.Instance.Hit.transform.GetComponent<Collider>();
                            if (!collider)
                            {
                                return; // nothing to do without a collider
                            }

                            Vector3 closestPoint = collider.ClosestPoint(unit.transform.position);
                            unit.GetComponent<Units.Player.PlayerUnit>().MoveTo(closestPoint,
                                unit.gameObject.GetComponent<NavMeshAgent>().stoppingDistance,
                                () =>
                                {
                                    InputHandler.Instance.Hit.transform.gameObject.GetComponent<Interactables.IBuilding>().AddToConstructionWorkingQueue(unit);
                                }
                            );
                        }
                    }
                    break;
                case 11: // EnemyBuilding Layer
                    break;
                case 12: // Node Layer
                    foreach (var unit in SelectedUnits)
                    {
                        if (unit.GetComponent<Interactables.IUnit>().unitType.type == Units.UnitData.EUnitType.Worker)
                        {
                            var collider = InputHandler.Instance.Hit.transform.GetComponent<Collider>();
                            if (!collider)
                            {
                                return; // nothing to do without a collider
                            }
                            Vector3 closestPoint = collider.ClosestPoint(unit.transform.position);
                            unit.GetComponent<Units.Player.PlayerUnit>().MoveTo(closestPoint,
                                unit.gameObject.GetComponent<NavMeshAgent>().stoppingDistance,
                                () =>
                                {
                                    StartCoroutine(unit.GetComponent<Interactables.IUnit>().Gather(InputHandler.Instance.Hit.transform.parent.GetComponent<ResourceNode>()));
                                }
                            );
                        }
                    }
                    break;
                default:
                    // Ahora mismo todas las unidades forman.
                    // Buscar la lógica para que los trabajadores no se unan a la formación
                    // Ordenar unidades por rango en la lista
                    if (formationBase != null)
                    {
                        var forward = (InputHandler.Instance.Hit.point - SelectedUnits.ElementAt(0).transform.position).normalized;
                        var relativePositions = PlayerManager.Instance.formationBase.EvaluatePoints(
                            forward,
                            SelectedUnits.Count
                        );

                        for (int i = 0; i < SelectedUnits.Count(); i++)
                        {
                            Units.Player.PlayerUnit pU = SelectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                            pU.MoveTo(relativePositions.ElementAt(i) + InputHandler.Instance.Hit.point, 0, () =>
                            {
                                //set idle animation
                            });
                        }
                    }
                    else
                    {
                        for (int i = 0; i < SelectedUnits.Count(); i++)
                        {
                            Units.Player.PlayerUnit pU = SelectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                            pU.MoveTo(InputHandler.Instance.Hit.point, 0f, () =>
                            {
                                //set idle animation
                            });
                        }
                    }
                    break;
            }
        }

        public void DeselectUnits()
        {
            if (SelectedBuilding != null)
            {
                SelectedBuilding.gameObject.GetComponent<Interactables.IBuilding>().OnInteractExit();
                SelectedBuilding = null;
            }
            for (int i = 0; i < SelectedUnits.Count(); i++)
            {
                SelectedUnits.ElementAt(i).gameObject.GetComponent<Interactables.IUnit>().OnInteractExit();
            }
            SelectedUnits.Clear();
            ActionFrame.instance.ClearActions();
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

                SelectedUnits.Add(iUnit.gameObject.transform);

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
            Interactables.IBuilding iBuilding = tf.GetComponent<Interactables.IBuilding>();
            if (iBuilding)
            {
                DeselectUnits();

                SelectedBuilding = iBuilding.gameObject.transform;

                iBuilding.OnInteractEnter();

                return iBuilding;
            }
            else
            {
                return null;
            }
        }

        public void FormateToPoints()
        {
            if (formationBase != null)
            {
                var forward = (InputHandler.Instance.Hit.point - SelectedUnits.ElementAt(0).transform.position).normalized;
                var relativePositions = formationBase.EvaluatePoints(
                    forward,
                    SelectedUnits.Count
                );

                for (int i = 0; i < SelectedUnits.Count(); i++)
                {
                    Units.Player.PlayerUnit pU = SelectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                    pU.MoveTo(relativePositions.ElementAt(i) + InputHandler.Instance.Hit.point, 0, () =>
                    {
                        //set idle animation
                    });
                }
            }
            else
            {
                for (int i = 0; i < SelectedUnits.Count(); i++)
                {
                    Units.Player.PlayerUnit pU = SelectedUnits.ElementAt(i).gameObject.GetComponent<Units.Player.PlayerUnit>();
                    pU.MoveTo(InputHandler.Instance.Hit.point, 0f, () =>
                    {
                        //set idle animation
                    });
                }
            }
        }
        public void BeginToPlaceGhostBuilding(Buildings.GhostPlaceable objectToPlace)
        {
            placingObject = objectToPlace;
            playerState = EPlayerState.Placing;
        }
    }
}