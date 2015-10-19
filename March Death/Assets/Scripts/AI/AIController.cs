using Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Class tasked with deciding what the enemy will do.
/// </summary>
namespace Assets.Scripts.AI 
{
    public class AIController : MonoBehaviour
    {
        public MacroManager Macro { get; set; }
        public MicroManager Micro { get; set; }

        /// <summary>
        /// Will be used to calculate the lvl of the AI
        /// </summary>
        public int DifficultyLvl { get; set; }

        List<AIModule> modules;
        //TODO: change this when decided about what do we really need to keep about buildings 
        public List<MonoBehaviour> OwnBuildings { get; set; }
        public List<MonoBehaviour> EnemyBuildings { get; set; }
        /// <summary>
        /// Just a basic way to keep track of what the enemy has more
        /// </summary>
        public Dictionary<UnitTypes,int> UnitsFound { get; set; }

        public List<Unit> Army { get; set; }
        public List<Unit> Workers { get; set; }

        /// <summary>
        /// The micro is asking if the macro can spare some civils to the army (for exploring or defending)
        /// </summary>
        /// <param name="num"></param>
        public void takeArms(int num)
        {
            Macro.takeArms(num);
        }

        float timer; //It's not going to overflow this millennium
        void Awake()
        {
            Macro = new MacroManager(this);
            Micro = new MicroManager(this);
            modules = new List<AIModule>();
            Army = new List<Unit>();
            Workers = new List<Unit>();

            modules.Add(new AIModule(Macro.MacroHigh, 30f));
            modules.Add(new AIModule(Macro.MacroLow, 1f));
            modules.Add(new AIModule(Micro.Micro, 1f));
            timer = 0;
        }
        void Update()
        {
            timer += Time.deltaTime;
            foreach (AIModule m in modules)
                if (Math.Round(timer % m.period) == 0)
                    m.Callback();
        }
    }
    struct AIModule
    {
        public AIModule(Action Callback, float period) {
            this.Callback = Callback;
            this.period = period;
        }
        public Action Callback;
        /// <summary>
        /// Time in seconds between each call to Callback
        /// </summary>
        public float period;
    }
    
}
