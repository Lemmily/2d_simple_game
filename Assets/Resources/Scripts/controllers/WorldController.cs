using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;
using System;

public class WorldController : MonoBehaviour
{

    public static WorldController Instance { get; protected set; }

    public World world { get; protected set; }


    static bool loadWorld = false;
    void OnEnable()
    {
        if (Instance != null)
            Debug.LogError("WorldCOntroller - There's already a world controller!");
        Instance = this;

        if (loadWorld)
        {
            loadWorld = false;
            CreateWorldFromSave();
        }
        else
            Debug.Log("Creating an empty world!");
        CreateEmptyWorld();

    }

    void Update()
    {
        world.Update(Time.deltaTime);
    }


    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x + 0.5f);
        int y = Mathf.FloorToInt(coord.y + 0.5f);
        if (world != null)
        {
            return world.GetTileAt(x, y);
        }
        else
        {
            return null;
        }

    }


    public void PlaceInventory()
    {
        //FIXME: Just for testung places 10 steel plate at 50,50.
       PlaceInventory(GetTileAtWorldCoord(new Vector3(50, 50, 0)), new Inventory("steel plate", 50, 7));
    }

    public bool PlaceInventory(Tile tile, Inventory inventory)
    {
        return world.PlaceInventory(tile, inventory);
    }
    public bool PlaceInventory(Job job, Inventory inventory)
    {
        return world.PlaceInventory(job, inventory);
    }

    public void MakePathTest() {
        world.SetupPathFindingTest();

        PathTileGraph tileGraph = new PathTileGraph(world);
        PlaceInventory();
    }


    public void NewWorld()
    {
        Debug.Log("New World!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void SaveWorld()
    {

        Debug.Log("Save World!");

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, world);
        writer.Close();

        Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());
    }

    public void LoadWorld()
    {

        Debug.Log("Load World!");
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void CreateEmptyWorld()
    {
        world = new World(100, 100);
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);
    }

    public void CreateWorldFromSave()
    {

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        Debug.Log(reader.ToString());
        world = (World)serializer.Deserialize(reader);
        reader.Close();

        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);
    }

}
