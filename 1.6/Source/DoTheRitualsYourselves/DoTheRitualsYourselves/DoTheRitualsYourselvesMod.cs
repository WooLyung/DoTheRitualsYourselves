using DoTheRitualsYourselves.Core;
using Verse;

namespace DoTheRitualsYourselves
{
    [StaticConstructorOnStartup]
    public class DoTheRitualsYourselvesMod
    {
        static DoTheRitualsYourselvesMod()
        {
            LongEventHandler.ExecuteWhenFinished(RitualLister.MakeRitualCache);
        }
    }
}