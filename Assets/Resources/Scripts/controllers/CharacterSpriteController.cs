using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour {
    private GameObject all_tiles_go;
    private Dictionary<Character, GameObject> characterGameObjectMap;

    public World World
    {
        get
        {
            return WorldController.Instance.world;
        }
    }


    void OnEnable () {

        Debug.Log("Character Sprite Controller Enabled");

        characterGameObjectMap = new Dictionary<Character, GameObject>();

        all_tiles_go = new GameObject();

        all_tiles_go.transform.SetParent(this.transform);
        all_tiles_go.name = "Furniture";


        World.RegisterCharacterCreated(OnCharacterCreated);

        foreach(Character c in World.characters) {
            OnCharacterCreated(c);
        }
        //c.SetDestination(world.GetTileAt(world.Width / 2 + 10, world.Height / 2 + 5));
    }


    public void OnCharacterCreated(Character character) {

        GameObject char_go = ResourceLoader.GetCharacterSprite(character);
        characterGameObjectMap.Add(character, char_go);

        char_go.name = "Character";
        char_go.transform.position = new Vector3(character.x, character.y, 0);
        char_go.transform.SetParent(this.transform, true);

        char_go.transform.SetParent(all_tiles_go.transform);

        character.RegisterOnChangedCallback(OnCharacterChanged);
    }

    void OnCharacterChanged(Character character) {
        if ( ! characterGameObjectMap.ContainsKey(character)) {
            Debug.LogError("OnFurnitureChanged - trie dto change visuals for something not in the map!");
            return;
        }
        GameObject char_go = characterGameObjectMap[character];
        char_go.transform.position = new Vector3(character.x, character.y, 0);
    }
}
