using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {
    public int theBoardSize;
    public GameObject prefabby;
    Tile[][] theTiles; 

	// Use this for initialization
	void Start () {
        theTiles = new Tile[theBoardSize][];
        for (int i = 0; i < theBoardSize; i++) {
            theTiles[i] = new Tile[theBoardSize];
            for (int j = 0; j < theBoardSize; j++) {
                GameObject gObj = Instantiate(prefabby);
                gObj.transform.parent = gameObject.transform;
                gObj.transform.localPosition = new Vector3(i, j, 0);
                theTiles[i][j] = gObj.GetComponent<Tile>();
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
