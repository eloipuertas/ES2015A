using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Reflection;

using Storage;
using Utils;
using Utils.UnitTests;

class TestEnvironment : SingletonMono<TestEnvironment>
{
    public bool testingEnabled { get; set; }
    public string testFile { get; set; }
    public float testTime { get; set; }
    public Races playerRace { get; set; }

    public enum States { IN_MENU, IN_RACES_SELECT, IN_GAME, UNIT_TESTS, KILLING }
    public States state { get; set; }

    private List<Tuple<String, String>> errorLogger = new List<Tuple<String, String>>();
    private float _elapsedTime;

    public void Init()
    {
        testingEnabled = false;
        testTime = 15;
        _elapsedTime = 0;
        playerRace = Races.MEN;

        String[] arguments = Environment.GetCommandLineArgs();

        foreach (String arg in (new List<String>(arguments)).Skip(1))
        {
            if (arg.StartsWith("--test="))
            {
                testingEnabled = true;
                testFile = arg.Substring(7);
            }
            else if (arg.StartsWith("--player-race="))
            {
                playerRace = (Races)Enum.Parse(typeof(Races), arg.Substring(14), true);
            }
            else if (arg.StartsWith("--test-time="))
            {
                testTime = float.Parse(arg.Substring(12), CultureInfo.InvariantCulture.NumberFormat);
                testTime /= 1000.0f;
            }
        }

        if (!testingEnabled)
        {
            Destroy(this.gameObject);
        }
        else
        {
            state = States.IN_MENU;
        }
    }

    void Update()
    {
        if (testingEnabled)
        {
            switch (state)
            {
                case States.IN_MENU:
                    Application.LoadLevel("seleccion personaje");
                    state = States.IN_RACES_SELECT;
                    break;

                case States.IN_RACES_SELECT:
                    GameObject.Find("GameInformationObject").
                        GetComponent<GameInformation>().
                        SetPlayerRace(playerRace);

                    Application.LoadLevel("ES2015A");
                    state = States.IN_GAME;

                    break;

                case States.IN_GAME:
                    _elapsedTime += Time.deltaTime;

                    if (_elapsedTime >= testTime)
                    {
                        state = States.UNIT_TESTS;
                    }
                    break;

                case States.UNIT_TESTS:
                    // Run all unit tests
                    var results = from type in Assembly.GetExecutingAssembly().GetTypes()
                                  where typeof(IUnitTest).IsAssignableFrom(type)
                                  select type;

                    // Execute one by one
                    foreach (Type test in results)
                    {
                        if (!test.IsInterface && !test.IsAbstract)
                        {
                            IUnitTest testObj = (IUnitTest)Activator.CreateInstance(test);
                            testObj.errorLogger = errorLogger;
                            testObj.run();
                        }
                    }

                    // Kill game
                    state = States.KILLING;
                    OnDisable(); // Force unregister
                    break;

                case States.KILLING:
                    StringBuilder output = new StringBuilder();
                    foreach (Tuple<String, String> error in errorLogger)
                    {
                        output.Append(error.Key0);
                        output.Append("\n");
                        output.Append(error.Key1);
                        output.Append("\n----------------------\n\n");
                    }

                    output.Append(errorLogger.Count);
                    System.IO.File.WriteAllText(testFile, output.ToString());

                    Application.Quit();
                    break;
            }
        }
    }

    void OnEnable()
    {
		Application.logMessageReceived += HandleLog;
	}

	void OnDisable()
    {
		Application.logMessageReceived -= HandleLog;
	}

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            errorLogger.Add(new Tuple<String, String>(logString, stackTrace));
        }
    }
}
