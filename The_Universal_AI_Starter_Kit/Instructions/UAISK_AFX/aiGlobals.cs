//-----------------------------------------------------------------------------
// Copyright (c) 2018 Twisted Jenius LLC
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------


//The following are global variables used to set the bot's basic default settings. General rules for default variable values:
//Don't use negative numbers, and be careful when using values of 1 or less, unless noted otherwise.
//Don't stray too far from the default values. For example; if the default is 6, you probably shouldn't change it to 9006.
//If you don't know exactly what a variable does, go ahead and experiment with it, but remember what the default was.

//Turns marker hiding on or off, useful when editing missions.
$AISK_MARKER_HIDE = true;

//How long the bot waits between firing bursts.
$AISK_FIRE_DELAY = 1000;

//How long the bot holds down the trigger when firing.
$AISK_TRIGGER_DOWN = 50;

//Aggressive bot's normal field of vision.
$AISK_FOV = 200;

//There is a 1 in x chance that the bot will see 360 deg vision to prevent it from being snuck up on. Set to 0 to disable.
$AISK_ENHANCED_FOV_CHANCE = 0;

//How long the bots field of vision is enhanced to 360 for.
$AISK_ENHANCED_FOV_TIME = 2000;

//How long the bot gets a 360 FOV and a longer detect distance for after being sniped.
$AISK_ENHANCED_DEFENDING_TIME = 5000;

//Detect distance is multiplied by this number after a bot has been sniped.
$AISK_ENHANCED_DEFENDING_DISTANCE = 3;

//The range at which a bot will start reacting to a target.
$AISK_DETECT_DISTANCE = 50;

//The detect distance is multiplied by this number to get the distance from the player at which the bot will
//start to pay less attention
$AISK_ATTENTION_RANGE = 2.5;

//The bot will stop and try to stay at this distance or less from the player.
$AISK_MAX_DISTANCE = 25;

//The bot will stop and try to stay at this distance or more from the player. This must be less than max distance
//and sidestep. Keeping this default value at less than 1 will greatly reduce the chance for errors.
$AISK_MIN_DISTANCE = 0;

//The maximum distance a "leashed" bot can be away from its marker
$AISK_LEASH_DISTANCE = 35;

//The maximum range the bots pace away from their guard point.
$AISK_MAX_PACE = 6;

//The minimum range the bots pace away from their guard point.
//If set to less than 1, this can conflict with the general movement min distance.
$AISK_MIN_PACE = 1.5;

//Set the speed of the bot while pacing (1.0 is 100%, 0.5 is 50%)
$AISK_PACE_SPEED = 0.5;

//Set the speed of the bot while on a path (1.0 is 100%, 0.5 is 50%)
$AISK_PATH_SPEED = 1.0;

//Sets how many think cycles the bot has to travel to it's location (or stand at its location
//if it's already there) before getting another one to move to. Random between 1 and this number.
$AISK_PACE_TIME = 4;

//This value determines how far a bot sidesteps when it is stuck or dodging.
$AISK_SIDESTEP = 10;

//The bot will dodge once in this many think cycles while in combat. Set to 0 to disable.
$AISK_ACTIVE_DODGE = 1;

//The amount of time after the bot loses sight of player that it will get their position.
//This helps the bot turn sharp corners. Set it to 1 or 0 if you don't want the bot to cheat.
$AISK_LOS_TIME = 150;

//The quickest time between think cycles.
$AISK_SCAN_TIME = 500;

//This number and $AISK_SCAN_TIME are multiplied to set the delay in the
//thinking loop. Used to free up processor time on bots out of the mix.
$AISK_MAX_ATTENTION = 10;

//The number of think cycles that the bot will 'hold' for before trying to return to a certain spot.
$AISK_HOLDCNT_MAX = 10;

//This is the number of random positions that pacing and sidestep will try before considering the bot
//to be stuck and giving up.
$AISK_LOOP_COUNTER = 16;

//How long a bot waits after creation before his think cycles are controlled by
//his attention rate. Used to help free up think cycles on bots while misison finishes loading.
$AISK_CREATION_DELAY = 1500;

//Controls whether bots respawn automatically or not.
$AISK_DEFAULT_RESPAWN = true;

//This sets at what damage level a bot will attempt to look for a HealthPatch nearby.
//When set to 0.7 the bot will seek health when it has taken 70% of the total damage it can take
//(and has 30% or less health left), 0.5 is 50% and so on. The default of 1.0 means that bots
//will not seek health because they'd have to have 0% health left.
//Item gathering has been commented out because it does not work properly
//$AISK_SEEK_HEALTH_LVL = 1.0;

//Sets how far around itself a bot will look for items to pick up
//Item gathering has been commented out because it does not work properly
//$AISK_DETECT_ITEM_RANGE = 50;

//Bots go for the target with the least health unless two or more targets are within this amount of one another,
//then they go for the closest target. If you set this number really high, bots will always just go by range.
$AISK_HEALTH_IGNORE = 25;

//Sets the default team for all bots, by default the player is on team 1
//This is used for selecting a target, and can also be used for spawning and killing bots.
$AISK_TEAM = 2;

//If set to true, every bot will try to attack every other bot, regardless of their team
$AISK_FREE_FOR_ALL = false;

//This is used for spawning and killing specified bots.
$AISK_SPAWN_GROUP = 1;

//This is the default datablock that the bot is spawned as.
$AISK_CHAR_TYPE = "AIPlayerDataHolder";

//Which weapons you want the bot to use. You can have more than one weapon, seperate each with a space.
//Example: $AISK_WEAPON = "Lurker Ryder";
$AISK_WEAPON = "Ryder";

//Set this to false for energy or melee weapons that do not use ammo.
$AISK_WEAPON_USES_AMMO = true;

//When set to true the bot will replenish its ammo perpetually
$AISK_ENDLESS_AMMO = true;

//The maximum range at which a bot will fire on a target with a weapon.
$AISK_IGNORE_DISTANCE = 50;

//Sets how often the bot should try to switch to a different weapon. You can have as many numbers as you want,
//seperated by spaces. Each number is gone through in order.
$AISK_CYCLE_COUNTER = "5";

//If the cycle counter is set to "x" it will get a random number between 1 and this value each time
$AISK_RAND_CYCLE_MAX = 10;

//If the active dodge is set to "x" it will get a random number between 1 and this value each time
$AISK_RAND_DODGE_MAX = 7;

//Sets how the next weapon is to be selected.
$AISK_WEAPON_MODE = "pattern";

//Sets what the bot's name should be displayed as. Can be set to "Show", "DontShow" or "Debug".
//When set to "Debug", warnings are sent to the console when certain potential errors occur.
$AISK_SHOW_NAME = "DontShow";

//The maximum height of an obstacle that the bot can run on top of, given as a vector.
$AISK_OBSTACLE = "0 0 1";

//About half the height an average character in your game, given as a vector.
$AISK_CHAR_HEIGHT = "0 0 1.5";

//The default name for all bots.
$AISK_REAL_NAME = "Bot";

//The amount of time in milliseconds that a bot should wait before doing an important think cycle.
$AISK_QUICK_THINK = 100;

//The minimum range at which a bot will fire on a target with a weapon.
//Keeping this default value at less than 1 will greatly reduce the chance for errors.
$AISK_MIN_IGNORE_DISTANCE = 0;

//Default weapon rating. This is here to prevent serious errors if a weapon isn't given a rating for some reason.
//But you will still get minor unless each weapon has a different rating, meaning this should not be used.
$AISK_WEAPON_RATING = 1;

//Overall default behavior.
$AISK_BEHAVIOR = "ChaseBehavior";

//Sets the amount of ammo the bot should start with
$AISK_STARTING_AMMO = 10;

//The value below times detect distance is how far away bots can assist from
$AISK_ASSIST_DISTANCE = 2;

//When a bot or player is hurt, have nearby team members come to their aid.
//This can be set to the following values:
//1 = Bots can only assist players or bots following the player that get injured.
//2 = Bots can only assist other bots that are injured.
//3 = Bots can assist all players and bots that get injured.
//All other values, such as 0, will completely disable assisting. 
$AISK_CAN_ASSIST = 3;

//The maximum number of times that bots can respawn, 0 is infinite
$AISK_RESPAWN_COUNT = 0;

//If the respawn count is set to "x" it will get a random number between 1 and this value
$AISK_RESPAWN_RANDOM = 5;

//Advanced active dodge settings:
//"Random" = Random, classic style UAISK dodging.
//"Side" = Side to side.
//"Back" = Front and back.
//"Serpentine" = Serpentine, which varies between side to side and back and forth.
$AISK_ADVANCED_DODGE = "Random";

//Sets if weapons should be aimed or not as they are being fired
$AISK_NOT_AIM = false;

//Name of the default simgroup
$AISK_GROUP = "MissionGroup";

//Determines how long a bot waits in between dying and respawning again
$AISK_RESPAWN_DELAY = 20 * 1000;

//Can teammates hit each other, this includes hitting themselves, for the AI only
$AISK_FRIENDLY_FIRE = true;

//----------------------------------------------------------------
//The values below are AFX specific

//The type of AFX datablock that the AI deals with
$AISK_AFX_DATA_TYPE = "afxRPGMagicSpellData";

//Bots casting AFX spells that can be interrupt by movement wait this long to cast after they
//stop moving, just to make sure that they have really come to a full and complete stop.
$AISK_AFX_WAIT_TIME = 250;

//----------------------------------------------------------------
//The values below are Walkabout specific

//Allow behaviors to make use of Walkabout navmeshes instead of dynamic obstacle avoidance
$AISK_WALKABOUT_ENABLE = false;

//Distance that characters should search for cover
$AISK_WALKABOUT_COVER_RADIUS = 20;
