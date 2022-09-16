using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPiece : MonoBehaviour
{
    public enum ColorType
    {
        Green,
        Red,
        Blue,
        Yellow,
        Purple,
        Orange,
        Any,
    };

    //associo a ogni colore uno sprite tramite una struttura
    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    };

    public ColorSprite[] colorSprites;
    private ColorType color;
    private SpriteRenderer sprite;
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    public ColorType Color
    {
        get { return color; }
        set { SetColor(value); }
    }

    public int NumColors
    {
        get { return colorSprites.Length; }
    }
    //sfrutto un dizionario che associa una key-color a uno sprite
    void Awake()
    {
        sprite = this.gameObject.GetComponent<SpriteRenderer>();
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        for (int i = 0; i< colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    //quando devo settare il colore ricavo lo sprite dal dictionary usando il colore come chiave
    public void SetColor(ColorType newColor)
    {
        color = newColor;
        if (colorSpriteDict.ContainsKey(newColor))
        {
            sprite.sprite = colorSpriteDict[newColor];
        }
    }
}
