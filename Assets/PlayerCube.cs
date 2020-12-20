
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCube : MonoBehaviour 
{
    public NetworkMan networkmanager;
    public Vector3 TrPose;
    public bool clear;
    public float speed;
    public string networkID;


    void Start()
    {
        clear = false; 
        networkmanager = GameObject.Find("NetworkMan").GetComponent<NetworkMan>(); 
        TrPose = Vector3.zero; 
        speed = 5.0f; 
    }
    void Update()
    {
        if (clear)
        {
            Destroy(gameObject);
        }

        if (networkID != networkmanager.pAdress)
        {
            transform.position = TrPose;
            return;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(-Vector3.left * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(-Vector3.forward * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
        }

    }

  
}
