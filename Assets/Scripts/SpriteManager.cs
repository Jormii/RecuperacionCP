using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager INSTANCE;

    [SerializeField] private Sprite storageSprite;
    [SerializeField] private Sprite askingEmployeeSprite;
    [SerializeField] private Sprite leaveSprite;
    [SerializeField] private Sprite questionMarkSprite;

    private void Start()
    {
        if (INSTANCE)
        {
            Debug.LogError("An Sprite Manager instance already exists. Deleting...");
            Destroy(gameObject);
            return;
        }

        INSTANCE = this;
    }

    public Sprite GetStoreSprite(int storeID)
    {
        return Mall.INSTANCE.GetStoreByID(storeID).GetComponent<SpriteRenderer>().sprite;
    }

    public Sprite GetStorageSprite()
    {
        return storageSprite;
    }

    public Sprite GetAskingEmployeeSprite()
    {
        return askingEmployeeSprite;
    }

    public Sprite GetLeaveSprite()
    {
        return leaveSprite;
    }

    public Sprite GetQuestionMarkSprite()
    {
        return questionMarkSprite;
    }

}
