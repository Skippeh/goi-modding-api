using System.Reflection;
using Harmony;

namespace ModAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var harmony = HarmonyInstance.Create("com.goimodapi");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}