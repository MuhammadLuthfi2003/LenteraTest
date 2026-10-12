public interface IInteractable
{
    string Prompt { get; }
    bool CanInteract(PlayerInteraction interactor);
    void Interact(PlayerInteraction interactor);

    void OnFocus(PlayerInteraction interactor);
    void OnLostFocus(PlayerInteraction interactor);

}