using DoTheRitualsYourselves.Tool;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

namespace DoTheRitualsYourselves
{
    [StaticConstructorOnStartup]
    public class DoTheRitualsYourselvesMod
    {
        private static Harmony harmony;

        static DoTheRitualsYourselvesMod()
        {
            harmony = new Harmony("ng.lyu.dotheritualsyourselvesMod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            LongEventHandler.ExecuteWhenFinished(RitualStarter.MakeRitualCache);
        }
    }
}