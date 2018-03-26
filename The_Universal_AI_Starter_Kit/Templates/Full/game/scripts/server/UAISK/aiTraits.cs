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
//Leashed Functions
//-----------------------------------------------------------------------------

//Checks where a leashed bot can move to
function AIPlayer::yesLeashed(%this, %obj)
{
    //Get the object the bot is leashed to
    eval("%leashPos = " @ %obj.behavior.leashedTo @ ";");

    //Make sure it's a valid object and get a new player if needed
    if (!isObject(%leashPos))
    {
        if (%obj.behavior.isFollowPlayer)
            %this.yesFollowPlayer(%obj);

        return false;
    }

    %leashPos = %leashPos.getposition();

    %dist = vectorDist(%this.moveDestinationA, %leashPos);

    //See if it's at the end of its leash
    if (%dist > %obj.leash)
    {
        %dist2 = vectorDist(%obj.getAimLocation(), %obj.getPosition());

        //See if its target is within its detect distance
        if (%dist2 < %obj.detDis)
        {
            %dist3 = vectorDist(%leashPos, %obj.getPosition());

            //See if it could get any closer to its target while still being
            //inside of its leash area
            if (%dist3 + 1.0 < %obj.leash && %obj.action $= "Attacking")
            {
                %this.getCloserToTarget(%obj, %leashPos);
                return true;
            }
            //It's too far and needs to go back
            else if (%dist3 > %obj.leash)
            {
                if (!%obj.behavior.isFollowPlayer)
                    %obj.clearaim();

                %this.moveDestinationA = %leashPos;
                return true;
            }
            //Just right, so stop
            else
                return false;
        }
        else
        {
            if (!%obj.behavior.isFollowPlayer)
                %obj.clearaim();

            //If the target is too far, return to where it should
            %this.moveDestinationA = %leashPos;
            return true;
        }
    }
    else
    {
        %dist4 = vectorDist(%this.moveDestinationA, %obj.getPosition());

        if (%dist4 < 1.0)
            return false;
        else
            return true;
    }
}

//A basic check to see if a leashed bot can move to a position
function AIPlayer::testLeashed(%this, %obj, %newLoc)
{
    eval("%leashPos = " @ %obj.behavior.leashedTo @ ";");

    if (!isObject(%leashPos))
    {
        if (%obj.behavior.isFollowPlayer)
            %this.yesFollowPlayer(%obj);

        return false;
    }

    %leashPos = %leashPos.getposition();

    %dist = vectorDist(%newLoc, %leashPos);
    //If it's already outside its leash range, go ahead and just let it move
    //since it may be trying to get back within its leash range 
    %dist2 = vectorDist(%obj.getPosition(), %leashPos);

    //See if it's at the end of its leash
    if (%dist > %obj.leash && %dist2 <= %obj.leash)
        return false;
    else
        return true;
}

//Get the closest position to the player that's still within the bot's leash range
function AIPlayer::getCloserToTarget(%this, %obj, %leashPos)
{
    //Get some numbers to work with
    %aimPos = %obj.getAimLocation();
    %k = %obj.leash / vectorDist(%leashPos, %aimPos);

    //Do some math
    %x1 = getWord(%aimPos, 0);
    %x2 = getWord(%leashPos, 0);

    %x3 = %x1 - %x2;
    %x3 = %k * %x3;
    %x3 = %x1 - %x3;

    %y1 = getWord(%aimPos, 1);
    %y2 = getWord(%leashPos, 1);

    %y3 = %y1 - %y2;
    %y3 = %k * %y3;
    %y3 = %y1 - %y3;

    %z1 = getWord(%aimPos, 2);
    %z2 = getWord(%leashPos, 2);

    if (%z1 > %z2)
        %z3 = %z1;
    else
        %z3 = %z2;

    //Set the new position
    %obj.moveDestinationA = %x3 SPC %y3 SPC %z3;
}


//-----------------------------------------------------------------------------
//Assisting Functions
//-----------------------------------------------------------------------------

//See if any bots should go help the injured player/bot
function checkAboutAssisting(%obj)
{
    //Have other bots assist the injured if needed
    switch($AISK_CAN_ASSIST)
    {
        //Only have bots assist the player or bots following the player
        case 1:
            if (%obj.getClassName() $= "Player")
                AIPlayer::yesGoAssist(%obj);
            else if (%obj.behavior.isFollowPlayer)
                AIPlayer::yesGoAssist(%obj);

        //Only have bots assist other bots
        case 2:
            if (%obj.getClassName() $= "AIPlayer")
                AIPlayer::yesGoAssist(%obj);

        //Have everything assist everything
        case 3:
            AIPlayer::yesGoAssist(%obj);
    }
}

//See what bots should go help the injured player/bot
function AIPlayer::yesGoAssist(%injured)
{
    //Experimental: Assist on finding a valid target.
    //%pos = %injured.returningPos;

    //Get some values from the injured
    %pos = %injured.getPosition();
    %teamSet = "TeamSet" @ %injured.team;
    %count = %teamSet.getCount();

    //Cycle through all of the correct team members
    for (%j = 0; %j < %count; %j++)
    {
        %obj = %teamSet.getObject(%j);

        //Make sure this isn't a player or the bot that just got hurt
        if (%obj.getClassName() $= "AIPlayer" && %obj != %injured && isObject(%obj))
        {
            //Check if the bot can assist
            if (%obj.behavior.doesAssist)
            {
                //Make sure the bot isn't already in combat
                if (%obj.action $= "Guarding" || %obj.action $= "Returning" || %obj.action $= "Holding")
                {
                    //See if the bot is close enough to the injured
                    %basedist = vectorDist(%obj.getPosition(), %pos);

                    if (%basedist <= (%obj.detDis * $AISK_ASSIST_DISTANCE))
                    {
                        //Check if the bot has line of sight
                        if (%obj.checkMovementLos(%obj, %obj.getEyeTransform(), %pos) == 0)
                            %obj.goingToAssistNow(%pos);
                        else
                            %obj.checkThirdPosition(%obj, %pos);
                    }
                }
            }
        }
    }
}

//Check to see if a different path to the injured exists
function AIPlayer::checkThirdPosition(%this, %obj, %pos)
{
    //The "45" below is the angle
    %thirdPointA = %this.triangleBasedAviodance(%obj, 45, %pos);

    if (%this.checkMovementLos(%obj, %obj.getEyeTransform(), %thirdPointA) == 0)
    {
        if (%this.checkMovementLos(%obj, vectorAdd(%thirdPointA, $AISK_CHAR_HEIGHT), %pos) == 0)
            %obj.goingToAssistNow(%pos);
    }
    else
    {
        %thirdPointB = %this.triangleBasedAviodance(%obj, -45, %pos);

        if (%this.checkMovementLos(%obj, %obj.getEyeTransform(), %thirdPointB) == 0)
            if (%this.checkMovementLos(%obj, vectorAdd(%thirdPointB, $AISK_CHAR_HEIGHT), %pos) == 0)
                %obj.goingToAssistNow(%pos);
    }
}

//We found a bot that should go assist
function AIPlayer::goingToAssistNow(%obj, %pos)
{
    if (%obj.specialMove)
        return;

    //Set assisting values
    %obj.assisting = 1;
    %obj.assistPos = %pos;

    //Have the bot look around and stay alert
    %obj.enhancefov(%obj);
    %obj.attentionlevel = 1;

    if (%obj.behavior.isAggressive)
        %obj.ailoop = %obj.schedule($AISK_QUICK_THINK, "Think", %obj);
    else
        %obj.ailoop = %obj.schedule($AISK_QUICK_THINK, "npcThink", %obj);

    if (%obj.path !$= "")
    {
        %obj.oldPath = %obj.returningPos;
        %obj.path = "";
    }

    //Move the bot to the injured
    %obj.returningPos = %pos;
    %obj.moveDestinationA = %pos;
    %obj.movementPositionFilter(%obj);
    %obj.setAimLocation(%pos);
}


//-----------------------------------------------------------------------------
//Other Functions
//-----------------------------------------------------------------------------

//Follow the player if the behavior isFollowPlayer and the bot is idle
function AIPlayer::yesFollowPlayer(%this, %obj)
{
    if (%obj.path !$= "")
        return;

    //See if a new player is needed or if we can just follow the current one
    if (!isObject(%obj.master) || %obj.master.getstate() $= "Dead")
    {
        //Get the nearest valid player on the bot's team
        //No LOS checks are done with this
        %obj.master = %this.teammateCheck(%obj);
    }

    //Make sure the bot has a valid player to follow
    if (%obj.master > 0)
    {
        %masterPos = %obj.master.getPosition();
		%pos = %obj.getposition();
        %basedist = vectorDist(%pos, %masterPos);

        //The bot is too far from the player
        if (%basedist > (%obj.leash * 0.67))
        {
            //Have them think faster to help catch up to you
            %this.attentionlevel = 1;
            %obj.setMoveSpeed(1.0);
            %obj.returningPos = %masterPos;
            %this.moveDestinationA = %masterPos;

            //If the bot hasn't moved more than 1 unit it's probably stuck
            if (vectorDist(%pos, %obj.oldpos) < 1.0)
                %obj.isLost++;
            else
                %obj.isLost = 0;

            %obj.oldpos = %pos;

            //Sidestep if the bot is really stuck
            if (%obj.isLost >= 3) //This 3 probably shouldn't be hard coded
                %this.sidestep(%obj);
            else
                %this.movementPositionFilter(%obj);

            %obj.setAimObject(%obj.master);
            %obj.movingTowardsPlayer = 1;
        }
        else
        {
            //If the bot is still moving towards the player, have it stop
            if (%obj.movingTowardsPlayer)
            {
                %this.stop();
                %obj.movingTowardsPlayer = 0;
            }
            //If it already stopped, then pace
            else
            {
                %obj.returningPos = %obj.getposition();
                %this.pacing(%obj);
            }
        }
    }

    //Keep the bot's attention level down
    if (%this.attentionlevel > $AISK_MAX_ATTENTION/2)
        %this.attentionlevel = $AISK_MAX_ATTENTION/2;
}

//Check if this bot can move
function AIPlayer::noCanMove(%this, %obj)
{
    if (!%obj.behavior.canMove || %obj.isCasting)
    {
        %obj.action = "Guarding";
        %obj.clearaim();

        if (%obj.behavior.isAggressive)
            %this.ailoop = %this.schedule($AISK_SCAN_TIME * %obj.attentionlevel, "Think", %obj);
        else
            %this.ailoop = %this.schedule($AISK_SCAN_TIME * %obj.attentionlevel, "npcThink", %obj);

        return true;
    }
    else
        return false;
}
