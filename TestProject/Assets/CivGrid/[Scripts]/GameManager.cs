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

        public bool worldGenDone
        {
            get { return true; }
            set { worldEvent("World", "World Done"); }
        }

        public static event NextTurn nextTurn;

        void Awake()
        {
            GameManager.worldEvent += EventManager;

        }

        void StartSpawning()
        {
            SpawnUnit(Barbarian, new Vector3(-9, 28, -19));
            SpawnUnit(Barbarian, new Vector3(-10, 29, -19));
        }

        void SpawnUnit(GameObject obj, Vector3 hexLoc)
        {
            GameObject hex = GameObject.Find("HexTile " + hexLoc);

            GameObject unit = (GameObject)Instantiate(obj, hex.transform.position, Quaternion.identity);

            //TODO: FIX
            //unit.GetComponent<Unit>().currentLocation = hex.GetComponent<HexInfo>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                nextTurn.Invoke();
            }
        }

        void EventManager(string type, string message)
        {
            if (type == "Death")
            {
                print(message);
            }

            if (type == "World" && message == "World Done")
            {
                StartSpawning();
            }
        }

    }
}