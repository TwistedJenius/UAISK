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
//Pace and Sidestep Functions
//-----------------------------------------------------------------------------

//Causes AIPlayer to slowly pace around their current location
function AIPlayer::Pacing(%this, %obj)
{
    //Don't pace if the bot is pathed
    if (%obj.path !$= "" || %obj.assisting)
        return;

    %obj.pace--;

    //Check if the bot has paced recently and if it should be pacing at all.
    if (%obj.pace <= 0 && (%obj.maxPace > 0 && $AISK_PACE_TIME > 0 && $AISK_PACE_SPEED > 0))
    {
        //If the bot needs to pace, get a random time (which is an amount of think cycles)
        //that the bot has before it paces again
        %obj.pace = getRandom(1, $AISK_PACE_TIME);

        //Multiple the random pace time by the bot's attention level.
        //This is so the bot always paces at the same rate no matter what its attention is.
        //The if/else and the line after can be commented out if you wish.
        if (%obj.attentionlevel != $AISK_MAX_ATTENTION)
            %tempAttention = $AISK_MAX_ATTENTION - %obj.attentionlevel;
        else
            %tempAttention = 1;

        %obj.pace = %obj.pace * %tempAttention;
    }
    else
        return;

    //Set the bots returning position to its marker if it's guarding
    if (%obj.behavior.returnToMarker && %obj.behavior.isAggressive)
    {
        if (%obj.oldPath $= "")
            %obj.returningPos = %obj.marker.getposition();
        else
            %obj.returningPos = %obj.oldPath;
    }

    //Skittish bots don't return back to their markers
    if (%obj.behavior.isSkittish && !%obj.behavior.returnToMarker)
        %basedist = 0;
    else
        %basedist = vectorDist(%obj.getposition(), %obj.returningPos);

    //If the bot is away from its returning position, go back to it so it doesn't wander too far away
    if (%basedist > $AISK_MIN_PACE)
        %newLoc = %obj.returningPos;
    else
    {
        //Get max and min pace distances
        %max = %obj.maxPace;
        %min = $AISK_MIN_PACE;
        %foundObject = 1;

        for (%i = 0; (%i < $AISK_LOOP_COUNTER && %foundObject > 0); %i++)
        {
            //Get proper random numbers
            %xrand = %this.constrainedRandom(%obj, %max, %min);
            %yrand = %this.constrainedRandom(%obj, %max, %min);
            //%zrand = %this.constrainedRandom(%obj, %max, %min);
            //Change them into a vector
            %randVector = %xrand SPC %yrand SPC 0; //%zrand;

            //Add or subtract the numbers from the bot's current position. Subtraction
            //is done by adding a negative. Adding versus subtracting is the difference
            //between left vs right, or forward vs backward
            %newLoc =  vectorAdd(%obj.getposition(), %randVector);

            //Test LOS of the new position
            %foundObject = %this.positionLosCheck(%obj, %newLoc);

            //If the bot is leashed, make sure it stays in the proper range
            if (%obj.behavior.isLeashed)
            {
               if (!%this.testLeashed(%obj, %newLoc))
                   %foundObject = 1;
            }
        }
    }

    if (%foundObject > 0)
    {
        if ($AISK_SHOW_NAME $= "Debug")
            warn("Bot ID " @ %obj @ " could not find a valid PACE location.");

        return;
    }

    //Set the bot to move at a different speed than normal while pacing
    %obj.setMoveSpeed($AISK_PACE_SPEED);
    //Set the bot to look in the direction that it is moving.
    %obj.setaimlocation(vectorAdd(%newloc, $AISK_CHAR_HEIGHT));
    //Set the bot to move towards the new position.
    if (%obj.behavior.canMove && !%obj.isCasting)
        %obj.setMoveDestination(%newLoc);
}

//Sidestep is used to find a random spot near the bot and attempt to have them move towards it.
//This function is only called in combat, including after being hit by an attack.
function AIPlayer::SideStep(%this, %obj, %isDodge)
{
    //If this is a turret return now because turrets don't move
    if (!%obj.behavior.canMove || %obj.isCasting)
        return;

    //Get max and min sidestep distances
    %max = %obj.stepDis;
    %min = %obj.rangeMin;
    %foundObject = 1;

    for (%i = 0; (%i < $AISK_LOOP_COUNTER && %foundObject > 0); %i++)
    {
        //Get proper random numbers
        %xrand = %this.constrainedRandom(%obj, %max, %min);
        %yrand = %this.constrainedRandom(%obj, %max, %min);
        //%zrand = %this.constrainedRandom(%obj, %max, %min);
        //Change them into a vector
        %randVector = %xrand SPC %yrand SPC 0; //%zrand;

        if (%isDodge && %obj.advancedDodge !$= "Random")
            %newLoc =  %this.advancedActiveDodge(%obj, %xrand);
        //Add or subtract the numbers from the bot's current position. Subtraction
        //is done by adding a negative. Adding versus subtracting is the difference
        //between left vs right, or forward vs backward
        else
            %newLoc =  vectorAdd(%obj.getposition(), %randVector);

        %foundObject = %this.positionLosCheck(%obj, %newLoc);

        //If the bot is leashed, make sure it stays in the proper range
        if (%obj.behavior.isLeashed)
        {
           if (!%this.testLeashed(%obj, %newLoc))
               %foundObject = 1;
        }
    }

    //Switch the bot from side to back, or the other way
    if (%isDodge && %obj.advancedDodge $= "Serpentine")
    {
        if (%obj.dodgeCounter)
            %obj.dodgeCounter = false;
        else
            %obj.dodgeCounter = true;
    }

    if (%foundObject > 0)
    {
        if ($AISK_SHOW_NAME $= "Debug")
            warn("Bot ID " @ %obj @ " could not find a valid SIDESTEP location.");

        return;
    }

    //If there's a target, keep aiming at it while sidestepping
    if (%obj.action $= "Returning")
        %obj.setaimlocation(vectorAdd(%newloc, $AISK_CHAR_HEIGHT));

    //If the bot is pathed, get ready to move to the correct node
    %this.returningPath = 2;

    //Set the bot to move towards the new position.
    if (!%obj.notAim)
        %obj.setMoveDestination(%newLoc);
}

//Get a random number that's constrained between min and max values
function AIPlayer::constrainedRandom(%this, %obj, %max, %min)
{
    //Get a random number that's 10 times what we want the final number to be
    %numRand = getRandom(%min * 10, %max * 10);

    //See if the number should be negative or not
	if(getRandom(0, 1))
		%numRand *= -1;

    //Divide the random number by 10 to get one decimal place
    %numRand = %numRand/10;
    return %numRand;
}

//Do a LOS test between the bot and a location
function AIPlayer::positionLosCheck(%this, %obj, %newLoc)
{
    //Line of sight test for the position the bot wants to pace to
    %eyeTrans = %obj.getEyeTransform();
    %eyeEnd = vectorAdd(%newLoc, $AISK_OBSTACLE);
    %searchResult = containerRayCast(%eyeTrans, %eyeEnd, $TypeMasks::PlayerObjectType |
        $TypeMasks::TerrainObjectType | $TypeMasks::ItemObjectType | $TypeMasks::StaticObjectType, %obj);
    %foundObject = getword(%searchResult, 0);

    return %foundObject;
}

//Have the bot move side to side or back and forth
function AIPlayer::advancedActiveDodge(%this, %obj, %rand)
{
    //Get the bot's position
    %x1 = getWord(%obj.getPosition(), 0);
    %y1 = getWord(%obj.getPosition(), 1);
    %z1 = getWord(%obj.getPosition(), 2);

    //Get the bot's rotation for the Z axis within a range of -120 to 240
    %rot = getWord(%obj.rotation, 2) * getWord(%obj.rotation, 3);

    //Side to side movement
    if (%obj.advancedDodge $= "Side" || (%obj.advancedDodge $= "Serpentine" && %obj.dodgeCounter))
    {
        %rot += 90;

        if (%rot > 360)
            %rot -= 360;
    }

    %rot = mDegToRad(%rot);

    //Do some math to get the destination point based on the angle and distance
    %x2 = mSin(%rot) * %rand;
    %y2 = mCos(%rot) * %rand;

    %x2 += %x1;
    %y2 += %y1;

    %pos = %x2 SPC %y2 SPC %z1;
    return %pos;
}


//-----------------------------------------------------------------------------
//Normal Movement Functions
//-----------------------------------------------------------------------------

//Check if the location the bot is moving to is in sight.
//And if it's not, move somwhere that is in sight (if there's a better place).
function AIPlayer::movementPositionFilter(%this, %obj)
{
    if (!%obj.behavior.canMove || %obj.notAim || %obj.isCasting)
        return;

    if (%obj.behavior.isLeashed)
    {
        if (!%this.yesLeashed(%obj))
            return;
    }

    //Save the original destination to another variable for later use
    %this.moveDestinationB = %this.moveDestinationA;
    %eyeTrans = %obj.getEyeTransform();

	//Use Walkabout pathfinding
    if ($AISK_WALKABOUT_ENABLE && %this.behavior.useWalkabout && isObject(%this.getNavMesh()))
        %this.setPathDestination(%this.moveDestinationB);
    //Do a simple test to see if the bot can go directly to its target destination,
    //or if it needs to do something fancy to go around an obstacle
    else if (%this.checkMovementLos(%obj, %eyeTrans, %this.moveDestinationB) == 0)
        %obj.setmovedestination(%this.moveDestinationB);
    else
    {
        //The "45" below is the angle
        %this.moveDestinationB = %this.triangleBasedAviodance(%obj, 45, %this.moveDestinationA);
        %thirdPointA = %this.moveDestinationB;

        if (%this.checkMovementLos(%obj, %eyeTrans, %this.moveDestinationB) == 0)
        {
            %this.moveDestinationB = %this.triangleBasedAviodance(%obj, -45, %this.moveDestinationA);
            %thirdPointB = %this.moveDestinationB;

            //Both ways are clear so check which one is better
            if (%this.checkMovementLos(%obj, %eyeTrans, %this.moveDestinationB) == 0)
            {
                %start = vectorAdd(%thirdPointA, $AISK_CHAR_HEIGHT);
                %foundObject = %this.checkMovementLos(%obj, %start, %this.moveDestinationA);

                if (%foundObject == 0)
                    %obj.setmovedestination(%thirdPointA);
                else
                    %obj.setmovedestination(%thirdPointB);
            }
            else
                %obj.setmovedestination(%thirdPointA);
        }
        else
        {
            %this.moveDestinationB = %this.triangleBasedAviodance(%obj, -45, %this.moveDestinationA);

            if (%this.checkMovementLos(%obj, %eyeTrans, %this.moveDestinationB) == 0)
                %obj.setmovedestination(%this.moveDestinationB);
            //Nothing is unblocked
            else
            {
                //Move in a random direction
                //%this.sidestep(%obj);
                //Or brute force our way to the original destination, possibly sliding along obstacles
                //but the sliding will likely occur out of sight of the player
                %obj.setmovedestination(%this.moveDestinationA);
            }
        }
    }

    %aimAt = %obj.getAimObject();

    if (%aimAt > 0)
    {
        %className = %aimAt.getClassName();

        if ((%className !$= "Player" && %className !$= "AIPlayer") || %aimAt.getstate() $= "Dead")
            %obj.setAimLocation(vectorAdd(%obj.getMoveDestination(), $AISK_CHAR_HEIGHT));
    }
}

//Go around an obstacle based on a triangle with two of the points
//being the bot's current position and its target position.
function AIPlayer::triangleBasedAviodance(%this, %obj, %angle, %tgtPos)
{
	//return %this.volumetricNavigation(%obj, %angle, %tgtPos);

    %botPos = %obj.getPosition();

    //Get the X and Y values for the start and end points
    %x1 = getWord(%botPos, 0);
    %x2 = getWord(%tgtPos, 0);

    %y1 = getWord(%botPos, 1);
    %y2 = getWord(%tgtPos, 1);

    %angle = mDegToRad(%angle);

    //Get the slope of each line
    %M1 = (%y2 - %y1) / (%x2 - %x1);
    %M2 = mTan(%angle + mAtan(%M1, 1));
    %M3 = -(1 / %M2);

    //Get the intercept
    %B2 = %y1 - (%x1 * %M2);
    %B3 = %y2 - (%x2 * %M3);

    //Get the position of the third point in our triangle
    %x3 = -((%B3 - %B2) / (%M3 - %M2));
    %y3 = %M2 * %x3 + %B2;

    if (getWord(%tgtPos, 2) >= getWord(%botPos, 2))
        %z3 = getWord(%tgtPos, 2);
    else
        %z3 = getWord(%botPos, 2);

    %correctPos = %x3 SPC %y3 SPC %z3;
/*
    //Draw the bot's path if we're debugging
    if ($AISK_SHOW_NAME $= "Debug")
    {
        if (%angle > 0)
            %color = "0 0 1";
        else
            %color = "0 1 0";

        %time = 15 * 1000;

        DebugDraw.drawLine(%botPos, %correctPos, %color);
        DebugDraw.setLastTTL(%time);

        DebugDraw.drawLine(%correctPos, %tgtPos, %color);
        DebugDraw.setLastTTL(%time);

        DebugDraw.drawLine(%botPos, %tgtPos, "1 0 0");
        DebugDraw.setLastTTL(%time);
    }
*/
    return %correctPos;
}

/*
//This is an experimental function to allow full 3D navigation
//Also be sure to uncomment the "%zrand" lines above
function AIPlayer::volumetricNavigation(%this, %obj, %angle, %tgtPos)
{
    %botPos = %obj.getPosition();

    //Between the X, Y and Z axis, see which 2 distances are the furthest
    %distA = vectorDist(getWord(%botPos, 0), getWord(%tgtPos, 0));
    %distB = vectorDist(getWord(%botPos, 1), getWord(%tgtPos, 1));
    %distC = vectorDist(getWord(%botPos, 2), getWord(%tgtPos, 2));

    //There's probably a better way to compare these values
    if (%distA > %distB)
    {
        %valueX = 0;

        if (%distB > %distC)
        {
            %valueY = 1;
            %valueZ = 2;
        }
        else
        {
            %valueY = 2;
            %valueZ = 1;
        }
    }
    else if (%distA > %distC)
    {
        %valueX = 0;
        %valueY = 1;
        %valueZ = 2;
    }
    else
    {
        %valueX = 1;
        %valueY = 2;
        %valueZ = 0;
    }

    //Get the X and Y values for the start and end points
    %x1 = getWord(%botPos, %valueX);
    %x2 = getWord(%tgtPos, %valueX);

    %y1 = getWord(%botPos, %valueY);
    %y2 = getWord(%tgtPos, %valueY);

    %angle = mDegToRad(%angle);

    //Get the slope of each line
    %M1 = (%y2 - %y1) / (%x2 - %x1);
    %M2 = mTan(%angle + mAtan(%M1, 1));
    %M3 = -(1 / %M2);

    //Get the intercept
    %B2 = %y1 - (%x1 * %M2);
    %B3 = %y2 - (%x2 * %M3);

    //Get the position of the third point in our triangle
    %x3 = -((%B3 - %B2) / (%M3 - %M2));
    %y3 = %M2 * %x3 + %B2;

    //This should probably be an average!!!
    if (getWord(%tgtPos, %valueZ) >= getWord(%botPos, %valueZ))
        %z3 = getWord(%tgtPos, %valueZ);
    else
        %z3 = getWord(%botPos, %valueZ);

    %correctPos = setWord(%correctPos, %valueX, %x3);
    %correctPos = setWord(%correctPos, %valueY, %y3);
    %correctPos = setWord(%correctPos, %valueZ, %z3);

    //Draw the bot's path if we're debugging
    if ($AISK_SHOW_NAME $= "Debug")
    {
        if (%angle > 0)
            %color = "0 0 1";
        else
            %color = "0 1 0";

        %time = 15 * 1000;

        DebugDraw.drawLine(%botPos, %correctPos, %color);
        DebugDraw.setLastTTL(%time);

        DebugDraw.drawLine(%correctPos, %tgtPos, %color);
        DebugDraw.setLastTTL(%time);

        DebugDraw.drawLine(%botPos, %tgtPos, "1 0 0");
        DebugDraw.setLastTTL(%time);
    }

    return %correctPos;
}
*/

//Line of sight test for the position the bot wants to move to
function AIPlayer::checkMovementLos(%this, %obj, %start, %end)
{
    %end = vectorAdd(%end, $AISK_CHAR_HEIGHT);

    //Note: Not including the terrain in the ray cast can lead to better results on terrain that has a lot of hills
    //which are high, yet not impassibly steep. But if you just delete "$TypeMasks::TerrainObjectType |" the terrain
    //will still be included by "$TypeMasks::StaticObjectType", so you have to exempt the terrain as well.
    %searchResult = containerRayCast(%start, %end, $TypeMasks::TerrainObjectType |
        $TypeMasks::ItemObjectType | $TypeMasks::StaticObjectType, %obj);

    %foundObject = getWord(%searchResult, 0);

    return %foundObject;
}

//Tell a bot to move to a specific location
function AIPlayer::explicitAIMovement(%obj, %pos)
{
    //If no bot or position was given, return
    if (%obj $= "" || %pos $= "")
        return;

    //Set the bot's values and aim
    %obj.moveDestinationA = %pos;
    %obj.returningPos = %pos;
    %obj.setAimLocation(vectorAdd(%pos, $AISK_CHAR_HEIGHT));
    %obj.action = "Holding";

    //Have the bot move
    %obj.movementPositionFilter(%obj);
}
