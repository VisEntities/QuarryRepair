This plugin makes deployable quarries and pump jacks repairable, a feature surprisingly missing from the base game due to quarries being no longer craftable. It replicates the game's native repair mechanics, ensuring that repairing feels identical to repairing any other deployable.

In the config, you can specify the amount of health to restore with each hammer hit as well as the resources to consume from the player's inventory.

[Demonstration](https://youtu.be/vdGoMF6PIoI)

-----------------

## Configuration
```json
{
  "Version": "2.0.0",
  "Health Per Hit": 25.0,
  "Repair Cost": [
    {
      "Shortname": "wood",
      "Amount": 100
    },
    {
      "Shortname": "metal.fragments",
      "Amount": 100
    },
    {
      "Shortname": "metal.refined",
      "Amount": 5
    }
  ]
}
```

-----------------------------

## Credits
*  Rewritten from scratch and maintained to present by **VisEntities**
*  Originally created by **Orange**, up to version 1.0.21
