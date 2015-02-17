﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BGE.Scenarios;
using BGE.States;

namespace BGE
{
    public class SteeringManager : MonoBehaviour
    {
        List<Scenario> scenarios = new List<Scenario>();
        
        public Scenario currentScenario;
        StringBuilder message = new StringBuilder();
        
        public GameObject camFighter;

        public GameObject boidPrefab;
        public GameObject leaderPrefab;
        public Space space;
        static SteeringManager instance;
        // Use this for initialization
        GUIStyle style = new GUIStyle();

        float[] timeModifiers = { 0.2f, 0.5f, 1.0f, 2.0f, 0.0f };
        int timeModIndex = 2;
        
        GameObject monoCamera;
        GameObject activeCamera;
        GameObject riftCamera;             
        
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        void Start()
        {
            instance = this;
            Screen.showCursor = false;

            style.fontSize = 18;
            style.normal.textColor = Color.white;

            space = new Space();
            
            scenarios.Add(new SeekScenario());
            scenarios.Add(new ArriveScenario());
            scenarios.Add(new PursueScenario());
            scenarios.Add(new WanderScenario());
            scenarios.Add(new PathFollowingScenario());
            scenarios.Add(new ObstacleAvoidanceScenario());
            scenarios.Add(new FlockingScenario());
            scenarios.Add(new StateMachineScenario());
            scenarios.Add(new PathFindingScenario());
            currentScenario = scenarios[0];
            currentScenario.Start();

            monoCamera = GameObject.FindGameObjectWithTag("MainCamera");
            riftCamera = GameObject.FindGameObjectWithTag("ovrcamera");

            activeCamera = monoCamera;

        }

        public static SteeringManager Instance
        {
            get
            {
                return instance;
            }
        }

        void OnGUI()
        {
            if (Params.showMessages)
            {
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "" + message, style);
            }
            if (Event.current.type == EventType.Repaint)
            {
                message.Length = 0;
            }

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.F1)
                {
                    Params.camMode = (Params.camMode + 1) % 3;
                }

                for (int i = 0; i < scenarios.Count; i++)
                {
                    if (Event.current.keyCode == KeyCode.Alpha0 + i)
                    {
                        currentScenario.TearDown();
                        currentScenario = scenarios[i];
                        currentScenario.Start();                        
                    }
                }

                if (Event.current.keyCode == KeyCode.R)
                {
                    currentScenario.TearDown();
                    currentScenario = scenarios[6];
                    currentScenario.Start();
                    Params.showMessages = false;
                    Params.riftEnabled = true;
                    timeModIndex = 0;
                    Params.cellSpacePartitioning = true;
                    Params.drawDebugLines = true;
                    Params.camMode = (int) Params.camModes.boid;
                }

                if (Event.current.keyCode == KeyCode.F2)
                {
                    timeModIndex = (timeModIndex + 1) % timeModifiers.Length;
                }
                if (Event.current.keyCode == KeyCode.F4)
                {
                    Params.showMessages = !Params.showMessages;
                }

                if (Event.current.keyCode == KeyCode.F5)
                {
                    Params.drawVectors = !Params.drawVectors;
                }
                
                if (Event.current.keyCode == KeyCode.F6)
                {
                    Params.drawDebugLines = !Params.drawDebugLines;
                }

                if (Event.current.keyCode == KeyCode.F7)
                {
                    monoCamera.transform.up = Vector3.up;
                }
                if (Event.current.keyCode == KeyCode.F8)
                {
                    Params.cellSpacePartitioning = !Params.cellSpacePartitioning;
                }
                if (Event.current.keyCode == KeyCode.F9)
                {
                    Params.enforceNonPenetrationConstraint = !Params.enforceNonPenetrationConstraint;
                }
                if (Event.current.keyCode == KeyCode.F10)
                {
                    Params.riftEnabled = !Params.riftEnabled;                    
                }

                if (Event.current.keyCode == KeyCode.F11)
                {
                    Params.drawForces = !Params.drawForces;
                }                                
                if (Event.current.keyCode == KeyCode.Escape)
                {
                    Application.Quit();
                }                
            }
        }

        public static void PrintMessage(string message)
        {
            if (instance != null)
            {
                Instance.message.Append(message + "\n");
            }
        }

        public static void PrintFloat(string message, float f)
        {
            if (instance != null)
            {
                Instance.message.Append(message + ": " + f + "\n");
            }
        }

        public static void PrintVector(string message, Vector3 v)
        {
            if (instance != null)
            {
                Instance.message.Append(message + ": (" + v.x + ", " + v.y + ", " + v.z + ")\n");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("X Button"))
            {
                Params.drawDebugLines = !Params.drawDebugLines;
            }

            if (Input.GetButtonDown("A Button"))
            {
                Params.camMode = (Params.camMode + 1) % 3;
            }
            if (Input.GetButtonDown("B Button"))
            {
                Params.cellSpacePartitioning = !Params.cellSpacePartitioning;
            }
            if (Input.GetButtonDown("Y Button"))
            {
                timeModIndex = (timeModIndex + 1) % timeModifiers.Length;                
                
            }
            Params.timeModifier = timeModifiers[timeModIndex];
            if (Params.riftEnabled)
            {
                riftCamera.SetActive(true);
                activeCamera = riftCamera;
            }
            else
            {
                riftCamera.SetActive(false);
                activeCamera = monoCamera;
            }
            PrintMessage("Press F1 to toggle camera mode");
            PrintMessage("Press F2 to adjust speed");
            PrintMessage("Press F4 to toggle messages");
            PrintMessage("Press F5 to toggle vector drawing");
            PrintMessage("Press F6 to toggle debug drawing");
            PrintMessage("Press F7 to level camera");
            PrintMessage("Press F8 to toggle cell space partitioning");
            PrintMessage("Press F9 to toggle non-penetration constraint");
            PrintMessage("Press F10 to toggle Rift");
            PrintMessage("Press F11 to toggle force drawing");
            int fps = (int)(1.0f / Time.deltaTime);
            PrintFloat("FPS: ", fps);
            PrintMessage("Current scenario: " + currentScenario.Description());
            for (int i = 0; i < scenarios.Count; i++)
            {
                PrintMessage("Press " + i + " for " + scenarios[i].Description());
            }
           
            switch (Params.camMode)
            {
                case((int) Params.camModes.following):
                    currentScenario.leader.GetComponentInChildren<Renderer>().enabled = true;
                    monoCamera.transform.position = camFighter.transform.position;
                    if (!Params.riftEnabled)
                    {
                        monoCamera.transform.rotation = camFighter.transform.rotation;
                    }
                   break;
                case ((int)Params.camModes.boid):
                    currentScenario.leader.GetComponentInChildren<Renderer>().enabled = false;
                    monoCamera.transform.position = currentScenario.leader.transform.position;
                    if (!Params.riftEnabled)
                    {
                        monoCamera.transform.rotation = currentScenario.leader.transform.rotation;
                    }
                   break;
                case ((int)Params.camModes.fps):
                   currentScenario.leader.GetComponentInChildren<Renderer>().enabled = true;
                   break;
            }

            if (Params.cellSpacePartitioning)
            {
                PrintMessage("Cell space partitioning on");
            }
            else
            {
                PrintMessage("Cell space partitioning off");
            }

            if (Params.enforceNonPenetrationConstraint)
            {
                PrintMessage("Enforce non penetration constraint on");
            }
            else
            {
                PrintMessage("Enforce non penetration constraint off");
            }

            if (Params.drawDebugLines && Params.cellSpacePartitioning)
            {
                space.Draw();
            }
      
            currentScenario.Update();
        }

        void LateUpdate()
        {
            if (Params.cellSpacePartitioning)
            {
                space.Partition();                    
            }
        }
    }
}
