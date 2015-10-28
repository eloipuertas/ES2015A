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

    public enum States { IN_MENU, IN_RACES_SELECT, IN_GAME, KILLING }
    public States state { get; set; }

    private List<Tuple<String, String>> errorLogs = new List<Tuple<String, String>>();
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
                        OnDisable(); // Force unregister
                        state = States.KILLING;
                    }
                    break;

                case States.KILLING:
                    // Run all unit tests
                    var results = from type in Assembly.GetExecutingAssembly().GetTypes()
                                  where typeof(IUnitTest).IsAssignableFrom(type)
                                  select type;

                    // Execute one by one
                    foreach (Type test in results)
                    {
                        if (!test.IsInterface)
                        {
                            IUnitTest testObj = (IUnitTest)Activator.CreateInstance(test);
                            testObj.run(errorLogs);
                        }
                    }

                    StringBuilder output = new StringBuilder();
                    foreach (Tuple<String, String> error in errorLogs)
                    {
                        output.Append(error.Key0);
                        output.Append("\n");
                        output.Append(error.Key1);
                        output.Append("\n----------------------\n\n");
                    }

                    output.Append(errorLogs.Count);
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
            errorLogs.Add(new Tuple<String, String>(logString, stackTrace));
        }
    }
}
