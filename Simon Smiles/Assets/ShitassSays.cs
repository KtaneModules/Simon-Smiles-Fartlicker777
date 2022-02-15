using System;
using System.Collections;
using UnityEngine;
using KModkit;
using System.Collections.Generic;

public class ShitassSays : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   //Shit for Buttons
   public Material[] LightsAndNotLights;
   public KMSelectable[] Buttons;
   public GameObject[] Shitasses;
   public GameObject[] ButtonsButObjects;
   public GameObject SolvedShitass;
   public KMSelectable Test;

   Coroutine pressFlash;
   Coroutine strikeFlash;

   int[] FinalSequence = new int[10];
   int[] Sounds = new int[9];
   int Iteration;
   int LastPressed;
   int StageTwoPresses;
   int Temp;
   int Total;
   int Useless;

   float Timer = 10f;

   string[] SoundFiles = { "Low", "Normal", "High" };
   string LogForColors = "RBGY";
   string LogForColorsFinal;
   string Presses;
   string TotalAsString;

   bool[] AnimatingFlag = new bool[4];
   bool[] OnOrOffOfShitasses = new bool[4];
   bool Active;
   bool StageTwoActive;
   bool Playing;
   //bool Played;
   bool StrikeFlash;
   bool Transition;

   private SimonSmilesSettings Settings = new SimonSmilesSettings();

#pragma warning disable 0649
   bool TwitchPlaysActive;
#pragma warning restore 0649

   Vector3 FinalShitass = new Vector3(0, .01f, 0);

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   void Awake () {
      ModConfig<SimonSmilesSettings> modConfig = new ModConfig<SimonSmilesSettings>("SimonSmilesSettings");
      //Read from the settings file, or create one if one doesn't exist
      Settings = modConfig.Settings;
      //Update the settings file in case there was an error during read
      modConfig.Settings = Settings;
      moduleId = moduleIdCounter++;
      foreach (KMSelectable Button in Buttons) {
         Button.OnInteract += delegate () { ButtonPress(Button); return false; };
      }
      Test.OnInteract += delegate () { TestPress(); return false; };
   }

   void Start () {
      if (Settings.shitassMode) {
         SoundFiles = new string[]{ "speedbridge", "speedrun", "shitass" };
      }
      do {
         for (int i = 0; i < 4; i++) {
            if (UnityEngine.Random.Range(0, 2) == 1) {
               OnOrOffOfShitasses[i] = true;
            }
            else {
               OnOrOffOfShitasses[i] = false;
            }
         }
      } while (!OnOrOffOfShitasses[0] && !OnOrOffOfShitasses[1] && !OnOrOffOfShitasses[2] && !OnOrOffOfShitasses[3]);
      for (int i = 0; i < 4; i++) {
         if (OnOrOffOfShitasses[i]) {
            Shitasses[i].gameObject.SetActive(true);
         }
         else {
            Shitasses[i].gameObject.SetActive(false);
         }
      }
      SolvedShitass.gameObject.SetActive(false);
      if (TwitchPlaysActive) {
         Timer = 17.5f;
      }
      SequenceChooser();
   }

   void SequenceChooser () {
      for (int i = 0; i < 9; i++) {
         Sounds[i] = UnityEngine.Random.Range(0, 3);
      }
   }

   void ButtonPress (KMSelectable Button) {
      if (StrikeFlash || Transition) {
         return;
      }
      if (pressFlash != null)
      {
         StopCoroutine(pressFlash);
         for (int i = 0; i < 4; i++)
         {
            ButtonsButObjects[i].GetComponent<MeshRenderer>().material = LightsAndNotLights[i];
         }
      }
      if (strikeFlash != null)
      {
         StopCoroutine(strikeFlash);
         strikeFlash = null;
      }
      for (int i = 0; i < 4; i++) {
         if (Button == Buttons[i]) {
            StartCoroutine(KeyAnimation(i));
            if (moduleSolved) {
               switch (i) {
                  case 0:
                     Audio.PlaySoundAtTransform(SoundFiles[0], transform);
                     break;
                  case 1:
                     Audio.PlaySoundAtTransform(SoundFiles[1], transform);
                     break;
                  case 2:
                     Audio.PlaySoundAtTransform(SoundFiles[2], transform);
                     break;
                  case 3:
                     Audio.PlaySoundAtTransform(Settings.shitassMode ? "badyourbad" : "Wrong", transform);
                     break;
               }
               if (moduleSolved) {
                  return;
               }
            }
            if (StageTwoActive) {
               if (i == FinalSequence[StageTwoPresses]) {
                  StageTwoPresses++;
                  Debug.LogFormat("[Simon Smiles #{0}] You pressed {1}. Next up is {2}.", moduleId, new string[] { "R", "B", "G", "Y"}[i], new string[] { "R", "B", "G", "Y" }[FinalSequence[StageTwoPresses]]);
                  if (StageTwoPresses == TotalAsString.Length) {
                     StartCoroutine(Solved());
                  }
               }
               else {
                  Debug.LogFormat("[Simon Smiles #{0}] You pressed {1}. Next up is {2}.", moduleId, new string[] { "R", "B", "G", "Y" }[i], new string[] { "R", "B", "G", "Y" }[FinalSequence[StageTwoPresses]]);
                  GetComponent<KMBombModule>().HandleStrike();
                  strikeFlash = StartCoroutine(Flash());
                  Audio.PlaySoundAtTransform(Settings.shitassMode ? "badyourbad" : "Wrong", transform);
               }
            }
            else if (Active) {
               if (i == Temp) {
                  Presses += i.ToString();
                  LastPressed = i;
                  Iteration++;
                  if (Iteration == 9) {
                     StageTwoActive = true;
                     Active = false;
                     StartCoroutine(StageChange());
                     FinalAnswerGenerator();
                     return;
                  }
                  Temp = ColorChooser(OnOrOffOfShitasses[LastPressed], LastPressed);
                  Audio.PlaySoundAtTransform(SoundFiles[Sounds[Iteration]], transform);
                  Timer = 10f;
                  if (TwitchPlaysActive) {
                     Timer = 17.5f;
                  }
               }
               else {
                  Strikes();
               }
            }
            else {
               Presses += i.ToString();
               Active = true;
               LastPressed = i;
               Temp = ColorChooser(OnOrOffOfShitasses[LastPressed], LastPressed);
               Audio.PlaySoundAtTransform(SoundFiles[Sounds[Iteration]], transform);
               Timer = 10f;
               if (TwitchPlaysActive) {
                  Timer = 17.5f;
               }
            }
         }
      }
   }

   void TestPress () {
      if (Settings.shitassMode) return;
      Audio.PlaySoundAtTransform("Normal", transform);
      //if (!Played) {
      //Played = true;
      //}
   }

   int ColorChooser (bool Status, int LastPressedForThisIntThing) {
      if (Status) {
         Debug.LogFormat("[Simon Smiles #{0}] The face you last pressed had a face.", moduleId);
         switch (Sounds[Iteration]) { //Low
            case 0:
               Debug.LogFormat("[Simon Smiles #{0}] The clip played was \"{1}.\"", moduleId, SoundFiles[0]);
               switch (LastPressedForThisIntThing) {
                  case 0: //r
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was red.", moduleId);
                     goto Yellow;
                  case 1: //b
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was blue.", moduleId);
                     goto Red;
                  case 2: //g
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was green.", moduleId);
                     goto Blue;
                  case 3: //y
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was yellow.", moduleId);
                     goto Green;
               }
               break;
            case 1:
               Debug.LogFormat("[Simon Smiles #{0}] The clip played was \"{1}.\"", moduleId, SoundFiles[1]);
               switch (LastPressedForThisIntThing) {
                  case 0:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was red.", moduleId);
                     goto Green;
                  case 1:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was blue.", moduleId);
                     goto Blue;
                  case 2:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was green.", moduleId);
                     goto Yellow;
                  case 3:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was yellow.", moduleId);
                     goto Red;
               }
               break;
            case 2:
               Debug.LogFormat("[Simon Smiles #{0}] The clip played was \"{1}.\"", moduleId, SoundFiles[2]);
               switch (LastPressedForThisIntThing) {
                  case 0:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was red.", moduleId);
                     goto Blue;
                  case 1:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was blue.", moduleId);
                     goto Red;
                  case 2:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was green.", moduleId);
                     goto Green;
                  case 3:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was yellow.", moduleId);
                     goto Yellow;
               }
               break;
         }
      }
      else {
         Debug.LogFormat("[Simon Smiles #{0}] The face you last pressed had a face.", moduleId);
         switch (Sounds[Iteration]) { //Low
            case 0:
               Debug.LogFormat("[Simon Smiles #{0}] The clip played was \"{1}.\"", moduleId, SoundFiles[0]);
               switch (LastPressedForThisIntThing) {
                  case 0: //r
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was red.", moduleId);
                     goto Green;
                  case 1: //b
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was blue.", moduleId);
                     goto Yellow;
                  case 2: //g
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was green.", moduleId);
                     goto Blue;
                  case 3: //y
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was yellow.", moduleId);
                     goto Red;
               }
               break;
            case 1:
               Debug.LogFormat("[Simon Smiles #{0}] The clip played was \"{1}.\"", moduleId, SoundFiles[1]);
               switch (LastPressedForThisIntThing) {
                  case 0:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was red.", moduleId);
                     goto Yellow;
                  case 1:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was blue.", moduleId);
                     goto Red;
                  case 2:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was green.", moduleId);
                     goto Blue;
                  case 3:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was yellow.", moduleId);
                     goto Green;
               }
               break;
            case 2:
               Debug.LogFormat("[Simon Smiles #{0}] The clip played was \"{1}.\"", moduleId, SoundFiles[2]);
               switch (LastPressedForThisIntThing) {
                  case 0:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was red.", moduleId);
                     goto Yellow;
                  case 1:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was blue.", moduleId);
                     goto Green;
                  case 2:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was green.", moduleId);
                     goto Red;
                  case 3:
                     Debug.LogFormat("[Simon Smiles #{0}] Last pressed was yellow.", moduleId);
                     goto Blue;
               }
               break;
         }
      }
      Red:
      Debug.LogFormat("[Simon Smiles #{0}] You should press red.", moduleId);
      return 0;
      Blue:
      Debug.LogFormat("[Simon Smiles #{0}] You should press blue.", moduleId);
      return 1;
      Green:
      Debug.LogFormat("[Simon Smiles #{0}] You should press green.", moduleId);
      return 2;
      Yellow:
      Debug.LogFormat("[Simon Smiles #{0}] You should press yellow.", moduleId);
      return 3;
   }

   IEnumerator Solved () {
      moduleSolved = true;
      GetComponent<KMBombModule>().HandlePass();
      SolvedShitass.gameObject.SetActive(true);
      for (int i = 0; i < 100; i++) {
         SolvedShitass.transform.localPosition += FinalShitass;
         yield return new WaitForSeconds(.01f);
      }
      SolvedShitass.gameObject.SetActive(false);
   }

   IEnumerator ColorChanger (int i) {
      ButtonsButObjects[i].GetComponent<MeshRenderer>().material = LightsAndNotLights[i + 4];
      yield return new WaitForSeconds(1f);
      ButtonsButObjects[i].GetComponent<MeshRenderer>().material = LightsAndNotLights[i];
      pressFlash = null;
   }

   IEnumerator StageChange () {
      Transition = true;
      for (int j = 0; j < 10; j++) {
         for (int i = 0; i < 4; i++) {
            ButtonsButObjects[i].GetComponent<MeshRenderer>().material = LightsAndNotLights[i + 4];
         }
         yield return new WaitForSeconds(.1f);
         for (int i = 0; i < 4; i++) {
            ButtonsButObjects[i].GetComponent<MeshRenderer>().material = LightsAndNotLights[i];
         }
         yield return new WaitForSeconds(.1f);
      }
      Transition = false;
   }

   IEnumerator KeyAnimation (int HiKavin) {
      AnimatingFlag[HiKavin] = true;
      pressFlash = StartCoroutine(ColorChanger(HiKavin));
      Buttons[HiKavin].AddInteractionPunch(0.125f);
      Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
      for (int i = 0; i < 5; i++) {
         Buttons[HiKavin].transform.localPosition += new Vector3(0, -0.15f, 0);
         yield return new WaitForSeconds(0.005f);
      }
      for (int i = 0; i < 5; i++) {
         Buttons[HiKavin].transform.localPosition += new Vector3(0, +0.15f, 0);
         yield return new WaitForSeconds(0.005f);
      }
      AnimatingFlag[HiKavin] = false;
   }

   void Strikes () {
      GetComponent<KMBombModule>().HandleStrike();
      Audio.PlaySoundAtTransform(Settings.shitassMode ? "badyourbad" : "Wrong", transform);
      Presses = String.Empty;
      Iteration = 0;
      Timer = 10f;
      if (TwitchPlaysActive) {
         Timer = 17.5f;
      }
      Active = false;
      SequenceChooser();
   }

   IEnumerator Flash () {
      StrikeFlash = true;
      while (true) {
         for (int i = 0; i < 10; i++) {
            StartCoroutine(ColorChanger(int.Parse(Presses[i].ToString())));
            StartCoroutine(KeyAnimation(int.Parse(Presses[i].ToString())));
            yield return new WaitForSeconds(1f);
         }
         StrikeFlash = false;
         yield return new WaitForSeconds(2.5f);
         StrikeFlash = true;
      }
   }

   void Update () {
      if (Active) {
         Timer -= Time.deltaTime;
         if (Timer <= 0f) {
            Strikes();
         }
      }
   }

   void FinalAnswerGenerator () {
      Debug.LogFormat("[Simon Smiles #{0}] Presses so far are {1}.", moduleId, Presses);
      for (int i = 0; i < 10; i++) {
         Total += (int) Math.Pow(4, 9 - i) * int.Parse(Presses[i].ToString());
      }
      Debug.LogFormat("[Simon Smiles #{0}] In base 10 it is {1}.", moduleId, Total);
      TotalAsString = Total.ToString();
      while (TotalAsString.Length != 7) {
         TotalAsString += "0";
      }
      for (int i = 0; i < 7; i++) {
         switch (Bomb.GetBatteryCount()) {
            case 0:
            case 1:
               switch (TotalAsString[i]) {
                  case '0':
                  case '5':
                  case '6':
                  case '9':
                     FinalSequence[i] = 1;
                     break;
                  case '1':
                  case '8':
                     FinalSequence[i] = 3;
                     break;
                  case '2':
                  case '7':
                     FinalSequence[i] = 2;
                     break;
                  case '3':
                  case '4':
                     FinalSequence[i] = 0;
                     break;
               }
               break;
            case 2:
            case 3:
               switch (TotalAsString[i]) {
                  case '0':
                  case '4':
                     FinalSequence[i] = 2;
                     break;
                  case '1':
                  case '5':
                     FinalSequence[i] = 0;
                     break;
                  case '2':
                  case '6':
                  case '9':
                     FinalSequence[i] = 3;
                     break;
                  case '3':
                  case '7':
                  case '8':
                     FinalSequence[i] = 1;
                     break;
               }
               break;
            case 4:
            case 5:
               switch (TotalAsString[i]) {
                  case '0':
                  case '4':
                  case '7':
                     FinalSequence[i] = 3;
                     break;
                  case '1':
                  case '6':
                  case '9':
                     FinalSequence[i] = 2;
                     break;
                  case '2':
                  case '8':
                     FinalSequence[i] = 1;
                     break;
                  case '3':
                  case '5':
                     FinalSequence[i] = 0;
                     break;
               }
               break;
            default:
               switch (TotalAsString[i]) {
                  case '0':
                  case '3':
                  case '8':
                     FinalSequence[i] = 3;
                     break;
                  case '1':
                  case '5':
                  case '9':
                     FinalSequence[i] = 1;
                     break;
                  case '2':
                  case '4':
                  case '7':
                     FinalSequence[i] = 0;
                     break;
                  case '6':
                     FinalSequence[i] = 2;
                     break;
               }
               break;
         }
         LogForColorsFinal += LogForColors[FinalSequence[i]].ToString();
      }
      Debug.LogFormat("[Simon Smiles #{0}] Press {1}.", moduleId, LogForColorsFinal);
   }

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} SL/light to press the status light. Use !{0} R/G/B/Y/red/green/blue/yellow to press that button. Chain by using spaces. On TP the time between presses is extended to 17.5 seconds.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      string[] Parameters = Command.Trim().ToUpper().Split(' ');
      yield return null;
      if (Command.EqualsIgnoreCase("SL") || Command.EqualsIgnoreCase("LIGHT"))
      {
         if (Settings.shitassMode)
         {
            yield return "sendtochaterror The status light cannot be pressed in Shitass Mode!";
            yield break;
         }
         Test.OnInteract();
         yield break;
      }
      for (int i = 0; i < Parameters.Length; i++) {
         if (Parameters[i] != "R" && Parameters[i] != "Y" && Parameters[i] != "B" && Parameters[i] != "G" && Parameters[i] != "RED" &&
         Parameters[i] != "YELLOW" && Parameters[i] != "BLUE" && Parameters[i] != "GREEN") {
            yield return "sendtochaterror I don't understand!";
            yield break;
         }
      }
      while (StrikeFlash || Transition) {
         yield return null;
      }
      for (int i = 0; i < Parameters.Length; i++) {
         switch (Parameters[i]) {
            case "R":
            case "RED":
               Buttons[0].OnInteract();
               yield return new WaitForSeconds(.1f);
               break;
            case "Y":
            case "YELLOW":
               Buttons[3].OnInteract();
               yield return new WaitForSeconds(.1f);
               break;
            case "B":
            case "BLUE":
               Buttons[1].OnInteract();
               yield return new WaitForSeconds(.1f);
               break;
            case "G":
            case "GREEN":
               Buttons[2].OnInteract();
               yield return new WaitForSeconds(.1f);
               break;
         }
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      while (!StageTwoActive) {
         if (!Active) {
            Buttons[UnityEngine.Random.Range(0, 4)].OnInteract();
            yield return new WaitForSeconds(1f);
         }
         Buttons[ColorChooser(OnOrOffOfShitasses[LastPressed], LastPressed)].OnInteract();
         if (!StageTwoActive) yield return new WaitForSeconds(1f);
      }
      while (Transition || StrikeFlash) yield return true;
      for (int i = 0; i < 7; i++) {
         Buttons[FinalSequence[i]].OnInteract();
         yield return new WaitForSeconds(.1f);
      }
   }

    class SimonSmilesSettings
    {
        public bool shitassMode = false;
    }

    static Dictionary<string, object>[] TweaksEditorSettings = new Dictionary<string, object>[]
    {
        new Dictionary<string, object>
        {
            { "Filename", "SimonSmilesSettings.json" },
            { "Name", "Simon Smiles Settings" },
            { "Listing", new List<Dictionary<string, object>>{
                new Dictionary<string, object>
                {
                    { "Key", "shitassMode" },
                    { "Text", "Reverts the module back to when it was Shitass Says" }
                },
            } }
        }
    };
}