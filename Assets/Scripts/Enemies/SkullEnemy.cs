using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FunkyCode;

public class SkullEnemy : Enemy
{
    Light2D light2D;
    Tween idleScaleTween;
    float baseLightAlpha;
    
    [Header("Skull")]
    [SerializeField] ParticleEmitter particleEmitter;
    [SerializeField] bool particleEmitterEnabled = true;
    Tween deactivationTween;
    [SerializeField] protected GameObject flames;
    [Header("Distortion Sprite")]
    [SerializeField] protected bool useDistortionEffect = true;
    [SerializeField] protected GameObject distortionSprite;
    [SerializeField] protected Material flamesMaterial;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        light2D = GetComponent<Light2D>();
        baseLightAlpha = light2D.color.a;
        light2D.color = flames.GetComponent<SpriteRenderer>().color;

        particleEmitter.gameObject.SetActive(particleEmitterEnabled);
        particleEmitter.color = flames.GetComponent<SpriteRenderer>().color;

        flames.GetComponent<Animator>().Play("Base Layer.FlamingSkull_Flames", 0, Random.Range(0f, 1f));
        // light2D.color.a = baseLightAlpha;

        float currentScale = transform.localScale.x;
        idleScaleTween = transform.DOScale(new Vector3(currentScale * 1.15f, currentScale * 1.15f, 1f), 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetDelay(Random.Range(0f, .5f));
        
        // RectTransform rt = (RectTransform)transform;
 
        float width = GetComponent<SpriteRenderer>().bounds.size.x;
        float height = GetComponent<SpriteRenderer>().bounds.size.y;
        particleEmitter.GetComponent<ParticleEmitter>().SetSpawnRange(width * .9f, height / 2);
    
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), GameObject.Find("WizardBoxCollider").GetComponent<Collider2D>());

        distortionSprite.SetActive(useDistortionEffect);
        // flamesMaterial = flames.GetComponent<Renderer>().material;
    }

    protected override void InitializeMaterializeSpawn(){
        base.InitializeMaterializeSpawn();

        // if(defaultMaterial == null){
        //     defaultMaterial = GetComponent<Renderer>().material;
            // flames.GetComponent<Renderer>().material.SetVector("_DistortionVelocity", distortionVelocity);
        // }
    }    

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        UpdateFlames();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected void UpdateFlames(){
        flames.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder - 1;
    }

    protected override void UpdateCheckerboardMaterializeSpawn()
    {
        // base.UpdateCheckerboardMaterializeSpawn();
        if(!materializing){
            return;
        }
        currentCheckerSize += new Vector2(checkerChangeRate, checkerChangeRate);
        // Debug.Log($"Current checker size: {currentCheckerSize}");
        GetComponent<Renderer>().material.SetVector("_CheckerBoardFrequency", currentCheckerSize);
        if(currentCheckerSize.x >= checkerSizeTarget){
            materializing = false;
            GetComponent<Renderer>().material = defaultMaterial;
            Material material = GetComponent<Renderer>().material;
            // Debug.Log("Spawning finished");
        }
        // Debug.Log("flames spawn update");

        flames.GetComponent<Renderer>().material.SetVector("_CheckerBoardFrequency", currentCheckerSize);
        if(currentCheckerSize.x >= checkerSizeTarget){
            flames.GetComponent<Renderer>().material = flamesMaterial;
        }
    }

    public override void ChangeHP(int deltaHP)
    {
        base.ChangeHP(deltaHP);
        if(deltaHP < 0){
            Engaged = true;
            engagedDT = 0;
        }

        // Debug.Log($"Taking Damage: {hp}");
        if(hp <= 0){
            Die();
        }
    }

    protected override void Die(){
        base.Die();
        DieAnimation();
        particleEmitter.GetComponent<ParticleEmitter>().activelySpawning = false;
        flames.GetComponent<SpriteRenderer>().sortingLayerName = "Walls";
        flames.GetComponent<SpriteRenderer>().sortingOrder = spriteRenderer.sortingOrder - 1;
    }

    public override void Revive(bool setOriginPosition = true)
    {
        base.Revive(setOriginPosition);

        if(deactivationTween != null && deactivationTween.IsPlaying()){
            deactivationTween.Pause();
            deactivationTween = null;
        }

        EnableRenderer(true);
        light2D.color.a = baseLightAlpha;
        particleEmitter.GetComponent<ParticleEmitter>().activelySpawning = true;
        flames.SetActive(true);
        flames.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        if(useDistortionEffect){
            distortionSprite.SetActive(true);
        }
        idleScaleTween.Restart();
    }

    private void DieAnimation(){
        float targetY = transform.position.y - .25f;
        Tween yTween = transform.DOMoveY(targetY, .5f).SetEase(Ease.OutBounce);
        transform.DORotate(new Vector3(0, 0, -80), .35f);
        deactivationTween = DOVirtual.DelayedCall(dieAnimationTime, Deactivate);
        flames.SetActive(false);
        DOTween.To(()=> light2D.color.a, x => light2D.color.a = x, 0f, .25f);
        // light2D.color.a = 0;
        if(useDistortionEffect){
            distortionSprite.SetActive(false);
        }
        idleScaleTween.Pause();
    }
    
    public void Deactivate(){
        EnableRenderer(false);
        // light2D.color.a = 0;
        // flames.SetActive(false);
        // if(useDistortionEffect){
        //     distortionSprite.SetActive(false);
        // }
        // HPBar.active = false;
        // HPBar.GetComponent<Renderer>().enabled = false;
        // GetComponent<Rigidbody2D>().Sleep();
    }

    public override void ApplyForceFromMovementTile(Vector2 force, MovementTile movementTile)
    {
        // base.ApplyForceFromMovementTile(force, movementTile);
    }

    public override void StartCheckerboardMaterializeSpawn()
    {
        base.StartCheckerboardMaterializeSpawn();

        Renderer flamesRenderer = flames.GetComponent<Renderer>();
        flamesRenderer.material = spawnMaterial;
        flamesRenderer.material.SetVector("_CheckerBoardFrequency", startingCheckerSize);
        flamesRenderer.material.SetVector("_DistortionVelocity", distortionVelocity);
    }

    protected override void OnCollisionEnter2D(Collision2D collision){
        base.OnCollisionEnter2D(collision);

        if(collision.gameObject.tag == "Player" && enableCollisionDamage && !useTriggerDamageCheck && !hitShield){
            if(!CheckShieldCollision()){
                movementPaused = true;
                movementPauseDT = 0f;
                
                if(followScript != null && followScript.enabled){
                    suspendEngageUpdate = true;
                    followScript.shouldFollow = false;
                }
            }
        }
    }
    protected override void OnCollisionStay2D(Collision2D collision){
        base.OnCollisionStay2D(collision);

        if(collision.gameObject.tag == "Player" && enableCollisionDamage && !useTriggerDamageCheck && !hitShield){
            if(!CheckShieldCollision()){
                movementPaused = true;
                movementPauseDT = 0f;
                
                if(followScript != null && followScript.enabled){
                    suspendEngageUpdate = true;
                    followScript.shouldFollow = false;
                }
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other){
        base.OnTriggerEnter2D(other);

        if(other.tag == "Player" && useTriggerDamageCheck && !hitShield){

            if(!CheckShieldCollision()){
                movementPaused = true;
                movementPauseDT = 0f;
                
                if(followScript != null && followScript.enabled){
                    suspendEngageUpdate = true;
                    followScript.shouldFollow = false;
                }
            }
            // collider2D.gameObject.GetComponent<CharacterControls>()
        }
    }

    protected override void OnTriggerStay2D(Collider2D other){
        base.OnTriggerStay2D(other);
        if(other.tag == "Player" && useTriggerDamageCheck && !hitShield){

            if(!CheckShieldCollision()){
                movementPaused = true;
                movementPauseDT = 0f;
                
                if(followScript != null && followScript.enabled){
                    suspendEngageUpdate = true;
                    followScript.shouldFollow = false;
                }
            }
            // collider2D.gameObject.GetComponent<CharacterControls>()
        }
    }
}
