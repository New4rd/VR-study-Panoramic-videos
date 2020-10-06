using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Exp;

public class ExperimentScriptNEW : MonoBehaviour
{

    /********************************************
     * VARIABLES D'INSTANCIATION DU SCRIPT
     * *****************************************/
    static private ExperimentScriptNEW _inst;
    static public ExperimentScriptNEW Inst
    {
        get { return _inst; }
    }


    /********************************************
     * VARIABLES PRIVÉES DU SCRIPT
     * *****************************************/
    private bool trainingDone = false;
    private bool questionDone = false;
    private bool transitionDone = false;
    private bool introDone = false; // indique si la scène d'intro est terminée
    private string[] playOrder;     // ordre de passage des questions
    private int orderIndex = 0;     // index courant de la question à afficher
    private string _answer;         // réponse à inscrire dans les fichiers résultats


    /********************************************
     * VARIABLES PUBLIQUES DU SCRIPT
     * *****************************************/
    public GameObject player;
    public int blockSize = 42;     // nombre de questions contenues dans 1 bloc
    public string answer
    {
        get { return _answer; }
        set { _answer = value; }
    }


    /********************************************
     * AWAKE::: INSTANCIATION DU SCRIPT
     * *****************************************/
    private void Awake()
    {
        _inst = this;
    }


    /********************************************
     * START::: LANCEMENT DE L'EXPÉRIMENTATION
     * *****************************************/
    private IEnumerator Start()
    {
        // création des champs qui contiendront les données à enregistrer
        MExp.Inst.Data.AddColumn("Distance");
        MExp.Inst.Data.AddColumn("Question");
        MExp.Inst.Data.AddColumn("Chrono");
        MExp.Inst.Data.AddColumn("Answer");

        // remplissage de l'ordre de passage à partir du fichier Parameters.txt
        playOrder = MExp.Inst.Parameters.Values["Block_order"].Split(',');

        SwitchControls(false, false);

        StartCoroutine(IntroductionPhase());
        yield return new WaitUntil(() => introDone);

        MExp.Inst.Protocol.StartExperimentation();
        StartCoroutine(ExperimentationPhase());
    }



    /*********************************************************************************
     * FONCTIONS POUR LES DIFFERENTES PHASES DE L'EXPERIMENTATION
     * ******************************************************************************/


    /// <summary>
    /// Fonction d'exécution des phases d'introduction de l'expérimentation
    /// </summary>
    /// <returns></returns>
    private IEnumerator IntroductionPhase()
    {
        List<string> sceneNames = new List<string>() { "IntroScene", "Instruction1Scene", "Instruction2Scene" };

        foreach (string name in sceneNames)
        {
            StartCoroutine(ScenesManager.Inst.LoadScene(name, true));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);

            // on attend que le joueur appuie sur un bouton
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));

            // déchargement de la scène d'INTRODUCTION
            StartCoroutine(ScenesManager.Inst.UnloadScene(name));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
        }
        introDone = true;
    }


    private IEnumerator ExperimentationPhase ()
    {
        if (MExp.Inst.Protocol.CurrentTrial.Index % blockSize == 0)
        {
            if (MExp.Inst.Protocol.CurrentTrial.Index != 0)
            {
                StartCoroutine(ScenesManager.Inst.LoadScene("PauseScene"));
                yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
                yield return new WaitForSecondsRealtime(int.Parse(MExp.Inst.Parameters.Values["Minimal_pause_duration"]));
                yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));
                StartCoroutine(ScenesManager.Inst.UnloadScene("PauseScene"));
            }

            StartCoroutine(TransitionPhase("TrainingPhaseScene", int.Parse(MExp.Inst.Parameters.Values["Interphase_duration"])));
            yield return new WaitUntil(() => transitionDone);

            StartCoroutine(TrainingPhase(int.Parse(playOrder[orderIndex])));
            yield return new WaitUntil(() => trainingDone);

            StartCoroutine(TransitionPhase("ExperimentPhaseScene", int.Parse(MExp.Inst.Parameters.Values["Interphase_duration"])));
            yield return new WaitUntil(() => transitionDone);

            if (int.Parse(playOrder[orderIndex]) == 2)
            {
                StartCoroutine(ScenesManager.Inst.LoadScene("InstructionOnHeightScene"));
                yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
                yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));
                StartCoroutine(ScenesManager.Inst.UnloadScene("InstructionOnHeightScene"));
                yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            }
            else
            {
                StartCoroutine(ScenesManager.Inst.LoadScene("InstructionOnQuestionScene"));
                yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
                StartCoroutine(ScenesManager.Inst.LoadQuestionScene(int.Parse(playOrder[orderIndex]), false));
                yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
                yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));
                StartCoroutine(ScenesManager.Inst.UnloadScene("InstructionOnQuestionScene"));
                yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
                StartCoroutine(ScenesManager.Inst.UnloadScene("Question" + int.Parse(playOrder[orderIndex]) + "Scene"));
                yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            }

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
            StartCoroutine(ScenesManager.Inst.LoadScene("PositionWarningScene", true));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            yield return new WaitUntil(() => PlayerManager.Inst.PlayerLooksInRightDirection(int.Parse(MExp.Inst.Parameters.Values["Direction_threshold"])));
            StartCoroutine(ScenesManager.Inst.UnloadScene("PositionWarningScene"));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
        }

        StartCoroutine(ScenesManager.Inst.LoadScene("CountdownScene", true));
        yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
        StartCoroutine(ScenesManager.Inst.LoadScene(MExp.Inst.Protocol.CurrentTrial.Variables["Photo_ID"], true));
        yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
        yield return new WaitForSecondsRealtime(1);
        yield return new WaitUntil(() => ProcessCountdown.Inst.waitOver);
        StartCoroutine(ScenesManager.Inst.UnloadScene("CountdownScene"));
        yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);

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
        StartCoroutine(ScenesManager.Inst.UnloadScene(MExp.Inst.Protocol.CurrentTrial.Variables["Photo_ID"]));
        yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);

        StartCoroutine(QuestionPhase(int.Parse(MExp.Inst.Protocol.CurrentTrial.Variables["Question_number"]), true));
        yield return new WaitUntil(() => questionDone);

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
            StartCoroutine(ExperimentationPhase());
        }
        else
        {
            StartCoroutine(ScenesManager.Inst.LoadScene("EndScene"));
        }
    }


    private IEnumerator TrainingPhase (int questionNumber)
    {
        trainingDone = false;
        List<string> trainingScenes = new List<string>() { "1m50TrainingScene", "3m50TrainingScene", "5m50TrainingScene" };

        if (questionNumber == 2)
        {
            StartCoroutine(ScenesManager.Inst.LoadScene("InstructionOnHeightScene"));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));
            StartCoroutine(ScenesManager.Inst.UnloadScene("InstructionOnHeightScene"));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
        }

        else
        {
            StartCoroutine(ScenesManager.Inst.LoadScene("InstructionOnQuestionScene"));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            StartCoroutine(ScenesManager.Inst.LoadQuestionScene(questionNumber, false));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));
            StartCoroutine(ScenesManager.Inst.UnloadScene("InstructionOnQuestionScene"));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            StartCoroutine(ScenesManager.Inst.UnloadScene("Question" + questionNumber + "Scene"));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
        }
        

        foreach (string name in trainingScenes)
        {
            StartCoroutine(ScenesManager.Inst.LoadScene("CountdownScene", true));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            StartCoroutine(ScenesManager.Inst.LoadScene(name, true));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            yield return new WaitForSecondsRealtime(1f);
            yield return new WaitUntil(() => ProcessCountdown.Inst.waitOver);
            StartCoroutine(ScenesManager.Inst.UnloadScene("CountdownScene"));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            yield return new WaitUntil(() => OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.P));
            StartCoroutine(ScenesManager.Inst.UnloadScene(name));
            yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
            StartCoroutine(QuestionPhase(questionNumber, false));
            yield return new WaitUntil(() => questionDone);
        }
        trainingDone = true;
    }


    /// <summary>
    /// Fonction effectuant la phase de question/réponse : chargement de la question > attente que le joueur ait
    /// choisi une réponse > enregistrement du résultat dans une variable > déchargement de la scène de question
    /// </summary>
    /// <param name="questionNumber">Numéro de la question à charger</param>
    /// <param name="recordResults">Booléen indiquant si la réponse doit être enregistrée ou non</param>
    /// <returns></returns>
    private IEnumerator QuestionPhase(int questionNumber, bool recordResults = false)
    {
        questionDone = false;
        Debug.Log("LOADING QUESTION " + questionNumber);
        StartCoroutine(ScenesManager.Inst.LoadQuestionScene(questionNumber, true));
        yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);

        if (questionNumber != 2) { ExperimentScriptNEW.Inst.SwitchControls(true, true); }
        else { ExperimentScriptNEW.Inst.SwitchControls(false, true); }

        // on attend que l'utilisateur ait sélectionné une réponse
        yield return new WaitUntil(() => ButtonManager.Inst.clicked);

        if (recordResults) { ExperimentScriptNEW.Inst.answer = ButtonManager.Inst.UserChoice; }

        ExperimentScriptNEW.Inst.SwitchControls(false, false);

        // déchargement de la scène de question
        StartCoroutine(ScenesManager.Inst.UnloadScene("Question" + questionNumber + "Scene"));
        yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);

        questionDone = true;
    }


    private IEnumerator TransitionPhase (string phaseName, float duration)
    {
        transitionDone = false;
        StartCoroutine(ScenesManager.Inst.LoadScene(phaseName, true));
        yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
        yield return new WaitForSecondsRealtime(duration);
        StartCoroutine(ScenesManager.Inst.UnloadScene(phaseName));
        yield return new WaitUntil(() => ScenesManager.Inst.sceneOperationDone);
        transitionDone = true;
    }


    /// <summary>
    /// Fonction permettant d'afficher ou non les contrôleurs à l'écran.
    /// </summary>
    /// <param name="switcherLaser">Booléen indiquant si le laser doit être actif</param>
    /// <param name="switcherController">Booléen indiquant si les manettes doivent être affichées</param>
    public void SwitchControls(bool switcherLaser, bool switcherController)
    {
        GameObject controller = player.transform.Find("OVRCameraRig/TrackingSpace/RightHandAnchor/RightControllerAnchor/OVRControllerPrefab").gameObject;
        GameObject laser = controller.transform.Find("LaserPointer").gameObject;
        controller.SetActive(switcherController);
        laser.SetActive(switcherLaser);
    }
}
