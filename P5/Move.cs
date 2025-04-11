using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ARModelController : MonoBehaviour
{
    [Header("Configuración")]
    public ObserverBehaviour[] ImageTargets; // Asigna tus ImageTargets en el inspector
    public float speed = 1.0f;

    [Header("Estado")]
    [SerializeField] private int currentTargetIndex = 0;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private GameObject currentModel;

    void Update()
    {
        // Actualizar modelo actual si el target cambia
        if (!isMoving && ImageTargets[currentTargetIndex].TargetStatus.Status != Status.TRACKED)
        {
            GetNextActiveTarget();
        }
    }

    // Método para mostrar modelo (asignar a Button 1)
    public void ShowModel()
    {
        if (currentModel != null)
            currentModel.SetActive(false);

        currentModel = GetCurrentModel();
        if (currentModel != null)
            currentModel.SetActive(true);
    }

    // Método para mover al siguiente target (asignar a Button 2)
    public void MoveToNextTarget()
    {
        if (!isMoving && currentModel != null)
            StartCoroutine(MoveModelCoroutine());
    }

    private IEnumerator MoveModelCoroutine()
    {
        isMoving = true;
        
        ObserverBehaviour nextTarget = GetNextActiveTarget();
        if (nextTarget == null) yield break;

        Vector3 startPos = currentModel.transform.position;
        Vector3 endPos = nextTarget.transform.position;
        float journey = 0f;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * speed;
            currentModel.transform.position = Vector3.Lerp(startPos, endPos, journey);
            yield return null;
        }

        // Actualizar referencias
        currentModel.transform.SetParent(nextTarget.transform);
        currentTargetIndex = System.Array.IndexOf(ImageTargets, nextTarget);
        isMoving = false;
    }

    private GameObject GetCurrentModel()
    {
        if (ImageTargets[currentTargetIndex].transform.childCount > 0)
            return ImageTargets[currentTargetIndex].transform.GetChild(0).gameObject;
        
        return null;
    }

    private ObserverBehaviour GetNextActiveTarget()
    {
        // Buscar siguiente target detectado en orden circular
        for (int i = 1; i <= ImageTargets.Length; i++)
        {
            int nextIndex = (currentTargetIndex + i) % ImageTargets.Length;
            var target = ImageTargets[nextIndex];
            
            if (target.TargetStatus.Status == Status.TRACKED || 
                target.TargetStatus.Status == Status.EXTENDED_TRACKED)
            {
                currentTargetIndex = nextIndex;
                return target;
            }
        }
        return null;
    }
}