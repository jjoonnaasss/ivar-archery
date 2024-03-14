# Jonas Wombacher - Interaction in Virtual and Augmented Reality 2023/24
This repository contains the Unity project I built for a university course called "Interaction in Virtual and Augmented Reality". The resulting app was developed for the Meta Quest 2/3. If you want to know more about this project, you can check out the [documentation on my homepage](https://www.jonaswombacher.de/ivar).

## Assets used in the project
This project is based on the [template project we were provided in the lecture](https://github.com/wenjietseng/VR-locomotion-parkour). In addition to the ones included in that project, I used the following assets for my implementation:
- [Models for the bows and arrows](https://assetstore.unity.com/packages/3d/props/weapons/free-cartoon-weapon-pack-mobile-vr-23956)
- [Model for the targets](https://assetstore.unity.com/packages/essentials/tutorial-projects/polygon-prototype-low-poly-3d-art-by-synty-137126)
- [An asset that allowed me to fracture the target model while keeping its texture intact](https://github.com/dgreenheck/OpenFracture)
- A few sound effects under Creative Commons from [freesound.org](https://freesound.org/) as well as [this sound from user gronnie](https://freesound.org/people/gronnie/sounds/563175/)

## Game controls
When starting out, both the teleportation bow and the coin collection bow are stored in the two bow holsters on your back. You can reach over your left or right shoulder to grab a bow. When you want to change to the other bow, you can put your bow back into the empty holster and grab the new one from the other holster.

Here is an overview over the controller mapping in the game:
- Grab button – press and release: take a bow or put a bow away
- Grab button – keep pressed: grab and draw a bowstring
- A/X Button: confirm current t-shape orientation
- B/Y Button: reset your position to the last checkpoint
- (Start Button: save JSON-file with your current stats)
