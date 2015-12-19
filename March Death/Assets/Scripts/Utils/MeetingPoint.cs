using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utils
{
     class MeetingPoint : MonoBehaviour
    {

        public static GameObject CreatePoint(Storage.Races race)
        {
            GameObject point = null;
            switch (race)
            {
                case Storage.Races.ELVES:
                    point = Instantiate((GameObject)Resources.Load("Prefabs/MeetingPoints/Elves", typeof(GameObject)));
                    break;
                case Storage.Races.MEN:
                    point = Instantiate((GameObject)Resources.Load("Prefabs/MeetingPoints/Humans", typeof(GameObject)));
                    break;              
            }
            point.SetActive(false);

            return point;
        }
    }
}
