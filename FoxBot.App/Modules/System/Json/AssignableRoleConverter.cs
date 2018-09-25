using FoxBot.App.Configurations;
using FoxBot.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.App.Modules.System.Json
{
    public class AssignableRoleConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IAssignableRole);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var aRole = new AssignableRole();

            serializer.Populate(jsonObject.CreateReader(), aRole);

            return aRole;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization");
        }
    }
}
