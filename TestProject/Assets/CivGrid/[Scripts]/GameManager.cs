using UnityEngine;
using System.Collections;
using CivGrid;

namespace CivGrid
{

    public delegate void WorldEvent(string type, string message);
    public delegate void NextTurn();

    public class GameManager : MonoBehaviour
    {

        public GameObject Barbarian;

        public static WorldEvent worldEvent;
        public static NextTurn nextTurn;

        WorldManager worldManager;

        void Awake()
        {
            GameManager.worldEvent += EventManager;
            GameManager.nextTurn += NextTurn;
            worldManager = GameObject.FindObjectOfType<WorldManager>();
        }

        void OnDisable()
        {
            GameManager.worldEvent -= EventManager;
            GameManager.nextTurn -= NextTurn;
        }

        void StartSpawning()
        {
            SpawnUnit(Barbarian, new Vector2(10,12));
            SpawnUnit(Barbarian, new Vector2(10, 13));
        }

        void SpawnUnit(GameObject obj, Vector2 hexLoc)
        {
            HexInfo hex = worldManager.GetHexFromAxialPosition(hexLoc);

            GameObject unit = (GameObject)Instantiate(obj, hex.worldPosition, Quaternion.identity);

            unit.GetComponent<Unit>().currentLocation = hex;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                nextTurn.Invoke();
            }
        }

        void NextTurn()
        {
            Debug.Log("going to next turn on GameManager");
        }

        void EventManager(string type, string message)
        {
            if (type == "Death")
            {
                print(message);
            }

            if (type == "World Done")
            {
                StartSpawning();
            }
        }

    }
}