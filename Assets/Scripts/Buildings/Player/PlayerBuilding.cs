using DRC.RTS.Interactables;
using DRC.RTS.Player;
using DRC.RTS.UI.HUD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Buildings.Player
{
    [RequireComponent(typeof(ActionTimer))]
    public class PlayerBuilding : IBuilding
    {

        public List<float> spawnQueue = new List<float>();
        public List<GameObject> spawnOrder = new List<GameObject>();

        public GameObject spawnPoint = null;
        ActionTimer actionTimer;

        private void Start()
        {
            actionTimer = GetComponent<ActionTimer>();
        }

        public override void OnInteractEnter()
        {
            if (isConstructed)
            {
                UI.HUD.ActionFrame.instance.SetActionButtons(actions);
                spawnMarkerGraphics.SetActive(true);
                GetComponent<PlayerBuilding>().SetSpawnLocation(spawnMarker);
            }

            base.OnInteractEnter();
        }
        public void SpawnObject()
        {
            GameObject spawnedObject = Instantiate(spawnOrder[0], spawnPoint.transform.parent.position, Quaternion.identity, PlayerManager.instance.playerUnits.Find(spawnOrder[0].name + "s"));
            spawnedObject.name = spawnOrder[0].name;
            Units.Player.PlayerUnit pU = spawnedObject.GetComponent<Units.Player.PlayerUnit>();

            pU.baseStats = Units.UnitHandler.instance.GetBasicUnitStats(spawnedObject.name.ToLower());
            pU.GetComponent<Health>().SetupHealth();
            spawnedObject.GetComponent<Units.Player.PlayerUnit>().MoveUnit(spawnPoint.transform.position, Units.Player.PlayerUnit.EUnitAction.Move);
            spawnOrder.Remove(spawnOrder[0]);
        }

        public void AddSpawn(float spawntime, GameObject objectToSpawn)
        {
            spawnQueue.Add(spawntime);
            spawnOrder.Add(objectToSpawn);

            if (spawnQueue.Count == 1)
            {
                actionTimer.StartCoroutine(actionTimer.SpawnQeueTimer());
            }
            else if (spawnQueue.Count == 0)
            {
                actionTimer.StopAllCoroutines();
            }
        }
        public void SetSpawnLocation(GameObject spawnLocation)
        {
            spawnPoint = spawnLocation;
        }
    }
}

