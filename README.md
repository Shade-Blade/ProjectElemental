# ProjectElemental
RPG game similar to Paper Mario

This is a Unity project made in version 2021.3.16f.
Packages are mostly normal except the bloom postprocessing has been modified to support negative bloom (HDR allows colors to have negative values, this modification causes objects with negative color to make the surroundings darker)

Currently, most of the systems are done although the content for the game is very early. Most art is either placeholders or rough draft level quality (though item, badge, etc sprites are closer to being final than the other art)
There are sprites for all enemies and all the npcs that appear in this prototype, but there are no animations for anyone except the main characters (and the main character's animations are not fully done, there may be additional frames for some animations, new animations for certain moves that don't have them right now)

Itch.io link (a build of this project): [https://shadeblade.itch.io/project-elemental-rabbit-hole-demo](https://shadeblade.itch.io/project-elemental-rabbit-hole-demo)

## Functional Systems
Note: You can look in the Input Manager for what keyboard inputs are used for certain buttons.

- Start Menu and save file management (The rainbow bordered sphere is the save point)
	- Note that there are save files in the github project that are not present in the itch page.
- All sections of the Pause Menu
	- Status menu works properly and reflects your current status (Descriptions are not written yet)
	- Item menu allows you to use items (Key Items may get a confirmation popup to be added later?)
	- Equip menu allows you to equip badges or ribbons (This menu can also be sorted with the Z button)
	- Quest menu properly displays quests and allows you to see descriptions and read the longer descriptions by pressing A	
	- Journal menu properly displays information for each subsection (Note that most of the intricacies of the battle system are explained here)
	- Map section works but is rudimentary right now (Placeholder map and only 2 locations show up on the map)
	- Settings menu allows you to change settings (Note that only the FPS setting is implemented, as there is no sound.)
	- Settings menu allows you to rebind controls
- Overworld abilities (Note: Weapon related abilities have no animations but there is a debug hitbox viewer)
	- A to jump
	- (Wilex) A in midair to double jump
	- (Wilex) Spin input + A to super jump
	- (Luna) Tap a direction + A to dash hop (After the first hop you can hold a direction and press A when you land to hop again)
	- (Luna) Spin input + Hold A to dig
	- B to use weapon (Wilex has a sword and Luna has a hammer)
	- (Wilex) Hold B to Aetherize (become invisible to enemies, pass through certain materials)
	- (Luna) Hold B to Illuminate (makes some materials tangible and others intangible)
- Items, Badges
	- Items have mostly written descriptions telling you what they do
	- Badges have descriptions (some have very rough descriptions that will need to be rewritten later)
- Writing in general
	- Most writing is either placeholder or rough draft level
	- Bestiary text is done but is rough draft ish level (will most likely be rewritten later to convey the personality of characters better)
	- Bestiary text for all the enemy moves is done for now
- Battle System
	- Most stuff works similar to Paper Mario but there are differences
	- Moves cost stamina and energy, you get stamina every turn, but if you use a move with a cost higher than your Agility you won't get more stamina next turn.
	- Blocking only reduces effect durations by half
- Maps
	- This demo contains 3 maps for the dungeon (but there are 100 floors to go through)
- Text System
  	- The textbox system is quite robust, with many different tags with different effects (See DialogueText.cs for the enum, use Analyze to find the code for the tags for more information on how they work than the comments provide)

Use the ~ key to open the cheat menu in the overworld, you can type and press enter to execute a cheat or escape to close the menu. Look at CheatMenu.cs for more information on how the cheat menu works. Look at the cheat flags in MainManager.cs to see what all of them do.
