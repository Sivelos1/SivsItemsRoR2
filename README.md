# SivsItemsRoR2
 A Risk of Rain 2 Mod that adds a variety of items.
 
## Uncommon Items

> Know Thy Enemy: Enemies with the same elite affix as you take massive damage.

 
## Enemy Items
 Enemy items are special yellow items that have a chance to drop from various monsters. Think of them as boss items but for individual enemies.

> Worker's Bond (Beetle): Having allies nearby boosts regen.

> Abandoned Wisp (Lesser Wisp): Summon a Little Wisp on kill.

> Frayed Tentacle (Jellyfish): Chance on hit to tether yourself to an enemy, dealing damage over time.

> Mourning Geode (Stone Golem): Gain a burst of armor and regen on kill.

> Imp's Eye (Imp): Bleeding now lowers armor and movement speed.

> Frenzied Tarbine (Clay Templar): Striking enemies rapidly also hits them with a barrage of Tar bullets.

> Bighorn Buckler (Bighorn Bison): Damage enemies by dashing into them.

> Living Furnace (Elder Lemurian): Chance on hit to ignite enemies.

> Null Seed (Void Reaver): Annihilate all nearby characters on kill. Recharges after 30 seconds.

## Change Log
Changes marked with "-" are general balance changes. Changes marked with "*" are bugfixes.
```
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
