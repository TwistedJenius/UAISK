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


//The bot uses a state machine to control it's actions. The bot's default actions are as follows:
//Guarding:         When guarding the bot paces, scans for new targets, and when one is found it switches to 'Attacking'
//Attacking:        The bot tries to close with the target while firing and checking for target updates.
//Holding:          When the bot loses a target it will go into a holding pattern. While holding the bot's FOV is
//                          enhanced. The bot holds for a set number of cycles before changing it's action state to 'Returning'
//Returning:        The bot tries to return to it's original position. While returning the bot looks for new targets
//                          and checks it motion relative to it's last movement to determine if it is stuck.


//The 'Think' function is the brains of the bot. The bot performs certain actions based on what its current 'action' state is.
//The bot thinks on a scheduled basis. How fast the bot 'thinks' is determined by the bots attention level and its default scan time.
function AIPlayer::Think(%this, %obj)
{
    //This cancels the current schedule - just to make sure that things are kept neat and tidy.
    cancel(%this.ailoop);

    //If the bot is dead, then there's no need to think or do anything. So let's bail out.
    if (!isObject(%obj) || %obj.getstate() $= "Dead")
        return;

    //The bot is doing something special right now, so don't interrupt it
    if (%obj.specialMove)
        return;

//Item gathering has been commented out because it does not work properly
/*
    if (%obj.action !$= "RetrievingItem")
    {
        //Check if the bot should try to get some health
        if (%obj.getDamagePercent() > $AISK_SEEK_HEALTH_LVL)
        {
            %prevaction = %obj.action;
            %this.enhancefov(%obj);
            %hlth = %this.getclosestiteminsightandrange(%obj, "HealthPatch");

            if (%hlth > 0)
                %obj.action = "GetHealth";
        }
        //Check if the bot should try to get some ammo
        else if ($AISK_ENDLESS_AMMO == false)
        {
            //Get the name of the weapon
            %tempWeapon = getWord(%obj.botWeapon, %obj.currentWeaponIs);

            if ((%tempWeapon.usesAmmo == true || (%tempWeapon.usesAmmo $= "" && $AISK_WEAPON_USES_AMMO == true))
            && %tempWeapon !$= "-noweapon")
            {
                if (%obj.getInventory(%tempWeapon.image.ammo) == 0)
                {
                    %prevaction = %obj.action;
                    %this.enhancefov(%obj);
                    %ammostr = %tempWeapon.image.ammo;
                    %i_ammo = %this.getclosestiteminsightandrange(%obj, %ammostr);

                    if (%i_ammo > 0)
                        %obj.action = "GetAmmo";
                }
            }
        }
    }
*/


    //The switch$ takes the value of the bots action variable and then chooses what code to run
    //according to what value it is.
    switch$(%obj.action)
    {
     //This is the bots default position. While guarding the bot will do 3 things by default.
     //The first is that the bot will run a random check to see if it can enhance it's fov.
     //The second thing the bot does is to check for nearby targets. If found the bot goes into attack mode.
     //The third is to pace back and fourth around the spot they're guarding.
     //If the bot is in the "teammate" behavior mode, it will try to go back to a player.
     case "Guarding":
         if (%obj.enhanced <= 0)
            %obj.fov = %obj.OldFOV;

        %obj.fireLater = 0;
        %obj.lostest = 0;

        if (!%obj.behavior.isFollowPlayer)
            %obj.isLost = 0;

        //Check if there's a target in sight and range, then attack if so.
        if (%this.attackTransition(%obj))
            return;

        //If the bot is a teammate, have it follow the player
        if (%obj.behavior.isFollowPlayer)
            %this.yesFollowPlayer(%obj);
        else
            %this.pacing(%obj);

        //There are no targets so continue guarding and call the scheduler to have the bot think
        //at it's regular interval
        %this.ailoop = %this.schedule($AISK_SCAN_TIME * %obj.attentionlevel, "Think", %obj);

    //The bot has been told that there is a target in sight and range and is set to attack it.
    //While attacking the bot's attention level is kept at its lowest value (quickest thinking).
    //The bot looks for the nearest target in sight. If the target is found the bot will aim at the
    //target, set it's move destination to the position of the target, and then openfire on the target.
    case "Attacking":
        //Set the bot's move speed back to normal
        %obj.setMoveSpeed(1.0);
        %obj.isLost = 0;
        %obj.assisting = 0;
        %obj.nextNode = 0;

        //Do an npc action if needed, this code can be uncommented if you wish
        //if (%obj.npcAction > 0)
            //%this.doingNpcAction(%obj);

        //Get the id of the nearest valid target
        %tgt = %this.GetClosestHumanInSightandRange(%obj);
        //Maintain a low attention value to keep the bot thinking quickly while attacking.
        %obj.attentionlevel = 1;

        //Check if there is a valid target
        if (%tgt > 0)
        {
            //Set the bot to aim at the target.
            if (!%obj.notAim)
                %obj.setAimObject(%tgt, $AISK_CHAR_HEIGHT);

            %dest = %tgt.getposition();
            %basedist = vectorDist(%obj.getposition(), %dest);

            //Check if the bot is already close enough to the target or needs to be closer
            if (%basedist > %obj.weapRange)
            {
                %this.moveDestinationA = %dest;
                %this.movementPositionFilter(%obj);
            }
            //If the bots not too far, check if its too close
            else if (%basedist < %obj.rangeMin)
            {
                %this.sidestep(%obj);

                if (!%obj.notAim)
                    %obj.setAimObject(%tgt, $AISK_CHAR_HEIGHT);
            }
            //The bot isn't too close or too far from its target
            else
            {
                //Since the bot is within the proper range, just have it stop where it is if it's not sidestepping
                if (%obj.activeDodge <= 0)
                    %this.stop();
                else if (%obj.dodgeTimer > 0)
                    %obj.dodgeTimer--;
                else
                {
                    //If dodge is set to random, get a number for it now
                    if (%obj.activeDodge $= "x")
                    {
                        %rand = getRandom(1, $AISK_RAND_DODGE_MAX);
                        //How much longer until the bot dodges
                        %obj.dodgeTimer = %rand;
                    }
                    else
                        %obj.dodgeTimer = %obj.activeDodge;

                    %this.sidestep(%obj, true);

                    if (!%obj.notAim)
                        %obj.setAimObject(%tgt, $AISK_CHAR_HEIGHT);
                }
            }

            if (%obj.behavior.isLeashed)
            {
                %this.moveDestinationA = %obj.getPosition();
                %this.movementPositionFilter(%obj);
            }

            //Change weapons if needed
            if (%obj.currentCycleCount <= 0)
                %this.weaponChange(%obj, %basedist);

            //Tells the bot to start shooting the target
            %obj.openFire(%obj, %tgt, %basedist);

            //Tells the scheduler to have us think again 
            %this.ailoop = %this.schedule($AISK_SCAN_TIME * %obj.attentionlevel, "Think", %obj);
            %this.displayBotNames(%obj);
            return;
        }

        //Stop looking at the old target
        if (%obj.getAimObject() > 0)
        {
            if (%obj.getAimObject().team == %obj.team)
                %obj.clearaim();
            else
            {
                %aimPosition = %obj.getAimObject().getPosition();
                %basedist = vectorDist(%obj.getposition(), %aimPosition);

                if (%basedist > %obj.detDis)
                    %obj.setAimLocation(vectorAdd(%aimPosition, $AISK_CHAR_HEIGHT));
            }
        }
        else
            %obj.clearaim();

        if (!%obj.behavior.returnToMarker && %obj.path !$= "")
            %obj.path = "";

        //Clear the firing variable
        %obj.firing = false;
        //Clear holdcnt
        %obj.holdcnt = 0;
        //Set our action to 'Holding'
        %obj.action = "Holding";
        //Again this sets the scheduler to have us think quickly to have the bot
        //react to the loss of it's attack target
        %this.ailoop = %this.schedule($AISK_QUICK_THINK, "Think", %obj);

    //When a bot loses it's target, or when the bot reaches it's destination as the result of
    //a sidestep the bot will go into a 'hold'. During a hold the bot will have enhanced
    //FOV (to emulate scanning around for targets.) The bot will look for targets in range and
    //attack if found. If no target is found the bot will increase it's holdcnt by 1. When the
    //bot reaches its maximum holdcnt value it will attempt to return to its return position.
    case "Holding":
        //Set the bot's move speed back to normal
        %obj.setMoveSpeed(1.0);

        //Check if there's a target in sight and range, then attack if so.
        if (%this.attackTransition(%obj))
            return;

        //There was no target found, so we need to have the bot continue to 'hold'
        //for a little bit before doing anything else.

        //Increase the holdcnt variable by one
        %obj.holdcnt++;
        %obj.fireLater = 0;

        if (%obj.behavior.isFollowPlayer)
            %this.yesFollowPlayer(%obj);

        //Check to see if we've passed our threshold of waiting
        if (%obj.holdcnt > $AISK_HOLDCNT_MAX)
        {
            //Set holdcnt back to 0 for the next time we need it.
            %obj.holdcnt = 0;
            %obj.isLost++;

            //Set the bot to return to where it last saw the player if it's not pathed
            if (%obj.path $= "")
            {
                //Reset returning positions for guard bots
                if (%obj.behavior.returnToMarker)
                {
                    if (%obj.oldPath $= "")
                        %obj.returningPos = %obj.marker.getposition();
                    else
                        %obj.returningPos = %obj.oldPath;
                }

                %this.moveDestinationA = %obj.returningPos;
                %this.movementPositionFilter(%obj);
            }
            //Set the bot to return to its path since it is pathed
            else
            {
               if (%obj.returningPath != 0)
               {
                  if (%obj.behavior.returnToMarker)
                      AIPlayer::followPath(%obj, %obj.path);
               }
               else
                  %obj.returningPath = 1;
            }

            if (%obj.behavior.isFollowPlayer)
                %obj.action = "Guarding";
            else
                %obj.action = "Returning";

            //Sets the bot's oldpos to the position it's returning to. This is done
            //due to the fact that we've been holding and our position hasn't been
            //changing. So we want to be sure that the bot doesn't think that it's
            //stuck as soon as it tries to return.
            %obj.oldpos = %obj.returningPos;
            //We've waited long enough, so let's think quickly and go into 'Return' mode
            %this.ailoop = %this.schedule($AISK_QUICK_THINK, "Think", %obj);
        }
        else
        {
            %this.moveDestinationA = %obj.returningPos;
            %basedist = vectorDist(%obj.getposition(), %this.moveDestinationA);

            //Check if the bot is already close enough to its return position
            if (%basedist > 1.0)
                %this.movementPositionFilter(%obj);

            %this.ailoop = %this.schedule($AISK_SCAN_TIME * %obj.attentionlevel, "Think", %obj);
        }

    //In Return mode the bot will do the following: It looks for the nearest target in sight and will attack it.
    //If no target is found, the bot is still in the process of returning so we check to see if the bot is stuck.
    //Stuck in the case means that the bot has moved a distance of less than 1 unit since the last time it thought.
    //If the bot is stuck, sidestep is called to have the bot try to move in a different direction.
    case "Returning":
         if (%obj.enhanced <= 0)
            %obj.fov = %obj.OldFOV;

        //Check if there's a target in sight and range, then attack if so.
        if (%this.attackTransition(%obj))
            return;

        if (%this.noCanMove(%obj))
            return;

        //There was no target so we're still returning. So now check for a pathed or stuck bot
        //This gets a value depicting the distance from the bots last known move point
        %movedist = vectorDist(%obj.getposition(), %obj.oldpos);

        //If the bot hasn't moved more than 1 unit we're probably stuck.
        //Remember - this is only checked for while returning - not guarding
        if (%movedist < 1.0 && !%obj.nextNode)
        {
            //Set our holdcnt to 1 less than the maximum so we only hold for 1 cycle
            %obj.holdcnt = $AISK_HOLDCNT_MAX - 1;

            if (%obj.path !$= "")
            {
                if (%obj.currentNode < %obj.path.getCount() - 1)
                    %node = %obj.path.getObject(%obj.currentNode + 1);
                else
                    %node = %obj.path.getObject(0);

                if (%this.checkMovementLos(%obj, %obj.getposition(), %node.getposition()) == 0)
                    %this.moveToNextNode(%obj);
                else
                    %this.sidestep(%obj);
            }
            else
                %this.sidestep(%obj);
        }
        else
            %obj.clearaim();

        //See if the bot seems to be stuck, and if so then have it stop moving
        if (%obj.isLost >= $AISK_LOOP_COUNTER)
        {
            if ($AISK_SHOW_NAME $= "Debug")
                warn("Bot ID " @ %obj @ " was unable to get to its intended destination.");

            if (!%obj.behavior.returnToMarker)
            {
                %obj.returningPos = %obj.getposition();
                %obj.action = "Guarding";
            }
        }

        //Set our oldpos to match our current position so that next time we cycle through
        //we'll know if we're going anywhere or not
        %obj.oldpos = %obj.getposition();
        //Scedhule ourselves to think at our regular interval
        %this.ailoop = %this.schedule($AISK_SCAN_TIME * %obj.attentionlevel, "Think", %obj);

//Item gathering has been commented out because it does not work properly
/*
    case "GetHealth":
        %hlth= %this.getclosestiteminsightandrange(%obj, "HealthPatch");

        if (%hlth > 0)
        {
            %obj.action = "RetrievingItem";
            %dest = %hlth.getposition();
            %this.moveDestinationA = %dest;
            %this.movementPositionFilter(%obj);

            %this.enhancefov(%obj);
        }
        else
            %obj.action = %prevaction;

        %this.ailoop = %this.schedule($AISK_QUICK_THINK, "Think", %obj);

    case "GetAmmo":
        %ammostr = %tempWeapon.image.ammo;
        %i_ammo = %this.getclosestiteminsightandrange(%obj, %ammostr);

        if (%i_ammo > 0)
        {
            %obj.action = "RetrievingItem";
            %dest = %i_ammo.getposition();
            %this.moveDestinationA = %dest;
            %this.movementPositionFilter(%obj);

            %this.enhancefov(%obj);
        }
        else
            %obj.action = %prevaction;

        %this.ailoop = %this.schedule($AISK_QUICK_THINK, "Think", %obj);

    case "RetrievingItem":
        %obj.setMoveSpeed(1.0);
        %tgt = %this.GetClosestHumanInSightandRange(%obj);

        if (%tgt > 0)
        {
            %basedist = vectorDist(%obj.getposition(), %tgt.getposition());
            %obj.attentionlevel = 1;
            %obj.setAimObject(%tgt, $AISK_CHAR_HEIGHT);
            %obj.openfire(%obj, %tgt, %basedist);
        }
        else
        {
            %obj.firing = false;
            %movedist = vectorDist(%obj.getposition(), %obj.oldpos);

            if (%movedist < 1.0)
            {
                %obj.holdcnt = $AISK_HOLDCNT_MAX - 1;
                %this.sidestep(%obj);
            }
            else
                %obj.clearaim();
        }

        %obj.oldpos = %obj.getposition();
        %this.ailoop = %this.schedule($AISK_SCAN_TIME * %obj.attentionlevel, "Think", %obj);
*/

    default:
        %obj.action = "Holding";
        %this.ailoop = %this.schedule($AISK_QUICK_THINK, "Think", %obj);
    }

    %this.displayBotNames(%obj);
}

function AIPlayer::attackTransition(%this, %obj)
{
    //if holding, enhance the bot's FOV
    if (%obj.action $= "Holding")
        %this.enhancefov(%obj);
    else if ($AISK_ENHANCED_FOV_CHANCE >= 1)
    {
        //The bot will enhance it's FOV if it picks a 1 from a range of 1 to $AISK_ENHANCED_FOV_CHANCE
        %chance = getRandom(1, $AISK_ENHANCED_FOV_CHANCE);

        if (%chance == 1)
            %this.enhancefov(%obj);
    }

    //The bot checks for the nearest valid target if any.
    %tgt = %this.GetClosestHumanInSightandRange(%obj);

    //Check if a target was found
    if (%tgt > 0)
    {
        //Set the bots action to 'Attacking'.
        %obj.action = "Attacking";
        //The bots thinking is sped up to enable the bot to react more quickly as seems appropriate.
        %this.ailoop = %this.schedule($AISK_QUICK_THINK, "Think", %obj);
        %this.displayBotNames(%obj);
        return true;
    }
    //If there was no target, stay in the same state
    else
        return false;
}

//This is the thinking cycle for non aggressive bots.
function AIPlayer::npcThink(%this, %obj)
{
    //This cancels the current schedule - just to make sure that things are kept neat and tidy.
    cancel(%this.ailoop);

    //If the bot is dead, then there's no need to think or do anything. So let's bail out.
    if (%obj.getstate() $= "Dead" || !isObject(%obj))
        return;

    if (%obj.behavior.isFollowPlayer)
        %this.yesFollowPlayer(%obj);
    else
        %this.pacing(%obj);

    //Do an npc action if needed, this code is in the main think cycle as well but commented out
    if (%obj.npcAction > 0)
        %this.doingNpcAction(%obj);

    %this.displayBotNames(%obj);

    //Schedule this function to loop through again after a certain period of time.
    %this.ailoop = %this.schedule($AISK_SCAN_TIME * %obj.attentionlevel, "npcThink", %obj);
}

//Set the bot's name as needed
function AIPlayer::displayBotNames(%this, %obj)
{
    switch$($AISK_SHOW_NAME)
    {
        case "DontShow":
            %obj.setshapename("");

        case "Show":
            %obj.setshapename(%obj.realName);

        case "Debug":
            //Sets the hud above the bot to show it's current- action : attention level : damage : ID
            %objname = %obj.action @ " : " @ %this.attentionlevel @ " : " @ %obj.getDamagePercent() * 100 @ "% : " @ %obj;
            %obj.setshapename(%objname);

        default:
            %obj.setshapename("");
    }
}

function AIPlayer::nonStateDefending(%this, %obj, %source)
{
    //If the bot got sniped, enhance its vision
    if (%obj.action !$= "Attacking" && %obj.getstate() !$= "Dead")
    {
        %obj.enhancedefending(%obj);
        %obj.attentionlevel = 1;
    }

    //Set the bot's move speed back to normal
    %obj.setMoveSpeed(1.0);
    %obj.isLost = 0;
    %obj.assisting = 0;
    %obj.nextNode = 0;

    //Set the bot to it's highest awareness
    %obj.attentionlevel = 1;
    //Set the hldcnt to 1 less than the max
    %obj.holdcnt = $AISK_HOLDCNT_MAX - 1;

    //Sidestep to a random position
    %this.sidestep(%obj, true);

    //Set our action to 'Holding'
    %obj.action = "Holding";
    //Set a quick think schedule to start us looking for targets quickly.
    %this.ailoop = %this.schedule($AISK_QUICK_THINK, "Think", %obj);
}


//-----------------------------------------------------------------------------
//Special Move Functions
//-----------------------------------------------------------------------------

//Have the bot completely stop all other actions and just do what you want
function AIPlayer::doSpecialMove(%obj, %time)
{
    if (!isObject(%obj) || %obj.getstate() $= "Dead")
        return;

    //Make sure we have a time
    if (%time < 1)
        %time = 1000;

    //Make sure the bot doesn't think due to damage or touch
    %obj.specialMove = true;

    //Cancel any old specials and start a new one
    cancel(%obj.specialTimer);
    %obj.specialTimer = schedule(%time, %obj, "clearSpecial", %obj);

    //Cancel the bot's think cycle
    cancel(%obj.ailoop);
    %obj.stop();
    %obj.clearaim();
}

//Allow the bot to start thinking again now that it's done with its special move
function clearSpecial(%obj)
{
    if (!isObject(%obj) || %obj.getstate() $= "Dead")
        return;

    cancel(%obj.specialTimer);

    //In case the bot's aim was set as part of the special move
    %obj.clearaim();
    %obj.specialMove = false;

    //Make sure the bot doesn't act odd if it was attacking when the special started
    %obj.fireLater = 0; 
    %obj.lostest = 0;

    //Continue on its path if needed
    if (%obj.path !$= "")
    {
        %obj.oldpos = "0 0 0";
        %obj.moveToNode(%obj.currentNode);
        //%obj.moveToNextNode();
    }

    //Start thinking again
    %obj.ailoop = %obj.schedule($AISK_QUICK_THINK, "Think", %obj);
}
