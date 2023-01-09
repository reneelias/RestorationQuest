using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stair_Trigger : MonoBehaviour
{
    [SerializeField] int floorLevel = 1;
    public int FloorLevel{
        get => floorLevel;
        protected set => floorLevel = value;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player"){
            other.gameObject.GetComponent<CharacterControls>().ChangeFloorLevel(floorLevel);
        }
    }
}
