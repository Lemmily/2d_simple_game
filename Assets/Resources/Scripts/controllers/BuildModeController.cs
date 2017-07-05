using UnityEngine;
using System.Collections.Generic;

public class BuildModeController : MonoBehaviour
{

    private bool buildModeTile = true;
    public TileType tileType = TileType.Floor;
    public string objectBuildType;

    public static BuildModeController Instance { get; private set; }

    void Start() {
        Instance = this;
    }

    public void SetTileType(TileType tileType) {
        this.tileType = tileType;
    }

    public void SetToFloor() {
        buildModeTile = true;
        SetTileType(TileType.Floor);
    }
    public void SetToEmpty() {
        buildModeTile = true;
        SetTileType(TileType.Empty);
    }
    public void SetTo_BuildObject(string objectType) {
        //wall not a tile.
        objectBuildType = objectType;
        buildModeTile = false;
    }

    public void DoBuild(Tile t) {
        if (buildModeTile) {
            t.Type = tileType;
        }
        else {
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
                
                

                t.pendingFurnitureJob = j;
                j.RegisterJobCancelCallback((theJob) =>
                {
                    theJob.tile.pendingFurnitureJob = null;
                }
                );
                WorldController.Instance.world.jobQueue.Enqueue(j);


            }
        }
    }

}
