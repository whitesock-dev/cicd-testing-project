using UnityEngine;

public class PrintBuildNumber : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Application Version : " + Application.version);
    }
}
