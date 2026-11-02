using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Barrel;

    public void spawnBarrel()
    {
        Instantiate(Barrel);
    }

}