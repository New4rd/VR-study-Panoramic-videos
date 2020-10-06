using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class ProtocolGenerator : MonoBehaviour
{
    // tableau contenant les caractéristiques du protocole
    private string[] protocolValues = { "Photo_ID", "Question_number" };
    public bool done;

    private void Awake()
    {
        done = false;
        string path = Application.dataPath;
        string protocPath = path.Substring(0, path.LastIndexOf('/')) + "/DefaultSubject/Protocol.txt";
        Debug.Log(protocPath);

        // récupération des noms des scènes
        List<string> sceneNames = ScenesManager.Inst.scenesName;
       
        if (File.Exists(protocPath))
            File.Delete(protocPath);

        File.Create(protocPath);
        System.IO.StreamWriter file = new System.IO.StreamWriter(protocPath);

        WriteFirstLines(file, 0, -1, 0, protocolValues);

        for (int q_nb = 1; q_nb < 4; q_nb++)
        {
            List<string> shuffledNames = ShuffleList(sceneNames);

            foreach(var scene in shuffledNames)
            {
                file.WriteLine(scene + "\t" + q_nb);
            }
        }

        file.Close();

        Debug.Log("DONE WITH INITIALIZATION");

        done = true;
    }


    private void WriteFirstLines (System.IO.StreamWriter file, int start_index, int block_size, int replay_failed_trial_type, string [] protocolValues)
    {
        file.WriteLine("StartIndex\t" + start_index);
        file.WriteLine("BlockSize\t" + block_size);
        file.WriteLine("ReplayFailedTrialType\t" + replay_failed_trial_type);

        file.WriteLine(protocolValues[0] + "\t" + protocolValues[1]);
    }

    private List<string> ShuffleList (List<string> list)
    {
        System.Random rnd = new System.Random();
        return list.OrderBy(item => rnd.Next()).ToList<string>();
    }
    
}

