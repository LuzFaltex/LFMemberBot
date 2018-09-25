using FoxBot.App.Configurations;
using FoxBot.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoxBot.App.Modules.System.Json
{
    public class RoleCategoryConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IRoleCategory);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var roleCategory = new RoleCategory();

            serializer.Populate(jsonObject.CreateReader(), roleCategory);

            return roleCategory;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException("Use default serialization");
        }
    }
}
