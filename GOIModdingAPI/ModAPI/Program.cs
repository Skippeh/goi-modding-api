using System.Reflection;
using Harmony;

namespace ModAPI
{
    public class Program
    {
        public static void Main()
        {
            var harmony = HarmonyInstance.Create("com.goimodapi");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}