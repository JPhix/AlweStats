# AlweStats

You can find it here : https://valheim.thunderstore.io/package/Padank/AlweStats/

Valheim Mod to easily see "FPS", "Ping", "Clock", "Ship health", "Ship speed", "Wind speed", "Wind direction", 
"Days passed", "Time played", "Current Biome" and "Health" for every environment element.

**YOU HAVE TO DELETE YOUR CONFIG FILE IF YOU HAVE A VERSION BEFORE 2.0.0**

The UI is separated in four blocks :
- GameStats, that contains "FPS" and "Ping"
- WorldStats, that contains "Days passed", "Time played" and "Current biome"
- WorldClock, that contains "Clock"
- ShipStats, that contains "Ship health", "Ship speed", "Wind speed", "Wind direction"

For each block you can enable or disable it and you can also set :
- Position
- Margin 
- Text color
- Text size 
- Text alignment (not for the Clock)

For the "EnvStats" section, you can set :
- The string format for the health of any environment element
- Whether or not to show the health separately for rocks, bushes and trees

In the config file you can also choose whether or not to show :
- A "Days passed" counter in the world selection panel
- A reset button in the pause menu to reset the positions of all four blocks with their default values
- The clock in 12h or 24h format
- The current biome in the WorldStats block instead of the top-left corner in minimap 

**If you have an idea for smoething to add or any suggestion regarding the implemented features, feel free to create an issue on the GitHub repository !**

> You can move the blocks in game with your mouse ! 
> To enable the editing mode you just have to click the key chose in the config file (default: F9)
> and move the blocks where you prefer ! To save your changes, you need to press again the key.
> You can also reset all the positions and margins of the blocks with the button in the pause menu.

> Since the default day length declared in Valheim doesn't match the real one,
> to successfully use the "Days passed" counter in the world selection panel you need to join in a world at least one time, 
> The mod will then get the day length in that world and save it in the "Alwe.stats" file (you have to do it for each world).

> As I don't know the proportions that the game uses, 
> I have established that the maximum **ship speed** is equal to **30 kts** and the maximum **wind speed** is equal to **100 km/h**,
> so the values ​​are proportioned according to this.

### Known bugs

- Health starts showing from second hit for rocks and minerocks.
- Environment element name doesn't show if you hit an element without aiming at it.
- Rock name doesn't show for small rocks.

### Changelog

**2.5.0**
- Added an health string for every environment element (rocks, minerocks, trees, bushes, ecc...)
- Added new config values to match the new additions
- Added a key to reload the plugin's config file while in game

**2.4.2**
- Fixed the ship speed that was showing 0 when revesing

**2.4.1**
- Fixed the check if the player in on a ship or not
- Fixed the ship speed with more credible values (in order to better respect the true speeds of Viking ships)

**2.4.0**
- Fixed error when exiting server
- Added possibility to remove biome from minimap top-left corner and set it in the WorldStats block

**2.3.0**
- Optimized the editing mode
- Now you don't need to close and open again the game to save your changes
- Optimized all the blocks for better readability

**2.2.0**
- Added "Ship speed" in the ShipStats block
- Optimized the check if the player is on board or not (ShipStats) 

**2.1.1**
- Added the ShipStats block in the editing mode
- Overall adjustments

**2.1.0**
- Added the ShipStats block, to see ship health, wind speed and wind direction
- Added new config values to match the new additions

**2.0.1**
- Overall fixes and optimization

**2.0.0**
- Optimized the code and made it cleaner to read
- Added an editing mode
- Added new config values to match the new additions

**1.2.0**
- Fixed "Text color", "Position" and "Margin" values to be culture invariant (so now you can set decimal values with the point)
- Added a customizable in-game clock, separated from WorldStats

**1.1.1**
- Commented out the "OnDestroy" method (it is for development only)
- Removed commented code

**1.1.0**
- Added a "Days passed" string in the world selection panel
- Added the chance to disable it in the config file

**1.0.0**
- Added GameStats, that contains "FPS" and "Ping"
- Added WorldStats, that contains "Days passed" and "Time played"