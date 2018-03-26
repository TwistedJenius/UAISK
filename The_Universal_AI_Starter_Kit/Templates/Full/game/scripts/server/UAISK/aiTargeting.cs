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
//Target Selection Functions
//-----------------------------------------------------------------------------

//This function gets the best valid target for the bot and sets its attention level
function AIPlayer::GetClosestHumanInSightandRange(%this, %obj)
{
    //Set the initial values to -1.
    %dist = -1;
    %dist2 = -1;
    %health = -1;
    %index = -1;
    %index2 = -1;
    //The bots current position
    %botpos = %this.getposition();

    //This for loop cycles through all possible teams
    for (%j = 1; %j <= $TotalTeams; %j++)
    {
        //If the bot is on this team, don't check it for targets
        if (%obj.team != %j || $AISK_FREE_FOR_ALL == true)
        {
            //Get the name of the SimSet the bot is checking
            %teamSet = "TeamSet" @ %j;

            //The number of things to check
            %count = %teamSet.getCount();

            //This for loop cycles through all possible targets
            for (%i = 0; %i < %count; %i++)
            {
                //Get the target
                %tgt = %teamSet.getobject(%i);
                %namedClass = %tgt.getClassName();

                //If the target is invalid then the function bails out returning a -1 value
                if (%tgt != %obj && isObject(%tgt) && ((%tgt.behavior.isKillable && %namedClass $= "AIPlayer")
                    || %namedClass $= "Player") && %tgt.getstate() !$= "Dead")
                {
                    //Determine the distance from the bot to the target
                    %tempdist = vectorDist(%tgt.getposition(), %botpos);

                    //The first test we perform is to see if the target is within the bots range
                    //Is target in range? If not bail out of checking to see if its in view.
                    if (%tempdist <= %obj.detDis)
                    {
                        //The second check is to see if the target is within the FOV of the bot.
                        //Is the target within the fov field of vision of the bot?
                        if (%this.IsTargetInView(%obj, %tgt, %obj.fov))
                        {
                            //The third check we run is to see if there is anything blocking the target from the bot.
                            if (%this.CheckLOS(%obj, %tgt))
                            {
                                //Subtract the damage from the total health to get current health of the target
                                %tempHealth = %tgt.getDatablock().maxDamage - %tgt.getdamagelevel();

                                //If %health still equals -1, then this is our first valid target
                                //Set all values to be based off of our first valid target
                                if (%health == -1)
                                {
                                    %health = %tempHealth;
                                    %dist = %tempdist;
                                    %index = %tgt;
                                    %obj.returningPos = %tgt.getposition();
                                }
                                //Check this target has less health left than the last target did
                                else if (%tempHealth + $AISK_HEALTH_IGNORE < %health)
                                {
                                    %health = %tempHealth;
                                    %dist = %tempdist;
                                    %index = %tgt;
                                    %obj.returningPos = %tgt.getposition();
                                }
                                //Check the distance to the new target as compared to the current set target.
                                //If the new target is closer, then set the index and distance to the new target.
                                //Also make sure that the reason the previous else if got pasted up was because
                                //the health was inside the ignore value
                                else if (%tempdist < %dist && %tempHealth - %health < $AISK_HEALTH_IGNORE)
                                {
                                    %health = %tempHealth;
                                    %dist = %tempdist;
                                    %index = %tgt;
                                    %obj.returningPos = %tgt.getposition();
                                }

                                //Experimental: Assist on finding a valid target.
                                //Only call the assist function when a new target is found
                                //if (%index != %obj.oldTarget)
                                    //checkAboutAssisting(%obj);
                            }
                        }
                    }

                    //Get the closest valid target
                    if (%tempdist < %dist2 || %dist2 == -1)
                    {
                        %dist2 = %tempdist;
                        %index2 = %tgt;
                    }
                }
            }
        }
    }

    if (%index2 > 0)
        %this.adjustAttentionLevel(%obj, %index2, %botpos);
    else
        %this.attentionlevel = $AISK_MAX_ATTENTION;

    if (%obj.lostest == 1 && (!isObject(%obj.oldTarget) || %obj.oldTarget.getstate() $= "Dead"))
    {
        //Experimental: Assist on finding a valid target.
        //%obj.oldTarget = 0;

        //If 2 or more bots just killed the player, we don't want them all standing exactly
        //where the player died. So we have them stay where they are.
        if (!%obj.behavior.returnToMarker)
        {
            %obj.returningPos = %obj.getposition();
            %obj.lostest = 0;

            if (%obj.behavior.isFollowPlayer)
                %this.yesFollowPlayer(%obj);
        }
    }

    //If the bot had sight of a target lasst think cycle, and now it can't find a target,
    //get its last target's current position
    if (%index == -1)
    {
        if (%obj.lostest == 1)
        {
            %obj.lostest = 0;

            //Make sure at least one target is somewhere near the bot before chasing
            if (%dist2 <= %obj.detDis * $AISK_ATTENTION_RANGE)
                %this.schedule($AISK_LOS_TIME, "getnewguardposition", %obj);
        }
    }
    else
        %obj.oldTarget = %index;

    return %index;
}

//This function is for finding the nearest player by non aggressive bots.
//It does not take teams or FOV into account.
function AIPlayer::GetClosestPlayerInSightandRange(%this, %obj)
{
    %dist = -1;
    %dist2 = -1;
    //Set the initial index value to -1. The index is the nearest target found.
    %index = -1;
    %index2 = -1;
    //The bots current position
    %botpos = %this.getposition();
    //The number clients to check
    %count = ClientGroup.getCount();

    //This for loop cycles through all clients
    for (%i = 0; %i < %count; %i++)
    {
        //Get the target
        %tgt = ClientGroup.getobject(%i);
        %tgt = %tgt.player;

        //If the target is invalid then the function bails out returning a -1 value
        if (isObject(%tgt) && %tgt.getstate() !$= "Dead")
        {
            //Determine the distance from the bot to the target
            %tempdist = vectorDist(%tgt.getposition(), %botpos);

            //The first test we perform is to see if the target is within the bots range
            //Is target in range? If not bail out of checking to see if its in view.
            if (%tempdist <= %obj.detDis)
            {
                //We don't do a FOV check for non aggressive bots
                //The second check we run is to see if there is anything blocking the target from the bot.
                if (%this.CheckLOS(%obj, %tgt))
                {
                    //If there is a current target, then check the distance to the new target as
                    //compared to the current set target. If the new target is closest, then set
                    //the index and tempdistance to the new target.
                    if (%tempdist < %dist || %dist == -1)
                    {
                        %dist = %tempdist;
                        %index = %tgt;

                        if (%obj.behavior.isFollowPlayer)
                            %obj.returningPos = %tgt.getposition();
                    }
                }
            }

            //Get the closest valid target
            if (%tempdist < %dist2 || %dist2 == -1)
            {
                %dist2 = %tempdist;
                %index2 = %tgt;
            }
        }
    }

    if (%index2 > 0)
        %this.adjustAttentionLevel(%obj, %index2, %botpos);
    else
        %this.attentionlevel = $AISK_MAX_ATTENTION/2;

    //If 2 or more bots just killed the player, we don't want them all standing exactly
    //where the player died. So we have them stay where they are.
    if (%obj.lostest == 1 && %obj.behavior.isFollowPlayer && (!isObject(%obj.oldTarget) || %obj.oldTarget.getstate() $= "Dead"))
    {
        %obj.returningPos = %obj.getposition();
        %obj.lostest = 0;
    }

    //If the bot had sight of a target lasst think cycle, and now it can't find a target,
    //get its last target's current position
    if (%index == -1)
    {
        if (%obj.lostest == 1)
        {
            %obj.lostest = 0;
            %this.schedule($AISK_LOS_TIME, "getnewguardposition", %obj);
        }
    }
    else
        %obj.oldTarget = %index;

    return %index;
}

//Get the position of the closest player on the bot's team
function AIPlayer::teammateCheck(%this, %obj)
{
    %dist = -1;
    //Set the initial index value to -1. The index is the nearest target found.
    %index = -1;
    //The bots current position
    %botpos = %this.getposition();
    %count = ClientGroup.getCount();

    //This for loop cycles through all clients
    for (%i = 0; %i < %count; %i++)
    {
        //Get the target
        %tgt = ClientGroup.getobject(%i);
        %tgt = %tgt.player;

        //If the target is invalid then the function bails out returning a -1 value
        if (isObject(%tgt) && %tgt.team == %obj.team && %tgt.getstate() !$= "Dead" && %tgt.getClassName() $= "Player")
        {
            //Determine the distance from the bot to the target
            %tempdist = vectorDist(%tgt.getposition(), %botpos);

            //Get the closest valid target
            if (%tempdist < %dist || %dist == -1)
            {
                %dist = %tempdist;
                %index = %tgt;
            }
        }
    }

    //If no valid target were found, try changing the bot's team
    if (%index <= 0)
    {
        %obj.loopCounter++;

        //Make sure it doesn't get stuck in this loop
        if (%obj.loopCounter <= $TotalTeams)
        {
            if (%obj.team < $TotalTeams)
                %team = %obj.team + 1;
            else
                %team = 1;

            changeTeams(%obj, %team);
            %this.teammateCheck(%obj);
            return;
        }
        else if ($AISK_SHOW_NAME $= "Debug")
            warn("Bot ID " @ %obj @ " has checked all teams.");
    }

    %obj.loopCounter = 0;
    return %index;
}

//Item gathering has been commented out because it does not work properly
/*
//This function gets the closest valid item
function AIPlayer::GetClosestItemInSightandRange(%this, %obj, %itemname)
{
    %dist = -1;
    %index = -1;
    %botpos = %this.getposition();

    InitContainerRadiusSearch(%botpos, $AISK_DETECT_ITEM_RANGE, $TypeMasks::ItemObjectType);

    while ((%item = containerSearchNext()) != 0)
    {
        if (%item.getDataBlock().getName() $= %itemname)
        {
            %itempos = %item.getposition();
            %tempdist = vectorDist(%itempos, %botpos);

            if (%this.IsTargetInView(%obj, %item, %obj.fov))
            {
                if (%this.CheckLOStoItem(%obj, %item))
                {
                    if (%tempdist < %dist || %dist == -1)
                    {
                        %dist = %tempdist;
                        %index = %item;
                    }
                }
            }
        }
    }

    return %index;
}
*/

//Adjust the bot's attention level based on its distance from the closest valid target
//This will make the bot think more or less often
function AIPlayer::adjustAttentionLevel(%this, %obj, %index2, %botpos)
{
    //Determine the distance from the bot to the target
    %tempdist = vectorDist(%index2.getposition(), %botpos);

    if (%tempdist <= %obj.detDis)
    {
        //Since the target is close, make sure the current attention is no greater than half the max
        if (%this.attentionlevel > $AISK_MAX_ATTENTION/2)
            %this.attentionlevel = $AISK_MAX_ATTENTION/2;
        //Lower attentionlevel to increase response time if it's currently 2 or more
        else if (%this.attentionlevel >= 2)
            %this.attentionlevel--;
        else
            %this.attentionlevel = 1;
    }
    //Check if the target is more than X times the bots detect distance
    else if (%tempdist >= %obj.detDis * $AISK_ATTENTION_RANGE)
    {
        //This will slow down how often the bot thinks and checks for threats
        if (%this.attentionlevel <= $AISK_MAX_ATTENTION - 0.5)
            %this.attentionlevel = %this.attentionlevel + 0.5;
    }
    else
    {
        //If the target isn't extremely far or close, raise the attention level upto half of the max
        if (%this.attentionlevel <= $AISK_MAX_ATTENTION/2 - 0.5)
            %this.attentionlevel = %this.attentionlevel + 0.5;
        else
            %this.attentionlevel = $AISK_MAX_ATTENTION/2;
    }
}

//Get the player's position a short time after sight is lost
function AIPlayer::GetNewGuardPosition(%this, %obj)
{
    %obj.returningPos = %obj.oldTarget.getposition();
}


//-----------------------------------------------------------------------------
//Vision Functions
//-----------------------------------------------------------------------------

//It checks to see if there are any static objects blocking the view from the AIPlayer to the target.
function AIPlayer::CheckLOS(%this, %obj, %tgt)
{
    %foundObject = %this.checkMovementLos(%obj, %obj.getEyeTransform(), %tgt.getPosition());

    if (%foundObject == 0)
    {
        if (%obj.behavior.isAggressive || (!%obj.behavior.isAggressive && %obj.behavior.isFollowPlayer))
            %obj.lostest = 1;

        return true;
    }
    else
        return false;
}

//Item gathering has been commented out because it does not work properly
/*
//This function checks the line of sight to an item
function AIPlayer::CheckLOStoItem(%this, %obj, %item)
{
    %foundObject = %this.checkMovementLos(%obj, %obj.getEyeTransform(), %item.getWorldBoxCenter());

    if (%foundObject == %item)
        return true;
    else
        return false;
}
*/

//This function checks to see if the target supplied is within the bots FOV
function AIPlayer::IsTargetInView(%this, %obj, %tgt, %fov)
{
    //No need to check the rest if the bot can see everything
    if (%fov >= 360)
        return true;

    %ang = %this.check2dangletotarget(%obj, %tgt);
    %visleft = 360 - (%fov/2);
    %visright = %fov/2;

    if (%ang > %visleft || %ang < %visright)
        return true;
    else
        return false;
}

//This function gets the angle to a target
function AIPlayer::check2DAngletoTarget(%this, %obj, %tgt)
{
    %eyeVec = VectorNormalize(%obj.getEyeVector());
    %eyeangle = %this.getAngleofVector(%eyeVec);
    %posVec = VectorSub(%tgt.getPosition(), %obj.getPosition());
    %posangle = %this.getAngleofVector(%posVec);
    %angle = %posangle - %eyeAngle;
    %angle = %angle ? %angle : %angle * -1;

    if (%angle < 0)
        %angle += 360;

    return %angle;
}

//Return the angle of a vector in relation to world origin
function AIPlayer::getAngleofVector(%this, %vec)
{
    %vector = VectorNormalize(%vec);
    %vecx = getWord(%vector, 0);
    %vecy = getWord(%vector, 1);

    if (%vecx >= 0 && %vecy >= 0)
        %quad = 1;
    else
        if (%vecx >= 0 && %vecy < 0)
            %quad = 2;
        else
            if (%vecx < 0 && %vecy < 0)
                %quad = 3;
            else 
                %quad = 4;

    %angle = mATan(%vecy/%vecx, -1);
    %degangle = mRadToDeg(%angle);

    switch(%quad)
    {
      case 1:
        %angle = %degangle - 90;
      case 2:
        %angle = %degangle + 270;
      case 3:
        %angle = %degangle + 90;
      case 4:
        %angle = %degangle + 450;
    }

    if (%angle < 0)
        %angle += 360;

    return %angle;
}

//The EnhanceFOV function temporarily gives the bot a 360 degree field of vision
//This is used to emulate the bot looking around at different times. Namely when 'Holding'.
function AIPlayer::EnhanceFOV(%this, %obj)
{
    //Is the botFOV already 360 degrees? If not then we'll set it, and set the schedule to
    //turn it back off.
    if (%obj.fov < 360)
    {
        //Sets the field of vision to 360 deg.
        %obj.fov = 360;
        //Starts the timer to disable the enhanced FOV
        %obj.enhanced = %this.schedule($AISK_ENHANCED_FOV_TIME, "restorefov", %obj);
    }
}

//Enhances the defending bot's FOV and detect distance after being hit.
function AIPlayer::EnhanceDefending(%this, %obj)
{
    //Make sure enhanced defending is better than normal
    if (%obj.detDis < $AISK_ENHANCED_DEFENDING_DISTANCE * %obj.OldDetDis)
    {
        //Set the bots detect distance to be enhanced
        %obj.detDis = $AISK_ENHANCED_DEFENDING_DISTANCE * %obj.OldDetDis;
        %obj.enhanced = %this.schedule($AISK_ENHANCED_DEFENDING_TIME, "restoreDefending", %obj);
    }

    if (%obj.fov < 360)
    {
        //Set field of veiw
        %obj.fov = 360;
        %obj.enhanced = %this.schedule($AISK_ENHANCED_DEFENDING_TIME, "restorefov", %obj);
    }
}

//Restore FOV sets the bot's FOV back to it's regular default setting.
function AIPlayer::restoreFOV(%this, %obj)
{
    %obj.fov = %obj.OldFOV;
    %obj.enhanced = 0;
}

//Restores the defending bot's detect distance.
function AIPlayer::restoreDefending(%this, %obj)
{
    %obj.detDis = %obj.OldDetDis;
    %obj.enhanced = 0;
}
