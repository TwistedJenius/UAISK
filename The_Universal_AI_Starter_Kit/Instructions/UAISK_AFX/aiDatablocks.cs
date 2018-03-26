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


//The AIPlayer marker is placed in the map while editing. When the map is loaded the
//marker is replaced by an AI player. The marker is hidden or not depending on
//the value set in $AISK_MARKER_HIDE.

datablock StaticShapeData(AIPlayerMarker)
{
   //Mission editor category, this datablock will show up in the
   //specified category under the "shapes" root category.
   category = "AIMarker";
   //Basic Item properties
   shapeFile = "art/shapes/players/OrcMage/OrcMage.dts"; //UAISK+AFX Interop Change
   isAiMarker = true;

   isInvincible = "1";
   maxDamage = "1";
   noIndividualDamage = "1";
   shadowCanAnimate = "0";
   shadowCanMove = "0";
   shadowEnable = "0";
};


//*******************************************************************************
/*
This is the default datablock for the bot. This was done to allow the creation of different
classes of bots with their own thinking and reaction routines for each class.

You can specify as many datablocks as you have characters. Each datablock needs its own
onReachDestination, onCollision, and damage functions. Copy and paste those function
and then change "AIPlayerDataHolder" in the name to the name of your new datablock.

The first variable after PlayerData must be a unique name. The second variable (after the
semicolon) must be a valid body type.
*/
//*******************************************************************************

//UAISK+AFX Interop Changes: Start
datablock PlayerData(AIPlayerDataHolder : OrcMageData)
{
   shapeFile = "art/shapes/players/OrcMage/OrcMage.dts";
//UAISK+AFX Interop Changes: End

   //All values can be commented out if you wish
   //If you get disable them, the bot will just use the same values as the player

   maxDamage = 100;
   maxForwardSpeed = 14;
   maxBackwardSpeed = 13;
   maxSideSpeed = 13;

   //AI specific values that can be set for this datablock
   //These values can be overridden by the spawn marker,
   //but these values override the defaults
   Weapon = "Ryder";
   respawn = true;
   behavior = "ChaseBehavior";
   maxRange = 25;
   minRange = 0;
   distDetect = 50;
   sidestepDist = 10;
   paceDist = 6;
   npcAction = 0;
   spawnGroup = 1;
   fov = 200;
   leash = 35;
   cycleCounter = "5";
   weaponMode = "pattern";
   activeDodge = 1;
   advancedDodge = "Random";
   team = 2;
   realName = "Bot";
   countRespawn = 0;
};


//***************************************************************************
//Trigger Controler
//This code handles the placing and behavior of the guardTrigger datablock
//***************************************************************************

datablock TriggerData(guardTrigger)
{
    tickPeriodMS = 125;
    spawnGroup = 1;
    moreThanOnce = false;
};

//This function does an action after the trigger is entered by a player
function guardTrigger::onEnterTrigger(%this, %trigger, %obj)
{
    //Check to see if a player triggered this
    if (%obj.getClassName() $= "Player")
    {
        //Don't spawn more bots if they aren't needed
        if (%this.moreThanOnce || !%trigger.doneOnce)
        {
            //Use the value on the trigger if it has one, or else use the trigger's datablock's value
            if (%trigger.spawnGroup > 0)
                %spawn = %trigger.spawnGroup;
            else
                %spawn = %this.spawnGroup;

            //We've now triggered this trigger once
            if (!%this.moreThanOnce)
                %trigger.doneOnce = true;

            //Spawn the group, this can be changed to another action if you want
            AIPlayer::actionByGroup(%spawn, "unspawned");
        }
    }
}

//You can use the "inactive" action when leaving a trigger to make
//a simple zone based spawning system.
//function guardTrigger::onLeaveTrigger(%this, %trigger, %obj)
