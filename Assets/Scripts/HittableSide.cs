using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HittableSide : MonoBehaviour{


    void OnDestroy(){
        Destroy(transform.parent.gameObject);
    }

}
