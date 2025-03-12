using System;
using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour{

    int turn = 1;
    [SerializeField] CinemachineVirtualCamera virtualCamera1, virtualCamera2;
    [SerializeField] Camera mainCamera;
    Piece selectedPiece;
    [SerializeField] Material legalMoveMaterial, material1, material2;
    int floorLayer, laserLayer;

    int[,] specialTilesBoard = new int[8, 10]
        {
            {3, 1, 0, 0, 0, 0, 0, 0, 2, 1},
            {2, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {2, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {2, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {2, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {2, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {2, 0, 0, 0, 0, 0, 0, 0, 0, 1},
            {2, 1, 0, 0, 0, 0, 0, 0, 2, 3},
        };
    GameObject[,] tileGameObjects = new GameObject[8, 10];
    [SerializeField] bool showSpecialTiles;
    [SerializeField] float displayTileRadius;
    
    [SerializeField] EGA_Laser laser1, laser2;

    void Awake(){
        floorLayer = LayerMask.GetMask("Floor");
        laserLayer = LayerMask.GetMask("Laser");
        for (int i = 0 ; i < 8; i++){
            for (int j = 0; j < 10; j++){
                tileGameObjects[i,j] = GetTile(new Vector2Int(j, i));
            }
        }
    }

    void Start(){
        laser1.doneFiringEvent.AddListener(SetTurnTo2);
        laser2.doneFiringEvent.AddListener(SetTurnTo1);
        SetCurrentCamera1();
    }

    void SetTurnTo1(){
        turn = 1;
        Invoke("SetCurrentCamera1", 1.5f);
    }
    void SetTurnTo2(){
        turn = 2;
        Invoke("SetCurrentCamera2", 1.5f);
    }

    void SetCurrentCamera1(){
        virtualCamera1.gameObject.SetActive(true);
        virtualCamera2.gameObject.SetActive(false);
    }
    void SetCurrentCamera2(){
        virtualCamera1.gameObject.SetActive(false);
        virtualCamera2.gameObject.SetActive(true);
    }

    
    void Update(){
        if (Input.GetMouseButtonDown(0)) PlayerClick();
        if (Input.GetMouseButtonDown(1) && selectedPiece != null) {
            ResetBoardColours(selectedPiece);
            selectedPiece = null;
        }
        if (Input.GetKeyDown(KeyCode.D) && selectedPiece != null){
            selectedPiece.Turn(1);
            EndTurn();
        }
        if (Input.GetKeyDown(KeyCode.A) && selectedPiece != null){
            selectedPiece.Turn(-1);
            EndTurn();
        }
        
    }

    void EndTurn(){
        ResetBoardColours(selectedPiece);
        if (turn == 1){
            Invoke("shootLaser1", 1f);
        }else{
            Invoke("shootLaser2", 1f);
        }
        selectedPiece = null;
        turn = -1;
    }
    void shootLaser1(){          
        laser1.StartShooting();
    }
    void shootLaser2(){
        laser2.StartShooting();
    }
    void PlayerClick(){
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 1000f, floorLayer)){
            if (selectedPiece == null){
                
                Vector2Int selectedIndex = PositionToIndex(hitInfo.point);
                Piece pieceClicked = GetPiece(selectedIndex);
                


                if (pieceClicked == null) return;


                if (pieceClicked.team == turn){
                    selectedPiece = pieceClicked;
                    DisplayLegalMoves(selectedPiece);
                }
            }else{
                Vector2Int endIndex = PositionToIndex(hitInfo.point);
                Piece endPiece = GetPiece(endIndex);
                if (IsLegalMove(selectedPiece, endIndex)){
                    selectedPiece.Move(endIndex, endPiece);
                    EndTurn();
                }else if (endPiece != null){
                    if (endPiece.team == turn){
                        ResetBoardColours(selectedPiece);
                        selectedPiece = endPiece;
                        DisplayLegalMoves(endPiece);
                    }
                    else{
                        ResetBoardColours(selectedPiece);
                        selectedPiece = null;
                    }
                }
                else{
                    ResetBoardColours(selectedPiece);
                    selectedPiece = null;
                }
            }
        }
    }

    void ResetBoardColours(Piece piece){
        for (int i = -1; i <= 1; i++){
            for (int j = -1; j <= 1; j++){
                Vector2Int endIndex = new Vector2Int(i, j) + piece.Index;
                if (endIndex.x >= 10 || endIndex.x < 0 || endIndex.y >= 8 || endIndex.y < 0) continue;
                GameObject possibleEndTile = tileGameObjects[endIndex.y, endIndex.x];
                if (endIndex.x % 2 == 0){
                    if (endIndex.y % 2 == 0){
                        possibleEndTile.GetComponent<Renderer>().material = material2;
                    }else{
                         possibleEndTile.GetComponent<Renderer>().material = material1;
                    }
                }else{
                    if (endIndex.y % 2 == 0){
                        possibleEndTile.GetComponent<Renderer>().material = material1;
                    }else{
                         possibleEndTile.GetComponent<Renderer>().material = material2;
                    }
                }
            }
        }
    }

    void DisplayLegalMoves(Piece piece){
         if (piece.isLaser){
            GameObject endTile = tileGameObjects[piece.Index.y, piece.Index.x];
            endTile.GetComponent<Renderer>().material = legalMoveMaterial;
            return;
        }
        for (int i = -1; i <= 1; i++){
            for (int j = -1; j <= 1; j++){
                Vector2Int endIndex = new Vector2Int(i, j) + piece.Index;
                if (endIndex.x >= 10 || endIndex.x < 0 || endIndex.y >= 8 || endIndex.y < 0) continue;
                if (IsLegalMove(piece, endIndex)){
                    GameObject possibleEndTile = tileGameObjects[endIndex.y, endIndex.x];
                    possibleEndTile.GetComponent<Renderer>().material = legalMoveMaterial;
                }
            }
        }
    }

    Vector2Int PositionToIndex(Vector3 position){
        return new Vector2Int(Mathf.FloorToInt((position.x+23.75f)/2.5f), Mathf.FloorToInt((position.z+18.75f)/2.5f));
    }
    Vector3 IndexToPosition(Vector2Int index){
        return new Vector3(index.x * 2.5f - 22.5f, 1f, index.y * 2.5f - 17.50f);
    }
    Vector3 tilePosition = Vector3.zero;
    Piece GetPiece(Vector2Int index){
        tilePosition = IndexToPosition(index) - Vector3.up * 0.9f;
        if (Physics.Raycast(tilePosition, Vector3.up, out RaycastHit hitInfo)){
            return hitInfo.collider.gameObject.GetComponentInParent<Piece>();
        }
        return null;
    }
    GameObject GetTile(Vector2Int index){
        tilePosition = IndexToPosition(index) + Vector3.up;
        if (Physics.Raycast(tilePosition, Vector3.down, out RaycastHit hitInfo, 10, floorLayer)){
            return hitInfo.collider.gameObject;
        }
        return null;
    }


    bool IsLegalMove(Piece piece, Vector2Int endIndex){
        if (piece.Index == endIndex || piece.isLaser) return false;
        if (Vector2Int.Distance(piece.Index, endIndex) > Math.Sqrt(2)) return false;
        Piece endPiece = GetPiece(endIndex);
        if (endPiece != null){
            if (!piece.canSwap || !endPiece.isSwappable) return false;
        }
        if (specialTilesBoard[7 - endIndex.y, endIndex.x] != piece.team && specialTilesBoard[7 - endIndex.y, endIndex.x] != 0) return false;
        return true;
    }
    void OnDrawGizmos(){
        if (!showSpecialTiles) return;
        for (int y = 0; y < 8; y++){
            for (int x = 0; x < 10; x++){
                if (specialTilesBoard[y, x] == 0) continue;
                Gizmos.color = specialTilesBoard[7-y, x] == 1 ? Color.red : specialTilesBoard[7-y, x] == 2 ? Color.blue : Color.green;
                Gizmos.DrawCube(IndexToPosition(new Vector2Int(x, y)), Vector3.one * displayTileRadius);
            }
        }

    }
}
