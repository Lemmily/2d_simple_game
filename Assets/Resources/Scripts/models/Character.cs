﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character : IXmlSerializable{


    public float x {
        get {
            return  Mathf.Lerp( currTile.X, nextTile.X, movementPercentage);
             }
    }
    public float y
    {
        get
        {
            return  Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage);
        }
    }

    public Tile currTile;
    private Tile dTile;
    public Tile destTile
    {
        get {
            return dTile;
        }
        set
        {
            if (dTile != value) {
                dTile = value;
                path = null;
            }
        }
    }
    Tile nextTile;

    public PathAStar path;

    float speed;
    float travelled; //holds how far travelled - when reaching one, moves to next tile.
    private float movementPercentage;

    Action<Character> cbCharacterChanged;
    Action<Queue<Tile>> cbPathChanged;

    Job myJob;
    private Inventory inventory;

    public Character(Tile tile) {
        currTile = tile;
        nextTile = tile;
        destTile = tile;
        speed = 5f;
        movementPercentage = 0.0f;
    }

    public void SetDestination(Tile tile) {
        if (currTile.IsNeighbour(tile) == false) {
            //uhm?
        }
        destTile = tile;
    }
    

    public void Update(float deltaTime) {
        Update_DoJob(deltaTime);
        Update_DoMovement(deltaTime);

        if (cbCharacterChanged != null)
            cbCharacterChanged(this);
    }


    //private void Update_DoJob(float deltaTime) {
    //    if (myJob == null) {
    //        myJob = currTile.world.jobQueue.Dequeue();
    //        if (myJob != null) {
    //            SetDestination(myJob.tile);
    //            myJob.RegisterJobCancelCallback(OnJobEnded);
    //            myJob.RegisterJobCompleteCallback(OnJobEnded);
    //        }
    //    }

    //    if (currTile == destTile) {
    //        if (myJob != null) {
    //            myJob.DoWork(deltaTime);
    //        }
    //        path = null;
    //    }

    //}

    public void AbandonJob() {
        nextTile = destTile = currTile;
        path = null;
        currTile.world.jobQueue.Enqueue(myJob);
        myJob.UnregisterJobCancelCallback(OnJobEnded);
        myJob.UnregisterJobCompleteCallback(OnJobEnded);
        myJob = null;
    }

    private void GeneratePath()
    {
        if (currTile != destTile)
        {
            path = new PathAStar(currTile.world, currTile, destTile);
            if (cbPathChanged != null)
                cbPathChanged(path.path);
        }
    }

    //private void Update_DoMovement(float deltaTime) {
    //    if (currTile == destTile) {
    //        path = null;
    //        return;
    //    }

    //    if (nextTile == null || nextTile == currTile) { 
    //        if (path == null || path.Length() == 0)
    //        {
    //            GeneratePath();
    //            if (path.Length() == 0) {
    //                Debug.Log("No path returned by A*!");
    //                AbandonJob();
    //                path = null;
    //                return;
    //            }
    //            nextTile = path.Dequeue();
    //        }
    //        //grab the next tile!
    //        nextTile = path.Dequeue();
    //    }


    //    float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - destTile.X, 2) + Mathf.Pow(currTile.Y - destTile.Y, 2));


    //    if (nextTile.movementCost == 0) {
    //        Debug.LogError("FIXME: A character has tried to enter an unwalkable tile.");
    //        nextTile = null;
    //        path = null;

    //    }


    //    float distThisFrame = speed /nextTile.movementCost * deltaTime;

    //    float percThisFrame = distThisFrame / distToTravel;

    //    movementPercentage += percThisFrame;

    //    if (movementPercentage >= 1) {
    //        currTile = destTile;
    //        movementPercentage = 0;
    //        return;
    //    }

    //    //Debug.Log("Im updating " + currTile + " " + destTile + " " + distThisFrame + "/" + distToTravel + " " + movementPercentage + "%" );

    //}


    private void GetNewJob() {
        myJob = currTile.world.jobQueue.Dequeue();
        if (myJob == null)
            return;

        myJob.RegisterJobCompleteCallback(OnJobEnded);
        myJob.RegisterJobCancelCallback(OnJobEnded);
        destTile = myJob.tile;

        GeneratePath();

        if (path.Length() == 0) {
            Debug.LogError("Could not reach job site!");
            AbandonJob();
            path = null;
            destTile = currTile;
            return;
        }

    }

    void Update_DoJob(float deltaTime) {
        // Do I have a job?
        if (myJob == null) {
            // Grab a new job.

            GetNewJob();

            if (myJob == null) {
                destTile = currTile;
                return;
            }



        }
        // We have a job! And it's reachable.


        if (myJob.HasAllMaterials() == false) {
            //we're missing something!


            if (inventory != null) {

                //check to see if what I'm carrying is something the job needs.
                if(myJob.DesiresInventoryType(inventory)) {

                    if (currTile == myJob.tile) {
                        //myJob.inventoryRequirements[inventory.objectType];
                    }
                    else {
                        destTile = myJob.tile;
                    }
                }
                //otherwise, drop/stockpile it?
            }

            destTile = myJob.tile;



        }

        // Are we there yet?
        //if (myJob != null && currTile == myJob.tile) {
            if (myJob!= null && (Mathf.Abs(x - myJob.tile.X) <= 1 && (Mathf.Abs(y - myJob.tile.Y) <= 1))) {
                myJob.DoWork(deltaTime);
                path = null; // stop pathing causde its close enough
            }
        //}

    }


    void Update_DoMovement(float deltaTime) {
        if (currTile == destTile) {
            path = null;
            return; // We're already were we want to be.
        }

        if (nextTile == null || nextTile == currTile) {
            // Get the next tile from the pathfinder.
            if (path == null || path.Length() == 0) {
                // Generate a path to our destination
                GeneratePath(); // This will calculate a path from curr to dest.
                if (path.Length() == 0) {
                    Debug.LogError("Path_AStar returned no path to destination!");
                    AbandonJob();
                    path = null;
                    return;
                }
                //knock first tile off list, cause we're already in it!
                nextTile = path.Dequeue();
            }

            // Grab the next waypoint from the pathing system!
            nextTile = path.Dequeue();

            if (nextTile == currTile) {
                Debug.LogError("Update_DoMovement - nextTile is currTile?");
            }
        }

        /*		if(pathAStar.Length() == 1) {
                    return;
                }
        */
        // At this point we should have a valid nextTile to move to.

        // What's the total distance from point A to point B?
        // We are going to use Euclidean distance FOR NOW...
        // But when we do the pathfinding system, we'll likely
        // switch to something like Manhattan or Chebyshev distance
        float distToTravel = Mathf.Sqrt(
            Mathf.Pow(currTile.X - nextTile.X, 2) +
            Mathf.Pow(currTile.Y - nextTile.Y, 2)
        );

        switch(nextTile.IsEnterable()) {
            case ENTERABILITY.Never:
                nextTile = null;
                path = null;
                return;

            case ENTERABILITY.Soon:

                return;
            case ENTERABILITY.Yes:
                break;
        }


        // How much distance can be travel this Update?
        float distThisFrame = speed / nextTile.movementCost * deltaTime;

        // How much is that in terms of percentage to our destination?
        float percThisFrame = distThisFrame / distToTravel;

        // Add that to overall percentage travelled.
        movementPercentage += percThisFrame;

        if (movementPercentage >= 1) {
            // We have reached our destination

            // TODO: Get the next tile from the pathfinding system.
            //       If there are no more tiles, then we have TRULY
            //       reached our destination.

            currTile = nextTile;
            movementPercentage = 0;
            // FIXME?  Do we actually want to retain any overshot movement?
        }


    }

    internal void RegisterOnChangedCallback(Action<Character> cb) {
        cbCharacterChanged += cb;
    }
    internal void UnregisterOnChangedCallback(Action<Character> cb) {
        cbCharacterChanged -= cb;
    }

    internal void RegisterPathChangedCallback(Action<Queue<Tile>> cb) {
        cbPathChanged += cb;
    }
    internal void UnregisterOnChangedCallback(Action<Queue<Tile>> cb) {
        cbPathChanged -= cb;
    }

    void OnJobEnded(Job j) {
        //Job completed or cancelled.
        if(j != myJob) {
            Debug.Log("Character being told about job  thats not his. Maybe unregister something?");
            return;
        }

        myJob = null;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", x.ToString());
        writer.WriteAttributeString("Y", y.ToString());
    }
}
