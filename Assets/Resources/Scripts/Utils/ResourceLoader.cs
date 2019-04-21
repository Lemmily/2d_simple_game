using UnityEngine;
using System.Collections.Generic;
using System;

public class ResourceLoader : MonoBehaviour {

    public Dictionary<string, Sprite> furnitureSpriteMap { get; protected set;  }
    public Dictionary<string, Sprite> itemSpriteMap { get; protected set; }
    //Dictionary<string, Sprite> wallSpriteMap;
    public Dictionary<string, Sprite> tileSpriteMap { get; protected set; }
    public Dictionary<string, Sprite> inventorySpriteMap { get; protected set; }
    public Dictionary<string, AudioClip> audioClipMap { get; protected set; }
    public static ResourceLoader instance;

    public Sprite character;
    public Animator animator;

    // Use this for initialization
    void Awake () {
        instance = this;
        furnitureSpriteMap = new Dictionary<string, Sprite>();
        itemSpriteMap = new Dictionary<string, Sprite>();
        //wallSpriteMap = new Dictionary<string, Sprite>();
        tileSpriteMap = new Dictionary<string, Sprite>();
        inventorySpriteMap = new Dictionary<string, Sprite>();
        audioClipMap = new Dictionary<string, AudioClip>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("images/furniture");

        foreach (Sprite s in sprites) {
            furnitureSpriteMap.Add(s.name.ToLower(), s);
        }

        sprites = Resources.LoadAll<Sprite>("images/tiles");

        foreach (Sprite s in sprites) {
            tileSpriteMap.Add(s.name.ToLower(), s);
        }

        sprites = Resources.LoadAll<Sprite>("images/items");

        foreach (Sprite s in sprites) {
            itemSpriteMap.Add(s.name.ToLower(), s);
        }
        sprites = Resources.LoadAll<Sprite>("images/inventory");

        foreach (Sprite s in sprites) {
            inventorySpriteMap.Add(s.name.ToLower(), s);
        }


        AudioClip[] audioClips = Resources.LoadAll<AudioClip>("sounds");

        foreach (AudioClip a in audioClips) {
            audioClipMap.Add(a.name.ToLower(), a);
        }
    }


    public static Sprite GetFurnitureSprite(string name) {
        name = name.ToLower();
        if (instance.furnitureSpriteMap.ContainsKey(name)) {
            return instance.furnitureSpriteMap[name];
        }
        if (instance.furnitureSpriteMap.ContainsKey(name + "_")) {
            return instance.furnitureSpriteMap[name + "_"];
        }

        //FIXME: make a sprite for "I dunno what this is" 
        return null;

    }

    internal static GameObject GetCharacterSprite(Character chars) {
        GameObject char_obj = new GameObject();
        char_obj.AddComponent<SpriteRenderer>().sprite = instance.character;
        char_obj.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
        //char_obj.GetComponent<Animator>(). = instance.animator;
        return char_obj;
    }

    public static Sprite GetTileSprite(string name) {
        name = name.ToLower();
        return instance.tileSpriteMap[name];
    }
    public static Sprite GetItemSprite(string name)
    {
        name = name.ToLower();
        return instance.itemSpriteMap[name];
    }
    public static Sprite GetInventorySprite(string name)
    {
        name = name.ToLower();
        return instance.itemSpriteMap[name];
    }
    public static AudioClip GetSound(string name)
    {
        name = name.ToLower();
        if (instance.audioClipMap.ContainsKey(name)) 
            return instance.audioClipMap[name];
        return null;
    }

}
