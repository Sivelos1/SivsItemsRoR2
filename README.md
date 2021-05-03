# SivsItemsRoR2
 A Risk of Rain 2 Mod that adds a variety of items.
 
## Uncommon Items

> Know Thy Enemy: Enemies with the same elite affix as you take massive damage.

## Lunar Items

> Wheel of Agony: Sacrifice HP on hit to gain a stack of Agony. Gaining enough stacks of Agony creates a blast for massive damage.

## Equipment

> Guardian's Pacemaker: Recharge your shields on use.

 
## Enemy Items
 Enemy items are special yellow items that have a chance to drop from various monsters. Think of them as boss items but for individual enemies.

> Worker's Bond (Beetle): Having allies nearby boosts regen.

> Abandoned Wisp (Lesser Wisp): Summon a Little Wisp on kill.

> Scale Mail (Lemurian): Gain a stacking armor boost when hit.

> Frayed Tentacle (Jellyfish): Chance on hit to tether yourself to an enemy, dealing damage over time.

> Mourning Geode (Stone Golem): Gain a burst of armor and regen on kill.

> Imp's Eye (Imp): Bleeding now lowers armor and movement speed.

> Frenzied Tarbine (Clay Templar): Striking enemies rapidly also hits them with a barrage of Tar bullets.

> Chitin Hammer (Beetle Guard): Dropping from a great enough height damages nearby enemies on impact.

> Bighorn Buckler (Bighorn Bison): Damage enemies by dashing into them.

> Living Furnace (Elder Lemurian): Chance on hit to ignite enemies.

> The Second Stage (Parent): Enter a fury when an ally is killed.

> Null Seed (Void Reaver): Annihilate all nearby characters on kill. Recharges after 30 seconds.

## Change Log
```
Version 0.1.5
- Wheel of Agony:
	- HP deduction no longer procs on-damage items, like Razorwire
		- Apparently Wheel of Agony had a neat little interaction with Razorwire where it would create an infinite damage loop.
		Normally I'd be all about that, but it can quickly cause crashes the more procs you have, so unfortunately I had to
		axe it - and by extension ALL on-damage items from Wheel of Agonys interactions.
- Enemies now drop items again! Hurray.

Version 0.1.3 (and 0.1.4)
- Wheel of Agony no longer causes null reference exceptions
- Void Fields no longer fails to spawn reward items
- Living Furnace now actually deals damage over time, rather than just applying a debuff that does nothing
- Chitin Hammer now reduces fall damage independently of fall speed
	- Originally, Chitin Hammer only reduced fall speed if you were falling at its activation speed - yet, for some reason, this was
	woefully inconsistent compared to its attack, so now it ALWAYS reduces fall damage.
- Abandoned Wisp:
	- Summon Limit: 1 (+1 per stack) => Infinite
	- Little Wisps now can be summoned ad infinitum, with the side effect of only lasting 15 seconds before perishing.
	- Little Wisps gain 100% (+100% per stack) damage and health.
- New(?) Equipment:
	- Guardian's Pacemaker
		- I think this may have been in past updates, but it broke the mod at one point, so I had to remove it. Thankfully, that is no
		longer the case.
- New Lunar item:
	- Wheel of Agony

Version 0.1.2 (and 0.1.1)
- I'm aware of the Void Fields bug, as well as the Command bug; this version cleaned up some Project References, so HOPEFULLY that helps with one
of the two.
- Mod doesn't attempt to register removed / incomplete content as droppable (lol oops)
- Added new enemy item:
	- The Second Stage, drops from Parents (technically this was in the last update I just forgot to mention lool)
- The Second Stage:
	- Icon now properly matches item
	- Visual effects cleaned up
- Scale Mail:
	- Now grants a stacking buff to armor on hit.
	- Buff Length: 2s (+1s per stack) -> 2s (+2s per stack)
	- Armor Boost: 10 (+5 per stack) -> 5 (+5 per stack)
- Chitin Hammer:
	- Now properly updates the blast attack


Version 0.1.0
- Now updated to work with the anniversary update!
- Item displays are broken right now.
- Added two new enemy items:
	- Scale Mail, drops from Lemurians
	- Chitin Hammer, drops from Beetle Guards
- Frenzied Tarbine:
	- Follow-up duration is now affected by proc coefficients. Basically, the smaller your proc coefficient is, the less time you have 
	to proc Tarbine.
- Imp's Eye:
	- Now provides an invisible bleed chance.


Version 0.0.7
- Properly invokes Language API, allowing for the mod to function on its own
- Frayed Tentacle*:
	- Tethers now adhere to the tether radius and break if their owner goes too far from the target.
	- Tethers no longer linger on dead targets.
	- Tick Rate: 0.1 -> 0.25
	- Tether Radius: 200 -> 65; Frayed Tentacles tethers were never meant to be a long-ranged source of damage. As
	such, theyve been adjusted to something a bit more reasonable - more like Tesla Coils AOE radius.
* Some of these changes may not come into effect, due to the config files created by this mod. Remember to reset the 
values in your config if you want the latest balance changes!

Version 0.0.6
- Fixed Void Fields not spawning any items upon completing a round
- Fixed Mourning Geode raising exceptions on kill
- Adjusted position of Bighorn Buckler's hitbox so larger characters like Acrid can use it

Version 0.0.5
- Added configuration files
	- With the config, you can now edit variables for almost everything in the mod. Namely, enemy item drop rates!
- Reverted enemy item drop rates; enemy item drop rates are now static, as they were upon the mod's initial release.
- All enemy item drop rates:
	- [Varied by enemy] -> 0.1%
	- Keep in mind that enemy item drop rates are now changable through the mod's config file.
- Know Thy Enemy:
	- Now grants a 15% chance on kill to steal a slain elite's aspect for 5 seconds.
	- Damage Multiplier: 300% -> 175%; this is to compensate for Know Thy Enemy's new effect. The multiplier is 
	still exponential, but hopefully this'll make it a bit more balanced. That is, if you don't change it in the
	config.

Version 0.0.4
- Added new uncommon item: Know Thy Enemy
- Reverted drop chances for ALL enemy items. Hopefully Worker's Bonds and Abandoned Wisps will be a bit harder to find.

Version 0.0.3
- Added new enemy item: Abandoned Wisp
     - Drops from Lesser Wisps
- Added new character: Little Wisp
- Added director-influenced drop modifiers; this affects the drop chances of enemy items the later into your run. For example,
Abandoned Wisp has a modifier of 5.5. This means that it recieves a bonus to its drop chance based on the difficulty, and this bonus
is then increased by the 5.5 times modifer. Hopefully this should make the enemy items a bit more accessible.
- Reduced size of Mourning Geode's display particles
- Overgrown Printers can no longer roll Null Seed; drop chance increased to make up for scarcity
- Changed Item Drop Chances:
	- Worker's Bond: 0.1% -> 0.25%
	- Abandoned Wisp: N/A -> 0.5%
	- Frayed Tentacle: 0.1% -> 0.3%
	- Mourning Geode: 0.1% -> 0.15%
	- Imp's Eye: 0.1% -> 0.15%
	- Frenzied Tarbine: 0.1% -> 0.12%
	- Bighorn Buckler: 0.1% -> 0.2%
	- Null Seed: 0.1% -> 1%
- Cooldowns for Bighorn Buckler and Null Seed are properly displayed
* ACTUALLY fixed infinite explosion bug (hopefully)

Version 0.0.2
- Added changelog
- Stacking Null Seed no longer increases AOE radius
- Rebalanced Mourning Geode to make it worth its rarity; Mourning Geode was initially designed as a standard White item, 
and had the stats to match. I felt that since it's a bit harder to get now, it deserved a bit more punch.
     - Initial Buff Duration: 1 second -> 1.5 seconds
     - Stacking Buff Duration: +0.25 seconds -> +0.3 seconds
     - Initial Armor Buff: 10 -> 25
     - Stacking Armor Buff: +5 -> +10
     - Initial Regeneration Buff: +35% -> +85%
     - Stacking Regeneration Buff: +15% -> +35%
- Imp's Eye:
     - Initial Armor Debuff: 5 -> 10
* Fixed Frayed Tentacle causing explosive attacks to loop infinitely (?)

Version 0.0.1
- Initial Release
```
