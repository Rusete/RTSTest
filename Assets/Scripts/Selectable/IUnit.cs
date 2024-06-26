using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DRC.RTS.Units;
using UnityEngine.AI;
using System.Linq;
using DRC.RTS.Player;

namespace DRC.RTS.Interactables
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class IUnit : SelectableBehaviour
    {
        private enum State
        {
            Idle,
            Moving,
            Animating,
        }

        protected NavMeshAgent navAgent;
        public UnitStatTypes.Base baseStats;
        public UnitData unitType;

        protected List<PositionData> targets = new();
        private State state;
        [SerializeField] 
        private Resources.GatheringBag<Resources.GameResources.EResourceType, int> carryingResources = new Resources.GatheringBag<Resources.GameResources.EResourceType, int>();
        public float MeleeStoppingStoppingDistance { get; private set; }

        private void OnEnable()
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        protected new void Start()
        {
            MeleeStoppingStoppingDistance = navAgent.radius;
            navAgent.stoppingDistance = MeleeStoppingStoppingDistance;
            base.Start();
        }

        protected void Update()
        {
            switch (state)
            {
                case State.Idle:
                    //Handle Animation
                    break;
                case State.Moving:
                    HandleMovement();
                    break;
                case State.Animating:
                    break;
            }
        }

        public void MoveTo(Vector3 position, float stopDistance, System.Action onArrivedAtPosition, bool multiTarget = false)
        {
            var stopD = stopDistance == 0 ? stopDistance : navAgent.stoppingDistance;
            if (!multiTarget)
            {
                if (targets.Count() > 0)
                {
                    // Eliminarse de la cola de construcción
                    switch (unitType.type)
                    {
                        case UnitData.EUnitType.Worker:
                            Collider[] hitColliders = Physics.OverlapSphere(transform.position, navAgent.stoppingDistance + .5f);
                            foreach (var hitCollider in hitColliders)
                            {
                                var building = hitCollider.GetComponent<IBuilding>();
                                if (building)
                                {
                                    building.RemoveFromConstrcutionWorkingQueue(transform);
                                }
                            }
                        break;
                    }
                }
                targets.Clear();
                targets.Add(new PositionData { position = position, stopDistance = stopD, action = onArrivedAtPosition });

                navAgent.SetDestination(targets[0].position);
                state = State.Moving;
            }
            else
            {

                targets.Add(new PositionData { position = position, stopDistance = stopD, action = onArrivedAtPosition });
                if (targets.Count() == 1)
                {
                    navAgent.SetDestination(targets[0].position);
                    state = State.Moving;
                }
            }
        }

        public void MoveToNextTarget()
        {
            targets.RemoveAt(0);
            if (targets.Count > 0)
            {
                Vector3 closestPointToTarget = targets[0].position + (transform.position - targets[0].position).normalized * targets[0].stopDistance;
                navAgent.SetDestination(closestPointToTarget);
                state = State.Moving;
            }
        }
        public override void OnInteractEnter()
        {
            base.OnInteractEnter();
        }

        public override void OnInteractExit()
        {
            base.OnInteractExit();
        }

        private void HandleMovement()
        {
            if (Vector3.Distance(transform.position, navAgent.destination) > targets[0].stopDistance)
            {
                // Vector3 moveDir = (targetPosition - transform.position).normalized;

                // float distanceBefore = Vector3.Distance(transform.position, target[0].position);
                // animatedWalker.SetMoveVector(moveDir);
                // handle sprite
            }
            else if (navAgent.velocity.magnitude == 0)
            {
                // Arrived
                if (targets.Count() > 0 && targets[0].action != null)
                {
                    System.Action tmpAction = targets[0].action;
                    state = State.Animating;
                    tmpAction();
                }
            }
        }

        public IEnumerator Gather(Resources.ResourceNode node)
        {
            if (unitType.type == UnitData.EUnitType.Worker)            
            {
                if (carryingResources.TotalQuantity() < baseStats.gatheringCapacity && node.QuantityRemaining() > 0)
                {
                    do
                    {
                        carryingResources.AddOrUpdate(node.GrabResource(), 1);
                        yield return new WaitForSeconds(baseStats.gatherCD);
                    } while (node && carryingResources.TotalQuantity() < baseStats.gatheringCapacity && node.QuantityRemaining() > 0);
                }
                IBuilding storage = PlayerManager.Instance.FindClosestStorage(transform.position);
                Vector3 closestPoint = storage.GetComponent<Collider>().ClosestPoint(transform.position);
                var stopD = navAgent.stoppingDistance;
                PositionData dataResourceNode = targets[0];
                var data = targets[0];
                MoveTo(
                    closestPoint, stopD, () =>
                    {
                        PlayerManager.Instance.StoreResources(carryingResources);
                        carryingResources.Clear();
                        if(node.QuantityRemaining() > 0)
                            MoveTo(data.position, data.stopDistance, data.action);
                    }
                );
                yield return null;
            }            
        }

        public struct PositionData
        {
            public Vector3 position;
            public float stopDistance;
            public System.Action action;
        }
    }
}
