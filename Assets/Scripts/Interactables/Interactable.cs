using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Interactables
{
    public class Interactable : MonoBehaviour
    {
        public bool isInteracting = false;
        public GameObject highlight = null;
        public GameObject hpBar = null;
        public UI.HUD.PlayerActions actions;
        public virtual void Awake()
        {
            highlight .SetActive(false);
        }

        public virtual void OnInteractEnter()
        {
            ShowHighlight();
            ShowHpbar();
            isInteracting = true;
        }
        public virtual void OnInteractExit()
        {
            HideHighlight();
            HideHpBar();
            isInteracting = false;
        }
        public virtual void ShowHighlight()
        {
            highlight.SetActive(true);
        }
        public virtual void HideHighlight()
        {
            highlight.SetActive(false);
        }
        public virtual void ShowHpbar()
        {
            hpBar.SetActive(true);
        }
        public virtual void HideHpBar()
        {
            hpBar.SetActive(false);
        }
    }
}
