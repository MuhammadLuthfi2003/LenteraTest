using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float height = 2f;   // how far it travels from the start position (up and down)
    [SerializeField] private float speed = 1f;    // cycles per second

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Sine wave oscillates between -1 and 1, giving smooth up/down motion
        float offset = Mathf.Sin(Time.time * speed * Mathf.PI * 2f) * height;
        transform.position = startPosition + Vector3.up * offset;
    }

}
