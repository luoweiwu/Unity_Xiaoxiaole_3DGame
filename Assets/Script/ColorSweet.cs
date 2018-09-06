using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSweet : MonoBehaviour {

    public enum ColorType
    {
        YELLOW,
        PUPLE,
        RED,
        BLUE,
        GREEN,
        PNGK,
        ANY,
        COUNT
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }

    public ColorSprite[] ColorSprites;

    private Dictionary<ColorType, Sprite> colorSpriteDict;

    private SpriteRenderer sprite;

    public int NumColors
    {
        get{ return ColorSprites.Length; }
    }

    public ColorType Color
    {
        get
        {
            return color;
        }

        set
        {
            SetColor(value);
        }
    }

    private ColorType color;



    public void Awake()
    {
        sprite = transform.Find("Sweet").GetComponent<SpriteRenderer>();

        colorSpriteDict = new Dictionary<ColorType, Sprite>();

        for(int i = 0; i < ColorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(ColorSprites[i].color))
            {
                colorSpriteDict.Add(ColorSprites[i].color,ColorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType newColor)
    {
        color = newColor;
        if (colorSpriteDict.ContainsKey(newColor))
        {
            sprite.sprite = colorSpriteDict[newColor];
        }
    }


}
