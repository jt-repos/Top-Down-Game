using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : Destructible
{
    [SerializeField] GameObject bodyObject;
    [SerializeField] float startMoveSpeed = 5f;
    [SerializeField] float turningSpeed = 5f;
    [SerializeField] float attackCdOffset = 2f;
    [SerializeField] float moveSpeed;
    float attackCd;
    bool isAttacking = false;
    Player player;
    float attackTimer;

    [Header("Move Delayed")]
    [SerializeField] float moveSpeedBoost = 2f;
    [SerializeField] float updateTime = 1f;
    float updateTimer;
    Vector3 playerPos;

    [Header("Move Bounce")]
    [SerializeField] float bounceBoostTime = 2f;
    [SerializeField] float bounceOffset = 1f;
    [SerializeField] float minTimeToTurn = 0.3f;
    [SerializeField] int initialTailOffset = 20;
    [SerializeField] int tailOffset = 15;
    bool canBounce;
    float timeFromLastTurn;
    float refreshTimer;
    List<Vector3> trajectory;
    int wallMask = 1 << 8;
    int playerMask = 1 << 9;

    [Header("Health States")]
    [SerializeField] int phase;
    [SerializeField] [Range(0f, 1f)] float earlyPhaseFracHP;
    [SerializeField] [Range(0f, 1f)] float midPhaseFracHP;
    [SerializeField] [Range(0f,1f)] float endPhaseFracHP;

    [Header("Children")]
    [SerializeField] GameObject blockingChildren;
    [SerializeField] GameObject minionChildren;
    [SerializeField] Vector3 orbitOffset;
    [SerializeField] float minionOrbitRadius = 2.5f;
    [SerializeField] float ObstacleOrbitRadius = 4f;
    [SerializeField] float defaultOrbitSpeed = 1f;
    [SerializeField] float minionOrbitSpeedMultiplier = 1.5f;
    [SerializeField] Transform orbitCentre;
    [SerializeField] float defRotationSpeed = 1f;
    float obstacleOrbitSpeed;
    float rotationSpeed;
    bool positionMinions = true;
    bool positionObstacles = true;
    bool isOrbiting = true;
    float offset;

    [Header("Attack Forward A")]
    [SerializeField] float aForwardMagMult;
    [SerializeField] float aForwardInTime;
    [SerializeField] float aForwardOutTime;
    [SerializeField] float aForwardEndTime;
    [SerializeField] float aForwardCd = 5f;

    [Header("Attack Forward A Twist")]
    [SerializeField] float aForwardEndRotation = 360f;
    [SerializeField] Vector3 endAttackMovement;
    [SerializeField] float aForwardTwistTime;

    [Header("Attack Forward B")]
    [SerializeField] float bForwardInTime;
    [SerializeField] float bForwardOutTime;
    [SerializeField] float bForwardEndTime;
    [SerializeField] float bForwardCd = 5f;

    [Header("Attack Forward B Twist")]
    [SerializeField] float bForwardPullBackRotation = 60f;
    [SerializeField] float bForwardPullBackRotationTime = 0.1f;
    [SerializeField] float bForwardRotation = 360f;
    [SerializeField] float bForwardTwistTime;

    [Header("Attack Barrage")]
    [SerializeField] float barrageDelay = 0.2f;

    [Header("Attack Around A")]
    [SerializeField] float shakeMagnitude = 1.1f;
    [SerializeField] float aAroundDisMag = 3f;
    [SerializeField] float shakeTime = 3f;
    [SerializeField] float aAroundInTime = 0.5f;
    [SerializeField] float aAroundOutTime = 0.5f;
    [SerializeField] float aAroundEndOffset = 0.5f;
    [SerializeField] float aAroundCd = 3f;

    [Header("Attack Around B")]
    [SerializeField] float bAroundDisMag = 2.25f;
    [SerializeField] float bAroundOrbitSpeedUp = 3f;
    [SerializeField] int bAroundRepeats = 3;
    [SerializeField] float bAroundInTime = 0.5f;
    [SerializeField] float bAroundOutTime = 0.2f;
    [SerializeField] float bAroundEndTime = 0.5f;
    [SerializeField] float bAroundCd = 3f;

    [Header("Attack Around B End Twist")]
    [SerializeField] float bAroundEndRotation = 180f;

    [Header("Attack Around End Squish")]
    [SerializeField] Vector2 squishDimensions;
    [SerializeField] float squishInTime = 0.75f;
    [SerializeField] float squishOutTime = 0.5f;

    [Header("Spawn Cubes")]
    [SerializeField] int cubesToSpawn = 2;
    [SerializeField] GameObject cubeObject;
    [SerializeField] float spawnRotationSpeed;
    [SerializeField] float spawnTime = 0.5f;

    [Header("Surround Player")] 
    [SerializeField] float followBuildUpMag = 2f;
    [SerializeField] float followEndMovementMag = 0.75f;
    [SerializeField] float followWaitTime = 1.5f;
    [SerializeField] float followInTime = 0.5f;
    [SerializeField] float followOutTime = 0.5f;
    private IEnumerator coroutine;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        trajectory = new List<Vector3>();
        moveSpeed = startMoveSpeed;
        player = FindObjectOfType<Player>();
        rotationSpeed = defRotationSpeed;
        attackCd = aAroundCd;
        obstacleOrbitSpeed = defaultOrbitSpeed;
        playerPos = player.transform.position;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        PositionCubes();
        UpdatePhase();
        if (!isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer > attackCd) //just put all children in separate game object, could change attack forward
            {

                //RandAttack();
                //StartCoroutine(AttackAround());
                //StartCoroutine(SpawnCubes());
                //StartCoroutine(AttackAroundHard());
                //StartCoroutine(SurroundAttack());
                StartCoroutine(AttackBarrage());
            }
        }
    }

    private void UpdatePhase()
    {
        var healthFrac = health / startHealth;
        if (healthFrac >= earlyPhaseFracHP && phase != 0) //if no cubes, whiff and spawn cubes
        {
            print("phase 0");
            phase = 0; 
        }
        if (healthFrac >= midPhaseFracHP && phase != 1) 
        {
            print("phase 1");
            phase = 1; 
        }
        if (healthFrac >= endPhaseFracHP && phase != 2) 
        {
            print("phase 2");
            phase = 2; 
        }
        else if(healthFrac < endPhaseFracHP && phase != 3) 
        {
            print("phase 3");
            phase = 3; 
        }
        else { print("bugged"); }
    }

    private int GetRoll()
    {
        var roll = 0;
        switch(phase)
        {
            case 0:
                roll = Random.Range(0, 2); //next attack move towards with wide and fast orbit or spawn cubes at player pos
                break;
            case 1:
                roll = Random.Range(1, 5);
                break;
            case 2:
                roll = Random.Range(2, 7);
                break;
            case 3:
                roll = Random.Range(0, 0); //snake
                break;
        }
        return roll;
    }

    private void FixedUpdate()
    {
        MoveBounce();
        //health = 1;
        //if(phase == 3)
        //{
        //    MoveBounce();
        //}
        //else
        //{
        //    MoveRandom();
        //}
    }

    private void RandAttack()
    {
        RepositionCubes();
        switch (GetRoll())
        {
            case 0:
                StartCoroutine(AttackForward());
                break;
            case 1:
                StartCoroutine(AttackAround());
                break;
            case 2:
                StartCoroutine(AttackAroundHard());
                break;
            case 3:
                StartCoroutine(AttackForwardHard());
                break;
            case 4:
                StartCoroutine(SpawnCubes());
                break;
            case 5:
                StartCoroutine(AttackBarrage()); //reuse one attack and make it slower or faster
                break;
            case 6:
                StartCoroutine(SurroundPlayer());
                break;
            default:
                break;
        }

    }

    private void Move()
    {
        if (!isAttacking)
        {
            var movement = Vector3.MoveTowards(transform.position, bodyObject.transform.forward, moveSpeed * Time.deltaTime);
            var dir = (bodyObject.transform.position - player.transform.position).normalized;
            var endRotation = Quaternion.LookRotation(dir);
            transform.position = movement;
            bodyObject.transform.rotation = Quaternion.RotateTowards(bodyObject.transform.rotation, endRotation, turningSpeed);
        }
    }

    private void MoveRandom()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer > updateTime) //boost speed after turn change
        {
            playerPos = player.transform.position;
            updateTimer = 0f;
            StartCoroutine(DynamicMoveSpeed(updateTime, moveSpeed));
        }
        if (!isAttacking)
        {
            var movement = Vector3.MoveTowards(transform.position, playerPos, moveSpeed * Time.deltaTime);
            var dir = (bodyObject.transform.position - player.transform.position).normalized;
            var endRotation = Quaternion.LookRotation(dir);
            transform.position = movement;
            bodyObject.transform.rotation = Quaternion.RotateTowards(bodyObject.transform.rotation, endRotation, turningSpeed);
        }
    }

    private void MoveBounce()
    {
        positionMinions = false;
        isAttacking = true;
        MoveCubesTail();
        RaycastHit wallHit, leftHit, rightHit;
        var pos = transform.position;
        var forward = bodyObject.transform.forward;
        var right = bodyObject.transform.right;
        Debug.DrawRay(pos, forward * 5f, Color.green, 0.1f);
        Debug.DrawRay(pos, right * 20f, Color.green, 0.1f);
        Debug.DrawRay(pos, right * -20f, Color.green, 0.1f);


        bool hitWall = Physics.Raycast(pos, forward, out wallHit, 5f, wallMask);
        bool hitLeft = Physics.Raycast(pos, right*-1f, out leftHit, 20f, playerMask);
        bool hitRight = Physics.Raycast(pos, right, out rightHit, 20f, playerMask);
        var movement = Vector3.MoveTowards(pos, pos + forward, 2 * moveSpeed * Time.deltaTime);
        if (hitWall)
        {
            StartCoroutine(CountTimeToBounce());
            var target = Vector3.Reflect(forward, wallHit.normal);
            var offsetX = Random.Range(-bounceOffset, bounceOffset);
            var offsetZ = Random.Range(-bounceOffset, bounceOffset);
            var dir = new Vector3(target.x, 0f, target.z);
            var endRotation = Quaternion.LookRotation(dir);
            bodyObject.transform.rotation = Quaternion.RotateTowards(bodyObject.transform.rotation, endRotation, 180f);
        }
        if (canBounce && (hitRight || hitLeft))
        {
            canBounce = false;
            Quaternion endRotation = new Quaternion();
            if(hitRight) { endRotation = Quaternion.LookRotation(right); }
            else if(hitLeft) { endRotation = Quaternion.LookRotation(right * -1f); }
            bodyObject.transform.rotation = Quaternion.RotateTowards(bodyObject.transform.rotation, endRotation, 180f);
            StartCoroutine(DynamicMoveSpeed(bounceBoostTime, moveSpeedBoost));
        }
        transform.position = movement;
    }

    private IEnumerator CountTimeToBounce()
    {
        canBounce = false;
        yield return new WaitForSeconds(minTimeToTurn);
        canBounce = true;
    }

    private void MoveCubesTail()
    {
        trajectory.Add(bodyObject.transform.position); //change movement to bodyobject only, remember body hitboxes are separate, make snake
        int frameOffset = initialTailOffset;
        int count = trajectory.Count;
        foreach (Transform child in minionChildren.transform)
        {
            if (child.GetComponent<MinionCube>() != null)
            {
                int index = count - 1 - frameOffset;
                if (index < 0)
                {
                    index = 0;
                }
                orbitCentre.position = bodyObject.transform.position;
                child.transform.position = trajectory[index];// Vector3.MoveTowards(child.transform.position, trajectory[index], moveSpeed * Time.deltaTime); 
                frameOffset += tailOffset;
            }
        }
    }

    private IEnumerator DynamicMoveSpeed(float time, float boostMultiplier)
    {
        var endMoveSpeed = startMoveSpeed * boostMultiplier;
        for(float t = 0f; t < time; t += Time.deltaTime)
        {
            moveSpeed = EasingFunction.EaseOutQuad(endMoveSpeed, startMoveSpeed, t / time);
            yield return new WaitForEndOfFrame();
        }
    }

    public void RepositionCubes()
    {
        foreach(Transform child in blockingChildren.transform)
        {
            if(child.GetComponent<ObstacleCube>() != null)
            {
                if(child.GetComponent<ObstacleCube>().GetIsEnabled() == false)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void PositionCubes()
    {
        if (isOrbiting)
        {
            offset = offset - (int)offset + Time.deltaTime * obstacleOrbitSpeed;
        }
        float obstacleIndex = 0;
        float minionIndex = 0;
        float cubeAngle, cubeX, cubeZ;
        if(positionMinions)
        {
            foreach (Transform child in minionChildren.transform)
            {
                int minionCount = GetComponentsInChildren<MinionCube>().Length;
                if (child.GetComponent<MinionCube>() != null)
                {
                    cubeAngle = -((minionIndex / minionCount) + offset * minionOrbitSpeedMultiplier) * Mathf.PI * 2f;
                    cubeX = minionOrbitRadius * Mathf.Cos(cubeAngle);
                    cubeZ = minionOrbitRadius * Mathf.Sin(cubeAngle);
                    minionIndex++;
                }
                else { continue; }
                var newPos = orbitOffset + new Vector3(cubeX, 0f, cubeZ) + orbitCentre.localPosition;
                child.transform.localPosition = Vector3.MoveTowards(child.transform.localPosition, newPos, 1f);
                if (isOrbiting)
                {
                    child.transform.Rotate(new Vector3(0, rotationSpeed, 0));
                }
            }
        }
            

        if(positionObstacles)
        {
            foreach (Transform child in blockingChildren.transform)
            {
                int obstacleCount = GetComponentsInChildren<ObstacleCube>().Length;
                if (child.GetComponent<ObstacleCube>() != null)
                {
                    cubeAngle = ((obstacleIndex / obstacleCount) + offset) * Mathf.PI * 2f;
                    cubeX = ObstacleOrbitRadius * Mathf.Cos(cubeAngle);
                    cubeZ = ObstacleOrbitRadius * Mathf.Sin(cubeAngle);
                    obstacleIndex++;
                }
                else { continue; }
                var newPos = orbitOffset + new Vector3(cubeX, 0f, cubeZ) + orbitCentre.localPosition;
                child.transform.localPosition = Vector3.MoveTowards(child.transform.localPosition, newPos, 1f);
                if (isOrbiting)
                {
                    child.transform.Rotate(new Vector3(0, rotationSpeed, 0));
                }
            }
        }
    }

    private void MoveCubes(float posMagnitude, Vector3 posOffset, float time, bool moveMinions, LeanTweenType tweenType)
    {
        foreach (Transform child in blockingChildren.transform)
        {
            bool moveChild = true;
            if (!moveMinions && child.GetComponent<MinionCube>() != null || child.gameObject == bodyObject) { moveChild = false; }
            if (moveChild)
            {
                var endPos = child.transform.localPosition * posMagnitude + posOffset; //y = mx + c
                LeanTween.moveLocal(child.gameObject, endPos, time).setEase(tweenType);
            }
        }
    }

    private IEnumerator SpawnCubes() //change to set max cubes depending on phase
    {
        attackTimer = Random.Range(-attackCdOffset, 0f);
        isOrbiting = false;
        var rotation = GetComponentInChildren<Hazard>().transform.rotation;
        for (int i = 0; i < cubesToSpawn; i++)
        {
            yield return new WaitForSeconds(spawnTime);
            Instantiate(cubeObject, blockingChildren.transform.position, rotation, blockingChildren.transform);
        }
        isOrbiting = true;

    }

    private IEnumerator AttackForward()
    {
        attackTimer = Random.Range(-attackCdOffset, 0f);
        isAttacking = true;
        isOrbiting = false;
        var endPos = (player.transform.position - transform.position) * aForwardMagMult;
        MoveCubes(1f, endPos, aForwardInTime, false, LeanTweenType.easeInElastic);
        yield return new WaitForSeconds(aForwardInTime);
        MoveCubes(1f, -endPos, aForwardOutTime, false, LeanTweenType.easeOutBack);
        isAttacking = false;
        yield return new WaitForSeconds(aForwardEndTime);
        isOrbiting = true;
        StartCoroutine(AnimTwist(aForwardEndRotation, aForwardTwistTime, Vector3.up, LeanTweenType.easeOutBack));
    }
    private IEnumerator AttackForwardHard()
    {
        attackTimer = Random.Range(-attackCdOffset, 0f);
        isAttacking = true;
        isOrbiting = false;
        Vector3 endPos = Vector3.zero;
        StartCoroutine(AnimTwist(bForwardRotation, bForwardTwistTime, Vector3.left, LeanTweenType.easeOutElastic));
        yield return new WaitForSeconds(bForwardTwistTime / 2f);

        foreach (Transform child in blockingChildren.transform)
        {
            endPos = player.transform.position - child.transform.position;
            LeanTween.moveLocal(child.gameObject, endPos, bForwardInTime).setEaseInOutElastic();
        }
        yield return new WaitForSeconds(bForwardInTime);
        foreach (Transform child in blockingChildren.transform)
        {
            LeanTween.moveLocal(child.gameObject, Vector3.zero, bForwardOutTime).setEaseInQuad();
        }
        isAttacking = false;
        yield return new WaitForSeconds(bForwardEndTime);
        isOrbiting = true;
        StartCoroutine(AnimTwist(aForwardEndRotation, aForwardTwistTime, Vector3.up, LeanTweenType.easeOutBack));
    }

    private IEnumerator AttackBarrage()
    {
        attackTimer = Random.Range(-attackCdOffset, 0f);
        isAttacking = true;
        isOrbiting = false;
        positionMinions = false;
        StartCoroutine(AnimTwist(bForwardRotation, bForwardTwistTime, Vector3.left, LeanTweenType.easeOutElastic));
        yield return new WaitForSeconds(bForwardTwistTime / 2f);
        foreach (Transform child in blockingChildren.transform)
        {
            var endPos = (player.transform.position);
            yield return new WaitForSeconds(barrageDelay);
            LeanTween.move(child.gameObject, endPos, bForwardInTime).setEaseOutElastic();
            StartCoroutine(BarragePullBack(child.gameObject));
            //grow in size
        }
        yield return new WaitForSeconds(bForwardInTime);
        StartCoroutine(AnimShake(true));
        yield return new WaitForSeconds(bForwardInTime);
        StartCoroutine(AnimShake(false));
        isAttacking = false;
        isOrbiting = true;
        positionMinions = true;
        StartCoroutine(AnimTwist(aForwardEndRotation, aForwardTwistTime, Vector3.up, LeanTweenType.easeOutBack));
    }

    private IEnumerator BarragePullBack(GameObject child)
    {
        yield return new WaitForSeconds(bForwardInTime);
        LeanTween.moveLocal(child.gameObject, Vector3.zero, bForwardOutTime).setEaseInQuad();
        StartCoroutine(AnimTwist(60f, 0.1f, Vector3.up, LeanTweenType.easeOutElastic));
    }

    private IEnumerator AttackAround()
    {
        attackTimer = Random.Range(-attackCdOffset, 0f);
        isOrbiting = false;
        isAttacking = true;
        MoveCubes(shakeMagnitude, Vector3.zero, shakeTime, true, LeanTweenType.easeShake);
        yield return new WaitForSeconds(shakeTime);
        MoveCubes(aAroundDisMag, Vector3.zero, aAroundInTime, true, LeanTweenType.easeOutElastic);
        yield return new WaitForSeconds(aAroundInTime);
        MoveCubes(1/aAroundDisMag, Vector3.zero, aAroundOutTime, true, LeanTweenType.easeOutElastic);
        yield return new WaitForSeconds(aAroundEndOffset);
        StartCoroutine(AnimAAroundEnd());
        positionMinions = true;
        isAttacking = false;
        isOrbiting = true;
    }

    private IEnumerator AttackAroundHard()
    {
        attackTimer = Random.Range(-attackCdOffset, 0f);
        isAttacking = true;
        var startSpeed = obstacleOrbitSpeed;
        for (int i = 0; i < bAroundRepeats; i++)
        {
            StartCoroutine(AnimTwist(aForwardEndRotation, aForwardTwistTime, Vector3.down, LeanTweenType.easeInQuart));
            obstacleOrbitSpeed += bAroundOrbitSpeedUp;
            yield return new WaitForSeconds(bAroundEndTime);
            MoveCubes(bAroundDisMag, Vector3.zero, bAroundInTime, false, LeanTweenType.easeOutBounce);
            yield return new WaitForSeconds(bAroundInTime);
            MoveCubes(1/ bAroundDisMag, Vector3.zero, bAroundOutTime, false, LeanTweenType.easeOutBack);
            StartCoroutine(AnimShake(true));
        }
        yield return new WaitForSeconds(bAroundOutTime);
        var endSpeed = obstacleOrbitSpeed;
        StartCoroutine(AnimTwist(bAroundEndRotation, bAroundEndTime, Vector3.down, LeanTweenType.easeOutBack));
        StartCoroutine(AnimShake(false));
        for (float t = 0f; t < bAroundEndTime; t += Time.deltaTime)
        {
            obstacleOrbitSpeed = EasingFunction.EaseOutElastic(endSpeed, startSpeed, t / bAroundEndTime);
            yield return new WaitForEndOfFrame();
        }
        isAttacking = false;
    }

    private IEnumerator SurroundPlayer() //anim teleport cubes disappear
    {
        isAttacking = true;
        coroutine = OrbitPlayer();
        StartCoroutine(coroutine);
        yield return new WaitForSeconds(followWaitTime);
        MoveCubes(followBuildUpMag, Vector3.zero, aAroundInTime, false, LeanTweenType.easeOutElastic);
        yield return new WaitForSeconds(followInTime);
        MoveCubes(-1, Vector3.zero, aAroundInTime, false, LeanTweenType.easeOutBack);
        yield return new WaitForSeconds(followInTime);
        StopCoroutine(coroutine);
        blockingChildren.transform.position = transform.position;
        MoveCubes(-1 / followBuildUpMag * followEndMovementMag, Vector3.zero, aAroundInTime, false, LeanTweenType.easeOutBack);
        isAttacking = false;
        attackTimer = Random.Range(-attackCdOffset, 0f);
    }

    private IEnumerator OrbitPlayer()
    {
        while(true)
        {
            blockingChildren.transform.position = player.transform.position;
            yield return new WaitForEndOfFrame();
        }
    }

    public override void ProcessHit(float damage)
    {
        if (immunityTimer >= immunityTime)
        {
            StartCoroutine(AnimOnHit(true));
            base.ProcessHit(damage);
        }
    }

    private IEnumerator AnimOnHit(bool boolVar)
    {
        float floatVar = boolVar ? 1 : 0;
        var materials = bodyObject.GetComponent<MeshRenderer>().materials;
        foreach (Material material in materials) //change material into red one instead of animating in real time
        {
            material.SetFloat("emitLight", 1);
            print(material.GetFloat("emitLight"));
        }
        yield return new WaitForSeconds(immunityTime);
        foreach (Material material in materials) //change material into red one instead of animating in real time
        {
            material.SetFloat("emitLight", 0);
            print(material.GetFloat("emitLight"));
        }

    }

    private IEnumerator AnimTwist(float rotation, float time, Vector3 axis, LeanTweenType easeIn, LeanTweenType easeOut)
    {
        LeanTween.rotateAroundLocal(bodyObject, axis, aForwardEndRotation, time).setEase(easeIn).setEase(easeOut);
        LeanTween.moveLocal(bodyObject, bodyObject.transform.localPosition + endAttackMovement, time / 2).setEaseOutQuad();
        yield return new WaitForSeconds(time / 2);
        LeanTween.moveLocal(bodyObject, bodyObject.transform.localPosition - endAttackMovement, time / 2).setEaseInSine();
    }

    private IEnumerator AnimTwist(float rotation, float time, Vector3 axis, LeanTweenType ease)
    {
        LeanTween.rotateAroundLocal(bodyObject, axis, aForwardEndRotation, time).setEase(ease);
        LeanTween.moveLocal(bodyObject, bodyObject.transform.localPosition + endAttackMovement, time / 2).setEaseOutQuad();
        yield return new WaitForSeconds(time / 2);
        LeanTween.moveLocal(bodyObject, bodyObject.transform.localPosition - endAttackMovement, time / 2).setEaseInSine();
    }

    private IEnumerator AnimAAroundEnd()
    {
        float hSize, vSize;
        var materials = bodyObject.GetComponent<MeshRenderer>().materials;
        LeanTween.moveLocal(bodyObject, bodyObject.transform.localPosition + endAttackMovement, aForwardTwistTime).setEaseOutQuad();
        for (float t = 0f; t < squishInTime; t += Time.deltaTime)
        {
            hSize = EasingFunction.EaseOutElastic(1f, squishDimensions.x, t / squishInTime);
            vSize = EasingFunction.EaseOutElastic(1f, squishDimensions.y, t / squishInTime);
            var squishSize = new Vector4(hSize, vSize, hSize, 0);
            foreach (Material material in materials)
            {
                material.SetVector("SquishSize", squishSize);
            }
            yield return new WaitForEndOfFrame();
        }
        LeanTween.moveLocal(bodyObject, bodyObject.transform.localPosition - endAttackMovement, aForwardTwistTime).setEaseOutQuad();
        for (float t = 0f; t < squishOutTime; t += Time.deltaTime)
        {
            hSize = EasingFunction.EaseOutBounce(squishDimensions.x, 1f, t / squishOutTime);
            vSize = EasingFunction.EaseOutBounce(squishDimensions.y, 1f, t / squishOutTime);
            var squishSize = new Vector4(hSize, vSize, hSize, 0);
            foreach (Material material in materials)
            {
                material.SetVector("SquishSize", squishSize);

            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator AnimShake(bool boolVar)
    {
        var materials = bodyObject.GetComponent<MeshRenderer>().materials;
        var floatVar = boolVar ? 1 : 0;
        foreach (Material material in materials)
        {
            material.SetFloat("Shake", floatVar);
            print(floatVar); 
        }
        yield return null;
    }
}
