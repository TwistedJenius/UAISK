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


//Do whatever npc action the bot needs to do
function AIPlayer::doingNpcAction(%this, %obj)
{
    //Select a target based on the bots detect distance, but this target will be used to chat with, not attack.
    //This will also increase/decrease the bots attention level based on how close the target player is.
    %tgt = %this.GetClosestPlayerInSightandRange(%obj);

    //Check if there is a valid target gotten by the function above.
    if (%tgt > 0)
    {
        //Set the bot to aim at the target, so it will look at the player.
        %obj.setAimObject(%tgt, $AISK_CHAR_HEIGHT);

        //Get a string version of the of this bot's npcDecision variable to set a value to it later.
        %npcString = %obj @ ".npcDecision = ";

        //Since the bot is close enough to a player, do something based on what its npcAction is set to.
        switch(%obj.npcAction)
        {
        //If its npcAction is 1, then do the following
        case 1:
            //Only display this pop up if it hasn't recently been displayed.
            if (%obj.npcDecision == 0)
            {
                //This will make a yes/no pop up display to the player, the order is as follows:
                //MessageBoxYesNo(%title, %message, %yesCallback, %noCallback)
                //You can check messageBox.cs for other types of pop ups.
                MessageBoxYesNo("Fight me!", "Fight?", %npcString @ "2;", %npcString @ "1;");
                //Set npcDecision to a neutral number while we wait for the player's input
                %obj.npcDecision = 3;
            }

            //If an option in the below message box has already been selected, then
            //we'll set npcAction to do something else next think cycle.
            if (%obj.npcDecision == 1)
                //If "no" was selected, set npcAction to a high number so it will just use the default next cycle.
                %obj.npcAction = 99;
            //This is an "else if" instead of just an "else" because we only want this code to execute if
            //an option has been selected, not just as a default.
            else if (%obj.npcDecision == 2)
            {
                //If "yes" was selected, change the bot from being non aggressive, to being aggressive.
                //Double check to make sure the npc think loop is canceled.
                cancel(%this.ailoop);
                //Set the bots behavior mode from npc to chase.
                %this.behavior = "ChaseBehavior";
                //Execute a normal think cycle to start the bot thinking normally.
                %this.Think(%obj);
            }

        //The rest of these cases will have fewer comments since most of it has already
        //been explained, see case 1 for more details.
        case 2:
            if (%obj.npcDecision == 0)
            {
                //MessageBoxOK(%title, %message, %callback)
                MessageBoxOK("Ammo!", "Have some ammo.", %npcString @ "1;");
                %obj.npcDecision = 3;
            }
            //Make sure the player hit "ok".
            else if (%obj.npcDecision == 1)
            {
                //Give our target player 3 rounds of ammo.
                %tgt.incInventory($AISK_WEAPON.image.ammo, 3);
                //Set npcAction to go to the default.
                %obj.npcAction = 99;
            }

        //See case 1 for more details.
        case 3:
            if (%obj.npcDecision == 0)
            {
                MessageBoxYesNo("Talk 1", "Would you like to talk some more?", %npcString @ "2;", %npcString @ "1;");
                %obj.npcDecision = 3;
            }
            else if (%obj.npcDecision == 2)
            {
                MessageBoxYesNo("Talk 2", "Do you still want to talk?", %npcString @ "0;", %npcString @ "1;");
                %obj.npcDecision = 3;
            }

            //If the player selected "no", use the default npcAction.
            if (%obj.npcDecision == 1)
                %obj.npcAction = 99;

        //See case 1 for more details.
        case 4:
            %tempdist = vectorDist(%tgt.getposition(), %obj.getposition());

            //Make sure the bot is close enough and the player needs healing
            if (%tempdist > %obj.weapRange || %tgt.getDamagePercent() <= 0)
                return;

            if (%obj.npcDecision == 0)
            {
                //MessageBoxOK(%title, %message, %callback)
                MessageBoxOK("Medic!", "I'll fix you up.", %npcString @ "1;");
                %obj.npcDecision = 3;
            }
            //Make sure the player hit "ok".
            else if (%obj.npcDecision == 1)
            {
                //See how much health the player is missing then give him that much again
                %repairAmount = %tgt.getDamageLevel();
                %tgt.applyRepair(%repairAmount);

                //Update the player's GUI
                commandToClient(%tgt.client, 'setNumericalHealthHUD', %tgt.getDatablock().maxDamage);

                //Set npcAction to go to the default.
                %obj.npcAction = 99;
            }

        //UAISK+AFX Interop Changes: Start
        //See case 1 for more details.
        case 5:
            //Resurrect the player if they're dead
            if (%tgt.getstate() $= "Dead")
            {
                afxPerformSpellCast(%obj, "ReaperMadnessSpell_RPG", %tgt, "", %tgt.getPosition());
            }
            //Heal the player if they're damaged
            else if (%tgt.getDamagePercent() * 100 > 5)
            {
                %basedist = vectorDist(%tgt.getposition(), %obj.getposition());

                //Change weapons if needed
                if (%obj.currentCycleCount <= 0)
                    %this.weaponChange(%obj, %basedist);

                //Tells the bot to start shooting the target
                %obj.openFire(%obj, %tgt, %basedist);
            }
            //Heal have the bot itself
            /*else if (%obj.getDamagePercent() * 100 > 5)
            {
                //Change weapons if needed
                if (%obj.currentCycleCount <= 0)
                    %this.weaponChange(%obj, 0);

                //Tells the bot to start "attacking" itself
                %obj.openFire(%obj, %obj, 0);
            }*/
        //UAISK+AFX Interop Changes: End

        default:
            //Now that the player has selected an option, have the bot go back to its marker if needed
            if (%obj.behavior.returnToMarker && %obj.assisting == 0)
            {
                if (%obj.path $= "")
                {
                    if (%obj.oldPath $= "")
                        %goingTo = %obj.marker.getposition();
                    else
                        %goingTo = %obj.oldPath;

                    %basedist = vectorDist(%obj.getposition(), %goingTo);

                    //Make sure the bot isn't just pacing
                    if (%basedist > %obj.maxPace)
                    {
                        %obj.clearaim();
                        %obj.returningPos = %goingTo;
                        %obj.moveDestinationA = %goingTo;

                        %this.movementPositionFilter(%obj);
                    }
                }
                else
                    %obj.moveToNextNode();
            }
        }
    }
    //The player is too far away, so reset the bots actions to make sure it always does
    //the same thing next time the player gets near it.
    else
    {
        //Get the bot's marker's npcAction
        %marker = %obj.marker.getFieldValue(npcAction);

        //Check if the bot's marker has a npcAction value on it.
        if (%marker !$= "")
            //Set the bot's npcAction back to whatever it originally was on its marker.
            %obj.npcAction = %marker;
        //If the marker doesn't have a value, use the datablock's
        else
            %obj.npcAction = %obj.dataBlock.npcAction;

        //Check if the player was within the bots range last cycle.
        //If the player wasn't, we don't want to close the dialog boxes because
        //that may close things such as the exit game dialog.
        if (%obj.npcDecision > 0)
        {
            //Close all of our dialogs because we're now out of range of the bot.
            Canvas.popDialog(MessageBoxYesNoDlg);
            Canvas.popDialog(MessageBoxOKDlg);
        }

        //Reset the bot's npcDecision value so that it can start fresh if
        //the player gets within the bot's range again.
        %obj.npcDecision = 0;
    }
}
