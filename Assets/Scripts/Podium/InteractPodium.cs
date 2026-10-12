using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractPodium : MonoBehaviour, IInteractable
{
    [Header("On Interact Event")]
    public UnityEvent onInteract;

    [Header("Interaction Marker")]
    public GameObject interactionMarker;
    public string Prompt => throw new System.NotImplementedException();

    // Start is called before the first frame update
    void Start()
    {
        if (interactionMarker != null)
        {
            interactionMarker.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanInteract(PlayerInteraction interactor)
    {
        return true;
    }

    public void Interact(PlayerInteraction interactor)
    {
        throw new System.NotImplementedException();
    }

    public void OnFocus(PlayerInteraction interactor)
    {
        if (interactionMarker == null) return;
        interactionMarker.SetActive(true);
    }

    public void OnLostFocus(PlayerInteraction interactor)
    {
        if (interactionMarker == null) return;
        interactionMarker.SetActive(false);
    }
}
