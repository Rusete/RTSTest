using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DRC.RTS.InputManager;
using DRC.RTS.Formations;
using DRC.RTS.Units;

namespace DRC.RTS.Player
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager instance;
        public Transform playerUnits;
        public Transform enemyUnits;
        public Transform playerBuildings;

        public FormationBase formationBase = null;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            SetBasicStats(playerUnits);
            SetBasicStats(enemyUnits);
            SetBasicStats(playerBuildings);
        }

        private void Update()
        {
            InputHandler.instance.HandleUnitMovement();
            InputHandler.instance.HandleCamera();
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
                        pU.GetComponent<Health>().SetupHealth();
                    }
                    else if (type == enemyUnits)
                    {
                        Units.Enemy.EnemyUnit eU = entity.GetComponent<Units.Enemy.EnemyUnit>();
                        eU.baseStats = Units.UnitHandler.instance.GetBasicUnitStats(entityName);
                        eU.GetComponent<Health>().SetupHealth();
                    }
                    else if (type == playerBuildings)
                    {
                        Buildings.Player.PlayerBuilding pB = entity.GetComponent<Buildings.Player.PlayerBuilding>();
                        pB.baseStats = Buildings.BuildingsHandler.instance.GetBasicBuildingStats(entityName);
                        pB.GetComponent<Health>().SetupHealth();
                    }
                }
            }
        }
    }
}