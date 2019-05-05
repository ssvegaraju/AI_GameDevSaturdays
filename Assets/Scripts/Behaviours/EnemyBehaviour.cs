using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBehaviour : MonoBehaviour
{
    private NavMeshAgent nav;
    private List<TreeNode> nodes;
    private Transform player;
    private BT tree;

    private Vector3 lastPlayerPos = Vector3.zero;
    TreeNode idle, chase;

    public float viewDistance;
    [Range(0, 90)] private float viewAngle = 70;
    public LayerMask viewMask;
    private bool playerSpotted = false;

    public List<Vector3> points;
    private int pointIndex;

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        player = FindObjectOfType<PlayerMovement>().transform;
        idle = new TreeNode(IsPlayerSpotted, OnPlayerSpotted, OnFailure);
        chase = new TreeNode(IsPlayerStillInSight, OnPlayerSpotted, OnFailure);
        nodes = new List<TreeNode>() {
            idle, chase
        };
        tree = new BT(nodes);
        StartCoroutine(tree.Tick());
    }

    IEnumerator IsPlayerSpotted(Action<BTEvaluationResult> callback) {
        while (true) {
            playerSpotted = CanSeePlayer();
            if (playerSpotted) {
                playerSpotted = false;
                //tree.treeNodes.Remove(idle);
                callback(BTEvaluationResult.Success);
                print("anemone spotted");
                yield break;
            }
            int pointToLookAt = (pointIndex + 1 >= points.Count) ? 0 : pointIndex + 1;
            if (!nav.pathPending) {
                if (nav.remainingDistance <= nav.stoppingDistance) {
                    if (!nav.hasPath || nav.velocity.sqrMagnitude == 0f) {
                        yield return StartCoroutine(MoveToPoint(pointToLookAt));
                    }
                }
            }
            yield return null;
        }
    }

    IEnumerator IsPlayerStillInSight(Action<BTEvaluationResult> callback) {
        while (true) {
            playerSpotted = CanSeePlayer();
            if (playerSpotted) {
                Debug.Log("Spotted");
                lastPlayerPos = player.position;
                nav.SetDestination(lastPlayerPos);
                callback(BTEvaluationResult.Continue);
            }
            if (player == null) {
                Debug.Log("Success");
                callback(BTEvaluationResult.Success);
                yield break;
            }
            if (!nav.pathPending) {
                if (nav.remainingDistance <= nav.stoppingDistance) {
                    if (!nav.hasPath || nav.velocity.sqrMagnitude == 0f) {
                        Debug.Log("Failure");
                        callback(BTEvaluationResult.Failure);
                        yield break;
                    }
                }
            }
            yield return null;
        }
    }

    IEnumerator OnPlayerSpotted() {
        playerSpotted = true;
        yield return null;
    }

    IEnumerator OnFailure() {
        playerSpotted = false;
        yield return null;
    }

    IEnumerator MoveToPoint(int pointToLookAt) {
        pointIndex++;
        if (pointIndex >= points.Count) {
            pointIndex = 0;
        }
        yield return StartCoroutine(LookAtPoint(points[pointToLookAt]));
        nav.SetDestination(points[pointToLookAt]);
    }

    IEnumerator LookAtPoint(Vector3 point) {
        Vector3 lookDir = point - transform.position;
        lookDir.y = 0;
        Quaternion toRot = Quaternion.LookRotation(lookDir, Vector3.up);
        float startTime = Time.time;
        while (Time.time - startTime <= 1) {
            transform.rotation = Quaternion.Slerp(transform.rotation, toRot, Time.time - startTime);
            yield return null;
        }
    }

    bool CanSeePlayer() {
        if (Vector3.Distance(transform.position, player.position) > viewDistance) {
            return false;
        }
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer < viewAngle / 2f) {
            if (!Physics.Linecast(transform.position, player.position, viewMask)) {
                return true;
            }
        }
        return false;
    }
}
