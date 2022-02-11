using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] CharacterController controller;
    [SerializeField] float health = 3;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float empoweredTime = 1f;
    [SerializeField] float immunityTime = 0.3f;
    float timer;
    bool isEmpowered;

    [Header("Sword")]
    [SerializeField] GameObject sword;
    [SerializeField] GameObject swordProjectile;
    [SerializeField] LayerMask enemyLayer;
    Transform hitboxCentreStart;
    Transform hitboxCentreEnd;
    bool isAttacking;
    bool isGoingForward;
    bool isHitboxOn;
    float hitboxR;
    float damage;

    [Header("Primary")]
    [SerializeField] [Range(0f,1f)] float inTime = 0.2f;
    [SerializeField] [Range(0f,1f)] float outTime = 0.9f;
    [SerializeField] Transform startPos;
    [SerializeField] float maxDistance = 8f;
    [SerializeField] Transform primHitboxCentreStart;
    [SerializeField] Transform primHitboxCentreEnd;
    [SerializeField] float primHitboxR;
    [SerializeField] float primaryCd;
    [SerializeField] float primDamage = 1;
    float disFrac;
    float primaryTimer;

    [Header("Secondary")]
    [SerializeField] float secCd = 1f;
    [SerializeField] float spinRotation = 360f;
    [SerializeField] float spinTime = 1f;
    [SerializeField] Transform secHitboxCentre;
    [SerializeField] float secHitboxR;
    [SerializeField] float secDamage = 5;
    float secondaryTimer = 10f;
    bool isSecondary;

    [Header("Tertiary")]
    [SerializeField] float terCd = 1f;
    [SerializeField] float terDis = 5f;
    [SerializeField] float terSpeed = 10f;
    [SerializeField] float terDamageMult = 1.5f;
    [SerializeField] float terDamageMultTime = 1f;
    [SerializeField] float terDamage = 2f;
    float tertiaryTimer = 10f;
    bool isTertiary;
    bool isDamageMult;

    [Header("Parry")]
    [SerializeField] float parryR;
    [SerializeField] float parryCd;
    [SerializeField] float parryActiveTime;
    [SerializeField] LayerMask hazardLayer;
    [SerializeField] GameObject floor;
    float parryTimer;
    float parryActiveTimer;
    bool isParrying;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Run();
        
        Parry();
        SetAttackProperties();
    }

    private void FixedUpdate()
    {
        Primary();
        Secondary();
        Tertiary();

        CheckParry();
        CheckIfSwordHit();
        CheckIfGotHit();
    }

    private void Run()
    {
        if(!isTertiary)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(x, 0f, z).normalized * moveSpeed * Time.deltaTime;
            controller.Move(move);
        }
    }

    private void Primary()
    {
        primaryTimer += Time.deltaTime;
        if (Input.GetButton("Attack") && primaryTimer > primaryCd && !isSecondary)
        {
            isAttacking = true;
            isGoingForward = true;
            primaryTimer = 0f;
            var lookDir = GetComponent<LookAtMouse>().GetMousePos();
            sword.transform.LookAt(new Vector3(lookDir.x, sword.transform.position.y, lookDir.z));
            var endPos = GetSwordEndPos();
            disFrac = endPos.z / maxDistance;
            LeanTween.moveLocal(sword, endPos, primaryCd * inTime * disFrac).setEaseInBack();
            startPos.localPosition = new Vector3(-startPos.localPosition.x, startPos.localPosition.y, startPos.localPosition.z);
        }
        if (!LeanTween.isTweening(sword) && isGoingForward)
        {
            LeanTween.moveLocal(sword, startPos.localPosition, primaryCd * outTime * disFrac).setEaseOutBack();
            isGoingForward = false; //get rid of this but causes early animation end
        }
        if (sword.transform.position == startPos.position && isAttacking)
        {
            isAttacking = false;
            disFrac = 1f;
        }
    }

    private void Secondary()
    {
        secondaryTimer += Time.deltaTime; 
        if (Input.GetButtonDown("Secondary") && secondaryTimer > secCd)
        {
            isSecondary = true;
            secondaryTimer = 0f;
            var swordAttack = Instantiate(swordProjectile, transform.position, Quaternion.identity);
            StartCoroutine(PerformSecondary(swordAttack));
        }
    }

    private IEnumerator PerformSecondary(GameObject swordAttack)
    {
        for(float spin = 0; spin < spinRotation;)
        {
            var rotationThisFrame = spinRotation * Time.deltaTime / spinTime;
            swordAttack.transform.eulerAngles += new Vector3(0f, rotationThisFrame, 0f);
            swordAttack.transform.position = transform.position;
            spin += rotationThisFrame;
            yield return new WaitForEndOfFrame();
        }
        Destroy(swordAttack);
        isSecondary = false;
        yield return null;
    }

    private void Tertiary()
    {
        tertiaryTimer += Time.deltaTime;
        if (Input.GetButtonDown("Tertiary") && tertiaryTimer > terCd)
        {
            isTertiary = true;
            isDamageMult = true;
            tertiaryTimer = 0f;
            var dir = GetComponent<LookAtMouse>().GetMousePos() - transform.position;
            var distance = terDis < dir.magnitude ? terDis : dir.magnitude;
            StartCoroutine(PerformTertiary(dir.x, dir.z, distance));
        }
    }

    private IEnumerator PerformTertiary(float x, float z, float distance)
    {
        for(float disMoved = 0; disMoved < distance;)
        {
            Vector3 move = new Vector3(x, 0f, z).normalized * terSpeed * Time.deltaTime;
            controller.Move(move);
            disMoved += move.magnitude;
            yield return new WaitForEndOfFrame();
        }
        isTertiary = false;
        yield return new WaitForSeconds(terDamageMult);
        isDamageMult = false;
        yield return null;
    }
    
    private void SetAttackProperties()
    {
        if (isAttacking)
        {
            hitboxCentreStart = primHitboxCentreStart;
            hitboxCentreEnd = primHitboxCentreEnd;
            hitboxR = primHitboxR;
            damage = primDamage;
        }
        else if (isSecondary)
        {
            hitboxCentreStart = secHitboxCentre;
            hitboxCentreEnd = secHitboxCentre;
            hitboxR = secHitboxR;
            damage = secDamage;
        }
        else if (isTertiary)
        {
            hitboxCentreStart = transform;
            hitboxCentreEnd = transform;
            hitboxR = parryR;
            damage = terDamage;
        }
        else
        {
            hitboxCentreStart = transform;
            hitboxR = 0;
            damage = 0;
        }
        if (isDamageMult)
        {
            damage *= terDamageMult;
        }
    }

    private void CheckIfGotHit()
    {
        timer += Time.deltaTime;
        var radius = GetComponent<CharacterController>().radius;
        Collider[] hitHazards = Physics.OverlapSphere(transform.position, radius, hazardLayer);
        if (timer >= immunityTime)
        {
            timer = 0f;
            foreach (Collider hazard in hitHazards)
            {
                if(hazard.GetComponent<Hazard>())
                {
                    health -= hazard.GetComponent<Hazard>().GetDamage();
                }
            }
            if (health <= 0)
            {
                print("ded");
            }
        }
    }

    private void CheckIfSwordHit()
    {
        if (hitboxCentreStart && hitboxCentreEnd && isAttacking)
        {
            Collider[] destrucitblesHit = Physics.OverlapSphere(hitboxCentreStart.position, hitboxR, hazardLayer);
            foreach (Collider enemy in destrucitblesHit)
            {
                if (enemy.GetComponent<Destructible>())
                {
                    enemy.transform.GetComponent<Destructible>().ProcessHit(damage);
                }
            }
        }
    }

    private Vector3 GetSwordEndPos()
    {
        RaycastHit bodyHit, swordHit;
        Debug.DrawRay(sword.transform.position, sword.transform.forward * maxDistance, Color.green, 0.5f);
        bool objectAheadSword = Physics.Raycast(sword.transform.position, sword.transform.forward, out swordHit, maxDistance, hazardLayer);
        bool objectAheadBody = Physics.Raycast(transform.position, transform.forward, out bodyHit, maxDistance, hazardLayer);
        float distanceToObject = maxDistance;
        if(objectAheadSword)
        {
            distanceToObject = (swordHit.point - sword.transform.position).magnitude;
        }
        else if(objectAheadBody)
        {
            distanceToObject = (bodyHit.point - transform.position).magnitude;
        }
        var endPos = new Vector3(0f, 0f, distanceToObject);
        return endPos;
    }

    private void OnDrawGizmosSelected()
    {
        if(isAttacking)
        {
            Gizmos.DrawSphere(primHitboxCentreStart.position, primHitboxR);
            Gizmos.DrawSphere(primHitboxCentreEnd.position, primHitboxR);
        }
        if (isParrying)
        {
            Gizmos.DrawSphere(transform.position, parryR);
        }
        if(isSecondary)
        {
            Gizmos.DrawSphere(secHitboxCentre.position, secHitboxR);
        }
    }

    private void Parry()
    {
        parryTimer += Time.deltaTime;
        if (Input.GetButtonDown("Parry") && parryTimer > parryCd)
        {
            parryTimer = 0f;
            isParrying = true;
        }
    }

    private void CheckParry()
    {
        if(isParrying)
        {
            parryActiveTimer += Time.deltaTime;
            if(parryActiveTimer <= parryActiveTime)
            {
                ParryEffect();
            }
            else
            {
                isParrying = false;
                parryActiveTimer = 0f;
            }
        }
    }

    private void ParryEffect()
    {
        Collider[] parriedHazards = Physics.OverlapSphere(transform.position, parryR, hazardLayer);
        foreach (Collider hazard in parriedHazards)
        {
            isEmpowered = true;
            if (hazard.GetComponent<Hazard>())
            {
                hazard.GetComponent<Hazard>().SetCollisions(false);
            }
            StartCoroutine(AnimateParry());
        }
    }

    private IEnumerator AnimateParry()
    {
        var floorMaterial = floor.GetComponent<MeshRenderer>().material;
        var speed = floorMaterial.GetFloat("_Speed");
        var pos = new Vector2(transform.position.x, transform.position.z);
        floorMaterial.SetVector("_FocalPoint", pos);
        floorMaterial.SetFloat("_Play", 1);
        for (float phase = 0f; phase < 1f/speed; phase += Time.deltaTime)
        {
            floorMaterial.SetFloat("_Phase", phase);
            yield return new WaitForEndOfFrame();
        }
        floorMaterial.SetFloat("_Play", 0);
        yield return null;
    }
}
