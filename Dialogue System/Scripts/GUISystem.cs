using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Dialogue
{
    public class GUISystem : MonoBehaviour
    {
        public Text[] text;
        public List<Button> buttons = new List<Button>();

        private string[] responses;
        private bool[] optionUsed = new bool[4];
        private bool inChoice;
        private int waitType;
        private string buttonToUse;
        private int lastChoiceUsed;
        

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            HideButtons();
        }

        /// <summary>
        /// Displays the text given as a title
        /// </summary>
        /// <param name="title">Text to display</param>
        public IEnumerator DisplayTitle (string title, int time)
        {
            UsingTitle();

            text[2].text = title;

            yield return new WaitForSeconds((float)time);

            GetComponent<DialogueSystem>().SetActionDone(true);
        }

        /// <summary>
        /// Display a dialogue text on the screen in the dialogue slot
        /// </summary>
        /// <param name="textToDisplay">The text to display</param>
        /// <returns> Returns true if action was successful, false if not</returns>
        public bool DisplayDialogue(string textToDisplay)
        {
            UsingText();
            // Note: Make the system so characters write one after the other
            text[0].text = textToDisplay;

            return true;
        }

        /// <summary>
        /// The whole Interact/Button system
        /// </summary>
        /// <param name="choices"></param>
        /// <param name="responses"></param>
        /// <returns></returns>
        public bool DisplayChoices(string[] choices, string[] responses)
        {
            int allUsed = 0; // Number of options that were used, 4 if all
            foreach (bool use in optionUsed)
            {
                if (use)
                    allUsed += 1;
            }
            // If they are not all used
            if (allUsed != 4)
            {
                // If first display of current choice
                if (!inChoice)
                {
                    // Set to using choices
                    UsingChoices();

                    for (int i = 0; i < choices.Length; i++)
                    {
                        // Enable buttons and set their text to choice
                        buttons[i].gameObject.GetComponentInChildren<Text>().text = choices[i];
                    }

                    // Set all responses available
                    this.responses = responses;

                    // Reset all used options
                    optionUsed = new bool[choices.Length];

                    // Set to inChoice, so it doesn't reset next call
                    inChoice = true;
                    waitType = 0;

                }
                // Reloop next frame
                return false;
            }
            // If all the choices are used
            else
            {
                // End
                foreach (Button button in buttons)
                {
                    button.GetComponentInChildren<Text>().text = "";
                }
                return true;
            }

        }

        public bool DisplayChoices(string[] choices, string[] responses, string dataName, string buttonType)
        {
            int allUsed = 0; // Number of options that were used, 4 if all
            foreach (bool use in optionUsed)
            {
                if (use)
                    allUsed += 1;
            }

            // If they are not all used
            if (allUsed != 1)
            {
                // If first display of current choice
                if (!inChoice)
                {
                    // Set to using choices
                    UsingChoices();

                    for (int i = 0; i < choices.Length; i++)
                    {
                        // Enable buttons and set their text to choice
                        buttons[i].gameObject.GetComponentInChildren<Text>().text = choices[i];
                    }

                    // Set all responses available
                    this.responses = responses;

                    // Reset all used options
                    optionUsed = new bool[choices.Length];

                    // Set to inChoice, so it doesn't reset next call
                    inChoice = true;

                    waitType = 2;
                    this.buttonToUse = buttonType;

                }
                // Reloop next frame
                return false;
            }
            // If all the choices are used
            else
            {
                // End
                foreach (Button button in buttons)
                {
                    button.GetComponentInChildren<Text>().text = "";
                }
                UsingText();
                PlayerPrefs.SetInt(dataName, lastChoiceUsed);
                return true;
            }
        }

        public bool DisplayChoices(string[] choices, string[] responses, string buttonType)
        {
            int allUsed = 0; // Number of options that were used, 4 if all
            foreach (bool use in optionUsed)
            {
                if (use)
                    allUsed += 1;
            }

            // If they are not all used
            if (allUsed != 4)
            {
                // If first display of current choice
                if (!inChoice)
                {
                    // Set to using choices
                    UsingChoices();

                    for (int i = 0; i < choices.Length; i++)
                    {
                        // Enable buttons and set their text to choice
                        buttons[i].gameObject.GetComponentInChildren<Text>().text = choices[i];
                    }

                    // Set all responses available
                    this.responses = responses;

                    // Reset all used options
                    optionUsed = new bool[choices.Length];

                    // Set to inChoice, so it doesn't reset next call
                    inChoice = true;

                    waitType = 2;
                    this.buttonToUse = buttonType;
                }
                // Reloop next frame
                return false;
            }
            // If all the choices are used
            else
            {
                // End
                foreach (Button button in buttons)
                {
                    button.GetComponentInChildren<Text>().text = "";
                }
                return true;
            }
        }

        private void HideButtons()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (optionUsed[i] && buttons[i].interactable || buttons[i].GetComponentInChildren<Text>().text == " " && buttons[i].interactable)
                {
                    buttons[i].interactable = false;
                }
            }
        }

        // When a button is pressed, call PressButtonCo, used for choice
        public void PressButton(int choiceID)
        {
            lastChoiceUsed = choiceID;
            StartCoroutine_Auto(PressButtonCo(choiceID));
        }

        // If a choice button is pressed
        private IEnumerator PressButtonCo(int choiceID)
        {
            // Disable the buttons
            UsingText();

            // Set the text to the correct response
            text[0].text = responses[choiceID];

            if (waitType == 2)
            {
                while (true)
                {
                    while (!Input.GetButtonDown(buttonToUse))
                        yield return null;

                    break;
                }
            }
            else
                yield return new WaitForSeconds(3);


            UsingChoices();

            optionUsed[choiceID] = true;
        }

        private void UsingText()
        {
            text[0].enabled = true; // Original Text
            text[1].enabled = true; // Press to continue
            text[2].enabled = false; // Title

            foreach (Button btn in buttons)
            {
                btn.gameObject.SetActive(false);
            }
        }

        private void UsingTitle()
        {
            text[0].enabled = false; // Original Text
            text[1].enabled = false; // Press to continue
            text[2].enabled = true; // Title

            foreach (Button btn in buttons)
            {
                btn.gameObject.SetActive(false);
            }
        }

        private void UsingChoices()
        {
            text[0].enabled = false; // Original Text
            text[1].enabled = false; // Press to continue
            text[2].enabled = false; // Title

            foreach (Button btn in buttons)
            {
                btn.gameObject.SetActive(true);
                btn.interactable = true;
            }
        }

        private void UsingNone()
        {
            text[0].enabled = false; // Original Text
            text[1].enabled = false; // Press to continue
            text[2].enabled = false; // Title

            foreach (Button btn in buttons)
            {
                btn.gameObject.SetActive(false);
                btn.interactable = false;
            }
        }

        public void UsingSpace (string keyName)
        {
            text[1].text = "Press " + keyName + " to continue.";
            text[1].enabled = true; // Press to continue
        }

    }
}
