using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class Ninja : MonoBehaviour, IAgent
{
    [SerializeField] GameObject smokeGrenade;
    [SerializeField] LayerMask ignoreInRaycast;
    [SerializeField]Transform nearestCover;
    Vector3 nearestCoverLocation;
    [SerializeField] GameObject agentToHelp;
    Player playerAgent;
    IEnemyAttackable iPlayerAgent;
    [SerializeField] List<GameObject> enemiesAttackingWard = new();
    [SerializeField] TextMeshProUGUI displayText;
    [SerializeField] float hoverRangeMin, hoverRangeMax;
    NavMeshAgent agent;
    BehaviourTree tree;
    Dictionary<string, Func<bool>> strategyBreaks;
    public string currentActiveLeaf;
    GameObject[] covers;
    bool isBehindCover;
    public float arcHeight = 5f, speed = 10f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        tree = new BehaviourTree(GetType().Name);  
        covers = GameObject.FindGameObjectsWithTag("Cover");


        if(agentToHelp.GetComponent<Player>() is IEnemyAttackable playerAgent)
        {
            iPlayerAgent = playerAgent;
            Debug.Log("Player Agent = " + iPlayerAgent);
        }

        //Guard Enemy Behaviour
        NinjaBehaviour();
        AddStrategyBreaks();
    }

    private void Update()
    {
        enemiesAttackingWard = GetAttackingEnemies();
        bool interrupt = CheckStrategyBreaks();;
        tree.Process(interrupt); 
    }

    private void NinjaBehaviour()
    {
        PrioritySequence actions = new PrioritySequence("Ninja Logic");

        Sequence moveToCover = new Sequence("moveToCover", 10);
            moveToCover.AddChild(new Leaf("CheckDangerAndCover", new Condition(() => DetectDanger())));
            moveToCover.AddChild(new Leaf("MoveToCover", new MoveToTarget(gameObject, gameObject.transform, agent, nearestCover, 5f)));
            moveToCover.AddChild(new Leaf("ThrowGrenade", new ActionStrategy(() => FireProjectile())));
            moveToCover.AddChild(new Leaf("FollowPlayer", new MoveToTarget(gameObject, gameObject.transform, agent, iPlayerAgent.getTransform(), 5f)));

        Sequence followPlayer = new Sequence("FollowPlayer", 5);
            followPlayer.AddChild(new Leaf("CheckInHoverRange", new Condition(() => CalculateDistanceToPlayer() >= hoverRangeMin)));
            followPlayer.AddChild(new Leaf("FollowPlayer", new MoveToTarget(gameObject, gameObject.transform, agent, iPlayerAgent.getTransform(), 5f)));

        Sequence idle = new Sequence("Idle");
            idle.AddChild(new Leaf("Idle", new Idle(gameObject, agent)));

        actions.AddChild(moveToCover);
        actions.AddChild(followPlayer);
        actions.AddChild(idle);
        tree.AddChild(actions);
    }

    void AddStrategyBreaks()
    {
        strategyBreaks = new()
        {
            ["FollowPlayer"] = () => CalculateDistanceToPlayer() <= hoverRangeMin || DetectDanger(),
            ["Idle"] = () => CalculateDistanceToPlayer() >= hoverRangeMax || DetectDanger()
        };
    }

    public void FireProjectile()
    {
        Transform target = enemiesAttackingWard[0].transform;

        if (smokeGrenade != null && target != null)
        {
            GameObject projectile = Instantiate(smokeGrenade, transform.position, Quaternion.identity);
            StartCoroutine(TravelInArc(projectile, target));
        }
    }
    bool DetectDanger()
    {
        Debug.Log("Detecting Danger");
        if(iPlayerAgent != null)
        {
            if(!iPlayerAgent.getIsBeingAttacked()) 
            {
                Debug.Log($"Player not being attacked -{gameObject.name}");
                return false;
            }
            Debug.Log($"Player being attacked -{gameObject.name}"); 
            
            Transform cover = FindCover();
            if(cover != null) 
            {
                nearestCover.position = cover.position;
                Debug.Log("Cover found and set: " + nearestCover.position + " , " + cover.name);
            }
            if(cover == null) return false;
            Debug.Log("Detecting danger returning true");
            return true;
        }
        return false;
    }

    Transform FindCover()
    {
        float lowestDistance = Mathf.Infinity;
        Transform foundCover = null;
        GetAttackingEnemies();

        foreach(GameObject _cover in  covers)
        {
            Debug.Log("Cover: Checking for: " + enemiesAttackingWard[0].name);
            if(TryHitRaycast(_cover.transform, enemiesAttackingWard[0].transform, enemiesAttackingWard[0]))
            {
                float distance = (transform.position - enemiesAttackingWard[0].transform.position).magnitude;
                
                if(distance < lowestDistance)
                {
                    lowestDistance = distance;
                    foundCover = _cover.transform;
                }
                
            }
        }
        if(foundCover != null) Debug.Log("Cover Found At: " + foundCover);
        else Debug.Log("Cover not found");
        return foundCover;
    }

    float CalculateDistanceToPlayer()
    {
        Debug.Log("Player Agent Distance = " + (transform.position - iPlayerAgent.getTransform().position).magnitude);
        return (transform.position - iPlayerAgent.getTransform().position).magnitude;
    }

    bool CheckStrategyBreaks()
    {
        Debug.Log("Current Active Leaf: " + currentActiveLeaf);
        if(!strategyBreaks.ContainsKey(currentActiveLeaf)) return false;
        if(strategyBreaks[currentActiveLeaf]()) return true;
        return false;
    }
    Transform IAgent.getTransform()
    {
        return transform;
    }

    TextMeshProUGUI IAgent.getDisplayText()
    {
        return displayText;
    }

    void IAgent.setCurrentActiveLeaf(string leafName)
    {
        currentActiveLeaf = leafName;
    }

    bool TryHitRaycast(Transform _startPos, Transform _endPos, GameObject objectToHit)
    {
        Debug.Log("Guard tries to hit raycast");

        Vector3 startPos = _startPos.position;
        Vector3 direction = _endPos.transform.position - startPos;

        RaycastHit hit;
        Debug.DrawRay(startPos, direction, Color.red);

        if (Physics.Raycast(startPos, direction, out hit, Mathf.Infinity))
        {

            if ((hit.collider.gameObject != objectToHit) && (hit.collider.gameObject.layer != ignoreInRaycast)) 
            {
                Debug.Log("Raycast Hit Object: " + hit.collider.gameObject.name);
                return false;
            }
            else
            {
                return true;
            } 
        }


        return false;
    }

    Weapon IAgent.GetWeapon()
    {
        return null;
    }


    List<GameObject> GetAttackingEnemies()
    {
        if(!enemiesAttackingWard.Contains(iPlayerAgent.GetAttackingAgent()))
        {
            if(iPlayerAgent.GetAttackingAgent() == null) return enemiesAttackingWard;
            enemiesAttackingWard.Add(iPlayerAgent.GetAttackingAgent());
        }

        return enemiesAttackingWard;
    }

    private IEnumerator TravelInArc(GameObject projectile, Transform targetTransform)
    {
        Vector3 startPoint = projectile.transform.position;
        Vector3 targetPoint = targetTransform.position;

        float distance = Vector3.Distance(startPoint, targetPoint);
        float travelTime = distance / speed;

        float elapsedTime = 0f;

        while (elapsedTime < travelTime && projectile != null && targetTransform != null)
        {
            targetPoint = targetTransform.position;
            float t = elapsedTime / travelTime;
            Vector3 currentPosition = Vector3.Lerp(startPoint, targetPoint, t);
            float height = Mathf.Sin(t * Mathf.PI) * arcHeight;
            currentPosition.y += height;
            projectile.transform.position = currentPosition;
            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (projectile != null)
        {
            projectile.transform.position = targetPoint;
        }

        if(enemiesAttackingWard.Count > 0)
        {
            enemiesAttackingWard[0].GetComponent<IAgent>().setIsStunned();
            Debug.Log("Stun the enemy: " + enemiesAttackingWard[0].name);
        }

        Destroy(projectile);
    }

    void IAgent.setIsStunned()
    {
        //Noop
    }

    void IAgent.DropWeapon()
    {
        //Noop
    }

}
