using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private Stack<SceneObjects> objEaten = new Stack<SceneObjects>();

    //Inputs bools
    private bool inputInfiniteHealth = false;
    private bool inputResetMass = false;
    private bool inputLastLevel = false;
    private bool inputNextLevel = false;

    //Spawns that are in the Scene
    [SerializeField] private GameObject[] spawns = null;
    private int spawnToGo = 0; //Which spawn to teleport

    public void ObjectEaten(Vector3 _pos, Quaternion _rot, Objects.ObjType _type)
    {
        SceneObjects tempObj;
        tempObj.objPos = _pos;
        tempObj.objRot = _rot;
        tempObj.typeObj = _type;
        objEaten.Push(tempObj);
    }

    private void Update()
    {
        GetCheatInputs();
        ControlCheats();
    }
    public void ReSpawnObj()
    {
        GameObject assetPrefab = null;
        SceneObjects tempObj = objEaten.Peek();
        switch(tempObj.typeObj)
        {
            case (Objects.ObjType.JEWEL):
                assetPrefab = Resources.Load<GameObject>("Prefabs/jewel");
                Instantiate(assetPrefab, tempObj.objPos, tempObj.objRot, GameObject.Find("Eateable_Objects").transform);
                break;
            case (Objects.ObjType.LOG):
                assetPrefab = Resources.Load<GameObject>("Prefabs/Log");
                assetPrefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                Instantiate(assetPrefab, tempObj.objPos, tempObj.objRot, GameObject.Find("Eateable_Objects").transform);
                break;
            case (Objects.ObjType.LOGSTACK):
                assetPrefab = Resources.Load<GameObject>("Prefabs/Log_Stack");
                assetPrefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                Instantiate(assetPrefab, tempObj.objPos, tempObj.objRot, GameObject.Find("Eateable_Objects").transform);
                break;
            default:
                Debug.Log("Couldn't find gameObject to Respawn");
                break;
        }
        objEaten.Pop();
    }

    private void GetCheatInputs()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            inputInfiniteHealth = true;
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            inputResetMass = true;
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            inputLastLevel = true;
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            inputNextLevel = true;
        }
    }
    private void ControlCheats()
    {
        if(inputInfiniteHealth)
        {
            SetPlayerInfiniteHealth();
            inputInfiniteHealth = false;
        }

        if(inputResetMass)
        {
            ResetPlayerMass();
            inputResetMass = false;
        }

        if(inputLastLevel)
        {
            LastSpawn();
            inputLastLevel = false;
        }

        if(inputNextLevel)
        {
            NextSpawn();
            inputNextLevel = false;
        }
    }
    private void SetPlayerInfiniteHealth()
    {
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<Hungerin>().infiniteHealth)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Hungerin>().infiniteHealth = false;
        }
        else
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<Hungerin>().infiniteHealth = true;
        }
    }
    private void ResetPlayerMass()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Hungerin>().ResetMassPlayer();
    }
    private void LastSpawn()
    {
        spawnToGo--;
        if (spawnToGo <= -1)
        {
            spawnToGo = spawns.Length - 1;
            GameObject.FindGameObjectWithTag("Player").transform.position = spawns[spawnToGo].transform.position;
        }
        else
        {
            GameObject.FindGameObjectWithTag("Player").transform.position = spawns[spawnToGo].transform.position;
        }
    }
    private void NextSpawn()
    {
        spawnToGo++;
        if (spawnToGo >= spawns.Length)
        {
            spawnToGo = 0;
            GameObject.FindGameObjectWithTag("Player").transform.position = spawns[spawnToGo].transform.position;
        }
        else
        {
            GameObject.FindGameObjectWithTag("Player").transform.position = spawns[spawnToGo].transform.position;
        }
    }
}
struct SceneObjects
{
    public Vector3 objPos;
    public Quaternion objRot;
    public Objects.ObjType typeObj;
}
