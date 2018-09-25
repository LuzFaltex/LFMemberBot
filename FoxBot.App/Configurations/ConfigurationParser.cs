using FoxBot.App.Modules.System.Json;
using FoxBot.Core.Interfaces;
using FoxBot.Core.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NodaTime.Serialization.JsonNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FoxBot.App.Configurations
{
    public static class ConfigurationParser
    {       
        public static async Task WriteChanges(IApplicationConfiguration instance)
        {
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented
            };
            serializer.Converters.Add(NodaConverters.RoundtripPeriodConverter);
            serializer.Converters.Add(new VersionConverter());

            await Task.Run(() =>
            {
                using (StreamWriter sw = new StreamWriter("./config.json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, instance);
                }
            });
        }

        public static Task<ApplicationConfiguration> GetApplicationConfiguration()
        {
            if (File.Exists("../config.json"))
            {
                return Task.Run(() => JsonConvert.DeserializeObject<ApplicationConfiguration>(File.ReadAllText("./config.json"), NodaConverters.RoundtripPeriodConverter, new VersionConverter(), new GuildConfigConverter(), new RoleCategoryConverter(), new BonkConverter(), new AssignableRoleConverter()));
            }
            else
            {
                // return await GetDefaultApplicationConfiguration();
                return Task.FromResult(new ApplicationConfiguration());
            }
        }
    }
}
