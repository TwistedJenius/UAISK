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


//This function updates all controls in the editor to be equal to that of the selected marker
function AIMarkerEditor::updateControls(%this)
{
	//Clear the weapons list so we can populate it later
    AIME_WeaponsList.clearItems();

	%data = AIME_MarkerSelector.getText();

    if (!isObject(%data))
        return;

	//All the following if/else check if a data field has a value, and if not set it to the default
	if (%data.block !$= "")
		AIME_CharacterSelector.setText(getField(%data.block, 0));
	else
		AIME_CharacterSelector.setText($AISK_CHAR_TYPE);

	if (%data.getGroup().getName() !$= "")
		AIME_SimGroupSelector.setText(getField(%data.getGroup().getName(), 0));
	else
		AIME_SimGroupSelector.setText($AISK_GROUP);

	//For checking the datablock's defaults
	%data2 = AIME_CharacterSelector.getText();

    //Get the total number of weapons the bot is using, then cycle around once for each weapon
    if (%data.Weapon !$= "")
    {
        %l = getWordCount(%data.Weapon);

        for (%j = 0; %j < %l; %j++)
        {
            %i = getWord(%data.Weapon, %j);
		    AIME_WeaponsList.addItem(%i);
        }
    }
    else if (%data2.Weapon !$= "")
    {
        %l = getWordCount(%data2.Weapon);

        for (%j = 0; %j < %l; %j++)
        {
            %i = getWord(%data2.Weapon, %j);
            AIME_WeaponsList.addItem(%i);
        }
    }
    else
    {
        %l = getWordCount($AISK_WEAPON);

        for (%j = 0; %j < %l; %j++)
        {
            %i = getWord($AISK_WEAPON, %j);
            AIME_WeaponsList.addItem(%i);
        }
    }

	if (%data.pathname !$= "")
		AIME_PathSelector.setText(getField(%data.pathname, 0));
	else
		AIME_PathSelector.setText("-NOT PATHED");

	if (%data.maxRange !$= "")
		AIME_RangeSelector.setText(getField(%data.maxRange, 0));
	else
    {
        if (%data2.maxRange !$= "")
            AIME_RangeSelector.setText(%data2.maxRange);
        else
	        AIME_RangeSelector.setText($AISK_MAX_DISTANCE);
    }

	if (%data.minRange !$= "")
		AIME_MinRangeSelector.setText(getField(%data.minRange, 0));
	else
    {
        if (%data2.minRange !$= "")
            AIME_MinRangeSelector.setText(%data2.minRange);
        else
	        AIME_MinRangeSelector.setText($AISK_MIN_DISTANCE);
    }

	if (%data.distDetect !$= "")
		AIME_DetectSelector.setText(getField(%data.distDetect, 0));
	else
    {
        if (%data2.distDetect !$= "")
            AIME_DetectSelector.setText(%data2.distDetect);
        else
	        AIME_DetectSelector.setText($AISK_DETECT_DISTANCE);
    }

	if (%data.sidestepDist !$= "")
		AIME_SidestepSelector.setText(getField(%data.sidestepDist, 0));
	else
    {
        if (%data2.sidestepDist !$= "")
            AIME_SidestepSelector.setText(%data2.sidestepDist);
        else
	        AIME_SidestepSelector.setText($AISK_SIDESTEP);
    }

	if (%data.paceDist !$= "")
		AIME_PaceSelector.setText(getField(%data.paceDist, 0));
	else
    {
        if (%data2.paceDist !$= "")
            AIME_PaceSelector.setText(%data2.paceDist);
        else
	        AIME_PaceSelector.setText($AISK_MAX_PACE);
    }

	if (%data.behavior !$= "")
		AIME_BehaviorSelector.setText(getField(%data.behavior, 0));
	else
    {
        if (%data2.behavior !$= "")
            AIME_BehaviorSelector.setText(%data2.behavior);
        else
	        AIME_BehaviorSelector.setText($AISK_BEHAVIOR);
    }

	if (%data.weaponMode !$= "")
		AIME_WeaponModeSelector.setText(getField(%data.weaponMode, 0));
	else
    {
        if (%data2.weaponMode !$= "")
            AIME_WeaponModeSelector.setText(%data2.weaponMode);
        else
	        AIME_WeaponModeSelector.setText($AISK_WEAPON_MODE);
    }

	if (%data.fov !$= "")
		AIME_FOVSelector.setText(getField(%data.fov, 0));
	else
    {
        if (%data2.fov !$= "")
            AIME_FOVSelector.setText(%data2.fov);
        else
	        AIME_FOVSelector.setText($AISK_FOV);
    }

	if (%data.leash !$= "")
		AIME_LeashSelector.setText(getField(%data.leash, 0));
	else
    {
        if (%data2.leash !$= "")
            AIME_LeashSelector.setText(%data2.leash);
        else
	        AIME_LeashSelector.setText($AISK_LEASH_DISTANCE);
    }

	if (%data.team !$= "")
		AIME_TeamSelector.setText(getField(%data.team, 0));
	else
    {
        if (%data2.team !$= "")
            AIME_TeamSelector.setText(%data2.team);
        else
	        AIME_TeamSelector.setText($AISK_TEAM);
    }

	if (%data.cycleCounter !$= "")
		AIME_CycleCounterSelector.setText(getField(%data.cycleCounter, 0));
	else
    {
        if (%data2.cycleCounter !$= "")
            AIME_CycleCounterSelector.setText(%data2.cycleCounter);
        else
	        AIME_CycleCounterSelector.setText($AISK_CYCLE_COUNTER);
    }

	if (%data.countRespawn !$= "")
		AIME_RespawnCountSelector.setText(getField(%data.countRespawn, 0));
	else
    {
        if (%data2.countRespawn !$= "")
            AIME_RespawnCountSelector.setText(%data2.countRespawn);
        else
	        AIME_RespawnCountSelector.setText($AISK_RESPAWN_COUNT);
    }

	if (%data.respawn !$= "")
	{
		if (%data.respawn $= "1")
      		AIME_RespawnSelector.setValue("1");
   		else
      		AIME_RespawnSelector.setValue("0");
	}
    else if (%data2.respawn !$= "")
	{
		if (%data2.respawn $= "1")
      		AIME_RespawnSelector.setValue("1");
   		else
      		AIME_RespawnSelector.setValue("0");
	}
	else
	{
   		if ($AISK_DEFAULT_RESPAWN $= "1")
      		AIME_RespawnSelector.setValue("1");
   		else
      		AIME_RespawnSelector.setValue("0");
	}

	if (%data.activeDodge !$= "")
		AIME_DodgeSelector.setText(getField(%data.activeDodge, 0));
	else
    {
        if (%data2.activeDodge !$= "")
            AIME_DodgeSelector.setText(%data2.activeDodge);
        else
	        AIME_DodgeSelector.setText($AISK_ACTIVE_DODGE);
    }

	if (%data.advancedDodge !$= "")
		AIME_AdvancedSelector.setText(getField(%data.advancedDodge, 0));
	else
    {
        if (%data2.advancedDodge !$= "")
            AIME_AdvancedSelector.setText(%data2.advancedDodge);
        else
	        AIME_AdvancedSelector.setText($AISK_ADVANCED_DODGE);
    }

	if (%data.npcAction !$= "")
		AIME_NPCActionSelector.setText(getField(%data.npcAction, 0));
	else
    {
        if (%data2.npcAction !$= "")
            AIME_NPCActionSelector.setText(%data2.npcAction);
        else
	        AIME_NPCActionSelector.setText("0");
    }

	if (%data.spawnGroup !$= "")
		AIME_SpawnGroupSelector.text = %data.getFieldValue(spawnGroup);
	else
    {
        if (%data2.spawnGroup !$= "")
            AIME_SpawnGroupSelector.setText(%data2.spawnGroup);
        else
	        AIME_SpawnGroupSelector.setText($AISK_SPAWN_GROUP);
    }

	if (%data.getName() !$= "")
		AIME_NameSelector.text = %data.getName();
	else
    {
        if ($aiMarkerCount < 9)
           AIME_NameSelector.text = "Marker0" @ ($aiMarkerCount + 1);
        else
           AIME_NameSelector.text = "Marker" @ ($aiMarkerCount + 1);
    }

	if (%data.realName !$= "")
		AIME_RealNameSelector.setText(getField(%data.realName, 0));
	else
    {
        if (%data2.realName !$= "")
            AIME_RealNameSelector.setText(%data2.realName);
        else
	        AIME_RealNameSelector.setText($AISK_REAL_NAME);
    }

	if (%data.rotation !$= "")
		AIME_RotationSelector.text = %data.getFieldValue(rotation);
	else
		AIME_RotationSelector.text = "1 0 0 0";

	if (%data.position !$= "")
		AIME_PositionSelector.text = %data.getFieldValue(position);
	else
		AIME_PositionSelector.text = "0 0 0";

	if (%data.scale !$= "")
		AIME_ScaleSelector.text = %data.getFieldValue(scale);
	else
		AIME_ScaleSelector.text = "1 1 1";

	if (%data.botBelongsToMe !$= "")
        AIME_EditorText54.setText(%data.botBelongsToMe);
	else
		AIME_EditorText54.setText("-NONE");

    AIMarkerEditor::editorSelectObject(%data);

	echo("Selected Marker: " @ %data);
}

//This function updates all controls in the editor to be equal to that of the global or datablock default
function AIMarkerEditor::updateControls2(%this)
{
	//Clear the weapons list so we can populate it later
    AIME_WeaponsList.clearItems();

    //Reset everything
	AIME_SimGroupSelector.setText($AISK_GROUP);
	AIME_PathSelector.setText("-NOT PATHED");
	AIME_ScaleSelector.text = "1 1 1";
	AIME_EditorText54.setText("-NONE");

    if ($aiMarkerCount < 9)
       AIME_NameSelector.text = "Marker0" @ ($aiMarkerCount + 1);
    else
       AIME_NameSelector.text = "Marker" @ ($aiMarkerCount + 1);

    //Get the transform the marker should be made at
    AIMarkerEditor::setAIPositioning();
    AIMarkerEditor::setAIRotate();

	//For checking the datablock's defaults
	%data2 = AIME_CharacterSelector2.getText();

    if (%data2 $= "-RANDOM")
        %data2 = $AISK_CHAR_TYPE;

    //Get the total number of weapons the bot is using, then cycle around once for each weapon
    if (%data2.Weapon !$= "")
    {
        %l = getWordCount(%data2.Weapon);

        for (%j = 0; %j < %l; %j++)
        {
            %i = getWord(%data2.Weapon, %j);
            AIME_WeaponsList.addItem(%i);
        }
    }
    else
    {
        %l = getWordCount($AISK_WEAPON);

        for (%j = 0; %j < %l; %j++)
        {
            %i = getWord($AISK_WEAPON, %j);
            AIME_WeaponsList.addItem(%i);
        }
    }

	//All the following if/else check if a data field has a value, and if not set it to the default
    if (%data2.maxRange !$= "")
        AIME_RangeSelector.setText(%data2.maxRange);
    else
        AIME_RangeSelector.setText($AISK_MAX_DISTANCE);

    if (%data2.minRange !$= "")
        AIME_MinRangeSelector.setText(%data2.minRange);
    else
        AIME_MinRangeSelector.setText($AISK_MIN_DISTANCE);

    if (%data2.distDetect !$= "")
        AIME_DetectSelector.setText(%data2.distDetect);
    else
        AIME_DetectSelector.setText($AISK_DETECT_DISTANCE);

    if (%data2.sidestepDist !$= "")
        AIME_SidestepSelector.setText(%data2.sidestepDist);
    else
        AIME_SidestepSelector.setText($AISK_SIDESTEP);

    if (%data2.paceDist !$= "")
        AIME_PaceSelector.setText(%data2.paceDist);
    else
        AIME_PaceSelector.setText($AISK_MAX_PACE);

    if (%data2.behavior !$= "")
        AIME_BehaviorSelector.setText(%data2.behavior);
    else
        AIME_BehaviorSelector.setText($AISK_BEHAVIOR);

    if (%data2.weaponMode !$= "")
        AIME_WeaponModeSelector.setText(%data2.weaponMode);
    else
        AIME_WeaponModeSelector.setText($AISK_WEAPON_MODE);

    if (%data2.fov !$= "")
        AIME_FOVSelector.setText(%data2.fov);
    else
        AIME_FOVSelector.setText($AISK_FOV);

    if (%data2.leash !$= "")
        AIME_LeashSelector.setText(%data2.leash);
    else
        AIME_LeashSelector.setText($AISK_LEASH_DISTANCE);

    if (%data2.team !$= "")
        AIME_TeamSelector.setText(%data2.team);
    else
        AIME_TeamSelector.setText($AISK_TEAM);

    if (%data2.cycleCounter !$= "")
        AIME_CycleCounterSelector.setText(%data2.cycleCounter);
    else
        AIME_CycleCounterSelector.setText($AISK_CYCLE_COUNTER);

    if (%data2.npcAction !$= "")
        AIME_NPCActionSelector.setText(%data2.npcAction);
    else
        AIME_NPCActionSelector.setText("0");

    if (%data2.spawnGroup !$= "")
        AIME_SpawnGroupSelector.setText(%data2.spawnGroup);
    else
        AIME_SpawnGroupSelector.setText($AISK_SPAWN_GROUP);

    if (%data2.realName !$= "")
        AIME_RealNameSelector.setText(%data2.realName);
    else
        AIME_RealNameSelector.setText($AISK_REAL_NAME);

    if (%data2.countRespawn !$= "")
        AIME_RespawnCountSelector.setText(%data2.countRespawn);
    else
        AIME_RespawnCountSelector.setText($AISK_RESPAWN_COUNT);

    if (%data2.respawn !$= "")
	{
		if (%data2.respawn $= "1")
      		AIME_RespawnSelector.setValue("1");
   		else
      		AIME_RespawnSelector.setValue("0");
	}
	else
	{
   		if ($AISK_DEFAULT_RESPAWN $= "1")
      		AIME_RespawnSelector.setValue("1");
   		else
      		AIME_RespawnSelector.setValue("0");
	}

    if (%data2.activeDodge !$= "")
        AIME_DodgeSelector.setText(%data2.activeDodge);
    else
        AIME_DodgeSelector.setText($AISK_ACTIVE_DODGE);

    if (%data2.advancedDodge !$= "")
        AIME_AdvancedSelector.setText(%data2.advancedDodge);
    else
        AIME_AdvancedSelector.setText($AISK_ADVANCED_DODGE);

    %data = AIME_NameSelector.getText();

    AIMarkerEditor::editorSelectObject(%data);

	echo("Selected Marker: " @ %data);
}

//This function applies any changes made to the marker
function AIMarkerEditor::saveEffect(%this, %mode)
{
    AIMarkerEditor::editorDeselectObject();

	if (%mode $= "default" || (%mode $= "fromMarker" && $aiMarkerCount < 1))
    {
        AIMarkerEditor::updateControls2();
	    %data2 = AIME_CharacterSelector2.getText();
    }
    else
	    %data2 = AIME_CharacterSelector.getText();

    if (%data2 $= "-RANDOM")
        %data2 = $AISK_CHAR_TYPE;

	//Make sure the marker has a name
	if (AIME_NameSelector.getText() $= "" || %mode $= "fromMarker")
    {
        if ($aiMarkerCount < 9)
           AIME_NameSelector.text = "Marker0" @ ($aiMarkerCount + 1);
        else
           AIME_NameSelector.text = "Marker" @ ($aiMarkerCount + 1);
    }

	%data = AIME_MarkerSelector.getText();
	%data3 = AIME_NameSelector.getText();

    //Get the marker's team and spawn group to put it in simsets later
    %team = AIME_TeamSelector.getText();
    %group = AIME_SpawnGroupSelector.getText();

    //Get the transform the marker should be made at
	if (%mode $= "fromMarker")
    {
        AIMarkerEditor::setAIPositioning();
        AIMarkerEditor::setAIRotate();
    }

	//If something is the default value, set it blank so it's not added to the marker
	if (AIME_PathSelector.getText() $= "-NOT PATHED")
		AIME_PathSelector.setText("");

	if (%mode $= "default" || (%mode $= "fromMarker" && $aiMarkerCount < 1))
    {
	    if (AIME_CharacterSelector2.getText() $= $AISK_CHAR_TYPE)
		    AIME_CharacterSelector2.setText("");
    }
    else if (AIME_CharacterSelector.getText() $= $AISK_CHAR_TYPE)
		    AIME_CharacterSelector.setText("");

	if (AIME_RangeSelector.getText() $= %data2.maxRange)
		AIME_RangeSelector.setText("");
	else if (%data2.maxRange $= "" && AIME_RangeSelector.getText() $= $AISK_MAX_DISTANCE)
		AIME_RangeSelector.setText("");

	if (AIME_MinRangeSelector.getText() $= %data2.minRange)
		AIME_MinRangeSelector.setText("");
	else if (%data2.minRange $= "" && AIME_MinRangeSelector.getText() $= $AISK_MIN_DISTANCE)
		AIME_MinRangeSelector.setText("");

	if (AIME_DetectSelector.getText() $= %data2.distDetect)
		AIME_DetectSelector.setText("");
	else if (%data2.distDetect $= "" && AIME_DetectSelector.getText() $= $AISK_DETECT_DISTANCE)
		AIME_DetectSelector.setText("");

	if (AIME_SidestepSelector.getText() $= %data2.sidestepDist)
		AIME_SidestepSelector.setText("");
	else if (%data2.sidestepDist $= "" && AIME_SidestepSelector.getText() $= $AISK_SIDESTEP)
		AIME_SidestepSelector.setText("");

	if (AIME_PaceSelector.getText() $= %data2.paceDist)
		AIME_PaceSelector.setText("");
	else if (%data2.paceDist $= "" && AIME_PaceSelector.getText() $= $AISK_MAX_PACE)
		AIME_PaceSelector.setText("");

	if (AIME_BehaviorSelector.getText() $= %data2.behavior)
		AIME_BehaviorSelector.setText("");
	else if (%data2.behavior $= "" && AIME_BehaviorSelector.getText() $= $AISK_BEHAVIOR)
		AIME_BehaviorSelector.setText("");

	if (AIME_WeaponModeSelector.getText() $= %data2.weaponMode)
		AIME_WeaponModeSelector.setText("");
	else if (%data2.weaponMode $= "" && AIME_WeaponModeSelector.getText() $= $AISK_WEAPON_MODE)
		AIME_WeaponModeSelector.setText("");

	if (AIME_FOVSelector.getText() $= %data2.fov)
		AIME_FOVSelector.setText("");
	else if (%data2.fov $= "" && AIME_FOVSelector.getText() $= $AISK_FOV)
		AIME_FOVSelector.setText("");

	if (AIME_LeashSelector.getText() $= %data2.leash)
		AIME_LeashSelector.setText("");
	else if (%data2.leash $= "" && AIME_LeashSelector.getText() $= $AISK_LEASH_DISTANCE)
		AIME_LeashSelector.setText("");

	if (AIME_CycleCounterSelector.getText() $= %data2.cycleCounter)
		AIME_CycleCounterSelector.setText("");
	else if (%data2.cycleCounter $= "" && AIME_CycleCounterSelector.getText() $= $AISK_CYCLE_COUNTER)
		AIME_CycleCounterSelector.setText("");

	if (AIME_RealNameSelector.getText() $= %data2.realName)
		AIME_RealNameSelector.setText("");
	else if (%data2.realName $= "" && AIME_RealNameSelector.getText() $= $AISK_REAL_NAME)
		AIME_RealNameSelector.setText("");

	if (AIME_NPCActionSelector.getText() $= %data2.npcAction)
		AIME_NPCActionSelector.setText("");
	else if (AIME_NPCActionSelector.getText() $= "0")
		AIME_NPCActionSelector.setText("");

	if (AIME_RespawnCountSelector.getText() $= %data2.countRespawn)
		AIME_RespawnCountSelector.setText("");
	else if (%data2.countRespawn $= "" && AIME_RespawnCountSelector.getText() $= $AISK_RESPAWN_COUNT)
		AIME_RespawnCountSelector.setText("");

	if (AIME_RespawnSelector.getValue() $= %data2.respawn)
		%respawnTemp = "";
	else if (%data2.respawn $= "" && AIME_RespawnSelector.getValue() $= $AISK_DEFAULT_RESPAWN)
		%respawnTemp = "";
	else
		%respawnTemp = AIME_RespawnSelector.getValue();

    if (AIME_DodgeSelector.getText() $= %data2.activeDodge)
	    AIME_DodgeSelector.setText("");
    else if (%data2.activeDodge $= "" && AIME_DodgeSelector.getText() $= $AISK_ACTIVE_DODGE)
	    AIME_DodgeSelector.setText("");

	if (AIME_AdvancedSelector.getText() $= %data2.advancedDodge)
		AIME_AdvancedSelector.setText("");
	else if (%data2.advancedDodge $= "" && AIME_AdvancedSelector.getText() $= $AISK_ADVANCED_DODGE)
		AIME_AdvancedSelector.setText("");

    if (AIME_TeamSelector.getText() $= %data2.team)
	    AIME_TeamSelector.setText("");
    else if (%data2.team $= "" && AIME_TeamSelector.getText() $= $AISK_TEAM)
	    AIME_TeamSelector.setText("");

    if (AIME_SpawnGroupSelector.getText() $= %data2.spawnGroup)
	    AIME_SpawnGroupSelector.setText("");
    else if (%data2.spawnGroup $= "" && AIME_SpawnGroupSelector.getText() $= $AISK_SPAWN_GROUP)
	    AIME_SpawnGroupSelector.setText("");

    %team2 = AIME_TeamSelector.getText();
    %group2 = AIME_SpawnGroupSelector.getText();

    %l = AIME_WeaponsList.getItemCount();

    //Cycle around to get every item on the weapons list
    for (%j = 0; %j < %l; %j++)
    {
        %k = AIME_WeaponsList.getItemText(%j);
        %weaponTemp = setWord(%weaponTemp, %j, %k);
    }

	if (%weaponTemp $= %data2.Weapon)
		%weaponTemp = "";
	else if (%data2.Weapon $= "" && %weaponTemp $= $AISK_WEAPON)
		%weaponTemp = "";

    %markerData = %data2 @ "Marker";

    if (!isObject(%markerData))
        %markerData = "AIPlayerMarker";

    //Set the marker's values if we're editing
    if (%mode $= "edit")
    {
        %obj = %data.getid();

        //Hide and then later unhide it if needed
        %obj.sethidden(true);

        %obj.setName(%data3);
        %obj.setDatablock(%markerData);
        %obj.position = AIME_PositionSelector.getText();
        %obj.rotation = AIME_RotationSelector.getText();
        %obj.scale = AIME_ScaleSelector.getText();
        %obj.block = AIME_CharacterSelector.getText();
        %obj.maxRange = AIME_RangeSelector.getText();
        %obj.minRange = AIME_MinRangeSelector.getText();
        %obj.pathname = AIME_PathSelector.getText();
        %obj.behavior = AIME_BehaviorSelector.getText();
        %obj.weaponMode = AIME_WeaponModeSelector.getText();
        %obj.distDetect = AIME_DetectSelector.getText();
        %obj.sidestepDist = AIME_SidestepSelector.getText();
        %obj.paceDist = AIME_PaceSelector.getText();
        %obj.npcAction = AIME_NPCActionSelector.getText();
        %obj.fov = AIME_FOVSelector.getText();
        %obj.leash = AIME_LeashSelector.getText();
        %obj.cycleCounter = AIME_CycleCounterSelector.getText();
        %obj.realName = AIME_RealNameSelector.getText();
        %obj.Weapon = %weaponTemp;
        %obj.countRespawn = AIME_RespawnCountSelector.getText();
        %obj.respawn = %respawnTemp;
        %obj.activeDodge = AIME_DodgeSelector.getText();
        %obj.advancedDodge = AIME_AdvancedSelector.getText();

        //Change the marker's team and spawn group if needed
        changeMarkerTeams(%obj, %team);
        changeSpawnGroups(%obj, %group);

        //Set the team and spawn group after the change
        %obj.team = %team2;
        %obj.spawnGroup = %group2;

	    //Put it in the correct simgroup
	    %simGroupSet = AIME_SimGroupSelector.getText();

        if (getField(%obj.getGroup().getName(), 0) !$= %simGroupSet)
	        %simGroupSet.add(%obj);

	    AIMarkerEditor.initEditor();
        AIME_WeaponsList.clearItems();

        %order = AIME_MarkerSelector.findText(%data3);

        //Should the next marker we select be above or below the last one
        if ($AIME_SelectNextMarker == 0)
        {
            if (%order < $aiMarkerCount - 1)
                %order++;
            else
                %order = 0;
        }
        else if ($AIME_SelectNextMarker == 1)
        {
            if (%order > 0)
                %order--;
            else
                %order = $aiMarkerCount - 1;
        }

        AIME_MarkerSelector.setText(AIME_MarkerSelector.getTextById(%order));
        AIME_MarkerSelector2.setText(AIME_MarkerSelector2.getTextById(%order));

        //Unhide the marker if needed
        if ($AISK_MARKER_HIDE == false)
            %obj.sethidden(false);
        //Work around for T3D bug
        else
            %obj.setTransform(%obj.getPosition());

        AIMarkerEditor.updateControls();
    }
    //Make a new marker
    else
    {
        if (isObject(%data3))
            AIME_NameSelector.text = "Marker" @ getRandom(1, 999);

	    if (%mode $= "default" || (%mode $= "fromMarker" && $aiMarkerCount < 1))
            %charBlock = AIME_CharacterSelector2.getText();
        else
            %charBlock = AIME_CharacterSelector.getText();

        //Setup the objects data
        %marker = new StaticShape(AIME_NameSelector.getText()) {
            canSaveDynamicFields = "1";
            position = AIME_PositionSelector.getText();
            rotation = AIME_RotationSelector.getText();
            scale = AIME_ScaleSelector.getText();
            dataBlock = %markerData;
                block = %charBlock;
                Weapon = %weaponTemp;
                maxRange = AIME_RangeSelector.getText();
                minRange = AIME_MinRangeSelector.getText();
                pathname = AIME_PathSelector.getText();
                respawn = %respawnTemp;
                countRespawn = AIME_RespawnCountSelector.getText();
                activeDodge = AIME_DodgeSelector.getText();
                advancedDodge = AIME_AdvancedSelector.getText();
                behavior = AIME_BehaviorSelector.getText();
                weaponMode = AIME_WeaponModeSelector.getText();
                spawnGroup = %group2;
                distDetect = AIME_DetectSelector.getText();
                sidestepDist = AIME_SidestepSelector.getText();
                paceDist = AIME_PaceSelector.getText();
                npcAction = AIME_NPCActionSelector.getText();
                fov = AIME_FOVSelector.getText();
                leash = AIME_LeashSelector.getText();
                team = %team2;
                cycleCounter = AIME_CycleCounterSelector.getText();
                realName = AIME_RealNameSelector.getText();
        };

	    //Save it in the correct simgroup
	    %simGroupSet = AIME_SimGroupSelector.getText();

        if (!isObject(%simGroupSet))
            %simGroupSet = $AISK_GROUP;

	    %simGroupSet.add(%marker);

	    //Since we made a new marker, it needs to be added to the markers' SimSets
        allMarkersSet.add(%marker);
        changeMarkerTeams(%marker, %team);
        changeSpawnGroups(%marker, %group);

        //Set the team and spawn group after the change
        %marker.team = %team2;
        %marker.spawnGroup = %group2;

        if (%data !$= "")
            %selectSame = AIME_MarkerSelector.findText(%data);
        else
            %selectSame = 0;

        %selectSame2 = AIME_CharacterSelector2.getSelected();

	    AIMarkerEditor.initEditor();
        AIME_WeaponsList.clearItems();

        AIME_MarkerSelector.setSelected(%selectSame);
        AIME_CharacterSelector2.setSelected(%selectSame2);
    }
}
