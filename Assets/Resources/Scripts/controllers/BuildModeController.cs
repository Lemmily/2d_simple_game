using UnityEngine;
using System.Collections.Generic;


public enum BuildMode
{
    FLOOR,
    FURNITURE,
    DECONSTRUCT
}


public class BuildModeController : MonoBehaviour
{
    public BuildMode buildMode = BuildMode.FLOOR;
    //public bool buildModeTile = true;
    public TileType tileType = TileType.Floor;
    public string objectBuildType;



    public static BuildModeController Instance { get; private set; }

    void Start() {
        Instance = this;
        //fsc = FindObjectOfType<FurnitureSpriteController>();
        //mouseController = FindObjectOfType<MouseController>();

        //furniturePreview = new GameObject();
        //furniturePreview.transform.SetParent(this.transform);
        //furniturePreview.AddComponent<SpriteRenderer>();
        //furniturePreview.SetActive(false);
    }





    public bool IsObjectDraggable()
    {
        if (buildMode == BuildMode.FLOOR || buildMode == BuildMode.DECONSTRUCT)
            return true;

        Furniture proto = WorldController.Instance.world.furnitureProto[objectBuildType];

        return proto.Width == 1 && proto.Height == 1;
    }

    public void SetTileType(TileType tileType) {
        this.tileType = tileType;
    }

    public void SetTo_Floor() {
        buildMode = BuildMode.FLOOR;
        SetTileType(TileType.Floor);
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }
    public void SetTo_Empty()
    {
        buildMode = BuildMode.FLOOR;
        SetTileType(TileType.Empty);
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }
    public void SetTo_BuildObject(string objectType)
    {
        buildMode = BuildMode.FURNITURE;
        //wall not a tile.
        objectBuildType = objectType;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }
    public void SetTo_Deconstruct(string objectType)
    {
        buildMode = BuildMode.DECONSTRUCT;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();
    }

    public void DoBuild(Tile t) {
        if (buildMode == BuildMode.FLOOR) {
            t.Type = tileType;
        }
        else if (buildMode == BuildMode.FURNITURE) {
            string furnitureType = (string)objectBuildType.Clone();

            if (WorldController.Instance.world.IsFurniturePlacementValid(objectBuildType, t)
                && t.pendingFurnitureJob == null) {
                //make the object.
                Job j;

                if (WorldController.Instance.world.furnitureJobPrototypes.ContainsKey(furnitureType)) {
                    j = WorldController.Instance.world.furnitureJobPrototypes[furnitureType].Clone();
                    j.tile = t;
                } else {
                    Debug.LogError("No job furniture job prototypr for this object: " + furnitureType);
                    j = new Job(
                        t, 
                        furnitureType, 
                        FurnitureActions.JobComplete_FurnitureBuilding,
                        //null,
                        0.1f, 
                        null);
                }

                j.furniturePrototype = WorldController.Instance.world.furnitureProto[furnitureType];

                t.pendingFurnitureJob = j;
                j.RegisterJobCancelCallback((theJob) =>
                {
                    theJob.tile.pendingFurnitureJob = null;
                }
                );
                WorldController.Instance.world.jobQueue.Enqueue(j);


            }
        } else if (buildMode == BuildMode.DECONSTRUCT)
        {
            if (t.furniture != null)
                t.furniture.Deconstruct();  //UnplaceFurniture();

        } else
        {
            Debug.LogError("BuildModeController:- build mode not recognised");
        }
    }

}
