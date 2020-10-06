using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Exp;

public class ExperimentScript : MonoBehaviour
{

    private int blockSize = 42;
    private string[] playOrder;
    private int orderIndex = 0;

    public GameObject player;

    /*
     * Booléen décrivant si une opération est terminée. On l'utilise lors du chargement/déchargement des
     * scènes, avec les fonctions IEnumerator : cela permet au programme de ne pas continuer lorsqu'on
     * utilise des coroutines avec StartCoroutine. Ce booléen de contrôle est initialisé à false en début
     * de fonction (coroutine non-terminée), et est passé à true en fin de fonction (coroutine terminée, le
     * programme peut continuer)
     */
    private bool sceneOperationDone, trainingDone, transitionDone;

    private IEnumerator Start()
    {
        // création des champs qui contiendront les données à enregistrer
        MExp.Inst.Data.AddColumn("Distance");
        MExp.Inst.Data.AddColumn("Question");
        MExp.Inst.Data.AddColumn("Chrono");
        MExp.Inst.Data.AddColumn("Answer");
        Debug.Log("COLUMNS ADDED");

        string order = MExp.Inst.Parameters.Values["Block_order"];
        playOrder = order.Split(',');


        //////////////////////////////////////////////////////////////
        /// AFFICHAGE DE LA SCENE DE SELECTION DE TAILLE AVANT
        //////////////////////////////////////////////////////////////
        StartCoroutine(LoadScene("Question2AltScene", true));
        yield return new WaitUntil(() => sceneOperationDone);

        yield return new WaitUntil(() => ButtonManager.Inst.clicked);

        SwitchControls(false, false);

        // déchargement de la scène de question 
        StartCoroutine(UnloadScene("Question2AltScene"));
        yield return new WaitUntil(() => sceneOperationDone);
        //////////////////////////////////////////////////////////////


        // affichage des scène d'instruction
        StartCoroutine(LaunchIntroScenes());
        yield return new WaitUntil(() => sceneOperationDone);

        // démarrage de l'expérimentation
        MExp.Inst.Protocol.StartExperimentation();
        StartCoroutine(LaunchExperimentation());


        //////////////////////////////////////////////////////////////
        /// AFFICHAGE DE LA SCENE DE SELECTION DE TAILLE APRES
        //////////////////////////////////////////////////////////////
        StartCoroutine(LoadScene("Question2AltScene", true));
        yield return new WaitUntil(() => sceneOperationDone);

        yield return new WaitUntil(() => ButtonManager.Inst.clicked);

        SwitchControls(false, false);

        // déchargement de la scène de question 
        StartCoroutine(UnloadScene("Question2AltScene"));
        yield return new WaitUntil(() => sceneOperationDone);
        //////////////////////////////////////////////////////////////
    }

    private IEnumerator LaunchExperimentation() // ! FONCTION RECURSIVE, attention lors des modifications !
    {
        
        Debug.Log("INDEX EXP::: " + MExp.Inst.Protocol.CurrentTrial.Index);

        // si on est en début de bloc ...
        if (MExp.Inst.Protocol.CurrentTrial.Index % blockSize == 0)
        {
            // ... mais que ce n'est pas le premier bloc, on affiche la scène de pause avant l'expérimentation
            if (MExp.Inst.Protocol.CurrentTrial.Index != 0)
            {
                StartCoroutine(LoadScene("PauseScene"));
                yield return new WaitUntil(() => sceneOperationDone);
                // on attend un temps "minimal" avant que l'utilisateur puisse interagir (voir Parameters.txt)
                yield return new WaitForSecondsRealtime(int.Parse(MExp.Inst.Parameters.Values["Minimal_pause_duration"]));
                yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));
                StartCoroutine(UnloadScene("PauseScene"));
            }

            if (int.Parse(playOrder[orderIndex]) == 2)
            {
                StartCoroutine(PlayTrainingScenesHeight());
                yield return new WaitUntil(() => trainingDone);
            }

            else
            {
                StartCoroutine(PlayTrainingScenesQuestion(int.Parse(playOrder[orderIndex])));
                yield return new WaitUntil(() => trainingDone);
            }

            StartCoroutine(PlayTransitionScene("ExperimentPhaseScene", int.Parse(MExp.Inst.Parameters.Values["Interphase_duration"])));
            yield return new WaitUntil(() => transitionDone);

            orderIndex++;
        }


        /**
         * PHASE DE REDIRECTION DU REGARD
         * Si l'utilisateur ne regarde pas dans la bonne direction (à priori, là où se trouvent les points d'intérêt), on affiche un message suivant son regard
         * jusqu'à ce qu'il regarde dans la bonne direction. Une flèche s'affiche au sol pour diriger le regard de l'utilisateur.
         * Une fois que celui-ci regarde dans la bonne direction, on décharge cette scène et on lance l'expérimentation
         */
        if (!PlayerManager.Inst.PlayerLooksInRightDirection(int.Parse(MExp.Inst.Parameters.Values["Direction_threshold"])))
        {
            StartCoroutine(LoadScene("PositionWarningScene", true));
            yield return new WaitUntil(() => sceneOperationDone);

            yield return new WaitUntil(() => PlayerManager.Inst.PlayerLooksInRightDirection(int.Parse(MExp.Inst.Parameters.Values["Direction_threshold"])));

            StartCoroutine(UnloadScene("PositionWarningScene"));
            yield return new WaitUntil(() => sceneOperationDone);
        }

        /*
         * AFFICHAGE DE LA PHOTO PANORAMIQUE.
         * L'utilisateur s'immerge dans la scène, et peut passer à la suite en appuyant sur un bouton.
         * On mesure le temps passé à regarder la photo, et on enregistre cette donnée
         */

        // chargement de la photo à 360°
        StartCoroutine(LoadScene(MExp.Inst.Protocol.CurrentTrial.Variables["Photo_ID"], true));
        yield return new WaitUntil(() => sceneOperationDone);

        // lancement de l'essai: début du chrono
        MExp.Inst.Protocol.CurrentTrial.Start();
        float t1 = Time.realtimeSinceStartup;

        // on attend que le joueur appuie sur un bouton pour afficher la question AVEC les réponses
        yield return new WaitUntil(() => VideoManager.Inst.videoDone);

        // récupération du chrono au moment où l'utilisateur appuie sur le bouton, enregistrement de son temps passé
        //float chrono = MExp.Inst.Protocol.CurrentTrial.Chrono;
        float chrono = Time.realtimeSinceStartup - t1;
        Debug.Log("Subject chrono = " + chrono);

        // déchargement de la photo à 360°
        StartCoroutine(UnloadScene(MExp.Inst.Protocol.CurrentTrial.Variables["Photo_ID"]));
        yield return new WaitUntil(() => sceneOperationDone);

        if (int.Parse(MExp.Inst.Protocol.CurrentTrial.Variables["Question_number"]) == 2)
        {
            StartCoroutine(LoadScene("Question2AltScene"));
            yield return new WaitUntil(() => sceneOperationDone);
            SwitchControls(false, true);
        }

        else
        {
            StartCoroutine(LoadQuestionScene(int.Parse(MExp.Inst.Protocol.CurrentTrial.Variables["Question_number"]), true));
            yield return new WaitUntil(() => sceneOperationDone);
            SwitchControls(true, true);
        }

        // on attend que l'utilisateur ait sélectionné une réponse
        yield return new WaitUntil(() => ButtonManager.Inst.clicked);
        string answer = ButtonManager.Inst.UserChoice;
        Debug.Log("USER CHOICE::: " + answer);

        SwitchControls(false, false);

        if (int.Parse(MExp.Inst.Protocol.CurrentTrial.Variables["Question_number"]) == 2)
        {
            StartCoroutine(UnloadScene("Question2AltScene"));
            yield return new WaitUntil(() => sceneOperationDone);
            SwitchControls(false, false);
        }

        else
        {
            StartCoroutine(UnloadScene("Question" + MExp.Inst.Protocol.CurrentTrial.Variables["Question_number"] + "Scene"));
            yield return new WaitUntil(() => sceneOperationDone);
            SwitchControls(false, false);
        }

        // déchargement de la scène de question 

        Debug.Log("DATA TO WRITE::: " + chrono + answer);

        // on enregistre les données dans le fichier résultat
        MExp.Inst.Data.Write("Distance", MExp.Inst.Protocol.CurrentTrial.Variables["Photo_ID"]);
        MExp.Inst.Data.Write("Question", MExp.Inst.Protocol.CurrentTrial.Variables["Question_number"]);
        MExp.Inst.Data.Write("Chrono", chrono.ToString());
        MExp.Inst.Data.Write("Answer", answer);

        // fin de l'essai, le chrono s'arrête
        MExp.Inst.Protocol.CurrentTrial.End(true);

        // s'il y a encore des essais restants, on relance récursivement la fonction
        if (MExp.Inst.Protocol.NextTrial())
        {
            // affichage d'un compteur faisant passer d'une scène à l'autre
            StartCoroutine(LoadScene("CountdownScene"));
            yield return new WaitUntil(() => sceneOperationDone);

            
            yield return new WaitUntil(() => ProcessCountdown.Inst.waitOver);

            StartCoroutine(UnloadScene("CountdownScene"));
            yield return new WaitUntil(() => sceneOperationDone);
            
            StartCoroutine(LaunchExperimentation());
        }
        else
        {
            StartCoroutine(LoadScene("EndScene"));
        }
    }


    /**
     * Fonction de chargement d'une scène affichant les questions. Elle prend en paramètres :
     * -> questionNumber : le numéro de la question à charger (1, 2 ou 3)
     * -> activateAnswers : ce paramètre définit si les réponses à la question doivent s'afficher
     */
    private IEnumerator LoadQuestionScene(int questionNumber, bool activateAnswers = false)
    {
        sceneOperationDone = false;
        AsyncOperation sceneLoad = SceneManager.LoadSceneAsync("Question" + questionNumber + "Scene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => sceneLoad.isDone);

        GameObject.Find("Canvas").transform.GetChild(1).gameObject.SetActive(activateAnswers);
        sceneOperationDone = true;
    }


    /**
     * Fonction de chargement d'une scène, définie par un nom passé en paramètre.
     * La fonction passe également le booléen active, qui permet ou non de définir si la scène est passée active
     */
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
     * Fonction de déchargement d'une scène dont le nom est passé en paramètre
     */
    public IEnumerator UnloadScene(string sceneName)
    {
        sceneOperationDone = false;
        AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(sceneName);
        yield return new WaitUntil(() => sceneUnload.isDone);
        Debug.Log("Unloaded scene with index" + sceneName);
        sceneOperationDone = true;
    }


    /**
     * Fonction effectuant le déroulement des scènes d'introduction. Ces scène contiennent les consignes
     * que l'utilisateur devra suivre afin de réaliser l'expérimentation. Ces scènes ne font pas partie
     * de la partie expérimentale du programme
     */
    private IEnumerator LaunchIntroScenes()
    {
        sceneOperationDone = false;

        // chargement de la scène d'INTRODUCTION
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("IntroScene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);
        Debug.Log("Loaded Intro scene");
        SceneManager.SetActiveScene(SceneManager.GetActiveScene());

        // on attend que le joueur appuie sur un bouton
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));

        // déchargement de la scène d'INTRODUCTION
        SceneManager.UnloadSceneAsync("IntroScene");
        yield return new WaitUntil(() => asyncLoad.isDone);

        // chargement de la scène d'INSTRUCTION 1
        asyncLoad = SceneManager.LoadSceneAsync("Instruction1Scene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);
        Debug.Log("Loaded Instruction scene");
        SceneManager.SetActiveScene(SceneManager.GetActiveScene());

        // on attend que le joueur appuie sur un bouton
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));

        // déchargement de la scène d'INSTRUCTION 1
        SceneManager.UnloadSceneAsync("Instruction1Scene");
        yield return new WaitUntil(() => asyncLoad.isDone);

        // chargement de la scène d'INSTRUCTION2
        asyncLoad = SceneManager.LoadSceneAsync("Instruction2Scene", LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);
        Debug.Log("Loaded Instruction scene");
        SceneManager.SetActiveScene(SceneManager.GetActiveScene());

        // on attend que le joueur appuie sur un bouton
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));

        // déchargement de la scène d'INSTRUCTION 2
        SceneManager.UnloadSceneAsync("Instruction2Scene");
        yield return new WaitUntil(() => asyncLoad.isDone);

        sceneOperationDone = true;
    }


    private IEnumerator PlayTrainingScenesQuestion(int questionNumber)
    {
        trainingDone = false;
        string[] sceneNames = { "1m50TrainingScene", "3m50TrainingScene", "5m50TrainingScene" };

        SwitchControls(false, false);

        StartCoroutine(PlayTransitionScene("TrainingPhaseScene", int.Parse(MExp.Inst.Parameters.Values["Interphase_duration"])));
        yield return new WaitUntil(() => transitionDone);

        // chargement de la question
        StartCoroutine(LoadQuestionScene(questionNumber));
        yield return new WaitUntil(() => sceneOperationDone);

        // chargement des instructions
        StartCoroutine(LoadScene("InstructionOnQuestionScene"));
        yield return new WaitUntil(() => sceneOperationDone);

        // on attend que le joueur appuie sur un bouton pour afficher la photo à 360° et démarrer l'essai
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));

        // déchargement des instructions
        StartCoroutine(UnloadScene("InstructionOnQuestionScene"));
        yield return new WaitUntil(() => sceneOperationDone);

        // déchargement de la question
        StartCoroutine(UnloadScene("Question" + questionNumber + "Scene"));
        yield return new WaitUntil(() => sceneOperationDone);

        foreach (string scene in sceneNames)
        {
            
            if (!PlayerManager.Inst.PlayerLooksInRightDirection(int.Parse(MExp.Inst.Parameters.Values["Direction_threshold"])))
            {

                StartCoroutine(LoadScene("PositionWarningScene", true));
                yield return new WaitUntil(() => sceneOperationDone);

                yield return new WaitUntil(() => PlayerManager.Inst.PlayerLooksInRightDirection(int.Parse(MExp.Inst.Parameters.Values["Direction_threshold"])));

                StartCoroutine(UnloadScene("PositionWarningScene"));
                yield return new WaitUntil(() => sceneOperationDone);

            }
            StartCoroutine(LoadScene(scene, true));
            yield return new WaitUntil(() => sceneOperationDone);

            // on attend que le joueur appuie sur un bouton pour afficher la question AVEC les réponses
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));

            // déchargement de la photo à 360°
            StartCoroutine(UnloadScene(scene));
            yield return new WaitUntil(() => sceneOperationDone);

            SwitchControls(true, true);

            // affichage de la scène de question, AVEC les réponses
            StartCoroutine(LoadQuestionScene(questionNumber, true));
            yield return new WaitUntil(() => sceneOperationDone);

            yield return new WaitUntil(() => ButtonManager.Inst.clicked);
            string answer = ButtonManager.Inst.UserChoice;
            Debug.Log("USER CHOICE::: " + answer);

            SwitchControls(false, false);

            // déchargement de la scène de question 
            StartCoroutine(UnloadScene("Question" + questionNumber + "Scene"));
            yield return new WaitUntil(() => sceneOperationDone);

            // affichage d'un compteur faisant passer d'une scène à l'autre
            StartCoroutine(LoadScene("CountdownScene"));
            yield return new WaitUntil(() => sceneOperationDone);

            yield return new WaitUntil(() => ProcessCountdown.Inst.waitOver);

            StartCoroutine(UnloadScene("CountdownScene"));
            yield return new WaitUntil(() => sceneOperationDone);


        }
        trainingDone = true;
    }


    private IEnumerator PlayTrainingScenesHeight ()
    {
        trainingDone = false;
        string[] sceneNames = { "1m50TrainingScene", "3m50TrainingScene", "5m50TrainingScene" };

        SwitchControls(false, false);

        StartCoroutine(PlayTransitionScene("TrainingPhaseScene", int.Parse(MExp.Inst.Parameters.Values["Interphase_duration"])));
        yield return new WaitUntil(() => transitionDone);

        // chargement des instructions
        StartCoroutine(LoadScene("InstructionOnHeightScene"));
        yield return new WaitUntil(() => sceneOperationDone);

        // on attend que le joueur appuie sur un bouton pour afficher la photo à 360° et démarrer l'essai
        yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));

        // déchargement des instructions
        StartCoroutine(UnloadScene("InstructionOnHeightScene"));
        yield return new WaitUntil(() => sceneOperationDone);

        foreach (string scene in sceneNames)
        {

            if (!PlayerManager.Inst.PlayerLooksInRightDirection(int.Parse(MExp.Inst.Parameters.Values["Direction_threshold"])))
            {

                StartCoroutine(LoadScene("PositionWarningScene", true));
                yield return new WaitUntil(() => sceneOperationDone);

                yield return new WaitUntil(() => PlayerManager.Inst.PlayerLooksInRightDirection(int.Parse(MExp.Inst.Parameters.Values["Direction_threshold"])));

                StartCoroutine(UnloadScene("PositionWarningScene"));
                yield return new WaitUntil(() => sceneOperationDone);

            }
            StartCoroutine(LoadScene(scene, true));
            yield return new WaitUntil(() => sceneOperationDone);

            // on attend que le joueur appuie sur un bouton pour afficher la question AVEC les réponses
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));

            // déchargement de la photo à 360°
            StartCoroutine(UnloadScene(scene));
            yield return new WaitUntil(() => sceneOperationDone);

            SwitchControls(false, true);

            // affichage de la scène de question, AVEC les réponses
            StartCoroutine(LoadScene("Question2AltScene", true));
            yield return new WaitUntil(() => sceneOperationDone);

            yield return new WaitUntil(() => ButtonManager.Inst.clicked);
            string answer = ButtonManager.Inst.UserChoice;
            Debug.Log("USER CHOICE::: " + answer);

            SwitchControls(false, false);

            // déchargement de la scène de question 
            StartCoroutine(UnloadScene("Question2AltScene"));
            yield return new WaitUntil(() => sceneOperationDone);

            // affichage d'un compteur faisant passer d'une scène à l'autre
            StartCoroutine(LoadScene("CountdownScene"));
            yield return new WaitUntil(() => sceneOperationDone);

            yield return new WaitUntil(() => ProcessCountdown.Inst.waitOver);

            StartCoroutine(UnloadScene("CountdownScene"));
            yield return new WaitUntil(() => sceneOperationDone);

        }

        trainingDone = true;

    }

    private IEnumerator PlayTransitionScene(string sceneName, int duration)
    {
        transitionDone = false;

        StartCoroutine(LoadScene(sceneName));
        yield return new WaitUntil(() => sceneOperationDone);

        yield return new WaitForSecondsRealtime(duration);

        StartCoroutine(UnloadScene(sceneName));
        yield return new WaitUntil(() => sceneOperationDone);

        transitionDone = true;
    }

    private void SwitchControls (bool switcherLaser, bool switcherController)
    {
        GameObject controller = player.transform.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/RightControllerAnchor/OVRControllerPrefab").gameObject;
        GameObject laser = controller.transform.Find("LaserPointer").gameObject;
        controller.SetActive(switcherController);
        laser.SetActive(switcherLaser);
    }
}

