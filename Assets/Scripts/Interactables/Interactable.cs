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
            isInteracting = true;
            ShowHighlight();
            ShowHpbar();
        }
        public virtual void OnInteractExit()
        {
            isInteracting = false;
            HideHighlight();
            HideHpBar();
        }
        public virtual void ShowHighlight()
        {
            highlight.SetActive(true);
        }
        public virtual void HideHighlight()
        {
            if (isInteracting) return;
            highlight.SetActive(false);
        }
        public virtual void ShowHpbar()
        {
            hpBar.SetActive(true);
        }
        public virtual void HideHpBar()
        {
            if (isInteracting) return;
            hpBar.SetActive(false);
        }
    }
}
