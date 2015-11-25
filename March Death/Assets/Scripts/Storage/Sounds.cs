using UnityEngine;
using System.Collections.Generic;
using Utils;
namespace Storage
{
    sealed class Sounds : Singleton<Sounds>
    {

        public enum SoundType { CREATION, SELECTION, ACTION, ATTACK }
        private Dictionary<Tuple<BuildingTypes, SoundType>, AudioClip> buildings = new Dictionary<Tuple<BuildingTypes, SoundType>, AudioClip>();


        private Sounds()
        {
            parseSoundFiles<BuildingTypes>("Sounds/Buildings", buildings);

        }

        private void parseSoundFiles<EnumType>(string folder, Dictionary<Tuple<EnumType, SoundType>, AudioClip> dict) where EnumType : struct
        {
            Object[] assets = Resources.LoadAll(folder, typeof(AudioClip));
            foreach(AudioClip audio in assets)
            {

                string[] name = audio.name.Split('-');
                EnumType entity = (EnumType)System.Enum.Parse(typeof(EnumType), name[0], true);
                SoundType type = (SoundType)System.Enum.Parse(typeof(SoundType), name[1], true);
                Tuple<EnumType, SoundType> key = new Tuple<EnumType, SoundType>(entity, type);
                dict.Add(key, audio);
            }
        }



        public AudioClip Clip(BuildingTypes bType, SoundType sType)
        {
            Tuple<BuildingTypes, SoundType> key = new Tuple<BuildingTypes, SoundType>(bType, sType);

            if (!buildings.ContainsKey(key))
            {
                throw new System.ArgumentException("Sound  for ('" + bType + "', '" + sType + "') not found");
            }
            return buildings[key];
        }

    }
}
