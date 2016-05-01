using UnityEngine;
using System.Collections;
using System.IO;
using SimpleJSON;
using System.Collections.Generic;
using System;

namespace Dialogue
{
    public class DialogueSystem : MonoBehaviour
    {

        // VARIABLES

        public string path;
        public string version = "1.0";
        JSONNode Node;
        private List<Action> actionQueue = new List<Action>();
        public bool actionDone = true;
        private StoryTeller st = new StoryTeller();
        Tools tools = new Tools();


        // BASIC CLASSES

        void Start()
        {
            path = Application.dataPath + "/Resources/dialogue.json";

            Node = JSON.Parse(Tools.ArrayToString(File.ReadAllLines(path)));

            if (tools.CheckFile(path, version))
                Debug.Log("File is correct!");
            else
            {
                Debug.LogError("File not found or version is invalid!");
                return;
            }
            //Test(6);

            st.ReadStory(this);
        }

        void Update()
        {
            if (actionQueue.Count > 0)
                ExecuteQueue();
        }


        //TESTING

        // 0: All, 1: Dialogue, 2: Part, 3: Interact, 4: Queue System, 5: Choice System All, 6: Choice System One
        /// <summary>
        /// Used to test all of my methods to check if they work! 
        /// </summary>
        /// <param name="testingCode">Test Code, a different code can be used to test different parts of the code. Use 0 for all!</param>
        private void Test(int testingCode)
        {
            if (testingCode == 0 || testingCode == 1)
            {
                Debug.Log("Testing Dialogue()");
                Debug.Log(Dialogue("me", 1, 1));
            }

            if (testingCode == 0 || testingCode == 2)
            {
                Debug.Log("Testing Part()");
                Debug.Log(Part(1));
            }

            if (testingCode == 0 || testingCode == 3)
            {
                Debug.Log("Testing Interact()");
                foreach (string option in Choice("player", 0, 1))
                {
                    Debug.Log(option);
                }
            }

            if (testingCode == 0 || testingCode == 4)
            {
                AddToQueue(() => Wait((int)0.5));
                AddToQueue(() => Talk("me", 1, 1));
            }

            if (testingCode == 0 || testingCode == 5)
            {
                Debug.Log("Using Interact() with All");
                AddToQueue(() => StartCoroutine(InteractAll("player", 0, 1, "me", 1, 1)));
                AddToQueue(() => Wait((int)0.5));
                AddToQueue(() => Talk("me", 1, 1));
            }

            if (testingCode == 0 || testingCode == 6)
            {
                Debug.Log("Using Interact() with Only one");
                AddToQueue(() => StartCoroutine(InteractOne("player", 0, 1, "me", 1, "Part2")));
                AddToQueue(() => Wait((int)0.5));
                AddToQueue(() => Talk("me", 1, 1));
            }

            if (testingCode == 0 || testingCode == 7)
            {
                Debug.Log("Displaying Title()");
                AddToQueue(() => Title(1));
            }
        }

        // GETTING DATA FROM FILE

        /// <summary>
        /// Returns a string for a dialogue of a player using the given parameters
        /// </summary>
        /// <param name="speaker">Who is talking, ex: player</param>
        /// <param name="partID">ID of the part used, ex: 1 = part1</param>
        /// <param name="textID">ID of the text to use, ex: 1 = text1</param>
        /// <returns>A string with the correct speech</returns>
        public string Dialogue(string speaker, int partID, int textID)
        {
            string characterName = Node["characters"][speaker];
            string speech = Node["dialogue"]["part" + partID][speaker]["text" + textID];

            return characterName + ": " + speech;
        }

        /// <summary>
        /// Returns a string containing the name of the part using the part id
        /// </summary>
        /// <param name="partID">ID of the part to use, ex: 1 = part1</param>
        /// <returns>Part name, ex: Rambling</returns>
        public string Part(int partID)
        {
            return Node["dialogue"]["part" + partID]["part_name"];
        }
        /// <summary>
        /// Returns the name of the character based on the speaker id
        /// </summary>
        /// <param name="speaker">The speaker (character's speaker id)</param>
        /// <returns></returns>
        public string CharacterName(string speaker)
        {
            return Node["characters"][speaker];
        }

        /// <summary>
        /// Returns all the possible choices for an interaction (player chooses what to say)
        /// </summary>
        /// <param name="speaker">Who is doing the interaction, used to detect in file</param>
        /// <param name="partID">What part is the interaction in</param>
        /// <param name="optionID">What option ID is it, ex: option1</param>
        /// <returns>An array of possible interactions</returns>
        public string[] Choice(string speaker, int partID, int optionID)
        {
            JSONArray options = Node["dialogue"]["part" + partID][speaker]["option" + optionID].AsArray;

            List<string> optionsFinal = new List<string>();

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] != "" && options[i] != " ")
                    optionsFinal.Add(options[i].ToString().Trim('"'));
            }

            return optionsFinal.ToArray();
        }

        /// <summary>
        /// Returns all the possible responses according to the choices for an interaction
        /// </summary>
        /// <param name="speaker">Who is responding to the interaction, used to detect in file</param>
        /// <param name="partID">What part is the interaction in</param>
        /// <param name="optionID">What option ID is it, ex: option1</param>
        /// <returns>An array of possible reponses</returns>
        public string[] Responses(string speaker, int partID, int optionID)
        {
            JSONArray options = Node["dialogue"]["part" + partID][speaker]["response" + optionID].AsArray;

            List<string> optionsFinal = new List<string>();

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i] != "" && options[i] != " ")
                    optionsFinal.Add(CharacterName(speaker) + ": " + options[i].ToString().Trim('"'));
            }

            return optionsFinal.ToArray();
        }


        // BASIC DIALOGUE

        /// <summary>
        /// Display a dialogue to the GUI System
        /// </summary>
        /// <param name="text">Text to display</param>
        private void DisplayDialogue(string text)
        {
            if (GetComponent<GUISystem>().DisplayDialogue(text))
                actionDone = true;
            else
            {
                DisplayDialogue(text, 1);
            }
        }

        /// <summary>
        /// DO NOT USE! This is only used internally!
        /// </summary>
        private void DisplayDialogue(string text, int numTries)
        {
            if (GetComponent<GUISystem>().DisplayDialogue(text))
                actionDone = true;
            else
            {
                if (numTries > 10)
                {
                    DisplayDialogue(text, numTries + 1);
                }
                else
                {
                    actionDone = true;
                }
            }
        }

        /// <summary>
        /// Use this to make a character say a text.
        /// </summary>
        /// <param name="speaker">Speaker saying the dialogue</param>
        /// <param name="partID">Part of the dialogue</param>
        /// <param name="textID">Id of the text to use</param>
        public void Talk(string speaker, int partID, int textID)
        {
            DisplayDialogue(Dialogue(speaker, partID, textID));
        }

        /// <summary>
        /// Displays the text given as a title
        /// </summary>
        /// <param name="partID">Text to display</param>
        public void Title(int partID)
        {
            GetComponent<GUISystem>().DisplayTitle(Part(partID));
        }


        // DIALOGUE WITH CHOICE

        /// <summary>
        /// Creates an interaction where the player chooses what to say! This one makes it so you go back to the menu after doing one.
        /// </summary>
        /// <param name="speaker">Who is talking, used in the file</param>
        /// <param name="partID">What part is that in, used in file</param>
        /// <param name="optionID">What optionID is it in, used in file</param>
        /// <param name="responder">Who is saying the responses, used in the file</param>
        /// <param name="optionID">What responseID is it in, used in file</param>
        public IEnumerator InteractAll(string speaker, int partID, int optionID, string responder, int responseID) // Allowing all of them
        {
            string[] options = Choice(speaker, partID, optionID);
            string[] responses = Responses(responder, partID, responseID);

            while (true)
            {

                while (GetComponent<GUISystem>().DisplayChoices(options, responses, "Space") == false)
                {
                    yield return null;
                }

                actionDone = true;
                yield break;
            }
        }


        /// <summary>
        /// Creates an interaction where the player chooses what to say! This one makes it so you go back to the menu after doing one.
        /// </summary>
        /// <param name="speaker">Who is talking, used in the file</param>
        /// <param name="partID">What part is that in, used in file</param>
        /// <param name="optionID">What optionID is it in, used in file</param>
        /// <param name="responder">Who is saying the responses, used in the file</param>
        /// <param name="optionID">What responseID is it in, used in file</param>
        public IEnumerator InteractAll(string speaker, int partID, int optionID, string responder, int responseID, int waitType) // Allowing all of them
        {
            string[] options = Choice(speaker, partID, optionID);
            string[] responses = Responses(responder, partID, responseID);



            while (true)
            {
                if (waitType == 0) // Wait 2 seconds
                {
                    while (GetComponent<GUISystem>().DisplayChoices(options, responses) == false)
                    {
                        yield return null;
                    }
                }
                else if (waitType == 1) // Wait until space
                {
                    while (GetComponent<GUISystem>().DisplayChoices(options, responses, "Space") == false)
                    {
                        yield return null;
                    }
                }
                else if (waitType == 2) // Wait until Fire
                {
                    while (GetComponent<GUISystem>().DisplayChoices(options, responses, "Fire") == false)
                    {
                        yield return null;
                    }
                }

                actionDone = true;
                yield break;
            }
        }

        /// <summary>
        /// Creates an interaction where the player chooses what to say! This one makes it so you can't go back to the menu after doing one.
        /// </summary>
        /// <param name="speaker">Who is talking, used in the file</param>
        /// <param name="partID">What part is that in, used in file</param>
        /// <param name="optionID">What optionID is it in, used in file</param>
        /// <param name="dataName">What to call the data saved, this is used to save the player's choice to be used for different outcomes.</param>
        /// <param name="responder">Who is saying the responses, used in the file</param>
        /// <param name="optionID">What responseID is it in, used in file</param>
        /// <param name="dataName">Name of the data to keep/save for later use. (Used for different outcomes)</param>
        public IEnumerator InteractOne(string speaker, int partID, int optionID, string responder, int responseID, string dataName) // Allowing only one
        {
            string[] options = Choice(speaker, partID, optionID);
            string[] responses = Responses(responder, partID, responseID);

            while (true)
            {
                while (GetComponent<GUISystem>().DisplayChoices(options, responses, dataName, "Space") == false)
                {
                    yield return null;
                }

                actionDone = true;
                yield break;
            }
        }

        // WAITING

        /// <summary>
        /// Wait for the given amount of time until proceeding to the next action
        /// </summary>
        /// <param name="time">Time in seconds to wait, ex: 0.5</param>
        public void Wait(int time)
        {
            StartCoroutine_Auto(WaitCo((float)time));
        }

        /// <summary>
        /// DO NOT USE! This is only used internally!
        /// </summary>
        private IEnumerator WaitCo(float time)
        {
            yield return new WaitForSeconds(time);
            actionDone = true;
        }


        /// <summary>
        /// Wait for an input before continuing, Keyboard and KeyCodes only!
        /// </summary>
        /// <param name="input">QAny Keycode, Ex: Q, Esc</param>
        public IEnumerator WaitForInput(KeyCode input)
        {
            while (true)
            {
                while (!Input.GetKeyDown(input))
                    yield return null;

                actionDone = true;
                yield return null;
            }
        }

        /// <summary>
        /// Wait for an input before continuing, Referenced Unity Input only!
        /// </summary>
        /// <param name="input">Any KeyCode referenced in Unity's Input Manager</param>
        public IEnumerator WaitForInput(string input)
        {
            while (true)
            {
                while (!Input.GetButtonDown(input))
                    yield return null;

                actionDone = true;
                yield return null;
            }
        }


        // CHARACTER SPRITES

        // TODO: Make the displaying of character system!
        // TODO: Make the chaging of character sprite system!
        // TODO: Make the animation of character system!
        // TODO: Make the removal of character system

        // QUEUE

        /// <summary>
        /// Adds an action (method to execute) in a queue, which will run one after the other. EX: AddToQueue(() => Wait(1));
        /// </summary>
        /// <param name="action"></param>
        public void AddToQueue(Action action)
        {
            actionQueue.Add(action);
        }

        /// <summary>
        /// Executes the first action of the queue and removes it
        /// </summary>
        public void ExecuteQueue()
        {
            if (actionDone)
            {
                actionDone = false;

                actionQueue[0]();

                actionQueue.RemoveAt(0);
            }
        }

        /// <summary>
        /// Removes an action from the action/event queue based on index
        /// </summary>
        /// <param name="index">The index of the queue item to remove</param>
        public void RemoveInQueue(int index)
        {
            actionQueue.RemoveAt(index);
        }

        /// <summary>
        /// Removes a the first occurence of the given action
        /// </summary>
        /// <param name="action">The action to remove</param>
        public void RemoveInQueue(Action action)
        {
            actionQueue.Remove(action);
        }


    }
}
