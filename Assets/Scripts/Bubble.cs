using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] private Transform centerTransform;
    [SerializeField] private Transform upperTransform;
    [SerializeField] private Transform lowerLeftTransform;
    [SerializeField] private Transform lowerRightTransform;

    private SpriteRenderer[] sprites;
    private Vector2 individualScale;
    private Vector2 smallScale;
    private Vector2 referenceSpriteSize;

    private void Awake()
    {
        sprites = new SpriteRenderer[4];

        sprites[0] = centerTransform.GetComponent<SpriteRenderer>();
        sprites[1] = upperTransform.GetComponent<SpriteRenderer>();
        sprites[2] = lowerLeftTransform.GetComponent<SpriteRenderer>();
        sprites[3] = lowerRightTransform.GetComponent<SpriteRenderer>();

        individualScale = centerTransform.localScale;
        smallScale = upperTransform.localScale;
        referenceSpriteSize = new Vector2(
            sprites[0].sprite.rect.width,
            sprites[0].sprite.rect.height
        );
    }

    public void Draw(List<Sprite> spritesList)
    {
        if (spritesList.Count == 0)
        {
            return;
        }

        if (spritesList.Count == 1)
        {
            DrawOne(spritesList[0]);
        }
        else
        {
            DrawMany(spritesList);
        }
    }

    private void DrawOne(Sprite sprite)
    {
        HideExistent();

        sprites[0].sprite = sprite;
        sprites[0].enabled = true;
        sprites[0].transform.localScale = GetScale(sprite, individualScale);
    }

    private void DrawMany(List<Sprite> spritesList)
    {
        HideExistent();

        int maxIndex = Mathf.Min(spritesList.Count, sprites.Length - 1);
        for (int i = 0; i < maxIndex; ++i)
        {
            sprites[i + 1].sprite = spritesList[i];
            sprites[i + 1].enabled = true;
            sprites[i + 1].transform.localScale = GetScale(spritesList[i], smallScale);
        }
    }

    private void HideExistent()
    {
        for (int i = 0; i < sprites.Length; ++i)
        {
            sprites[i].enabled = false;
        }
    }

    private Vector2 GetScale(Sprite sprite, Vector2 referenceScale)
    {
        float w = sprite.rect.width;
        float h = sprite.rect.height;
        if (w > h)
        {
            float aspectRatio = h / w;

            float scaleX = referenceSpriteSize.x / w * referenceScale.x;
            float scaleY = aspectRatio * scaleX;
            return new Vector2(scaleX, scaleY);
        }
        else
        {
            float aspectRatio = w / h;

            float scaleY = referenceSpriteSize.y / h * referenceScale.y;
            float scaleX = aspectRatio * scaleY;
            return new Vector2(scaleX, scaleY);
        }
    }
}