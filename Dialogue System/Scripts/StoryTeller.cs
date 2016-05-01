using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using SimpleJSON;
using System;
using System.Text.RegularExpressions;

namespace Dialogue
{
    public class StoryTeller
    {
        // TODO: Make Story Teller

        string path;
        Tools tools = new Tools();
        JSONNode Node;


        private JSONNode jsonFile ()
        {
            return Tools.ArrayToString(File.ReadAllLines(path));
        }

        public void ReadStory (DialogueSystem sys)
        {
            path = Application.dataPath + "/Resources/" + "story.json";
            //DialogueSystem sys = GetComponent<DialogueSystem>();

            // Checks if file exists
            if (tools.CheckFile(path))
            {
                Node = JSONNode.Parse(jsonFile());

                foreach (JSONNode item in Node["story"].Keys)
                {

                    List<object> parameters = new List<object>();

                    string methodName = Regex.Replace(item.ToString().Replace("\"", ""), "[0-9]", "");

                    var method = sys.GetType().GetMethod(methodName);

                    for (int i = 0; i < Node["story"][item].Count; i++)
                    {
                        string data = Node["story"][item][i];
                        int n;
                        bool isNum = int.TryParse(data, out n);

                        if (isNum)
                        {
                            parameters.Add(n);
                        }
                        else
                        {
                            parameters.Add(data);
                        }
                    }

                    //sys.AddToQueue(() => sys.DisplayDialogue(sys.Dialogue("me", 1, 1)));
                    //sys.AddToQueue(() => sys.Talk((string)parameters[0], (int)parameters[1], (int)parameters[2]));
                    //method.Invoke(sys, parameters.ToArray());
                    sys.AddToQueue(() => method.Invoke(sys, parameters.ToArray()));

                }
            }
        }

    }
}
