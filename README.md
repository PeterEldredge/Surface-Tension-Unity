# Surface-Tension-Unity
Summary:
This is my current project. It is a 2D puzzle platformer where the player can change surfaces on the ground. So far, I have been nearly solely in change of programming the movement, and I am very happy with how the game is coming along.

My Contributions:
I have worked primarily on the player script. This script mostly handles the player’s movement and interactions with different surfaces and objects. The only functions I did not write in this script are:

InitializeSurfaceSpeeds()

HandleAnimation()

EquipSurface()

In these functions I only made small tweaks if anything. This code has been changed a lot over time, and still needs to be segmented a little better, but I do really like how nearly all of it functions. The things I think you should pay the most attention to are the state/action system I made to handle each frame's info, and my solution to checking for collisions with ray casts.
In addition to the player script, I have done a lot of other polish in all areas and some minor other scripts. I worked on the object scripts, collaborated on the Game Controller scripts, and assisted with/ created some level designs. I have worked kind of as a group manager, coordinating what should get done each week and how work is partitioned off, and helping to polish other people's work. 

Issues / What I would like to fix:
This project is coming along nicely, but it still needs a lot more before it is complete. Currently, nearly all art assets are not completed or not being used in their proper context. I still need to add surface interactions with objects, and we have a lot to explore with in terms of level designs. There is also the need for some more surfaces, such as a sticky one that allows players and objects to stick to walls. A lot of polish is still necessary, but I am very happy with how movement feels and how easily it can be tweaked in its current state. Occasionally, builds start off pretty choppy, but I have yet to find the cause of this, sorry if this bug happens to you.

One side note about the movement or the player/ the player script is that if I could go back/had more time I would do it a bit differently. The idea initially was to have Unity handle a lot of the physics for us, so the player has a rigid body and generally moves through Unity’s physics system. The problem with this is that it can be restrictive in some cases and a little harder to tweak than affecting the player’s transform directly. I think if I went this transform route and created my own physics system, I would be more confident in the way all movement interactions happen and have a bit more control. Also, I plan on cleaning up the code A little more and splitting it into a few scipts.

Controls:
Move the player left and right with A and D. Jump with Spacebar. Grab objects with Left Shift and then move in the direction you want to move them. 1 and 2 swaps between the “Bouncy” surface (1) and the “Slippery” surface (2). Left click an all-white surface to place the currently equipped surface on it.

How to play it:
First download the repository as a zip. Then extract all files from the zip and open the project in Unity. In Unity you can click “File” and then click “Build and Run” to start up a build of the game.

Additionally, you can play the game at this link:
https://petereldredge.itch.io/surfacetensiongame
This is not the reccomended way to play it however, as there is a graphical issue in browsers that are not chrome. Additionally, the controls may feel less responsive.


