using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character : IXmlSerializable{


    public float x {
        get {
            if (nextTile != null)
            {

                return Mathf.Lerp(currTile.X, nextTile.X, movementPercentage);
            } else
            {
                return currTile.X;
            }
             }
    }
    public float y{
        get
        {
            if (nextTile != null)
            {

                return Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage);
            }
            else
            {
                return currTile.Y;
            }
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

    private World world {
        get
        {
            return this.currTile.world;
        }
    }



    //used by UI!!
    public string CurrentJob
    {
        get
        {
            if (myJob == null)
            {
                return "nothing";
            }
            return this.myJob.ToString();
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
    public Inventory inventory;
    private int maxStackSize; //could be used in future to limit the amount units carried?
    //think in future I will want to make a weight limit for each character. LIKE RIMWORLD.

    public Character(Tile tile) {
        currTile = tile;
        nextTile = tile;
        destTile = tile;
        speed = 5f;
        movementPercentage = 0.0f;
        maxStackSize = 50; // i can carry a maximum of 50 things.
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


    private void GeneratePath()
    {
        if (currTile != destTile)
        {
            path = new PathAStar(currTile.world, currTile, destTile);
            if (cbPathChanged != null)
                cbPathChanged(path.path);
        }
        else if (currTile == destTile) {
            //we're there! but do i need to do anything?
            Debug.Log("On the tile!");
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


    public void AbandonJob()
    {
        nextTile = null;
        destTile = null;
        path = null;
        currTile.world.jobQueue.Enqueue(myJob); // was "Requeue" ..reurn the job to the queue.
        myJob.UnregisterJobCancelCallback(OnJobEnded);
        myJob.UnregisterJobCompleteCallback(OnJobEnded);
        myJob = null;

        if (inventory != null)
        {
            currTile.world.inventoryManager.PlaceInventory(currTile, inventory);
        }
    }

    private void GetNewJob() {

        //FIXME: just gets first job.
        if (myJob != null) {
            Debug.LogError("Character:- trying tog et a new job even though I have one!");
            return;
        }


        myJob = world.jobQueue.Dequeue();
        if (myJob == null)
            return;


        //If I can't find any items for this job, cancel it!
        string objectType = myJob.GetFirstDesiredInventory().objectType;
        if (objectType != null
            &&
            myJob.canTakeFromStockpile == true
            &&
            world.inventoryManager.GetClosestInventoryOfType(objectType, currTile) == null)
            //trying to include the check here to bin job, becuase no loose items.
            {
            AbandonJob();
            path = null;
            destTile = currTile;
            return;
        } else if (objectType != null
            &&
            world.inventoryManager.GetClosestLooseInventoryOfType(objectType, currTile) == null)
        {
            AbandonJob();
            path = null;
            destTile = currTile;
            return;
        } else
        {
            Debug.Log("Character:- GetNewJob() - found some materials, or dont need them!");
        }


        //Same results for either atm.
        myJob.RegisterJobCompleteCallback(OnJobEnded);
        myJob.RegisterJobCancelCallback(OnJobEnded);

        destTile = myJob.tile;

        // check to see if we can reach the job....
        // though maybe this should be done in movement?
        GeneratePath();
        if (path == null || path.Length() == 0 ) {
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
                //I couldn't get a new job.
                //Debug.Log("Character:- No Job Available");
                path = null;
                destTile = null;
                return;
            }
        }
        // We have a job! And it's reachable.

        if (myJob.HasAllMaterials() == false) {
            GetJobMaterials();
            return; // we can't get further until we have all items
        }

        // Are we there yet and we can't get here unless the job has all items.
        if (currTile.IsNeighbour(myJob.tile) || currTile == myJob.tile) {
            //if ((Mathf.Abs(x - myJob.tile.X) <= 1 && (Mathf.Abs(y - myJob.tile.Y) <= 1))) {
            myJob.DoWork(deltaTime);
            path = null; // stop pathing causde its close enough
            //discard inventory?
            if (inventory != null)
                currTile.world.inventoryManager.PlaceInventory(currTile, inventory);
        }
    }




    public void GetJobMaterials()
    {
        //we're missing something!
        if (inventory != null && inventory.stackSize > 0) {
            //Im already carrying something
            //check to see if what I'm carrying is something the job needs.
            if (myJob.DesiresInventory(inventory) > 0) {
                if (currTile == myJob.tile || currTile.IsNeighbour(myJob.tile)) {
                    //if I'm next to it or on it, deliver the goods!
                    //myJob.inventoryRequirements[inventory.objectType];
                    currTile.world.inventoryManager.PlaceInventory(myJob, inventory);
                    myJob.DoWork(0);
                }
                else {
                    destTile = myJob.tile;
                }
            }
            else {
                //otherwise, drop/stockpile it?
                currTile.world.inventoryManager.PlaceInventory(currTile.GetNeighbours(true)[0], inventory);
            }
        }
        else {

            //if I don't have an inventory OR my inventory is no longer needed.
            if (inventory == null || (inventory != null && myJob.DesiresInventory(inventory) <= 0)) {
                FindNextJobMaterial();
                return;
            }

            if (myJob.DesiresInventory(destTile.inventory) > 0 && destTile.IsNeighbour(currTile)) {
                currTile.world.inventoryManager.PlaceInventory(
                        this,
                        destTile.inventory,
                        Mathf.Min(destTile.inventory.stackSize, inventory.maxStackSize - inventory.stackSize));
            } else if (currTile == destTile && myJob.DesiresInventory(destTile.inventory) > 0) {
                currTile.world.inventoryManager.PlaceInventory(
                        this,
                        destTile.inventory,
                        Mathf.Min(currTile.inventory.stackSize, inventory.maxStackSize - inventory.stackSize));
            }

            //// lets see if I can pickup the inventory off the floor.
            //if (inventory != null && currTile.inventory != null && (myJob.DesiresInventory(currTile.inventory) > 0)) {
            //    currTile.world.inventoryManager.PlaceInventory(this, currTile.inventory);


                //} else if (nextTile.IsNeighbour(currTile)) {
                //    if (nextTile.inventory != null && myJob.DesiresInventory(nextTile.inventory) > 0) {
                //        nextTile.world.inventoryManager.PlaceInventory(
                //            this,
                //            nextTile.inventory,
                //            Mathf.Min(nextTile.inventory.stackSize, inventory.maxStackSize - inventory.stackSize));
                //    }
                //}

                // else if (nextTile != currTile && nextTile.IsNeighbour(currTile) && nextTile.inventory != null) {
                //    currTile.world.inventoryManager.PlaceInventory(this, nextTile.inventory);
                //}
        }
    }

    public void FindNextJobMaterial()
    {
        Inventory desired = myJob.GetFirstDesiredInventory();
        Inventory supplier;

        if (myJob.canTakeFromStockpile) {
            supplier = currTile.world.inventoryManager.GetClosestInventoryOfType(
                desired.objectType,
                currTile,
                desired.maxStackSize - desired.stackSize
                );
        } else {
            supplier = currTile.world.inventoryManager.GetClosestLooseInventoryOfType(
                desired.objectType,
                currTile,
                desired.maxStackSize - desired.stackSize
                );
        }

        if (supplier == null) {
            Debug.Log("No tile available containing objects of " + desired.objectType + " available");
            AbandonJob();
            return;
        }
        inventory = desired.Clone();
        inventory.stackSize = 0;
        inventory.maxStackSize = Mathf.Min(desired.maxStackSize - desired.stackSize, maxStackSize); //
        inventory.character = this;


        Debug.Log(" CHaracter:- Inventory type got - " + inventory.objectType + " , " + inventory.maxStackSize);

        destTile = supplier.tile;
        return;
    }

    public override string ToString()
    {
        return "Character is currently in:- " + this.currTile + this.myJob;
    }

    void Update_DoMovement(float deltaTime) {
        if (currTile == destTile || destTile == null || destTile.IsNeighbour(currTile))
        {
            path = null;
            return; // We're already were we want to be.
        }
        if (currTile == null) {
            Debug.Log("My current tile is null??");

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
            //if (nextTile == currTile) {
            //    //we're now next to our destination "one tile over".
            //    Debug.Log("Update_DoMovement - grabbing next waypoint but nextTile is currTile?" + nextTile + "\n cur: " + currTile);
            //}
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
    internal void UnregisterPathChangedCallback(Action<Queue<Tile>> cb) {
        cbPathChanged -= cb;
    }

    void OnJobEnded(Job j)
    {
        //Job completed or cancelled.
        if (j != myJob) {
            Debug.Log("Character being told about job  thats not his. Maybe unregister something?");
            return;
        }

        j.UnregisterJobCompleteCallback(OnJobEnded);
        // j.UnregisterJobWorkedCallback() TODO: for when this is actually assigned.
        j.UnregisterJobCancelCallback(OnJobEnded);

        myJob = null;
        //if (inventory != null) {
        //    currTile.world.inventoryManager.PlaceInventory(currTile, inventory);
        //    if (inventory != null) {
        //        Debug.LogError("Trying to get rid of character inventory failed. SMUSHING");
        //        inventory = null;
        //    }
        //}
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
