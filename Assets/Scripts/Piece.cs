
using System.Collections;
using UnityEngine;

public class Piece : MonoBehaviour{


    float speed = 0.1f;
    float turnSpeed = 0.001f;
    
    float endAngle = -1;
    
    public bool canSwap;
    public bool isSwappable;
    public bool isLaser;
    public int team {get; private set;} 
    public Vector2Int Index => PositionToIndex(transform.position);

    void Awake(){
        team = transform.tag[6] - 48;
    }

    public void Move(Vector2Int endPoint, Piece endPiece){
        StopAllCoroutines();
        if (endPiece != null){
            endPiece.Move(Index, null);
        }
        StartCoroutine(Slide(IndexToPosition(endPoint)));
    }

    public void Turn(int sign){
        StopAllCoroutines();
        if (endAngle != -1){
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, endAngle, transform.eulerAngles.z);
        }
        StartCoroutine(Rotate(sign));
    }

    IEnumerator Rotate(int sign){
        endAngle = transform.eulerAngles.y + 90f * sign;
        float lerpAmount = 0;
        float angle = transform.eulerAngles.y;
        while (transform.eulerAngles.y != endAngle){
            angle = Mathf.LerpAngle(angle, endAngle, lerpAmount);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, angle, transform.eulerAngles.z);
            lerpAmount = Mathf.Clamp01(turnSpeed + lerpAmount);
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator Slide(Vector3 endPos){
        float lerpAmount = 0;
        while (transform.position != endPos){
            transform.position = Vector3.Lerp(transform.position, endPos, Time.deltaTime * lerpAmount);
            lerpAmount += speed;
            yield return new WaitForEndOfFrame();
        }
    }

    Vector2Int PositionToIndex(Vector3 position){
        return new Vector2Int(Mathf.FloorToInt((position.x+23.75f)/2.5f), Mathf.FloorToInt((position.z+18.75f)/2.5f));
    }
    Vector3 IndexToPosition(Vector2Int index){
        return new Vector3(index.x * 2.5f - 22.5f, 1f, index.y * 2.5f - 17.50f);
    }
}
