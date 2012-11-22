Workshop Notes
--------------
Things you will need
- Git
- Unity 3.5
- The project repository

How to Navigate the Workshop
----------------------------
Checkout the project from this repository. I've created
several branches of the project at various stages. These branches
are named step1, step2, step3, and so one. To view the project at
each step, navigate to the project directory in a git terminal and
type `git checkout step1`, replacing step1 with the step of 
interest.

Step 1
------

	* `Workshop/Scenes/up10.unity`
	* `Workshop/Scripts/Up10/BUp10.boo`
	* `Workshop/Scripts/Up10/CUp10.cs`
	* `Workshop/Scripts/Up10/JUp10.js`

This stage of the project illustrates a trivial script in each of
the three languages supported by Unity. Enable each script on the
Sphere object in the up10 scene, and press play to see the effect. 
You may want to check the scene view, as the sphere goes off camera
in the game view.

Step 2
------

	* `Workshop/Scenes/pitball.unity`
	* `Workshop/Scripts/Pitball/Player.cs`

Step 2 illustrates a basic first attempt at character movement. The
scene contains a stadium composed of primitive shapes, as well as
some basic lighting. Note the input configuration, which can be 
found in `Edit > Project Settings... > Input`, and how the names
match the configuration on the Player game objects.

Step 3
------

	* `Workshop/Scenes/pitball.unity`
	* `Workshop/Scripts/Pitball/PlayerInput.cs`
	* `Workshop/Scripts/Pitball/PlayerMotor.cs`

In step 3 the `Player.cs` script is split into `PlayerInput.cs` and
`PlayerMotor.cs`. Input is interpreted by the former and the result
is used by the latter to move the players more realistically than
those in Step 2. For even more advanced movement systems, check out
the `CharacterMotor.js` script in the StandardAssets folder.

Step 4
------

	* `Workshop/Scenes/pitball.unity`
	* `Workshop/Scripts/Pitball/GameState.cs`
	* `Workshop/Scripts/Pitball/PlayerInput.cs`
	* `Workshop/Scripts/Pitball/PlayerMotor.cs`

A scoreboard object has been added to the pitball scene. The 
GameState component on this object keeps provides a GUI to start
games, keep track of scores, and display winners at the end of
the round. Take note of how this script keeps track of time and
displays the appropriate GUI depending on what is happening in the
game.

Step 5
------

	* `Workshop/Scenes/pitball.unity`
	* `Workshop/Scripts/Pitball/Ball.cs`
	* `Workshop/Scripts/Pitball/GameState.cs`
	* `Workshop/Scripts/Pitball/Net.cs`
	* `Workshop/Scripts/Pitball/PlayerInput.cs`
	* `Workshop/Scripts/Pitball/PlayerMotor.cs`

A ball and and two "nets" have been added to the scene. Logic has
been added to the player components to support picking up and 
shooting balls. When triggered by the ball, each net will report a
goal to the scoreboard object for the appropriate player. Players
are able to score on themselves. 