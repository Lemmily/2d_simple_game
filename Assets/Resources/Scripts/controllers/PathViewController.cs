using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathViewController : MonoBehaviour
{

    private Dictionary<Tile, GameObject> tileGameObjectMap;
    public GameObject pathNodePrefab;
    private List<GameObject> pathPreviews;

    public World world
    {
        get
        {
            return WorldController.Instance.world;
        }
    }

    void Start() {
        //world.character.RegisterPathChangedCallback(OnPathChanged);
        //pathPreviews = new List<GameObject>();
    }

    private void OnPathChanged(Queue<Tile> obj) {
        while (pathPreviews.Count > 0) {
            GameObject go = pathPreviews[0];
            pathPreviews.RemoveAt(0);
            SimplePool.Despawn(go);
        }
        if (world.character.path != null && world.character.path.path != null) {
            foreach (Tile tile in world.character.path.path.ToArray()) {
                GameObject go = SimplePool.Spawn(pathNodePrefab, new Vector3(tile.X, tile.Y, 0), Quaternion.identity);
                go.GetComponent<SpriteRenderer>().sortingLayerName = "Path";
                pathPreviews.Add(go);
                go.transform.SetParent(this.transform);
            }
        }
    }
    
    

}
