using BaseLib.Abstracts;
using Tidus.TidusCode.Extensions;
using Godot;

namespace Tidus.TidusCode.Character;

public class TidusPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Tidus.Color;


    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}