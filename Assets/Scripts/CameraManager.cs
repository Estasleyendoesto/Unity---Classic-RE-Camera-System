using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // Necesitamos asociar las cámaras dentro de este array desde el editor de Unity
    public Camera[] cameras;

    // Necesitamos asociar al gameObject Target desde el editor de Unity
    public Transform target;

    // Un array de capas que podemos seleccionar cuáles desde el editor de Unity
    public LayerMask layerMask;

    // Variables para el caso B, SphereCast
    public float sphereCastRadius;
    public Transform debugSphere; // Del debug

    void Start()
    {
        /*
            Tendremos un objeto transparente similar a una ventana en el juego
            Es un gameObject con un collider pero el rayo lo ignora
            Lo ignora porque en Layer (capa) lo hemos establecido en Ignore Raycast.

            Otra opción es crear nuestras propias capas, pero debemos programar que sean ignoradas por el rayo manualmente.
            Lo haremos mediante LayerMask, en el editor de Unity aparecerá una nueva opción donde elegiremos qué capas estarán disponibles y cuales no.
            En este caso seleccionamos todo con Everything y luego deseleccionamos Glass (la que acabamos de crear).
            Entonces, desde el rayo podemos asignarlo para que ignore ciertas capas.

            Tenemos dos formas de IsVisible(), la primera es lanzar un rayo.
            La segunda es lanzar una esfera en lugar de un rayo. Es una esfera que viaja desde la posición A hacia la B.
            No solo podemos definir una esfera, también un cubo, cápsula, línea, etc.

            Desplaza el cubo Target usando los gizmos de posición.
        */
    }

    void Update()
    {
       WhatcameraWatchTarget();
    }

    void WhatcameraWatchTarget()
    {
        // Este algoritmo comprueba qué cámara del array tiene al objetivo visible
        // En ese caso las demás cámaras son desabilitadas

        bool alreadyVisible = false;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera currentCamera = cameras[i];

            if (alreadyVisible)
            {
                currentCamera.enabled = false;
                continue;
            }

            if ( IsVisible(target, currentCamera) )
            {
                currentCamera.enabled = true;
                alreadyVisible = true;
            }
            else
                currentCamera.enabled = false;
        }
    }

    // Forma A, lanzar un rayo
    bool IsVisible(Transform target, Camera camera)
    {
        // Bound -> límites de algo (bound de una esfera sería un cubo)
        Bounds bounds = new Bounds(target.position, Vector3.zero);
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        bool isInsideFrustrum = GeometryUtility.TestPlanesAABB(planes, bounds);

        // Si no está dentro de la vista de cámara, sale sin lanzar el rayo
        if ( !isInsideFrustrum ) return false;

        // En caso contrario lanza un rayo desde la cámara hacia el target y comprueba si hay paredes de por medio
        Vector3 towardsTarget = (target.transform.position - camera.transform.position).normalized;
        RaycastHit hit;
        Ray ray = new Ray(camera.transform.position, towardsTarget);
        if ( 
            Physics.Raycast( ray, out hit, 100, layerMask ) // layerMask son las capas que mostrará e ignorará el rayo  
        )
        {
            Debug.DrawLine(camera.transform.position, hit.point, (hit.transform == target) ? Color.green : Color.red );
            return (hit.transform == target);
        }

        return false;
    }

    // Forma B, lanzar una esfera en lugar de un rayo
    bool _IsVisible(Transform target, Camera camera)
    {
        Bounds bounds = new Bounds(target.position, Vector3.zero);
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        bool isInsideFrustrum = GeometryUtility.TestPlanesAABB(planes, bounds);

        if ( !isInsideFrustrum ) return false;

        Vector3 towardsTarget = (target.transform.position - camera.transform.position).normalized;
        RaycastHit hit;
        Ray ray = new Ray(camera.transform.position, towardsTarget);
        if ( 
            Physics.SphereCast(ray, sphereCastRadius, out hit, 100, layerMask)
        )
        {
            // Para el debug
            Debug.DrawLine(camera.transform.position, hit.point, (hit.transform == target) ? Color.green : Color.red );
            debugSphere.position = hit.point;
            debugSphere.localScale = Vector3.one * sphereCastRadius;

            return (hit.transform == target);
        }

        return false;
    }

}
