using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager INSTANCE;

    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite askingEmployeeSprite;
    [SerializeField] private Sprite leaveSprite;
    [SerializeField] private Sprite storageSprite;

    private void Start()
    {
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

    public Sprite GetLeaveSprite()
    {
        return leaveSprite;
    }

    public Sprite GetQuestionMarkSprite()
    {
        return questionMarkSprite;
    }

    public Sprite GetAskingEmployeeSprite()
    {
        return askingEmployeeSprite;
    }
}
