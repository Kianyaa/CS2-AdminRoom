# CS2-AdminRoomPlugin

**CS2 plugin for teleport admin to admin room and also can set the specific location of admin room usually using for ze-map**

`!adminroom` : teleport player to admin room <br><br>
![image](https://github.com/user-attachments/assets/6fa1d733-95cc-4e74-8ac4-3d99bec32ac9) <br><br>
`!adminroomset` : set current position as admin room for this map <br><br>
![image](https://github.com/user-attachments/assets/0fd984f2-d977-428a-8eaa-a80224ed59e3) <br><br>
If the admin room still not has been set <br><br>
![image](https://github.com/user-attachments/assets/1d38bc27-e73a-490d-9f82-d215528084e8) <br><br>


## Features

- teleport flag admin (`css\@admin`) to location that has been set as admin room.
- can set specific position as admin room for the current map.
- save and can config in the JSON file format.

## Requirements
- [MetaMod](https://cs2.poggu.me/metamod/installation)
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) (Build version 1.0.305)

## Installation

download latest relase version from [Releases Latest](https://github.com/Kianyaa/CS2-AdminRoom/releases/tag/Latest)
and extract zip file and paste `AdminRoomPlugin.dll` on `addons\counterstrikesharp\plugins\AdminRoomPlugin` folder <br><br>
restart server or change the map, the plugin config file will generate at `\addons\counterstrikesharp\configs` name `adminroom_config.json`

## Example of config file
```json
{
  "AdminRoomPlugin": {
    "de_dust2": {
      "MapWorkshopId": 0,
      "X": -737.02606,
      "Y": -536.64325,
      "Z": 646.03125,
      "AngleX": 0,
      "AngleY": -77.54631,
      "AngleZ": 0
    },
    "ze_immortal_flame": {
      "MapWorkshopId": 0,
      "X": 7393.882,
      "Y": 1236.0566,
      "Z": -4127.9688,
      "AngleX": 0,
      "AngleY": 179.31128,
      "AngleZ": 0
    }
  },
  "ConfigVersion": 1
}
```
> [!NOTE]  
> Currently property `MapWorkshopId` is not used for any context.
>
> If you want to set the admin room position in manually no need to add property `MapWorkshopId`


    
