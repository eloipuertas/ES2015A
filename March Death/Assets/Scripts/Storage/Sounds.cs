using UnityEngine;
using System.Collections.Generic;
using Utils;
namespace Storage
{
    sealed class Sounds : Singleton<Sounds>
    {

        public enum SoundType { CREATION, SELECTION, ACTION, ATTACK, DEAD, TRAP, FULLHOUSE, RECRUITEXPLORER }
        public enum SoundSource { BUILDING, UNIT}

        private Dictionary<Tuple<BuildingTypes, SoundType>, AudioClip> buildings = new Dictionary<Tuple<BuildingTypes, SoundType>, AudioClip>();

        private Dictionary<Tuple<SoundSource, SoundType>, AudioClip[]> sounds = new Dictionary<Tuple<SoundSource, SoundType>, AudioClip[]>();



        private Sounds()
        {
            parseSoundFiles<BuildingTypes>("Sounds/Buildings", buildings);

            parseCommonSoundFiles("Sounds/common/units/selection", sounds, SoundSource.UNIT, SoundType.SELECTION);
            parseCommonSoundFiles("Sounds/common/units/action", sounds, SoundSource.UNIT, SoundType.ACTION);
            parseCommonSoundFiles("Sounds/common/units/death", sounds, SoundSource.UNIT, SoundType.DEAD);
            parseCommonSoundFiles("Sounds/common/buildings/destroyed", sounds, SoundSource.BUILDING, SoundType.DEAD);
            parseCommonSoundFiles("Sounds/common/buildings/trap", sounds, SoundSource.BUILDING, SoundType.TRAP);
            parseCommonSoundFiles("Sounds/common/buildings/fullHouse", sounds, SoundSource.BUILDING, SoundType.FULLHOUSE);

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


        private void parseCommonSoundFiles(string folder, Dictionary<Tuple<SoundSource, SoundType>, AudioClip[]> dict, SoundSource source, SoundType type)
        {
            Object[] assets = Resources.LoadAll(folder, typeof(AudioClip));
            AudioClip[] list = new AudioClip[assets.Length];
            int iter = 0;

            foreach (AudioClip audio in assets)
            {
                list[iter++] = audio;

            }
            Tuple<SoundSource, SoundType> key = new Tuple<SoundSource, SoundType>(source, type);
            dict.Add(key, list);
        }


        /// <summary>
        /// Returns an AudioClip for the specified params
        /// </summary>
        /// <param name="bType"></param>
        /// <param name="sType"></param>
        /// <returns></returns>
        public AudioClip Clip(BuildingTypes bType, SoundType sType)
        {
            Tuple<BuildingTypes, SoundType> key = new Tuple<BuildingTypes, SoundType>(bType, sType);

            if (!buildings.ContainsKey(key))
            {
                throw new System.ArgumentException("Sound  for ('" + bType + "', '" + sType + "') not found");
            }
            return buildings[key];
        }

        /// <summary>
        /// Returns a random AudioClip of the specified type
        /// </summary>
        /// <param name="sType"></param>
        /// <returns></returns>
        public AudioClip RandomClip(SoundSource sSource, SoundType sType)
        {
            Tuple<SoundSource, SoundType> key = new Tuple<SoundSource, SoundType>(sSource, sType);
            AudioClip[] choices = sounds[key];
            AudioClip choice = choices[Random.Range(0, choices.Length)];
            return choice;

        }

    }
}
