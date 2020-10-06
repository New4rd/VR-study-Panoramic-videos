using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{

    static private ScenesManager inst;

    static public ScenesManager Inst
    {
        get { return inst; }
    }

    public List<string> scenesName;
    private bool _sceneOperationDone;

    public bool sceneOperationDone
    {
        get { return _sceneOperationDone; }
        set { _sceneOperationDone = value; }
    }

    private void Awake()
    {
        inst = this;
        LoadSceneNames();
    }


    private void LoadSceneNames ()
    {
        Object[] data = Resources.LoadAll("Scenes");

        for (int i = 0; i < 11; i++)
        {
            scenesName.Add(data[i].name);
        }
    }


    public IEnumerator LoadScene(string sceneName, bool active = false)
    {
        sceneOperationDone = false;
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => sceneLoad.isDone);
        if (active)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        Debug.Log("Active scene" + sceneName);
        sceneOperationDone = true;
    }


    /**
     * Fonction de chargement d'une scène affichant les questions. Elle prend en paramètres :
     * -> questionNumber : le numéro de la question à charger (1, 2 ou 3)
     * -> activateAnswers : ce paramètre définit si les réponses à la question doivent s'afficher
     */
    public IEnumerator LoadQuestionScene(int questionNumber, bool activateAnswers = false)
    {
        sceneOperationDone = false;
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Question" + questionNumber + "Scene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => sceneLoad.isDone);

        if (questionNumber != 2)
        { GameObject.Find("Question Canvas").transform.GetChild(1).gameObject.SetActive(activateAnswers); }
        
        sceneOperationDone = true;
    }


    public IEnumerator UnloadScene(string sceneName)
    {
        sceneOperationDone = false;
        AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(sceneName);
        yield return new WaitUntil(() => sceneUnload.isDone);
        Debug.Log("Unloaded scene with name " + sceneName);
        sceneOperationDone = true;
    }


    
}
