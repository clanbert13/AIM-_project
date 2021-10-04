using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpawnPointScript : MonoBehaviour
{
    private float randTime;
    [Header("UI")]
    public Text difficultiText;

    [Header("SpawnTime")]
    private bool afterSec = true;
    public GameObject obj;      //prefab
    [SerializeField] private float minTime = 2.0f;
    [SerializeField] private float maxTime = 5.0f;

    [Header("SpawnRange")]
    [Tooltip("Rnage : -X ~ X, 0 ~ Y, -Z ~ Z")]
    [SerializeField] private float X = 5f;
    [SerializeField] private float Y = 5f;
    [SerializeField] private float Z = 0.5f;
    private void Awake()
    {
        if (minTime < (obj.GetComponent<Target>().difficulty + 4) * 0.25f) minTime = (obj.GetComponent<Target>().difficulty + 4) * 0.25f;
        randTime = Random.Range(minTime, maxTime);
    }

    private void Update()
    {
        if (afterSec == true) {
            //Debug.Log("start timer");
            StartCoroutine(SpawnTimer());
            Instantiate(obj, new Vector3(Random.Range(X * -1, X), Random.Range(0, Y), Random.Range(Z * -1, Z)), Quaternion.identity);   //Global Location
            //Debug.Log("spawn");
            afterSec = false;
        }
        difficultiText.text = DifficultyReturn();
    }
    private IEnumerator SpawnTimer()
    {
        //Debug.Log("spawning");
        yield return new WaitForSeconds(randTime);
        afterSec = true;
    }

    public string DifficultyReturn()
    {
        if (obj.GetComponent<Target>().difficulty == 0f) return "Difficulty : insane";
        if (obj.GetComponent<Target>().difficulty == 1f) return "Difficulty : difficult";
        if (obj.GetComponent<Target>().difficulty == 2f) return "Difficulty : normal";
        if (obj.GetComponent<Target>().difficulty == 3f) return "Difficulty : easy";
        else return "Difficulty : ????";
    }
}
