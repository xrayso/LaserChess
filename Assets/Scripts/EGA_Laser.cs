using System.Collections;
using UnityEngine;
using UnityEditor.Events;
using UnityEngine.Events;

public class EGA_Laser : MonoBehaviour {
    public GameObject HitEffect;
    public float HitOffset = 0;

    private LineRenderer Laser;
    public float MainTextureLength = 1f;
    public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1,1,1,1);
    
    private bool LaserSaver = false;
    private bool UpdateSaver = false;

    private ParticleSystem[] Effects;
    private ParticleSystem[] Hit;

    [SerializeField] GameObject laserPrefab; 
    [SerializeField] float speed, MaxLength;
    float currentLength = 0f;
    private int mirrorLayer, destroyableLayer, boundaryLayer; 
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] bool shootLaser;
    float lerpAmount;
    EGA_Laser nextLaser; 
    public EGA_Laser originalLaser;

    public UnityEvent doneFiringEvent;

    
    void Awake (){
        Laser = GetComponent<LineRenderer>();
        Effects = GetComponentsInChildren<ParticleSystem>();
        Hit = HitEffect.GetComponentsInChildren<ParticleSystem>();
        mirrorLayer = LayerMask.NameToLayer("Mirror");
        destroyableLayer = LayerMask.NameToLayer("Destroyable");
        boundaryLayer = LayerMask.NameToLayer("Boundary");
        doneFiringEvent ??= new UnityEvent();
        
        for (int i = 0; i < transform.childCount; i++){
            if (transform.GetChild(i).gameObject.name == "Sphere" || transform.GetChild(i).gameObject.name == "Canon") continue;
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }



    public void StartShooting(){
        shootLaser = true;
        lerpAmount = 0;
    }
    public void StopShooting(){
        if (this != originalLaser) {
            originalLaser.StopShooting();
            return;
        }
        shootLaser = false;
        currentLength = 0;
        if (nextLaser != null) Destroy(nextLaser.gameObject);
        Laser.material.SetTextureScale("_MainTex", Vector2.zero);                    
        Laser.material.SetTextureScale("_Noise", Vector2.zero);
        doneFiringEvent.Invoke();
    }
    
    void Update(){
        
        if (!shootLaser) return;
        

        Laser.material.SetTextureScale("_MainTex", new Vector2(Length[0], Length[1]));                    
        Laser.material.SetTextureScale("_Noise", new Vector2(Length[2], Length[3]));
        //To set LineRender position
        if (Laser != null && UpdateSaver == false){
            Laser.SetPosition(0, transform.position);
            RaycastHit hit; 
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, currentLength, ~ignoreLayer)) {
                //End laser position if collides with object
                Laser.SetPosition(1, hit.point);
                HitEffect.transform.position = hit.point + hit.normal * HitOffset;
                currentLength = hit.distance;
                //Hit effect zero rotation
                HitEffect.transform.rotation = Quaternion.identity;
                foreach (var AllPs in Effects)
                {
                    if (!AllPs.isPlaying) AllPs.Play();
                }
                //Texture tiling
                Length[0] = MainTextureLength * Vector3.Distance(transform.position, hit.point);
                Length[2] = NoiseTextureLength * Vector3.Distance(transform.position, hit.point);
                
                int hitLayer = hit.collider.gameObject.layer;
                if (hitLayer == mirrorLayer){
                    Vector3 direction = Vector3.Reflect(transform.forward, hit.normal);
                    if (nextLaser != null){
                        if (nextLaser.transform.position != hit.point || nextLaser.transform.forward != direction){
                            nextLaser.transform.position = hit.point;
                            nextLaser.transform.forward = direction;
                        } 
                    }else{
                        nextLaser = Instantiate(laserPrefab).GetComponent<EGA_Laser>();
                        nextLaser.transform.position = hit.point;
                        nextLaser.transform.forward = direction;
                        nextLaser.originalLaser = originalLaser;
                        nextLaser.StartShooting();
                        
                    }       
                }else if (hitLayer == destroyableLayer || hitLayer == boundaryLayer){
                    if (hitLayer == destroyableLayer) Destroy(hit.collider.gameObject);
                    StopShooting();
                }else{
                    StopShooting();
                }
                
                
            }else if (currentLength < MaxLength){
                if (nextLaser != null) Destroy(nextLaser.gameObject);
                var EndPos = transform.position + transform.forward * currentLength;
                Laser.SetPosition(1, EndPos);
                HitEffect.transform.position = EndPos;
                foreach (var AllPs in Hit)
                {
                    if (AllPs.isPlaying) AllPs.Stop();
                }
                Length[0] = MainTextureLength * Vector3.Distance(transform.position, EndPos);
                Length[2] = NoiseTextureLength * Vector3.Distance(transform.position, EndPos);
                currentLength = Mathf.Lerp(currentLength, MaxLength, lerpAmount);
                lerpAmount += speed * Time.deltaTime;
            }
            if (Laser.enabled == false && LaserSaver == false){
                LaserSaver = true;
                Laser.enabled = true;
            }
        }  
        
    }


    public void DisablePrepare(){
        if (Laser != null)
        {
            Laser.enabled = false;
        }
        UpdateSaver = true;
        //Effects can = null in multiply shooting
        if (Effects != null)
        {
            foreach (var AllPs in Effects)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
    }
    
    void OnDestroy(){
        if (nextLaser != null){
            Destroy(nextLaser.gameObject);
        }
    }
}
