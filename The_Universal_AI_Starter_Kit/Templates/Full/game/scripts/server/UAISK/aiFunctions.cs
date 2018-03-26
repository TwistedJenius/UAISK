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
//Datablock Specific Function
//-----------------------------------------------------------------------------

//The onReachDestination function is responsible for setting the bot's 'action' state to the
//appropriate setting depending on what action the bot was following to reach the destination.
function PlayerData::onReachDestination(%this, %obj)
{
    if (%obj.isbot == false)
        return;

    //Picks an appropriate set of actions based on the bots 'action' variable
    switch$(%obj.action)
    {
    case "Guarding":
        %obj.clearaim();

        //If the bot is assisting, make sure it gets to its final destination
        if (%obj.assisting)
        {
            %basedist2 = vectorDist(%obj.getposition(), %obj.assistPos);

            if (%basedist2 < 1.0)
                %obj.assisting = 0;
            else
            {
                %obj.moveDestinationA = %obj.assistPos;
                %obj.movementPositionFilter(%obj);
            }
        }

    case "Returning":
        %obj.clearaim();

        //If the bot is pathed have it move to the next node on its path
        if (%obj.path !$= "")
        {
               //Check if the bot's guarding
               if (!%obj.behavior.returnToMarker)
               {
                    if (%obj.returningPos == %obj.marker.getposition())
                        %obj.moveToNextNode();
                    else
                        %obj.path = "";
               }
               else
                  %obj.moveToNextNode();
        }
        //The bot is not pathed
        else
        {
            if (%obj.behavior.returnToMarker)
            {
               if (%obj.oldPath $= "")
                   %basedist = vectorDist(%obj.getposition(), %obj.marker.getposition());
               else
                   %basedist = vectorDist(%obj.getposition(), %obj.oldPath);
            }
            else
               %basedist = vectorDist(%obj.getposition(), %obj.returningPos);

            //If the bot is close to his original position then set it's action to Guarding
            if (%basedist < 1.0)
            {
                //Set the bots returning position to its marker if it's guarding
                if (%obj.behavior.returnToMarker)
                {
                   if (%obj.oldPath $= "")
                       %obj.setTransform(%obj.marker.gettransform());
                   else
                       %obj.setTransform(%obj.oldPath);
                }

                %obj.action = "Guarding";
            }
            //If the bot is away from his post, then he must have gotten here
            //as a result of sidestepping so set him to do a quick hold to scan
            //for targets then return to post.
            else
            {
                //Sets holdcnt to 1 less than the max. Ensures that the bot only holds for 1 cycle
                //before trying to return.
                %obj.holdcnt = $AISK_HOLDCNT_MAX - 1;
                %obj.action = "Holding";
            }
        }

        //If the bot is assisting, make sure it gets to its final destination
        if (%obj.assisting)
        {
            %basedist2 = vectorDist(%obj.getposition(), %obj.assistPos);

            if (%basedist2 < 1.0)
                %obj.assisting = 0;
            else
            {
                %obj.moveDestinationA = %obj.assistPos;
                %obj.movementPositionFilter(%obj);
            }
        }

    //The bot was defending and sidestepped. So set it to 'hold' to check for targets
    //and to prepare to return to post if no targets are found.
    case "Defending":
        %obj.action = "Holding";

//Item gathering has been commented out because it does not work properly
/*
    case "RetrievingItem":
        %obj.holdcnt = $AISK_HOLDCNT_MAX - 1;
        %obj.action = "Holding";
*/
    }
}

function AIPlayerDataHolder::damage(%this, %obj, %sourceObject, %position, %damage, %damageType)
{
   if (!isObject(%obj) || %obj.getState() $= "Dead" || !%damage)
      return;

   //If friendly fire is turned off, and the source and target are on the same team, then return
   //NOTE: This "if" should also be added to the player's "damage" function too
   if ($AISK_FRIENDLY_FIRE == false && $AISK_FREE_FOR_ALL == false)
   {
        if (%sourceObject.team == %obj.team)
            return;
   }

   //If this is a bot, set its attention level
   if (%obj.isbot == true)
   {
        //Move a little when hit, aggressive bots move in the "Defending" state
        if (!%obj.behavior.isAggressive)
        {
            if ($AISK_WALKABOUT_ENABLE && %this.behavior.useWalkabout && %this.behavior.useCover && isObject(%this.getNavMesh()))
            {
                //Try to take cover. If that fails, just sidestep!
                if (!%this.findCover(%sourceObject.getPosition(), $AISK_WALKABOUT_COVER_RADIUS))
                    %this.sidestep(%this, true);
            }
            else
                %obj.sidestep(%obj, true);
        }
        else if (!%obj.specialMove)
        {
        //Item gathering has been commented out because it does not work properly
        //if (%obj.action !$= "GetHealth")
        //{
            %obj.nonStateDefending(%obj, %source);
        //}
        }

        //Don't hurt unkillable bots
        if (!%obj.behavior.isKillable)
            return;
   }

   //Have other bots assist the injured if needed
   checkAboutAssisting(%obj);

   if (%obj.getState() $= "Dead")
   {
         %marker = %obj.marker;

         //Check if the bots should still be respawning
         if (%marker.respawnCount > 0)
         {
            if (%marker.respawnCounter <= 0)
                %obj.respawn = false;

            %marker.respawnCounter--;
         }

         //Respawn the bot if needed
         if (%obj.respawn == true)
         {
            %marker.delayRespawn = schedule($AISK_RESPAWN_DELAY, %marker, "AIPlayer::spawn", %marker, true);
            %this.player = 0;
         }
         else
         {
            %marker.botBelongsToMe = "";
            %marker.respawnCount = "";
            %marker.respawnCounter = "";
         }
   }

   Parent::damage(%this, %obj, %sourceObject, %position, %damage, %damageType);
}

function AIPlayerDataHolder::onCollision(%this, %obj, %col)
{
   if (!isObject(%col) || %obj.getState() $= "Dead")
      return;

   //If this is a bot that collided with an enemy, face that enemy
   if (%col.team != %obj.team && %col.team !$= "" && !%obj.specialMove)
   {
        //If the bot is skittish, have it run away; or move if it's in the way
        if ((%obj.behavior.isSkittish) || (%obj.action !$= "Attacking" && ((%col.action $= "Attacking" ||
        %col.action $= "Holding") || (isObject(%obj.master) && %col == %obj.master))))
            %obj.sidestep(%obj);

        if (%obj.getAimObject() <= 0)
        {
            if (!%obj.behavior.isSkittish && !%obj.notAim)
                %obj.setAimObject(%col, $AISK_CHAR_HEIGHT);

            if (%obj.behavior.isAggressive)
               %obj.ailoop = %obj.schedule($AISK_QUICK_THINK, "Think", %obj);
        }
   }

   //Have NPCs think if bumped into
   if (!%obj.behavior.isAggressive)
         %obj.ailoop = %obj.schedule($AISK_QUICK_THINK, "npcThink", %obj);

   Parent::onCollision(%this, %obj, %col);
}
