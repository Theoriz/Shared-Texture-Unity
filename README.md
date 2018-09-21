# Shared-Texture-Unity
Syphon Spout Funnel plugin for Unity. Choose automatically the right plugin depending on compilation platform.

Based on the amazing work of : 

- https://github.com/sloopidoopi/Spout4Unity for Spout
- https://github.com/keijiro/KlakSpout for last Spout update (15/09/2018)
- https://github.com/Syphon/Unity3D for Syphon
- https://github.com/keijiro/Funnel for Syphon on Unity 5.3+

## How to add this plugin in your project as a submodule

### With Sourcetree

1. Open your repository in SourceTree
2. Go to Repository/Add submodule...
3. In the popup window, use following settings : 
    - Source path/URL : **https://github.com/Lyptik/Shared-Texture-Unity.git**
    - Local Relative Path : **Assets/Plugins/SharedTexture**

### With command line

1. Open a terminal and move to your Unity project's root folder
2. Then use this command : `git submodule add https://github.com/Lyptik/Shared-Texture-Unity.git Assets/Plugins/SharedTexture`

## How to use

1. Create a camera that will render into the spout texture and add the SharedTexture.cs script to it. This is your Spout camera.  
2. Choose a TargetCamera for the SharedTexture script. The TargetCamera is the camera you want to see via Spout, its settings will be copied to the Spout camera. This allows to have the TargetCamera rendering in the resolution of the Game view, and the Spout camera rendering in the resolution you want for your Spout output.