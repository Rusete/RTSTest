using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DRC.RTS.Interactables;

namespace DRC.RTS.Units.Player
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerUnit : IUnit
    {
        private NavMeshAgent navAgent;


        private void OnEnable()
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        public void MoveUnit(Vector3 destination)
        {
            if(navAgent == null) navAgent=GetComponent<NavMeshAgent>();
            navAgent.SetDestination(destination);
        }
    }
}
