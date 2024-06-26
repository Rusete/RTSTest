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
using UnityEngine.UI;
using UnityEngine.Events;

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
        public HashSetListener SelectedUnits { get; private set; } = new();
        public Buildings.GhostPlaceable placingObject;
        public List<Transform> storageBuildings;


        public Resources.GatheringBag<Resources.GameResources.EResourceType, int> storage = new();

        public InputHandler.EActions EPlayerAction
        {
            get { return InputHandler.Instance.ECurrentAction; }
        }
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetBasicStats(playerUnits);
            SetBasicStats(enemyUnits);
            SetBasicStats(playerBuildings);
            InitializeStorageBuildings();
        }

        private void InitializeStorageBuildings()
        {
            GameObject[] storageObjects = GameObject.FindGameObjectsWithTag("Storage");
            foreach (GameObject storageObject in storageObjects)
            {
                storageBuildings.Add(storageObject.transform);
            }
        }

        public void AddToStorage(Storage storage)
        {
            storageBuildings.Add(storage.transform);
        }

        public Interactables.IBuilding FindClosestStorage(Vector3 position)
        {
            Transform closestStorage = null;
            float shortestDistance = Mathf.Infinity;

            foreach (Transform storage in storageBuildings)
            {
                float distance = Vector3.Distance(position, storage.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestStorage = storage;
                }
            }
            return closestStorage.GetComponent<Interactables.IBuilding>();
        }

        private void Update()
        {
            switch (EPlayerAction)
            {
                case InputHandler.EActions.None:
                    break;
                case InputHandler.EActions.Placing:
                    placingObject.UpdateGhostStatus(InputHandler.Instance.ray);
                    break;
                case InputHandler.EActions.Dragging:
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
        [System.Serializable]
        public class HashSetListener : HashSet<Transform>
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
            if (AddedUnit(tf))
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
            var collider = InputHandler.Instance.Hit.transform.GetComponent<Collider>();
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
                        var pU = unit.GetComponent<Units.Player.PlayerUnit>();
                        if (pU.unitType.type == Units.UnitData.EUnitType.Worker)
                        {
                            if (!collider)
                            {
                                return; // nothing to do without a collider
                            }

                            Vector3 closestPoint = collider.ClosestPoint(unit.transform.position);
                            pU.MoveTo(closestPoint,
                                collider.bounds.size.magnitude + pU.MeleeStoppingStoppingDistance,
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
                        var pU = unit.GetComponent<Units.Player.PlayerUnit>();
                        if (pU.unitType.type == Units.UnitData.EUnitType.Worker)
                        {
                            if (!collider)
                            {
                                return; // nothing to do without a collider
                            }
                            Vector3 closestPoint = collider.ClosestPoint(unit.transform.position);
                            pU.MoveTo(closestPoint,
                                collider.bounds.size.magnitude + pU.MeleeStoppingStoppingDistance,
                                () =>
                                {
                                    StartCoroutine(pU.Gather(InputHandler.Instance.Hit.transform.GetComponent<Resources.ResourceNode>()));
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
        private Interactables.IUnit AddedUnit(Transform tf)
        {
            Interactables.IUnit iUnit = tf.GetComponent<Interactables.IUnit>();
            if (iUnit)
            {
                SelectedUnits.Add(iUnit.gameObject.transform);

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
        }

        public void StoreResources(Resources.GatheringBag<Resources.GameResources.EResourceType, int> Resources)
        {
            Debug.Log("Se añade lo siguiente:");
            foreach (KeyValuePair<Resources.GameResources.EResourceType, int> entry in Resources)
            {
                Debug.Log(entry.Key + ": " + entry.Value);
                storage.AddOrUpdate(entry.Key, entry.Value);
            }

            Debug.Log("Total de recursos:");
            foreach (KeyValuePair<Resources.GameResources.EResourceType, int> entry in storage)
            {
                Debug.Log(entry.Key+ ": " + entry.Value);
            }
        }
    }
}