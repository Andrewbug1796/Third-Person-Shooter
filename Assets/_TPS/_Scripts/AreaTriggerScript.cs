using UnityEngine;

public class AreaTriggerScript : MonoBehaviour
{
    public GameObject[] zombieGroup;
    public bool isTriggered;

    private void Start()
    {
        foreach(GameObject go in zombieGroup) 
            go.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && !isTriggered)
        {
            isTriggered = true;
            Debug.Log("AreaTrigger");
            foreach (GameObject go in zombieGroup)
                go.SetActive(true);
        }
    }
}
