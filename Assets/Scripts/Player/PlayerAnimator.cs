using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] Animator playerAnimator;

    [Header("Player Component Refrence")]
    [SerializeField] PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        if (playerAnimator == null)
        {
            playerAnimator = GetComponent<Animator>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        BindEvent();
    }

    void BindEvent()
    {
        if (playerMovement != null)
        {
            playerMovement.OnJumped += SetJump;
            playerMovement.OnMoveAction += SetSpeed;
        }
    }

    private void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnJumped -= SetJump;
            playerMovement.OnMoveAction -= SetSpeed;
        }
    }

    void SetJump()
    {
        playerAnimator.SetTrigger("Jump");
    }

    void SetSpeed(float speed)
    {
        playerAnimator.SetFloat("Speed", speed);
    }

}
