using UnityEngine;
using System.Collections.Generic;

public class JobSpriteController : MonoBehaviour {


    FurnitureSpriteController fsc;
    private Dictionary<Job, GameObject> jobGameObjectMap;

    // Use this for initialization
    void Start () {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);
                
	}
	void OnJobCreated(Job j) {

        if(j.jobObjectType == null || j.jobObjectType.Equals("")) {
            //no associated sprite needing to be drawn. No need to render.
            return;
        }

        if (jobGameObjectMap.ContainsKey(j)) {
            Debug.Log("OnJObCreated:- for a job sprite that already exists. Most likely caused by requeueing");
            return;
        }

        Sprite sprite = ResourceLoader.GetFurnitureSprite(j.jobObjectType);

        GameObject job_go = new GameObject();

        jobGameObjectMap.Add(j, job_go);

        job_go.name = j.jobObjectType + "_" + j.tile.X + "_" + j.tile.Y;
        job_go.transform.position = new Vector3(j.tile.X + (j.furniturePrototype.Width-1)/2f, j.tile.Y + (j.furniturePrototype.Height-1)/2f, 0);
        job_go.transform.SetParent(this.transform, true);

        //FIXME: this is hardcoded - not ideal!!!
        if (j.jobObjectType == "door") {
            //check for e-w or n-s walls.

            Tile northTile = j.tile.world.GetTileAt(j.tile.X, j.tile.Y + 1);
            Tile southTile = j.tile.world.GetTileAt(j.tile.X, j.tile.Y - 1);

            if (northTile != null && southTile != null && (southTile.furniture != null && southTile.furniture.objectType == "wall") ||
                (northTile.furniture != null && northTile.furniture.objectType == "wall")) {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        sr.sprite = ResourceLoader.GetFurnitureSprite(j.jobObjectType);
        sr.color = new Color(0.8f, 1f, 0.8f, 0.4f);
        sr.sortingLayerName = "Jobs";


        j.RegisterJobCancelCallback(OnJobEnded);
        j.RegisterJobCompleteCallback(OnJobEnded);
    }

    void OnJobEnded(Job j) {
        //dont care wether completed or cancelled.
        j.UnregisterJobCancelCallback(OnJobEnded);
        j.UnregisterJobCompleteCallback(OnJobEnded);

        GameObject job_go = jobGameObjectMap[j];
        jobGameObjectMap.Remove(j);
        Destroy(job_go);
    }
}
