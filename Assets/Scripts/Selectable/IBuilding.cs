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
    public class IBuilding : SelectableBehaviour
    {
        public GameObject spawnMarker = null;
        public GameObject spawnMarkerGraphics = null;
        public float maxMarkerDistance = 10f;


        public BuildingStatTypes.Base baseStats;
        public BuildingData buildingType;
        public List<Transform> workers = new List<Transform>();
        [SerializeField] protected bool isConstructed = false;
        protected bool beingConstructed;
        [SerializeField] protected float progression = 0;

        private new void Start()
        {
            baseStats = buildingType.baseStats;
            if (!isConstructed) progression = 0;
            base.Start();
        }
        private void OnEnable()
        {
            gameObject.name = buildingType.name;
            isConstructed = false;
            progression = 0;
        }
        public override void OnInteractEnter()
        {
            base.OnInteractEnter();
        }

        public override void OnInteractExit()
        {
            UI.HUD.ActionFrame.instance.ClearActions();
            spawnMarkerGraphics.SetActive(false);
            isInteracting = false;
            HideHighlight();
            if(isConstructed)
                HideHpBar();
        }

        public void SetSpawnMarkerLocation(Vector3 point)
        {
            spawnMarker.transform.position = point;
        }

        public IEnumerator Construct()
        {
            var health = GetComponent<Health>();
            do
            {
                if (workers.Count == 0) break;
                var prog = (3 / (workers.Count + 2)) * Time.deltaTime;
                progression += prog;
                health.SetHealing(health.maxHealth * prog / buildingType.constructionTime);
                yield return null;

            } while (progression <= buildingType.constructionTime);

            if(progression >= buildingType.constructionTime)
            {
                isConstructed = true;
                HideHpBar();
                foreach (var worker in workers)
                {
                    worker.GetComponent<IUnit>().MoveToNextTarget();
                }
                workers.Clear();
            }
        }

        public IEnumerator Repair()
        {
            var health = GetComponent<Health>();
            do
            {
                if (workers.Count == 0) break;
                var prog = (3 / (workers.Count + 2)) * Time.deltaTime;
                health.SetHealing(health.maxHealth * prog / buildingType.constructionTime);
                yield return null;

            } while (health.currentHealth == health.maxHealth);

            if (health.maxHealth == health.currentHealth)
            {
                HideHpBar();
                foreach (var worker in workers)
                {
                    worker.GetComponent<IUnit>().MoveToNextTarget();
                }
                workers.Clear();
            }
        }

        public void AddToConstructionWorkingQueue(Transform unit)
        {
            if (workers.Contains(unit)) return;
            if (unit.GetComponent<IUnit>().unitType.type != UnitData.EUnitType.Worker) return;
            workers.Add(unit);
            if (!beingConstructed) StartCoroutine(Construct());
            else StartCoroutine(Repair());
        }

        public void RemoveFromConstrcutionWorkingQueue(Transform unit)
        {
            if (!workers.Contains(unit)) return;
            workers.Remove(unit);
        }
    }
}
