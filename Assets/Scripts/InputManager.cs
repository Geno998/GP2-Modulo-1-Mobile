using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{


    [SerializeField] LevelManager levelManager;

    [SerializeField] LayerMask mask;

    Ingridient selectedIngridient;

    Vector2 touchStartPos;
    Vector2 touchEndPos;
    Vector2 endPos;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!levelManager.CanInteract) return;
        getInput();
    }


    void getInput()
    {
        if (Input.touchCount <= 0) return;

        Touch touch = Input.GetTouch(0);



        if (touch.phase == TouchPhase.Began)
        {
            touchStartPos = touch.position;

            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                if (hit.collider != null && hit.transform.gameObject.tag == "Ingridient")
                {
                    selectedIngridient = hit.transform.gameObject.GetComponent<Ingridient>();

                }
            }


        }
        else if (touch.phase == TouchPhase.Ended)
        {
            if (selectedIngridient != null)
            {
                touchEndPos = touch.position;

                float deltaX = touchEndPos.x - touchStartPos.x;
                float deltaY = touchEndPos.y - touchStartPos.y;

                Vector3 rayDir;

                if (Mathf.Abs(deltaX) <= 0.2f && Mathf.Abs(deltaY) <= 0.2f) return;

                if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
                {
                    rayDir = new Vector3(deltaX / Mathf.Abs(deltaX), 0, 0);
                    if (!selectedIngridient.onPile)
                    {
                    selectedIngridient.moveIngridient(rayDir);
                    }
                }
                else if (Mathf.Abs(deltaX) <= Mathf.Abs(deltaY))
                {
                    rayDir = new Vector3(0, 0, deltaY / Mathf.Abs(deltaY));
                    if (!selectedIngridient.onPile)
                    {
                    selectedIngridient.moveIngridient(rayDir);
                    }
                }

                selectedIngridient = null;
            }
        }
    }
}
