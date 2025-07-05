Simply add the .dll file in the latest release to the LabApi plugins folder

## How to use
To create a Custom Effect, do the following
```c#
public class ExampleEffect : CustomEffectBase
{
    public override EffectClassification Classification => EffectClassification.Positive;
}
```
This simply creates a Empty Positive Custom Effect, which does nothing
It uses the SCP Secret Laboratory Player Effect Layout, so you can fully modify it all!<br>
<br>
It should support most interfaces that SL uses on its own Effects
The only thing that isn't currently supported is `IMovementSpeedModifier`
As at the moment it's impossible to change a player's movement speed without using the `MovementBoost`, `Scp207` or `AntiScp207` Effect<br>
<br>
If you wish to create a Effect that does something every tick, do the following
```c#
public class ExampleEffect : CustomTickingBase
{
    public override EffectClassification Classification => EffectClassification.Positive;

    public override void OnTick()
    {
    }
}
```
Here just override the `OnTick` method, which usually gets called every second, unless you override the `TimeTillTick` variable to be faster or slower<br>
<br>
To access the Player that has the Effect, simply use the `ReferenceHub Hub` variable