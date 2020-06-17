using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] private Transform upperPosition;
    [SerializeField] private Transform lowerLeftPosition;
    [SerializeField] private Transform lowerRightPosition;

    private Vector3[] positions;
    private GameObject[] gameObjects;

    private void Awake()
    {
        positions = new Vector3[3];
        gameObjects = new GameObject[3];

        positions[0] = upperPosition.position;
        positions[1] = lowerLeftPosition.position;
        positions[2] = lowerRightPosition.position;
    }

    public void DrawOne(GameObject item)
    {
        DestroyExistent();
        GameObject.Instantiate(item, positions[0], Quaternion.identity, transform);
    }

    public void DrawMany(List<GameObject> itemsList)
    {
        DestroyExistent();

        int maxIndex = Mathf.Min(itemsList.Count, positions.Length);
        for (int i = 0; i < maxIndex; ++i)
        {
            GameObject item = itemsList[i];
            GameObject.Instantiate(item, positions[i], Quaternion.identity, transform);
        }
    }

    private void DestroyExistent()
    {
        for (int i = 0; i < gameObjects.Length; ++i)
        {
            GameObject go = gameObjects[i];
            if (go != null)
            {
                Destroy(go);
            }
        }
    }
}