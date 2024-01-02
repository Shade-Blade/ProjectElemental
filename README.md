# ProjectElemental
RPG game similar to Paper Mario

This is a Unity project made in version 2021.3.16f.
Packages are mostly normal except the bloom postprocessing has been modified to support negative bloom (HDR allows colors to have negative values, this modification causes objects with negative color to make the surroundings darker)

Currently, most of the systems are done although the content for the game is basically nonexistent. Most art is either placeholders or rough draft level quality (though item, badge, etc sprites are closer to being final than the other art)

## Functional Systems
Note: You can look in the Input Manager for what keyboard inputs are used for certain buttons.

- Start Menu and save file management (The rainbow bordered sphere is the save point)
- All sections of the Pause Menu
	- Status menu works properly and reflects your current status (Descriptions are not written yet)
	- Item menu allows you to use items (Key Items may get a confirmation popup to be added later?)
	- Equip menu allows you to equip badges or ribbons (This menu can also be sorted with the Z button)
	- Quest menu properly displays quests and allows you to see descriptions and read the longer descriptions by pressing A	
	- Journal menu properly displays information for each subsection (Flags for everything are to be added later) (Note that most of the intricacies of the battle system are explained here)
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
	- Badges have very rough descriptions (the descriptions in my notes)
- Writing in general
	- Most writing is either placeholder or rough draft level
	- Bestiary text is kind of haphazard but most of the bio text is written (rough draft level)
	- Bestiary text for all the enemy moves is done for now
- Battle System
	- Most stuff works similar to Paper Mario but there are differences
	- Moves cost stamina and energy, you get stamina every turn, but if you use a move with a cost higher than your Agility you won't get more stamina next turn.
	- Blocking only reduces effect durations by half
- Maps
	- 10 maps corresponding to different areas (with mostly placeholder level materials)
	- Each of these maps has enemies with the logic for enemies of the area (though no graphics)
	- Main map (the large green plane with stuff on it) has a lot of shader tests in it. (It is somewhat bad for GPUs?)

Use the ~ key to open the cheat menu in the overworld, you can type and press enter to execute a cheat or escape to close the menu. Look at CheatMenu.cs for more information on how the cheat menu works. Look at the cheat flags in MainManager.cs to see what all of them do.