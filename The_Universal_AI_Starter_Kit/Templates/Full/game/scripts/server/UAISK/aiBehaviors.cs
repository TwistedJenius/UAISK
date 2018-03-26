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


//This function contains all of the AI behavior script objects
function loadAiBehaviors()
{
    //Create a new simgroup for behaviors, this is for the editor
    new SimGroup(allBehaviorsSet);
    UaiskData.add(allBehaviorsSet);

// -------------------------------------------------------

%behavior = new ScriptObject(LeashedBehavior) {
    //Sets if the bot attacks or not
    isAggressive = true;
    //Sets if the bot can move at all
    canMove = true;
    //Sets if the bot should return to its marker or path after killing or losing
    //track of its target
    returnToMarker = true;
    //Sets both if the bot can be killed and if other bots can target it
    isKillable = true;
    //Puts the bot on the player’s team and has it follow the player
    isFollowPlayer = false;
    //Sets if the bot can move away from a certain object, the object is set below
    isLeashed = true;
    //This is used in an eval if the behavior is leashed
    leashedTo = "%obj.marker";
    //Sets the bot to run away when touched and move farther from its spawn marker
    isSkittish = false;
    //Sets whether or not the bot should come to the aid of nearby team member
    //when that team member gets hurt
    doesAssist = false;
	//Use the Walkabout navmesh instead of dynamic obstacle avoidance
	useWalkabout = false;
	//Use the Walkabout navmesh to find cover when attacked
	useCover = false;
};

//Add to the behavior simgroup for the editor
allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(ChaseBehavior) {
    isAggressive = true;
    canMove = true;
    returnToMarker = false;
    isKillable = true;
    isFollowPlayer = false;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = false;
    doesAssist = false;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(GuardBehavior) {
    isAggressive = true;
    canMove = true;
    returnToMarker = true;
    isKillable = true;
    isFollowPlayer = false;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = false;
    doesAssist = false;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(TurretBehavior) {
    isAggressive = true;
    canMove = false;
    returnToMarker = true;
    isKillable = true;
    isFollowPlayer = false;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = false;
    doesAssist = false;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(TeammateBehavior) {
    isAggressive = true;
    canMove = true;
    returnToMarker = false;
    isKillable = true;
    isFollowPlayer = true;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = false;
    doesAssist = true;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(NPCBehavior) {
    isAggressive = false;
    canMove = true;
    returnToMarker = false;
    isKillable = false;
    isFollowPlayer = false;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = false;
    doesAssist = false;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(KillableNPCBehavior) {
    isAggressive = false;
    canMove = true;
    returnToMarker = false;
    isKillable = true;
    isFollowPlayer = false;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = false;
    doesAssist = false;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(EscortBehavior) {
    isAggressive = false;
    canMove = true;
    returnToMarker = false;
    isKillable = true;
    isFollowPlayer = true;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = false;
    doesAssist = false;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(PetBehavior) {
    isAggressive = true;
    canMove = true;
    returnToMarker = false;
    isKillable = true;
    isFollowPlayer = true;
    isLeashed = true;
    leashedTo = "%obj.master";
    isSkittish = false;
    doesAssist = true;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(CritterBehavior) {
    isAggressive = false;
    canMove = true;
    returnToMarker = false;
    isKillable = true;
    isFollowPlayer = false;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = true;
    doesAssist = false;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------

%behavior = new ScriptObject(MedicBehavior) {
    isAggressive = false;
    canMove = true;
    returnToMarker = true;
    isKillable = true;
    isFollowPlayer = false;
    isLeashed = false;
    //leashedTo = "";
    isSkittish = false;
    doesAssist = true;
	useWalkabout = false;
	useCover = false;
};

allBehaviorsSet.add(%behavior);

// -------------------------------------------------------
}
