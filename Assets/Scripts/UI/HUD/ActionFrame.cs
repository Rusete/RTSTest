using DRC.RPG.Utils;
using DRC.RTS.Buildings.Player;
using DRC.RTS.InputManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DRC.RTS.UI.HUD
{
    public class ActionFrame : MonoBehaviour
    {
        public static ActionFrame instance = null;

        [SerializeField] private Button actionButton = null;
        [SerializeField] private Transform layoutGroup= null;

        private List<Button> buttons = new List<Button>();
        private PlayerActions actionList = null;
        private void Awake()
        {
            instance = this;
        }

        public void SetActionButtons(PlayerActions actions)
        {
            ClearActions();
            actionList = actions;

            if (actions.basicUnits.Count > 0)
            {
                foreach(var unit in actions.basicUnits)
                {
                    Button btn = Instantiate(actionButton, layoutGroup);
                    btn.GetComponent<Action>().type = Action.EActionType.InstantiateUnit;
                    btn.name = unit.name;
                    GameObject icon = Instantiate(unit.icon, btn.transform);
                    //add text?
                    buttons.Add(btn);
                }
            }
            if(actions.basicBuildings.Count > 0)
            {
                foreach (var building in actions.basicBuildings)
                {
                    Button btn = Instantiate(actionButton, layoutGroup);
                    btn.GetComponent<Action>().type = Action.EActionType.InstantiatBuilding;
                    btn.name = building.name;
                    GameObject icon = Instantiate(building.icon, btn.transform);
                    //add text?
                    buttons.Add(btn);
                }

            }
        }
        public void ClearActions()
        {
            foreach(Button btn in buttons)
            {
                Destroy(btn.gameObject);
            }
            buttons.Clear();
        }

        public void StartSpawnTimer(string objectToSpawn)
        {
            if (IsUnit(objectToSpawn))
            {
                Units.UnitData unit = IsUnit(objectToSpawn);
                InputManager.InputHandler.instance.selectedBuilding.GetComponent<PlayerBuilding>().AddSpawn(unit.spawnTime, unit.unitPrefab);
            }
            else
            {
                Debug.Log($"{objectToSpawn} is not a spawnable object");
            }
        }

        private Units.UnitData IsUnit(string name)
        {
            if (actionList.basicUnits.Count > 0)
            {
                foreach(Units.UnitData unit in actionList.basicUnits)
                {
                    if(unit.name == name)
                    {
                        return unit;
                    }
                }
            } 

            return null;
        }

        private Buildings.BuildingData IsBuilding(string name)
        {
            if (actionList.basicBuildings.Count > 0)
            {
                foreach (Buildings.BuildingData building in actionList.basicBuildings)
                {
                    if (building.name == name)
                    {
                        return building;
                    }
                }
            }
            return null;
        }

        public void InstantiateBuildingConstruction(string objectToSpawn)
        {
            if (IsBuilding(objectToSpawn))
            {
                Buildings.BuildingData building = IsBuilding(objectToSpawn);
                var obj = ObjectPoolManager.SpawnObject(building.buildingPrefab, Mouse.current.position.value, Quaternion.identity, ObjectPoolManager.PoolType.GhostPlaceable).GetComponent<Buildings.GhostPlaceable>();
                InputHandler.instance.BeginConstruction(obj.GetComponent<Buildings.GhostPlaceable>());
            }
            else
            {
                Debug.Log($"{objectToSpawn} is not a spawnable object");
            }

        }
    }
}