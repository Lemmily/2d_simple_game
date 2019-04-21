using System;  
using System.Collections.Generic;
using UnityEngine;

public class TileSprites : MonoBehaviour  
{
    public SpriteRenderer A;
    public SpriteRenderer B;
    public SpriteRenderer C;
    public SpriteRenderer D;

    public void Awake()
    {

        GameObject go = new GameObject();
        go.transform.parent = this.transform;
        go.name = "A";
        go.transform.localPosition = new Vector3(-0.25f, 0.25f, 0f);
        A = go.AddComponent<SpriteRenderer>();
        A.sortingLayerName = "Furniture";

        go = new GameObject();
        go.transform.parent = this.transform;
        go.name = "B";
        go.transform.localPosition = new Vector3(0.25f, 0.25f, 0f);
        B = go.AddComponent<SpriteRenderer>();
        B.sortingLayerName = "Furniture";

        go = new GameObject();
        go.transform.parent = this.transform;
        go.name = "C";
        go.transform.localPosition = new Vector3(-0.25f, -0.25f, 0f);
        C = go.AddComponent<SpriteRenderer>();
        C.sortingLayerName = "Furniture";

        go = new GameObject();
        go.transform.parent = this.transform;
        go.name = "D";
        go.transform.localPosition = new Vector3(0.25f, -0.25f, 0f);
        D = go.AddComponent<SpriteRenderer>();
        D.sortingLayerName = "Furniture";
    }



    public void tint(Color c)
    {
        A.color = c;
        B.color = c;
        C.color = c;
        D.color = c;
    }

    public void SetTestSprite()
    {

        Sprite s = ResourceLoader.instance.furnitureSpriteMap["walls_a3"];
        A.sprite = s;
        A.sortingLayerName = "Furniture";
        B.sprite = s;
        B.sortingLayerName = "Furniture";
        C.sprite = s;
        C.sortingLayerName = "Furniture";
        D.sprite = s;
        D.sortingLayerName = "Furniture";
    }


    public void SetA(Sprite s)
    {
        A.sprite = s;
    }
    public void SetB(Sprite s)
    {
        B.sprite = s;
    }
    public void SetC(Sprite s)
    {
        C.sprite = s;
    }
    public void SetD(Sprite s)
    {
        D.sprite = s;
    }
}
