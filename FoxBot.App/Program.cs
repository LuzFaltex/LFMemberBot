using System;
using System.Threading.Tasks;

namespace FoxBot.App
{
    class Program
    {
        public static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            ContainerBootstrapper containerBootstrapper = new ContainerBootstrapper();

            await containerBootstrapper.Invoke();
        }
    }
}
