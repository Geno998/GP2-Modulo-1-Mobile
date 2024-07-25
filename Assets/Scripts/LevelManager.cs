using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] List<Ingridient> Ingridients;
    [SerializeField] List<GameObject> breds;
    [SerializeField] Transform startingParent;

    private bool canInteract = true;
    public bool CanInteract { get { return canInteract; } }

    private bool allIsOnBred = true;
    public bool AllIsOnBred { get { return allIsOnBred; } }

    [SerializeField] GameObject winPanel;

    [SerializeField] Button undoButton;
    [SerializeField] Button ResetButton;
    [SerializeField] Button PauseButton;

    private void OnEnable()
    {
        Ingridient.OnIngridientMove += onIngMove;
        Ingridient.OnIngridientStop += onIngStop;
        Ingridient.OnWinGame += OnWinGame;
    }



    private void OnDisable()
    {
        Ingridient.OnIngridientMove -= onIngMove;
        Ingridient.OnIngridientStop -= onIngStop;
        Ingridient.OnWinGame += OnWinGame;
    }



    void Start()
    {
        foreach (var Ingridient in Ingridients)
        {
            Ingridient.levelManager = this;
        }
    }


    private void onIngMove()
    {
        canInteract = false;
        undoButton.interactable = false;
        ResetButton.interactable = false;
        PauseButton.interactable = false;
    }

    private void onIngStop()
    {
        canInteract = true;
        undoButton.interactable = true;
        ResetButton.interactable = true;
        PauseButton.interactable = true;

        allIsOnBred = true;

        foreach (var Ingridient in Ingridients)
        {
            if (!Ingridient.onBred)
            {
                allIsOnBred = false;
            }
        }
    }

    private void OnWinGame()
    {
        winPanel.SetActive(true);
        canInteract = false;
    }

    public void pause()
    {
        canInteract = false;
    }

    public void UnPause()
    {
        canInteract = true;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }


    public void Undo()
    {
        StartCoroutine(UndoOrder());
    }

    public void UpdateAllUndoInfo()
    {
        foreach (var ingridient in Ingridients)
        {
            ingridient.lastPos = ingridient.transform.position;

            ingridient.wasOnBred = ingridient.onBred;
            ingridient.wasOnPile = ingridient.onPile;

            ingridient.previousParentIngridientTransform = ingridient.parentIngridientTransform;
            ingridient.previousParentIngridient = ingridient.parentIngridient;
        }
    }

    public void RestLevel()
    {
        foreach (var bred in breds)
        {
            bred.transform.parent = startingParent;
        }

        foreach (var ingridient in Ingridients)
        {

            ingridient.transform.position = ingridient.startPos;
            ingridient.transform.eulerAngles = Vector3.zero;
            ingridient.transform.parent = startingParent;

            ingridient.onBred = false;
            ingridient.wasOnBred = false;
            ingridient.onPile = false;
            ingridient.wasOnPile = false;

            ingridient.previousParentIngridientTransform = null;
            ingridient.parentIngridientTransform = null;
            ingridient.previousParentIngridient = null;
            ingridient.parentIngridient = null;

            ingridient.ingridinetsOnPile.Clear();
        }
    }

    IEnumerator UndoOrder()
    {
        foreach (var bred in breds)
        {
            bred.transform.parent = startingParent;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var ingridient in Ingridients)
        {

            ingridient.transform.parent = null;
            ingridient.transform.position = ingridient.lastPos;

            ingridient.onBred = ingridient.wasOnBred;
            ingridient.onPile = ingridient.wasOnPile;


            ingridient.parentIngridientTransform = ingridient.previousParentIngridientTransform;
            ingridient.parentIngridient = ingridient.previousParentIngridient;

            ingridient.ingridinetsOnPile.Clear();
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var ingridient in Ingridients)
        {
            ingridient.transform.parent = ingridient.previousParentIngridientTransform;
            if (!ingridient.onPile)
            {
                ingridient.SetFirstChild();
            }
        }

    }
}
