2RGuide
===
Unity3D navigation solution for 2D platformers using 2D colliders.

This is still early work in progress, should never be used on a production project

Add Dependency
===
In Unity's Packaga Manager add package from git url and use `https://github.com/TiagoJSM/2RGuide.git?path=Assets/2RGuide` as the url.

Features
===
* Builds navigation paths from scene colliders 
* Agent to traverse the shortest paths to reach a target position.
* Support for auto generated jumps and drops to connect segments segments

Getting Started
===
* Download the project and open it in Unity3D
* Create a parent `GameObject` to hold other GameObjects with the colliders to represent the world's paths
* Add the `NavWorld` component to the parent `GameObject`
* On the component's inspector panel press bake to generate the navigation segments

Planned Features
===
* Currently the tool only supports `BoxCollider2D` and `PolygonCollider2D`, `EdgeCollider2D` support will be added later
* Support for one way jump for some platforms
* Areas that forbid agent to move into to avoid obstacles
* Support for user designed jumps and drops to connect segments segments

License
===
[MIT licensed](./LICENSE)