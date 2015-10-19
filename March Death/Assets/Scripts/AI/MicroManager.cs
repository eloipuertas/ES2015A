using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Class tasked with mainly the control of the AI army.
/// this class will also control civils when they are either:
///     A: Exploring
///     B: Fighting for the motherland
/// </summary>
namespace Assets.Scripts.AI
{
    public class MicroManager
    {
        AIController ai;
        public MicroManager(AIController ai)
        {
            this.ai = ai;
        }
        /// <summary>
        /// Called pretty fast, it's just like Update()
        /// </summary>
        public void Micro()
        {
        }
    }
}
