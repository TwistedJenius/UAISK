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


//Note: UAISK+AFX Interop Changes are not marked in this
//file due to how extensive those changes are


//-----------------------------------------------------------------------------
//Weapon Equiping Functions
//-----------------------------------------------------------------------------

//This sets the bots beginning equipment and inventory
function AIPlayer::equipBot(%this, %obj, %tempWeapon, %weapNum)
{
    //See if the bot is using a random weapon
    if (%tempWeapon $= "-random")
    {
        randomDatablock(%obj, 0, %weapNum);
        %tempWeapon = getWord(%obj.botWeapon, %weapNum);
    }

    //Check if this is a spell or other weapon
    if (%tempWeapon.getClassName() !$= $AISK_AFX_DATA_TYPE)
    {
        //Make sure the bot is using its version of this weapon
        if (%tempWeapon.image.useEyeNode && isObject(%tempWeapon @ "Unset"))
        {
            %tempWeapon = %tempWeapon @ "Unset";
            %obj.botWeapon = setWord(%obj.botWeapon, %weapNum, %tempWeapon);
        }

        //Give the bot the ability to use this weapon if needed
        if (%obj.getDatablock().maxInv[%tempWeapon] < 1)
            %obj.getDatablock().maxInv[%tempWeapon] = 1;

        //This adds a weapon to the bots inventory
        %obj.setInventory(%tempWeapon, 1);

        //If the weapon doesn't have usesAmmo set, then use the default
        if (%tempWeapon.usesAmmo !$= "")
            %l = %tempWeapon.usesAmmo;
        else
            %l = $AISK_WEAPON_USES_AMMO;

        if (%l == true)
        {
            //Give the bot the ability to use this ammo if needed
            if (%obj.getDatablock().maxInv[%tempWeapon.image.ammo] < 1)
                %obj.getDatablock().maxInv[%tempWeapon.image.ammo] = 1000;

            if (%tempWeapon.weapStartAmmo)
                //This sets the bots beginning inventory of ammo.
                %obj.setInventory(%tempWeapon.image.ammo, %tempWeapon.weapStartAmmo);
            else
                %obj.setInventory(%tempWeapon.image.ammo, $AISK_STARTING_AMMO);
        }
    }
}

//Sort weapons based on their weapRating
function sortBestWeapons(%shape)
{
    //Get the total number of weapons
    %count = getWordCount(%shape.botWeapon);

    if (%count <= 1)
        return;

    //Put every weapon's rating into an array
    for (%i = 0; %i < %count; %i++)
    {
        %obj = getWord(%shape.botWeapon, %i);

        if (%obj.weapRating $= "")
            %rate = $AISK_WEAPON_RATING;
        else
            %rate = %obj.weapRating;

        %weaponsHolder[%i] = %rate;
    }

    for (%j = 0; %j < %count; %j++)
    {
        %obj = getWord(%shape.botWeapon, %j);
        %rating = %obj.weapRating;
        %rank = %count - 1;

        //For each weapon, cycle through every other weapon
        for (%k = 0; %k < %count; %k++)
        {
            //If it's not the same weapon, and it's worse than the one we're comparing it to
            //then take off one "point"
            if (%k != %j)
                if (%rating < %weaponsHolder[%k])
                    %rank--;
        }

        //The number of "points" the weapon has at the end determines its relative location
        %weaponNameHolder = setWord(%weaponNameHolder, %rank, %obj);
    }

    %shape.botWeapon = %weaponNameHolder;
}

//The bot should start with the appropriate weapon equipped based on its mode.
function equipBotWeapon(%obj)
{
    %weaponCount = getWordCount(%obj.botWeapon) - 1;

    switch$(%obj.weaponMode)
    {
        case "pattern":
            %obj.currentWeaponIs = 0;
            %tempWeapon = getWord(%obj.botWeapon, 0).image;

        case "random":
            %randNum = getRandom(0, %weaponCount);
            %obj.currentWeaponIs = %randNum;
            %tempWeapon = getWord(%obj.botWeapon, %randNum).image;

        case "range":
            if (%weaponCount > 0)
            {
                for (%i = 0; %i <= %weaponCount; %i++)
                {
                    //Get the name of the weapon we're checking on
                    %tempWeapon = getWord(%obj.botWeapon, %i);

                    if (!isObject(%tempWeapon) || %tempWeapon.getClassName() !$= $AISK_AFX_DATA_TYPE)
                    {
                        //If the weapon doesn't have an ignoreDistance set, then use the default
                        if (%tempWeapon.ignoreDistance !$= "")
                            %dist = %tempWeapon.ignoreDistance;
                        else
                            %dist = $AISK_IGNORE_DISTANCE;

                        //Start with the max distance
                        if (%dist2 $= "")
                            %dist2 = %obj.maxIgnore;
                    }
                    else
                    {
                        if (%tempWeapon.range !$= "")
                            %dist = %tempWeapon.range;
                        else
                            %dist = $AISK_IGNORE_DISTANCE;

                        if (%dist2 $= "")
                            %dist2 = %obj.maxIgnore;
                    }

                    //Check if the weapon range is higher than the last weapon
                    if (%dist >= %dist2)
                    {
                        %dist2 = %dist;
                        %highRange = %tempWeapon;
                        %obj.currentWeaponIs = %i;
                    }
                }

                %tempWeapon = %tempWeapon.image;
            }
            else
                %tempWeapon = getWord(%obj.botWeapon, 0).image;

        case "best":
            %obj.currentWeaponIs = %weaponCount;
            %tempWeapon = getWord(%obj.botWeapon, %weaponCount).image;

        default:
            %obj.currentWeaponIs = 0;
            %tempWeapon = getWord(%obj.botWeapon, 0).image;
    }

    %obj.mountImage(%tempWeapon, $WeaponSlot);
}


//-----------------------------------------------------------------------------
//Weapon Firing Functions
//-----------------------------------------------------------------------------

//This function sets the bots aim to the current target, and 'pulls' the trigger
//of the weapon of the bot to begin the firing sequence.
function AIPlayer::openFire(%this, %obj, %tgt, %rtt)
{
    //If the bot or the target is dead then let's bail out of here.
    if (%tgt.getstate() $= "Dead" || %obj.getState() $= "Dead")
    {
        %obj.clearaim();
        %obj.firing = false;
        return;
    }

    //Have the bot fire/cast less often if they're confused
    if (%obj.isConfused > 0)
    {
        if ((%obj.isConfused % 3) != 0)
            return;
    }

    //We've got two live ones. So let's kill something.
    //The firing variable is set while firing and is cleared at the end of the delay cycle.
    //This is done to allow the use of a firing delay - and prevent a bot from firing again prematurely.
    if (!%obj.firing)
    {
        //Get the name of the weapon
        %tempWeapon = getWord(%obj.botWeapon, %obj.currentWeaponIs);

        if (%tempWeapon $= "-noweapon")
        {
            //Sets the firing variable to true
            %obj.firing = true;
            //This sets a delay of $AISK_TRIGGER_DOWN to hold the trigger down for.
            %this.trigger = %this.schedule($AISK_TRIGGER_DOWN, "ceaseFire", %obj, %tempWeapon);
            return;
        }

        %namedClass = %tempWeapon.getClassName();

        if (%namedClass !$= $AISK_AFX_DATA_TYPE)
        {
            //If the weapon doesn't have an ignoreDistance set, then use the default
            if (%tempWeapon.ignoreDistance > 1)
                %weapMax = %tempWeapon.ignoreDistance;
            else
                %weapMax = $AISK_IGNORE_DISTANCE;

            //If the weapon doesn't have an minIgnoreDistance set, then use the default
            if (%tempWeapon.minIgnoreDistance !$= "")
                %weapMin = %tempWeapon.minIgnoreDistance;
            else
                %weapMin = $AISK_MIN_IGNORE_DISTANCE;
        }
        else
        {
            //Make sure enemy spells are only used on enemies and friend spells are only used on friendlies
            if (%obj.team == %tgt.team && %tempWeapon.target $= "enemy")
                //Get a new enemy target
                %tgt = %this.GetClosestHumanInSightandRange(%obj);
            else if (%obj.team != %tgt.team && %tempWeapon.target $= "friend")
                //Get a new friendly target
                %tgt = %this.GetClosestFriendInSightandRange(%obj);

            if (!isObject(%tgt))
            {
                %obj.currentCycleCount = 0;
                return;
            }
            else
                %rtt = vectorDist(%obj.getPosition(), %tgt.getPosition());

            //If the weapon doesn't have an ignoreDistance set, then use the default
            if (%tempWeapon.range > 1)
                %weapMax = %tempWeapon.range;
            else
                %weapMax = $AISK_IGNORE_DISTANCE;

            //If the weapon doesn't have an minIgnoreDistance set, then use the default
            if (%tempWeapon.areaDamageRadius !$= "" && ($AISK_FRIENDLY_FIRE == true || $AISK_FREE_FOR_ALL == true))
                %weapMin = %tempWeapon.areaDamageRadius;
            else
                %weapMin = $AISK_MIN_IGNORE_DISTANCE;
        }

        //If the target is within our weapon's ignore distance then we will attack. Range To Target - rtt
        if (%rtt < %weapMax && %rtt > %weapMin)
        {
            //Make sure the bot doesn't fire prematurely
            if (%obj.fireLater <= 0 && %obj.getAimLocation() != %tgt.getposition())
            {
                %obj.fireLater++;
                return;
            }

            if (%namedClass !$= $AISK_AFX_DATA_TYPE)
            {
                //Make sure the bot has ammo
                if ($AISK_ENDLESS_AMMO == true)
                {
                    if (%tempWeapon.usesAmmo == true || (%tempWeapon.usesAmmo $= "" && $AISK_WEAPON_USES_AMMO == true))
                    {
                        %clip = %tempWeapon.image.clip;

                        //If this weapon uses clips, reload one of those instead of a simple ammo refill
                        if (isObject(%clip))
                            %obj.setInventory(%clip, 2);
                        else
                            %obj.incinventory(%tempWeapon.image.ammo, 10);
                    }
                }
                else
                {
                    %ammoCount = %obj.getInventory(%tempWeapon.image.ammo);

                    if (%ammoCount < 1)
                    {
                        %obj.currentCycleCount = 0;
                        return;
                    }
                }

                %state = %obj.getImageState($WeaponSlot);

                //If the weapon isn't ready to fire, don't try to fire it
                if (%state !$= "Ready")
                    if (%state !$= "ReadyMotion")
                        if (%state !$= "ReadyFidget")
                        return;
            }

            //Do a line of sight (LoS) test for players
            %eyeTrans = %obj.getEyeTransform();
            %eyeEnd = vectorAdd(%tgt.getPosition(), $AISK_CHAR_HEIGHT);
            %searchResult = containerRayCast(%eyeTrans, %eyeEnd, $TypeMasks::PlayerObjectType, %obj);
            %foundObject = getword(%searchResult, 0);

            //Make sure the bot can see its target
            if (%foundObject != %tgt && %foundObject != 0)
            {
                //If it's not the bot's traget, move and don't fire on it
                %this.sidestep(%obj);
                return;
            }

            if (%namedClass !$= $AISK_AFX_DATA_TYPE)
            {
                //Sets the firing variable to true
                %obj.firing = true;

                //Set the AI to stop from aiming if this weapon needs that
                if (%tempWeapon.notAim == true || (%tempWeapon.notAim $= "" && $AISK_NOT_AIM == true))
                {
                    %obj.stop();
                    %obj.clearaim();
                    %obj.notAim = true;
                }

                //'Pulls' the trigger on the bot gun
                %obj.setImageTrigger($WeaponSlot, true);

                //If the weapon doesn't have a triggerDown set, then use the default
                if (%tempWeapon.triggerDown !$= "")
                    %l = %tempWeapon.triggerDown;
                else
                    %l = $AISK_TRIGGER_DOWN;

                //This sets a delay of %l length to hold the trigger down for.
                %this.trigger = %this.schedule(%l, "ceaseFire", %obj, %tempWeapon);
            }
            else
            {
                if (%tempWeapon.manaCost > %obj.getEnergyLevel())
                    return;

                //Sets the firing variable to true
                %obj.firing = true;

                //Set the AI to stop from aiming if this weapon needs that
                if (%tempWeapon.notAim == true || (%tempWeapon.notAim $= "" && $AISK_NOT_AIM == true))
                {
                    %obj.stop();
                    %obj.clearaim();
                    %obj.notAim = true;
                }

                //Don't interrupt the spell by moving
                if (%tempWeapon.spellFXData.allowMovementInterrupts)
                {
                    %obj.isCasting = true;
                    %obj.stop();

                    //The bot doesn't stop instantly so we wait a split second
                    %wait = $AISK_AFX_WAIT_TIME;

                    schedule(%wait, %this, "afxPerformSpellCast", %obj, %tempWeapon, %tgt, "", %tgt.getPosition());
                    %this.trigger = %this.schedule((%tempWeapon.castingDur * 1000) + 1000 + %wait, "ceaseFire", %obj, %tempWeapon);
                }
                else
                {
                    //Have the bot cast instead of shoot
                    afxPerformSpellCast(%obj, %tempWeapon, %tgt, "", %tgt.getPosition());
                    %this.trigger = %this.schedule((%tempWeapon.castingDur * 1000) + 1000, "ceaseFire", %obj, %tempWeapon);
                }
            }
        }
        else
        {
            //This simply clears the bots aim to have it look forward relative to it's movement
            //since there is now no target in range.
            %obj.clearaim();

            //Since we're out of range, pick a new weapon if the bot has another one
            if (getWordCount(%obj.botWeapon) > 1)
                %this.weaponChange(%obj, %rtt);
        }
    }
}

//ceaseFire is called by the openFire function after the set delay to hold the trigger down is met.
function AIPlayer::ceaseFire(%this, %obj, %tempWeapon)
{
   //Now that the bot is done firing, it gets closer to changing its weapon
   %obj.currentCycleCount--;

    if (!isObject(%tempWeapon) || %tempWeapon.getClassName() !$= $AISK_AFX_DATA_TYPE)
        //Stop holding the trigger
        %obj.setImageTrigger($WeaponSlot, false);
    else
        %obj.isCasting = false;

    //Start the AI aiming again if needed
    if (%obj.notAim)
    {
        %obj.notAim = false;
        %obj.setAimObject(%obj.oldTarget, $AISK_CHAR_HEIGHT);
    }

    //If the weapon doesn't have a fireDelay set, then use the default
    if (%tempWeapon.fireDelay !$= "")
        %k = %tempWeapon.fireDelay;
    else
        %k = $AISK_FIRE_DELAY;

    //Set a delay between when the bot let off the trigger and when it can fire again
    %this.trigger = %this.schedule(%k, "delayFire", %obj);
}

//delayFire is called to clear the firing variable. Clearing this allows
//the bot to fire again in the openFire function.
function AIPlayer::delayFire(%this, %obj)
{
    //this is the end of the firing cycle
    %obj.firing = false;
}


//-----------------------------------------------------------------------------
//Weapon Changing Function
//-----------------------------------------------------------------------------

//Change to a different weapon if needed
function AIPlayer::weaponChange(%this, %obj, %dist)
{
    //Get the total number of weapons this bot is using
    %weaponCount = getWordCount(%obj.botWeapon);

    //If the bot has more than one weapon, change what it is
    if (%weaponCount > 1)
    {
        %inRangeCount = 0;

        //Cycle through all the weapons the bot can use, then don't count any
        //that are out of range, out of ammo, or the bot doesn't have yet
        for (%i = 0; %i < %weaponCount; %i++)
        {
            //Get the name of the weapon we're checking on
            %tempWeapon = getWord(%obj.botWeapon, %i);

            if (isObject(%tempWeapon))
                %tempWeaponClass = %tempWeapon.getClassName();

            if (%tempWeapon !$= "-noweapon" && %tempWeaponClass !$= $AISK_AFX_DATA_TYPE)
            {
                //See if we have endless ammo, if so then give the bot some ammo
                if ($AISK_ENDLESS_AMMO == true)
                {
                    if (%tempWeapon.usesAmmo == true || (%tempWeapon.usesAmmo $= "" && $AISK_WEAPON_USES_AMMO == true))
                    {
                        %clip = %tempWeapon.image.clip;

                        //If this weapon uses clips, reload one of those instead of a simple ammo refill
                        if (isObject(%clip))
                            %obj.setInventory(%clip, 2);
                        else
                        %obj.incinventory(%tempWeapon.image.ammo, 10);
                    }

                    %hasAmmo = 1;
                }
                //If the weapon doesn't use ammo, count it as having ammo
                else if (%tempWeapon.usesAmmo == false || (%tempWeapon.usesAmmo $= "" && $AISK_WEAPON_USES_AMMO == false))
                    %hasAmmo = 1;
                //Get the amount of ammo for this weapon that the bot has
                else
                    %hasAmmo = %this.getInventory(%tempWeapon.image.ammo);
            }
            //If the bot isn't using a weapon, then just count it as having ammo
            else
                %hasAmmo = 1;

            //Make sure the bot has the weapon and at least 1 ammo
            if (%hasAmmo > 0)
            {
                if (%tempWeaponClass !$= $AISK_AFX_DATA_TYPE)
                {
                    //If the weapon doesn't have an ignoreDistance set, then use the default
                    if (%tempWeapon.ignoreDistance > 1)
                        %dist2 = %tempWeapon.ignoreDistance;
                    else
                        %dist2 = $AISK_IGNORE_DISTANCE;

                    //If the weapon doesn't have an minIgnoreDistance set, then use the default
                    if (%tempWeapon.minIgnoreDistance !$= "")
                        %dist4 = %tempWeapon.minIgnoreDistance;
                    else
                        %dist4 = $AISK_MIN_IGNORE_DISTANCE;
                }
                else
                {
                    if (%tempWeapon.range > 1)
                        %dist2 = %tempWeapon.range;
                    else
                        %dist2 = $AISK_IGNORE_DISTANCE;

                    if (%tempWeapon.areaDamageRadius !$= "" && ($AISK_FRIENDLY_FIRE == true || $AISK_FREE_FOR_ALL == true))
                        %dist4 = %tempWeapon.areaDamageRadius;
                    else
                        %dist4 = $AISK_MIN_IGNORE_DISTANCE;
                }

                //If the bot's in range mode, get the name of the weapon in the best range
                if (%obj.weaponMode $= "range")
                {
                    //Start with the max distance
                    if (%dist3 $= "")
                        %dist3 = %obj.maxIgnore;

                    //Check if the weapon is in range but closer than the last weapon
                    if (%dist2 > %dist && %dist4 < %dist && %dist2 <= %dist3)
                    {
                        %tempWeapon2 = %tempWeapon;
                        %dist3 = %dist2;
                        %inRangeCount++;
                    }
                }
                //If the bot isn't in range mode, get the name of each weapon in range
                else if (%dist2 > %dist && %dist4 < %dist)
                {
                    %tempWeapon[%inRangeCount] = %tempWeapon;
                    %inRangeCount++;
                }
            }
        }

        //Make sure at least one weapon is in range
        if (%inRangeCount > 0)
        {
            switch$(%obj.weaponMode)
            {
                case "pattern":
                    //Get which weapon we're currently using
                    %weapNum = %obj.currentWeaponIs;
                    //Then add one to get the next weapon
                    %weapNum++;

                    //Reset the number to 0 if we're past the last weapon
                    if (%weapNum >= %weaponCount)
                    {
                        %weapNum = 0;
                        %cycleNum = 0;
                    }
                    else
                        %cycleNum = %obj.currentCycleNumber + 1;

                    //Get the name of the weapon instead of just its number
                    %nameWeap = getWord(%obj.botWeapon, %weapNum);
                    %weaponInRange = 0;

                    //See if the weapon matches a weapon that's within range
                    for (%i = 0; %i < %inRangeCount; %i++)
                    {
                        if (%tempWeapon[%i] $= %nameWeap)
                            %weaponInRange = 1;
                    }

                case "random":
                    //Get a random weapon that's in range
                    %randNum = getRandom(0, %inRangeCount - 1);
                    //Get the number of the next cycle
                    %cycleNum = %obj.currentCycleNumber + 1;

                    //Get the number the weapon the bot decided to use
                    for (%i = 0; %i < %weaponCount; %i++)
                    {
                        %nameWeap = getWord(%obj.botWeapon, %i);

                        if (%nameWeap $= %tempWeapon[%randNum])
                            %weapNum = %i;
                    }

                    //Set the name of the weapon to equip later
                    %nameWeap = %tempWeapon[%randNum];

                case "range":
                    //Get the number of the next cycle
                    %cycleNum = %obj.currentCycleNumber + 1;

                    //Get the number the weapon the bot decided to use
                    for (%i = 0; %i < %weaponCount; %i++)
                    {
                        %nameWeap = getWord(%obj.botWeapon, %i);

                        if (%nameWeap $= %tempWeapon2)
                            %weapNum = %i;
                    }

                    //Set the name of the weapon to equip later
                    %nameWeap = %tempWeapon2;

                case "best":
                    //Get the number of the next cycle
                    %cycleNum = %obj.currentCycleNumber + 1;

                    //Get the number the weapon the bot decided to use
                    for (%i = 0; %i < %weaponCount; %i++)
                    {
                        %nameWeap = getWord(%obj.botWeapon, %i);

                        if (%nameWeap $= %tempWeapon[%inRangeCount - 1])
                            %weapNum = %i;
                    }

                    //Set the name of the weapon to equip later
                    %nameWeap = %tempWeapon[%inRangeCount - 1];

                default:
                    %weapNum = 0;
                    %cycleNum = 0;
            }

            //Reset the number to 0 if we're past the last cycle
            if (%cycleNum > (getWordCount(%obj.cycleCounter) - 1))
                %cycleNum = 0;

            //Save the number of the weapon the bot is using
            %obj.currentWeaponIs = %weapNum;
            //Save the number of the cycle the bot is using
            %obj.currentCycleNumber = %cycleNum;

            //If this cycle counter is set to random, get a number for it now
            if (getword(%obj.cycleCounter, %cycleNum) $= "x")
            {
                %rand = getRandom(1, $AISK_RAND_CYCLE_MAX);
                //Set the current cycle count up again
                %obj.currentCycleCount = %rand;
            }
            else
                %obj.currentCycleCount = getWord(%obj.cycleCounter, %cycleNum);

            //If the weapon is not in range, do the whole function again to get the weapon after that
            if (%obj.weaponMode $= "pattern" && %weaponInRange != 1)
            {
                %this.weaponChange(%obj, %dist);
                return;
            }

            //Unequip any weapons since we're using a spell
            if (isObject(%nameWeap) && %nameWeap.getClassName() $= $AISK_AFX_DATA_TYPE)
                %nameWeap = "-noweapon";

            //Equip the bot with the selected weapon
            switch$(%nameWeap)
            {
                case "-noweapon":
                    //Unequip any weapons since we're not using a weapon
                    %obj.unmountImage($WeaponSlot);

                default:
                    //Equip the weapon
                    %obj.mountImage(%nameWeap.image, $WeaponSlot);
            }
        }
        //The bot has multiple weapons but none are in range, so just set the next cycle counter
        else
        {
            //Don't get the next cycle number yet if this is a pattern
            if (%obj.weaponMode $= "pattern")
                return;

            %cycleNum = %obj.currentCycleNumber + 1;

            //Reset the number to 0 if we're past the last cycle
            if (%cycleNum > (getWordCount(%obj.cycleCounter) - 1))
                %cycleNum = 0;

            //Save the number of the cycle the bot is using
            %obj.currentCycleNumber = %cycleNum;

            //If this cycle counter is set to random, get a number for it now
            if (getword(%obj.cycleCounter, %cycleNum) $= "x")
            {
                %rand = getRandom(1, $AISK_RAND_CYCLE_MAX);
                //Set the current cycle count up again
                %obj.currentCycleCount = %rand;
            }
            else
                %obj.currentCycleCount = getWord(%obj.cycleCounter, %cycleNum);
        }
    }
    //The bot is only using one weapon so it doesn't need to change
    else
        %obj.currentCycleCount = getWord(%obj.cycleCounter, 0);
}
