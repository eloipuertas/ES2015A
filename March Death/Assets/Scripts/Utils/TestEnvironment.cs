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

    public enum States { IN_MENU, IN_RACES_SELECT, PRE_GAME_CHECKING, IN_GAME, POST_GAME_CHECKING, KILLING }
    public States state { get; set; }

    private List<Tuple<String, String>> errorLogger = new List<Tuple<String, String>>();
    private float _elapsedTime;

    private static bool _setupDone = false;
    private Dictionary<int, List<UnitTest>> unitTests = new Dictionary<int, List<UnitTest>>();

    public TestEnvironment()
    {
        // Read unit tests
        var results = from type in Assembly.GetExecutingAssembly().GetTypes()
                      where typeof(IUnitTest).IsAssignableFrom(type)
                      select type;

        foreach (var state in Enum.GetValues(typeof(States)))
        {
            unitTests[(int)state] = new List<UnitTest>();
        }

        // Load one by one
        foreach (Type test in results)
        {
            if (!test.IsInterface && !test.IsAbstract)
            {
                UnitTest testObj = (UnitTest)Activator.CreateInstance(test);
                testObj.testEnvironment = this;
                testObj.errorLogger = errorLogger;
                unitTests[(int)testObj.RunAt].Add(testObj);
            }
        }
    }

    public void Init()
    {
        if (!_setupDone)
        {
            _setupDone = true;
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

            // Destroy if not in use
            if (!testingEnabled)
            {
                Destroy(this.gameObject);
            }
            else
            {
                state = States.IN_MENU;
            }
        }
    }

    private void RunAllTests(States nextState, Action extraDone = null)
    {
        int notDone = 0;

        for (int i = 0; i <= (int)state; ++i)
        {
            notDone += unitTests[i].Count;

            foreach (var testObj in unitTests[i])
            {
                if (testObj.State != UnitTest.ExecutionState.DONE)
                {
                    try
                    {
                        testObj.Run(Time.deltaTime);
                        testObj.CheckDone();
                    }
                    catch (Exception e)
                    {
                        // In case of exception flag as done to avoid looping over the test
                        testObj.State = UnitTest.ExecutionState.DONE;
                        throw e;
                    }
                }

                if (testObj.State == UnitTest.ExecutionState.DONE)
                {
                    --notDone;
                }
            }
        }

        // Change phase if all tests are done
        if (notDone == 0)
        {
            state = nextState;
            if (extraDone != null)
            {
                extraDone();
            }
        }
    }

    void Update()
    {
        if (testingEnabled)
        {
            switch (state)
            {
                case States.IN_MENU:
                    if (_elapsedTime <= 1.0f)
                    {
                        _elapsedTime += Time.deltaTime;
                        return;
                    }
                    _elapsedTime = 0;

                    RunAllTests(States.IN_RACES_SELECT, () =>
                    {
                        Application.LoadLevel("seleccion personaje");
                    });
                    break;

                case States.IN_RACES_SELECT:
                    if (_elapsedTime <= 1.0f)
                    {
                        _elapsedTime += Time.deltaTime;
                        return;
                    }
                    _elapsedTime = 0;

                    RunAllTests(States.PRE_GAME_CHECKING, () =>
                    {
                        GameObject.Find("GameInformationObject").
                            GetComponent<GameInformation>().
                            setGameMode(GameInformation.GameMode.CAMPAIGN);

                        Application.LoadLevel("ES2015A");
                    });
                    break;

                case States.PRE_GAME_CHECKING:
                    // Hack to make camera stop moving
                    Camera.main.GetComponent<CameraController>().setCameraSpeed(0);

                    RunAllTests(States.IN_GAME);
                    break;

                case States.IN_GAME:
                    _elapsedTime += Time.deltaTime;

                    // Do not advance until elapsed time has gone by!
                    RunAllTests(States.IN_GAME, () =>
                    {
                        if (_elapsedTime >= testTime)
                        {
                            state = States.POST_GAME_CHECKING;
                        }
                    });
                    break;

                case States.POST_GAME_CHECKING:
                    RunAllTests(States.KILLING, () =>
                    {
                        OnDisable();
                    });
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
