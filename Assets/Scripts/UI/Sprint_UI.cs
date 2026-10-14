using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sprint_UI : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField] Slider sprintBar;

    private PlayerMovement player;
    private float sprintDuration;
    private float sprintCooldown;
    private float totalDuration;
    private float currentDuration = 0f;

    private bool isOnSprint = false;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance && GameManager.Instance.Player)
        {
            player = GameManager.Instance.Player;
            sprintCooldown = player.SprintCooldown;
            sprintDuration = player.SprintDuration;
            totalDuration = sprintDuration + sprintCooldown;
        }

        if (sprintBar)
        {
            sprintBar.value = 1;
        }

        // bind event
        player.OnSprintAction += ActivateSprintBar;
    }

    private void OnDestroy()
    {
        player.OnSprintAction -= ActivateSprintBar;
    }

    void ActivateSprintBar()
    {
        isOnSprint = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isOnSprint)
        {
            currentDuration += Time.deltaTime;

            if (currentDuration >= totalDuration)
            {
                sprintBar.value = 1;
                isOnSprint = false;
                currentDuration = 0f;
            }
            else if (currentDuration <= sprintDuration)
            {
                float progress = currentDuration / sprintDuration;
                progress = Mathf.Clamp01(progress);

                sprintBar.value = 1 - progress;
            }
            else if (currentDuration > sprintDuration)
            {
                float currentCooldown = currentDuration - sprintDuration;
                float cooldownProgress = currentCooldown / sprintCooldown;

                sprintBar.value = cooldownProgress;
            }
        }
    }
}
