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


//-----------------------------------------------------------------------------
//Spawning Functions
//-----------------------------------------------------------------------------

//This is the spawn function for the bot.
function AIPlayer::spawn(%obj, %isRespawn)
{
   //Select what datablock this bot should use
   if (%obj.block $= "")
      %block = $AISK_CHAR_TYPE;
   else
      %block = %obj.block;

   //If we're using a random datablock, get the exact one now
   if (%block $= "-random")
      %block = randomDatablock(%obj, %block);

   //If the field is not blank, set the weapon variable to the spawn marker weapon,
   //if the marker is blank, then try datablock
   if (%obj.Weapon !$= "")
      %tempWeapon = %obj.Weapon;
   else if (%block.Weapon !$= "")
      %tempWeapon = %block.Weapon;
   else
      %tempWeapon = $AISK_WEAPON;

   //First try to use the value on the marker, then the datablock and then the default, this is for max range
   if (%obj.maxRange !$= "" && %obj.maxRange >= 1)
      %tempMaxRange = %obj.maxRange;
   else if (%block.maxRange !$= "" && %block.maxRange >= 1)
      %tempMaxRange = %block.maxRange;
   else
      %tempMaxRange = $AISK_MAX_DISTANCE;

   //First try to use the value on the marker, then the datablock and then the default, this is for sidestep
   if (%obj.sidestepDist !$= "")
      %tempSidestep = %obj.sidestepDist;
   else if (%block.sidestepDist !$= "")
      %tempSidestep = %block.sidestepDist;
   else
      %tempSidestep = $AISK_SIDESTEP;

   //This is for min range
   if (%obj.minRange !$= "" && (%tempMaxRange > %obj.minRange && %tempSidestep > %obj.minRange))
      %tempMinRange = %obj.minRange;
   else if (%block.minRange !$= "" && (%tempMaxRange > %block.minRange && %tempSidestep > %block.minRange))
      %tempMinRange = %block.minRange;
   else
      %tempMinRange = $AISK_MIN_DISTANCE;

   //This is for detect distance
   if (%obj.distDetect !$= "" && %obj.distDetect >= 1)
      %tempDistDetect = %obj.distDetect;
   else if (%block.distDetect !$= "" && %block.distDetect >= 1)
      %tempDistDetect = %block.distDetect;
   else
      %tempDistDetect = $AISK_DETECT_DISTANCE;

   //This is for max pace distance
   if (%obj.paceDist !$= "")
      %tempPaceDist = %obj.paceDist;
   else if (%block.paceDist !$= "")
      %tempPaceDist = %block.paceDist;
   else
      %tempPaceDist = $AISK_MAX_PACE;

   //This is for the bots behavior
   if (isObject(%obj.behavior))
      %tempBehavior = %obj.behavior;
   else if (isObject(%block.behavior))
      %tempBehavior = %block.behavior;
   else
      %tempBehavior = $AISK_BEHAVIOR;

   //This is for respawn
   if (%obj.respawn !$= "")
      %tempRespawn = %obj.respawn;
   else if (%block.respawn !$= "")
      %tempRespawn = %block.respawn;
   else
      %tempRespawn = $AISK_DEFAULT_RESPAWN;

   //This is for active dodging
   if (%obj.activeDodge !$= "")
      %tempDodge = %obj.activeDodge;
   else if (%block.activeDodge !$= "")
      %tempDodge = %block.activeDodge;
   else
      %tempDodge = $AISK_ACTIVE_DODGE;

   //This is for active dodging
   if (%obj.advancedDodge !$= "")
      %tempAdvanced = %obj.advancedDodge;
   else if (%block.advancedDodge !$= "")
      %tempAdvanced = %block.advancedDodge;
   else
      %tempAdvanced = $AISK_ADVANCED_DODGE;

   //This is npc's actions
   if (%obj.npcAction !$= "")
      %tempNpcAction = %obj.npcAction;
   else
      %tempNpcAction = %block.npcAction;

   //This is for fov (field of view)
   if (%obj.fov !$= "")
      %tempFOV = %obj.fov;
   else if (%block.fov !$= "")
      %tempFOV = %block.fov;
   else
      %tempFOV = $AISK_FOV;

   //This is for leash distance
   if (%obj.leash !$= "")
      %tempLeash = %obj.leash;
   else if (%block.leash !$= "")
      %tempLeash = %block.leash;
   else
      %tempLeash = $AISK_LEASH_DISTANCE;

   //This is for teams
   if (%obj.team !$= "")
      %tempTeam = %obj.team;
   else if (%block.team !$= "")
      %tempTeam = %block.team;
   else
      %tempTeam = $AISK_TEAM;

   //This is for the weapon cycle counter
   if (%obj.cycleCounter !$= "")
      %tempCycleCounter = %obj.cycleCounter;
   else if (%block.cycleCounter !$= "")
      %tempCycleCounter = %block.cycleCounter;
   else
      %tempCycleCounter = $AISK_CYCLE_COUNTER;

   //This is for weapon mode
   if (%obj.weaponMode !$= "")
      %tempWeaponMode = %obj.weaponMode;
   else if (%block.weaponMode !$= "")
      %tempWeaponMode = %block.weaponMode;
   else
      %tempWeaponMode = $AISK_WEAPON_MODE;

   //This is for the bot's name
   if (%obj.realName !$= "")
      %tempRealName = %obj.realName;
   else if (%block.realName !$= "")
      %tempRealName = %block.realName;
   else
      %tempRealName = $AISK_REAL_NAME;

   //Set a unique name for internal use
   if (%tempRealName !$= "")
      %internalName = getWord(%tempRealName, 0);
   else
      %internalName = "BotIs" @ allBotsGroup.getCount() + 1;

   if (isObject(%internalName))
      %internalName = "BotNum" @ getRandom(100, 9999);

    //The bot's respawn count is set on to the marker on the original spawn only
    if (!%isRespawn)
    {
        if (%obj.countRespawn !$= "")
            %obj.respawnCount = %obj.countRespawn;
        else if (%block.countRespawn !$= "")
            %obj.respawnCount = %block.countRespawn;
        else
            %obj.respawnCount = $AISK_RESPAWN_COUNT;

        strlwr(%obj.respawnCount);

        //Respawn is set to random so get a number for it now
        if (%obj.respawnCount $= "x")
        {
            %rand = getRandom(1, $AISK_RESPAWN_RANDOM);
            %obj.respawnCount = %rand;
        }

        %obj.respawnCounter = %obj.respawnCount;
    }

   //Create the demo player object
   %player = new AIPlayer(%internalName) {
       //Sets the bot's dataBlock
       dataBlock = %block;
       //The marker is the static object that the bot is associated with.
       //The marker object is save on the bot because it's location, and
       //dynamic variable values are used in several functions.
       marker = %obj;
       //Sets the bot's field of vision
       fov = %tempFOV;
       OldFOV = %tempFOV;
       //Sets the bot's leash distance
       leash = %tempLeash;
       //Sets what team the bot is on
       team = %tempTeam;
       //Sets the bot's detect distance
       detDis = %tempDistDetect;
       OldDetDis = %tempDistDetect;
       //Sets the bot's sidestep
       stepDis = %tempSidestep;
       //Sets the bot's max pacing distance
       maxPace = %tempPaceDist;
       //Sets the bot's returning position
       returningPos = %obj.getposition();
       //Sets the bot not to return to a path as soon as it is loaded
       //The pathed bots will go to there paths at another point
       returningPath = 0;
       //Fix for premature firing
       fireLater = 0;
       //Sets the bot's pacing
       pace = getRandom(1, $AISK_PACE_TIME);
       //The pathname variable is a dynamic variable set during map editing.
       //This allows the designer to attach each bot to a seperate path
       path = %obj.pathname;
       //Is the bots max range
       weapRange = %tempMaxRange;
       //Is the bots min range
       rangeMin = %tempMinRange;
       //Sets the bots behavior
       behavior = %tempBehavior;
       //Sets whether the bot is AI or not
       isbot = true;
       //Thinking variables
       //Firing tells whether or not we're in the midst of a firing sequence.
       firing = false;
       //The 'action' variable holds the state of the bot - which controls how it thinks.
       action = "Guarding";
       holdcnt = $AISK_HOLDCNT_MAX-1;
       //The bots starting attention level is set to half of it's maximum.
       attentionlevel = $AISK_MAX_ATTENTION/2;
       //Oldpos holds the position of the bot at the end of it's last 'think' cycle
       //This is used to help determine if a bot is stuck or not.
       oldpos = %obj.getposition();
       //Added for bots use different weapons
       botWeapon = %tempWeapon;
       //Set the number of the weapon the bot is currently using
       currentWeaponIs = 0;
       //Set the number of the weapon cycle the bot is currently using
       currentCycleNumber = 0;
       //Should the bot respawn or not
       respawn = %tempRespawn;
       //Should the bot actively dodge attacks
       activeDodge = %tempDodge;
       //In what manner should the be dodge
       advancedDodge = %tempAdvanced;
       //What action should this take if it's an npc
       npcAction = %tempNpcAction;
       //How often should the bot switch to another weapon
       cycleCounter = %tempCycleCounter;
       //Should it switch its weapon randomly or in a pattern
       weaponMode = %tempWeaponMode;
       //This is the bot's name, but it isn't set as the object's name in case it's non-unique
       realName = %tempRealName;
       //Max range must be this number or higher
       maxIgnore = 1;
   };

    allBotsGroup.add(%player);

    //Set this marker as having this bot
    %obj.botBelongsToMe = %player;

    //Sets the bot's initial position to that of it's marker.
    %player.setTransform(%obj.getTransform());

    //Set the bot's scale based on the marker or datablock's scale
    if (%obj.getScale() !$= "1 1 1")
        %player.setScale(%obj.getScale());
    else if (%block.scale !$= "")
        %player.setScale(%block.scale);

    //Make sure any "X" values are lower case
    strlwr(%player.cycleCounter);
    strlwr(%player.activeDodge);

    //If the first cycle counter is set to random, get a number for it now
    if (getword(%obj.cycleCounter, 0) $= "x")
    {
        %rand = getRandom(1, $AISK_RAND_CYCLE_MAX);
        //How much longer until the bot changes weapons
        %player.currentCycleCount = %rand;
    }
    else
        %player.currentCycleCount = getword(%player.cycleCounter, 0);

    //Set the serpentine movment to the side first
    if (%player.advancedDodge $= "Serpentine")
        %player.dodgeCounter = true;

    //Min range must be this number or lower
    %ignoreMinimum = 10000;

    //Cycle through all the weapons
    for (%e = 0; %e < getWordCount(%player.botWeapon); %e++)
    {
        %weap = getWord(%player.botWeapon, %e);

       //UAISK+AFX Interop Changes: Start
        if (isObject(%weap) && %weap.getClassName() !$= $AISK_AFX_DATA_TYPE)
        {
            //Get the bot's longest weapon ignore distance
            %maxIgn = %weap.ignoreDistance;

            if (%maxIgn $= "" || %maxIgn <= 1)
                %maxIgn = $AISK_IGNORE_DISTANCE;

            if (%player.maxIgnore < %maxIgn)
                %player.maxIgnore = %maxIgn;

            //Get the bot's shortest weapon ignore distance
            %minIgn = %weap.minIgnoreDistance;

            if (%minIgn $= "")
                %minIgn = $AISK_MIN_IGNORE_DISTANCE;

            if (%ignoreMinimum > %minIgn)
                %ignoreMinimum = %minIgn;
        }
        else
        {
            //Get the bot's longest weapon ignore distance
            %maxIgn = %weap.range;

            if (%maxIgn $= "" || %maxIgn <= 1)
                %maxIgn = $AISK_IGNORE_DISTANCE;

            if (%player.maxIgnore < %maxIgn)
                %player.maxIgnore = %maxIgn;

            //Get the bot's shortest weapon ignore distance
            %minIgn = %weap.areaDamageRadius;

            if (%minIgn $= "" || ($AISK_FRIENDLY_FIRE == false && $AISK_FREE_FOR_ALL == false))
                %minIgn = $AISK_MIN_IGNORE_DISTANCE;

            if (%ignoreMinimum > %minIgn)
                %ignoreMinimum = %minIgn;
        }
       //UAISK+AFX Interop Changes: End

       //equipBot is called to set the bots inventory, if it has a weapon
       if (%weap !$= "-noweapon")
           %player.equipBot(%player, %weap, %e);
    }

    //Make sure all min and max values are properly set
    safeguardMinMax(%player, %ignoreMinimum);

    //Sort the bot's weapons by quality if needed
    if (%player.weaponMode $= "best")
        sortBestWeapons(%player);

    //This mounts the weapon on the bot.
    equipBotWeapon(%player);

    //Put the bot in a SimSet with its teammates
    TeamSimSets(%player, %player.team);

    //Randomize a bit so that bots spawned at the same time don't "think" all at once
    %randThink = getRandom(0, 99);

    //Sets the bot to begin thinking after waiting the length of $AISK_CREATION_DELAY
    if (%player.behavior.isAggressive)
       %player.ailoop = %player.schedule($AISK_CREATION_DELAY + %randThink, "Think", %player);
    else
       %player.ailoop = %player.schedule($AISK_CREATION_DELAY + %randThink, "npcThink", %player);

    //If the bot is pathed, have it go on its path soon
    if (%player.path !$= "" && %player.behavior.canMove && !%player.behavior.isFollowPlayer)
    {
       %player.action = "Returning";
       %player.oldpos = "0 0 0";
       %player.ailoop = %player.schedule($AISK_CREATION_DELAY + %randThink, "followPath", %player.path);
    }
    else
       %player.path = "";

    return %player;
}

//Get a random datablock
function randomDatablock(%obj, %block, %weapNum)
{
    if (%block $= "-random")
    {
        //Pick a random datablock and return it
        %l = playerRandomizer.getRandom();
        return %l.getName();
    }
    else
    {
        //Pick a random weapon and set it as the bot's weapon
        %k = weaponRandomizer.getRandom();
        %obj.botWeapon = setWord(%obj.botWeapon, %weapNum, %k.getName());
    }
}

//Make sure all min and max values are properly set
function safeguardMinMax(%obj, %ignoreMinimum)
{
    //Avoid dead zones where the bot could move to but couldn't attack from
    //Make sure the bot's max distance is the same or less than the max range on its longest weapon
    if (%obj.weapRange > %obj.maxIgnore)
    {
        %obj.weapRange = %obj.maxIgnore;

        if ($AISK_SHOW_NAME $= "Debug")
            warn("Bot ID " @ %obj @ " has max attack distance higher than max weapon distance.");
    }

    //Avoid dead zones where the bot could move to but couldn't attack from
    if (%obj.maxIgnore > %ignoreMinimum)
    {
        //Make sure the bot's min distance is the same or greater than the min range on its shortest weapon
        if (%obj.rangeMin < %ignoreMinimum)
        {
            %obj.rangeMin = %ignoreMinimum;

            if ($AISK_SHOW_NAME $= "Debug")
                warn("Bot ID " @ %obj @ " has min weapon distance higher than min attack distance.");
        }
    }

    //If the pace min is less than 1, it can conflict with the general movement min distance
    if ($AISK_MIN_PACE < 1)
    {
        $AISK_MIN_PACE = 1;

        if ($AISK_SHOW_NAME $= "Debug")
            warn("$AISK_MIN_PACE is set to less than 1.");
    }

    //Make sure min pace is less than max, if set to pace
    if ($AISK_MIN_PACE >= %obj.maxPace && %obj.maxPace > 0)
    {
        %obj.maxPace = $AISK_MIN_PACE + 1;

        if ($AISK_SHOW_NAME $= "Debug")
            warn("Bot ID " @ %obj @ " has min PACE higher than max.");
    }

    //Max sidestep should be 1 or higher
    if (%obj.stepDis < 1)
    {
        %obj.stepDis = 1;

        if ($AISK_SHOW_NAME $= "Debug")
            warn("Bot ID " @ %obj @ " has max SIDESTEP too low.");
    }

    //Make sure min sidestep is less than max
    if (%obj.rangeMin >= %obj.stepDis)
    {
        %obj.stepDis = %obj.rangeMin + 1;

        if ($AISK_SHOW_NAME $= "Debug")
            warn("Bot ID " @ %obj @ " has min SIDESTEP higher than max.");
    }
}


//UAISK+AFX Interop Changes: Start
//-----------------------------------------------------------------------------
//Corpse Spell Functions
//-----------------------------------------------------------------------------

function AIPlayer::burnCorpse(%this, %corpse)
{
    if (!isObject(%corpse))
    {
        error("AIPlayer::burnCorpse() -- missing corpse object.");
        return;
    }

    if (%corpse.isEnabled())
    {
        error("AIPlayer::burnCorpse() -- corpse object is still alive!");
        return;
    }

    //Set the bot to not respawn
    %corpse.respawn = false;
    cancel(%corpse.marker.delayRespawn);
    %corpse.marker.delayRespawn = "";

    //Get rid of the body
    %corpse.schedule(0, "startFade", 1000, 0, true);
    %corpse.schedule(2000, "delete");
}

function AIPlayer::resurrectCorpse(%this, %corpse)
{
    if (!isObject(%corpse))
    {
        error("AIPlayer::resurrectCorpse() -- missing corpse object.");
        return;
    }

    if (%corpse.isEnabled())
    {
        error("AIPlayer::resurrectCorpse() -- corpse object is still alive!");
        return;
    }

    //Set the bot to not respawn
    %corpse.respawn = false;
    cancel(%corpse.marker.delayRespawn);
    %corpse.marker.delayRespawn = "";

    %corpse.setDamageLevel(%corpse.getDatablock().maxDamage * 0.5);
    //Default dynamic armor stats
    %corpse.setRechargeRate(%corpse.getDatablock().rechargeRate);
    %corpse.setRepairRate(%corpse.getDatablock().repairRate);

    //Sets the bot to begin thinking after waiting the length of $AISK_CREATION_DELAY
    if (%corpse.behavior.isAggressive)
        %corpse.schedule($AISK_CREATION_DELAY, "Think", %corpse);
    else
        %corpse.schedule($AISK_CREATION_DELAY, "npcThink", %corpse);

    //If the bot is pathed, have it go on its path soon
    if (%corpse.path !$= "" && %corpse.behavior.canMove && !%corpse.behavior.isFollowPlayer)
    {
        %corpse.action = "Returning";
        %corpse.oldpos = "0 0 0";
        %corpse.schedule($AISK_CREATION_DELAY, "followPath", %corpse.path);
    }
    else
        %corpse.path = "";

    schedule(1000, 0, afxBroadcastTargetStatusbarReset);
}
//UAISK+AFX Interop Changes: End
