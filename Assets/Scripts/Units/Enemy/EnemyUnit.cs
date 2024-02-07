using DRC.RTS.Interactables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace DRC.RTS.Units.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyUnit : IUnit
    {
        private NavMeshAgent navAgent;

        private Collider[] rangeColliders;

        private Transform aggroTarget;

        public Health aggroUnit;

        private bool hasAggro = false;

        private float distance;

        public float atkCooldown;

        private void Start()
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            atkCooldown -= Time.deltaTime;
            if (!hasAggro)
            {
                CheckForEnemyTarget();
            }
            else
            {
                Attack();
                MoveToAggroTarget();
            }
        }

        private void CheckForEnemyTarget()
        {
            rangeColliders = Physics.OverlapSphere(transform.position, baseStats.aggroRange, UnitHandler.instance.pUnitLayer);

            for (int i = 0; i < rangeColliders.Length;)
            {
                aggroTarget = rangeColliders[i].gameObject.transform;
                aggroUnit = aggroTarget.gameObject.GetComponentInChildren<Health>();
                print(rangeColliders.Count());
                hasAggro = true;
                break;
            }
        }

        private void MoveToAggroTarget()
        {
            if (aggroTarget == null)
            {
                navAgent.SetDestination(transform.position);
                hasAggro = false;
            }
            else
            {
                distance = Vector3.Distance(aggroTarget.position, transform.position);
                navAgent.stoppingDistance = (baseStats.atkRange + 1);

                if (distance <= baseStats.aggroRange)
                {
                    navAgent.SetDestination(aggroTarget.position);
                }
            }
        }

        private void Attack()
        {
            if(atkCooldown <= 0 && distance <= baseStats.atkRange + 1)
            {
                aggroUnit.SetDamage(baseStats.attack);
                atkCooldown = baseStats.atkSpeed;
            }
        }
    }
}
