# Do The Rituals Yourselves

## Mod Overview

Do The Rituals Yourselves is a RimWorld mod that adds the ability to automatically perform events that operate through RimWorld's Ritual system.

RimWorld's Ritual system is used by various DLCs and mods to implement different types of events. This mod extends that system so that events normally started manually by the player can be performed automatically under specified conditions.

The automatically performed events provided by this mod by default include religious rituals from the Ideology DLC and speeches from the Royalty DLC.

## Supported Versions and Requirements

### Supported Versions

* RimWorld 1.5 (support discontinued)
* RimWorld 1.6

### Required Mods and DLC

There are no required mods or DLCs, and this mod can be applied even if no DLC is installed.
However, the automatic event features provided by default are designed for events from the Ideology DLC and the Royalty DLC.
If those DLCs are installed, automatic execution for the Ritual-based events provided by those DLCs will be enabled.

## Installation

Subscribe on Steam Workshop, or place the files from this repository into RimWorld's `Mods` folder.

## About Extension

This mod provides an entry-point API for automatically performing events based on the Ritual system.
By implementing this API, other events from DLCs or Ritual-based events added by other mods can be extended as automatic execution targets.
However, some systems from the Anomaly DLC are not based on the Ritual system, so they cannot be applied through this method.

## For Developers

Do The Rituals Yourselves provides an API that allows Ritual-based events added by other DLCs or mods to be extended as automatic execution targets.

Extensions are implemented by creating `RitualGroupLister` and `RitualGroup` classes.

### Implementing RitualGroupLister

`RitualGroupLister` is the entry point for registering one or more `RitualGroup` instances into the game.

By implementing this class, its child `RitualGroup` instances will be added in-game without any additional class registration.

For example, in the context of the Ideology DLC, a single ideology or religion corresponds to a `RitualGroupLister`, and the list of rituals available under that ideology is included as `RitualGroup` instances.

```csharp
public abstract class RitualGroupLister
{
    public abstract bool IsSkipped { get; }
    public abstract IEnumerable<RitualGroup> GetGroups();
    public abstract IEnumerable<ThingDef> GetBuildingDefs();
}
```

`IsSkipped` determines whether this `RitualGroupLister` should be used in the current game.

`GetGroups` returns the list of `RitualGroup` instances to register in the game.

`GetBuildingDefs` returns the building Defs used by this group.

### Implementing RitualGroup

`RitualGroup` represents a group of rituals displayed together on a single screen.

By implementing this class, a new ritual group can be added. For example, the list of rituals belonging to a specific ideology in the Ideology DLC corresponds to one `RitualGroup`.

```csharp
public abstract class RitualGroup
{
    private RitualGroupLister lister;

    public RitualGroup(RitualGroupLister lister)
    {
        this.lister = lister;
    }

    public abstract int Priority { get; }
    public abstract string Label { get; }
    public abstract Texture2D Icon { get; }
    public abstract Color Color { get; }

    public abstract IEnumerable<Ritual> GetVisibleRituals();
    public abstract IEnumerable<Ritual> GetAllRituals();

    public virtual bool ValidateSpot(TargetInfo t, Precept_Ritual ritual)
    {
        if (!(t.Thing is Building) || t.Thing.Faction != Faction.OfPlayer)
            return false;

        if (!ritual.activeObligations.NullOrEmpty())
            foreach (RitualObligation activeObligation in ritual.activeObligations)
                if (ritual.CanUseTarget(t.Thing, activeObligation).canUse)
                    return true;

        try
        {
            return ritual.CanUseTarget(t, null).canUse;
        }
        catch
        {
            return false;
        }
    }

    public IEnumerable<ThingDef> GetBuildingDefs() => lister.GetBuildingDefs();
}
```

`Priority` represents the sorting priority when multiple groups are displayed.

`Label`, `Icon`, and `Color` are used to display the ritual group in the in-game UI.

`GetVisibleRituals` returns the list of rituals to display in the current UI.

`GetAllRituals` returns the complete list of rituals managed by this group.

`ValidateSpot` checks whether the selected target location can be used to perform the ritual. The default implementation targets player-owned buildings and checks whether the ritual can use the selected target. If special target validation is required, this function can be overridden.

## License

This project is licensed under the [MIT License](LICENSE).

The source code of this mod may be freely used, modified, and distributed, provided that the original copyright notice and license text are included when distributing it.

See the [LICENSE](LICENSE) file for details.

## Links

* [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3511385838)
* [GitHub](https://github.com/WooLyung/DoTheRitualsYourselves)