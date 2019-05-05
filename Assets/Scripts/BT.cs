using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BT
{
    public List<TreeNode> treeNodes;

    public BT(List<TreeNode> treeNodes) {
        this.treeNodes = treeNodes;
    }

    public IEnumerator Tick() {
        int index = 0;
        while (treeNodes.Count > 0) {
            TreeNode node = treeNodes[index];
            while (true) {
                BTEvaluationResult result = BTEvaluationResult.Default;
                yield return PlayerMovement.instance.StartCoroutine(
                    node.Test(delegate (BTEvaluationResult res) {
                        result = res;
                    }));
                while (result == BTEvaluationResult.Default) {
                    yield return null;
                }
                switch(result) {
                    case BTEvaluationResult.Success:
                        yield return PlayerMovement.instance.StartCoroutine(node.success());
                        break;
                    case BTEvaluationResult.Failure:
                        yield return PlayerMovement.instance.StartCoroutine(node.failure());
                        break;
                    case BTEvaluationResult.Continue:
                        break;
                    default:
                        break;
                }
                if (result != BTEvaluationResult.Continue) {
                    index++;
                    if (index >= treeNodes.Count) {
                        index = 0;
                    }
                    break;
                }
                yield return null;
            }
            yield return null;
        }
    }
}
public class TreeNode {
    private Func<Action<BTEvaluationResult>, IEnumerator> test;

    public Func<IEnumerator> success, failure;

    public TreeNode(Func<Action<BTEvaluationResult>, IEnumerator> test, Func<IEnumerator> success,
        Func<IEnumerator> failure, TreeNode child = null) {
        this.test = test;
        this.success = success;
        this.failure = failure;
    }

    public IEnumerator Test(Action<BTEvaluationResult> callback) {
        return test(callback);
    }
}

public enum BTEvaluationResult {
    Success,
    Failure,
    Continue,
    Default
}
