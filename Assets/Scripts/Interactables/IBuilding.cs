using DRC.RTS.Buildings;
using DRC.RTS.Buildings.Player;
using DRC.RTS.Units;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DRC.RTS.Interactables
{
    [RequireComponent(typeof(Health))]
    public class IBuilding : Interactable
    {
        public GameObject spawnMarker = null;
        public GameObject spawnMarkerGraphics = null;
        public float maxMarkerDistance = 10f;


        public BuildingStatTypes.Base baseStats;
        public BuildingData buildingType;
        public List<Transform> workers = new List<Transform>();
        protected bool isConstructed;
        protected bool beingConstructed;
        [SerializeField] protected float progression = 0;

        private void Start()
        {
            baseStats = buildingType.baseStats;
        }
        private void OnEnable()
        {
            gameObject.name = buildingType.name;
        }
        public override void OnInteractEnter()
        {
            if (!isConstructed) return;
            InputManager.InputHandler.instance.selectedBuilding.GetComponent<PlayerBuilding>().SetSpawnLocation(spawnMarker);
            spawnMarkerGraphics.SetActive(true);
            base.OnInteractEnter();
        }

        public override void OnInteractExit()
        {
            if (!isConstructed) return;
            UI.HUD.ActionFrame.instance.ClearActions();
            spawnMarkerGraphics.SetActive(false);
            base.OnInteractExit();
        }

        public void SetSpawnMarkerLocation(Vector3 point)
        {
            spawnMarker.transform.position = point;
        }

        public IEnumerator Construct()
        {
            beingConstructed = true;
            do
            {
                if (workers.Count == 0) break;
                progression += (3 * buildingType.constructionTime / (workers.Count + 2)) * Time.deltaTime;
                yield return null;

            } while (progression <= buildingType.constructionTime);

            if(progression>= buildingType.constructionTime)
            {
                isConstructed = true;
                workers.Clear();
            }
            beingConstructed = false;
        }

        public void AddToConstructionWorkingQueue(Transform unit)
        {
            workers.Add(unit);
            if(!beingConstructed) StartCoroutine(Construct());
        }

        public void RemoveFromConstrcutionWorkingQueue(Transform unit)
        {
            workers.Remove(unit);
        }
    }
}
