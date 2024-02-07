using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.UI.HUD
{
    public class Action : MonoBehaviour
    {
        enum EActionType
        {
            Spawn,
            Improvement,
            InvestigateTechnology
        }
        [SerializeField] private EActionType type;
        public void OnClick()
        {
            switch (type)
            {
                case EActionType.Spawn:
                    ActionFrame.instance.StartSpawnTimer(name);
                    break;
                case EActionType.Improvement:
                    break;
                case EActionType.InvestigateTechnology: 
                    break;
            }
        }
    }
}

