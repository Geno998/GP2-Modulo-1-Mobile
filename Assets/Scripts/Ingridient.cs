using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;
using System;

public class Ingridient : MonoBehaviour
{
    public LevelManager levelManager;

    public bool moving;
    public bool onBred, wasOnBred;
    public bool onPile, wasOnPile;

    public Transform parentIngridientTransform, previousParentIngridientTransform;
    public Ingridient parentIngridient, previousParentIngridient;
    public List<Ingridient> ingridinetsOnPile;

    [SerializeField] float RotSpeed;
    [SerializeField] LayerMask mask;
    [SerializeField] LayerMask maskNoBred;
    [SerializeField] LayerMask maskBred;

    public Vector3 startPos, lastPos, currentPosition;

    Vector3 RotPV;
    Vector3 rotDir;

    Vector3 TargetPos;

    float timer = 3;

    public Transform startPrent;

    public float lerpSpeed = 1.0f;
    private float startTime;

    public static event Action OnIngridientMove;
    public static event Action OnIngridientStop;
    public static event Action OnWinGame;


    public Bred bred;

    void Start()
    {
        startPrent = transform.parent;
        startPos = transform.position;
        currentPosition = transform.position;
        parentIngridientTransform = startPrent;
    }




    public void moveIngridient(Vector3 rayDir)
    {
        Vector3 rayStart;

        if (onBred)
        {
            rayStart = bred.transform.position;
        }
        else
        {

            if (ingridinetsOnPile.Count > 0)
            {
                rayStart = ingridinetsOnPile[ingridinetsOnPile.Count - 1].transform.position;
            }
            else
            {
                rayStart = transform.position;
            }
        }

        if (onBred)
        {
            if (!levelManager.AllIsOnBred) return; 
        }


        Debug.DrawRay(rayStart, rayDir * 2f, Color.red, 5);


        if (Physics.Raycast(rayStart, rayDir, out RaycastHit hit, 2f, maskNoBred))
        {

            if (hit.transform.gameObject.GetComponent<Ingridient>().onPile)
            {
                levelManager.UpdateAllUndoInfo();
                TargetPos = hit.transform.parent.position;
                moving = true;
                RotPV = transform.position + (rayDir * 0.78f);
                rotDir = rayDir;
                timer = 0;
                startTime = Time.time;
                StartCoroutine(MoveBlockCorutine());
            }
            else
            {
                levelManager.UpdateAllUndoInfo();
                TargetPos = hit.transform.position;
                moving = true;
                RotPV = transform.position + (rayDir * 0.78f);
                rotDir = rayDir;
                timer = 0;
                startTime = Time.time;
                StartCoroutine(MoveBlockCorutine());
            }

            OnIngridientMove?.Invoke();
        }


        if (Physics.Raycast(rayStart, rayDir, out RaycastHit hit2, 2f, maskBred))
        {

            if (hit2.transform.parent.tag == "Ingridient")
            {
                levelManager.UpdateAllUndoInfo();
                TargetPos = hit2.transform.parent.position;
                moving = true;
                RotPV = transform.position + (rayDir * 0.78f); ;
                rotDir = rayDir;
                timer = 0;
                startTime = Time.time;
                StartCoroutine(MoveBlockCorutine());
            }
            else
            {
                levelManager.UpdateAllUndoInfo();
                TargetPos = hit2.transform.position;
                moving = true;
                RotPV = transform.position + (rayDir * 0.78f); ;
                rotDir = rayDir;
                timer = 0;
                startTime = Time.time;
                StartCoroutine(MoveBlockCorutine());


            }

            OnIngridientMove?.Invoke();
        }

    }

    IEnumerator MoveBlockCorutine()
    {

        startTime = Time.time;

        Vector3 LerpStartPos = transform.position;

        float distCovered;
        float fractionOflerp;
        float lerpLength;

        lerpLength = Vector3.Distance(LerpStartPos, new Vector3(LerpStartPos.x, TargetPos.y, LerpStartPos.z));

        if (transform.position.y < TargetPos.y)
        {
            while (transform.position.y != TargetPos.y)
            {
                distCovered = (Time.time - startTime) * lerpSpeed;

                fractionOflerp = distCovered / lerpLength;
                transform.position = Vector3.Lerp(LerpStartPos, new Vector3(LerpStartPos.x, TargetPos.y, LerpStartPos.z), fractionOflerp);
                yield return null;
            }
        }


        timer = 0;
        RotPV = new Vector3(RotPV.x, transform.position.y, RotPV.z);

        while (timer <= 0.5)
        {
            timer += 1 * Time.deltaTime;

            transform.RotateAround(RotPV, new Vector3(rotDir.z, rotDir.y, -rotDir.x), RotSpeed * Time.deltaTime);
            yield return null;
        }

        if (!onBred)
        {
            transform.position = TargetPos + new Vector3(0, 0.2f, 0);
            transform.eulerAngles = Vector3.zero;

            for (int i = 0; i < ingridinetsOnPile.Count; i++)
            {

                ingridinetsOnPile[i].transform.eulerAngles = Vector3.zero;
                ingridinetsOnPile[i].transform.localPosition = new Vector3(0, 0.2f * (i + 1), 0);
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);


            SerchForTop();
            moving = false;
            OnIngridientStop?.Invoke();
        }
        else
        {

            transform.position = TargetPos + new Vector3(0, 0.2f, 0);
            Debug.Log("win Game");
            OnWinGame?.Invoke();
        }

    }

    public void SerchForTop()
    {

        if (Physics.Raycast(transform.position, Vector3.up, out RaycastHit hit, 0.5f, maskNoBred))
        {
            Debug.Log("Found Under " + hit.transform.gameObject.name);
            hit.transform.gameObject.GetComponent<Ingridient>().SerchForTop();
            return;
        }

        transform.parent = startPrent;
        Debug.Log(name + " set his first child");
        SetFirstChild();
    }


    public void SetFirstChild()
    {
        onPile = false;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, maskNoBred))
        {
            Ingridient nextIngridient = hit.transform.gameObject.GetComponent<Ingridient>();
            Debug.Log("set first child to " + nextIngridient.name);

            ingridinetsOnPile.Clear();
            ingridinetsOnPile.Add(nextIngridient);
            parentIngridient = null;
            parentIngridientTransform = null;

            nextIngridient.transform.parent = transform;
            nextIngridient.parentIngridient = this;
            nextIngridient.parentIngridientTransform = transform;
            nextIngridient.SetNextChild();
        }

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit2, 0.5f, maskBred))
        {
            setOnBred(hit2.transform);
        }
    }

    public void SetNextChild()
    {
        onPile = true;
        ingridinetsOnPile.Clear();

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.5f, maskNoBred))
        {
            Ingridient nextIngridient = hit.transform.gameObject.GetComponent<Ingridient>();
            Debug.Log("set next child to " + nextIngridient.name);

            parentIngridient.ingridinetsOnPile.Add(nextIngridient);

            nextIngridient.transform.parent = parentIngridientTransform;
            nextIngridient.parentIngridient = parentIngridient;
            nextIngridient.parentIngridientTransform = parentIngridientTransform;
            nextIngridient.SetNextChild();
            return;
        }

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit2, 0.5f, maskBred))
        {
                parentIngridient.setOnBred(hit2.transform);
        }
    }


    public void setOnBred(Transform breadTransform)
    {
        onBred = true;
        bred = breadTransform.GetComponent<Bred>();
        bred.transform.parent = transform;


        if (ingridinetsOnPile.Count > 0)
        {
            foreach (var ingridient in ingridinetsOnPile)
            {
                ingridient.onBred = true;
                ingridient.bred = breadTransform.GetComponent<Bred>();
            }
        }
    }
}
