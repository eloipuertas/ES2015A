using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Storage
{
    class UnitAttributesDataConverter : JsonConverter
    {
        public override bool CanConvert (Type objectType)
        {
            return objectType == typeof(UnitInfo);
        }

        public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<UnitAttributes> (reader);
        }

        public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize (writer, value);
        }
    }

    class UnitActionsDataConverter : JsonConverter
    {
        public override bool CanConvert (Type objectType)
        {
            return objectType == typeof(UnitInfo);
        }

        public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<UnitAbility> list = serializer.Deserialize<List<UnitAbility>> (reader);
            return list.ConvertAll (x => (EntityAbility)x);
        }

        public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize (writer, value);
        }
    }
}
