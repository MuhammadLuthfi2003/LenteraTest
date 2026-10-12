using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractPodium : MonoBehaviour, IInteractable
{
    public UnityEvent onInteract;

    public string Prompt => throw new System.NotImplementedException();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        
    }

    public bool CanInteract(PlayerInteraction interactor)
    {
        throw new System.NotImplementedException();
    }

    public void Interact(PlayerInteraction interactor)
    {
        throw new System.NotImplementedException();
    }
}
