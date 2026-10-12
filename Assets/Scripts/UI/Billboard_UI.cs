using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard_UI : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float fixedYRotation;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null)
            return;

        Vector3 direction = cam.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(-direction);
    }
}
