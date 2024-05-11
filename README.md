2RGuide
===
Unity3D navigation solution for 2D platformers using 2D colliders.

This is still early work in progress, it's not advised to use on a production project

Add Dependency
===
In Unity's Packaga Manager add package from git url and use `https://github.com/TiagoJSM/2RGuide.git?path=Assets/2RGuide` as the url.

Features
===
* Builds navigation paths from scene colliders 
* Agent to traverse the shortest paths to reach a target position.
* Support for auto generated jumps and drops to connect segments segments
* Support for one way platforms jumps
* Areas that forbid agent to move into to avoid obstacles

Getting Started
===
* Download the project and open it in Unity3D
* Create a parent `GameObject` to hold other GameObjects with the colliders to represent the world's paths
* Add the `NavWorld` component to the parent `GameObject`
* On the component's inspector panel press bake to generate the navigation segments
* To tweat the bake procedure go to `Edit` > `Project Settings` > `Nav2RGuide Settings` and modify the values to your liking
* Add Obstacle component to define obstacle nav segments  

Planned Features
===
* Support for user designed jumps and drops to connect segments

Examples
===
Baking navigation segments:

<img src="./Resources/Bake%20Scene.gif" alt="bake" width="70%" height="70%"/>

Moving to target in game:

<img src="./Resources/Chase.gif" alt="chase" width="50%" height="50%"/>

Special Thanks
===
[Clipper2](https://github.com/AngusJohnson/Clipper2) library used to get the world geometry.

License
===
[MIT licensed](./LICENSE)