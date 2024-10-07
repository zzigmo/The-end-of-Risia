using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Игрок, за которым следим
    public float smoothSpeed = 0.125f; // Скорость сглаживания движения камеры
    public Vector3 offset = new Vector3(0, 0, -10); // Смещение камеры

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 desiredPosition = player.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
