using BaseLib.Abstracts;
using BaseLib.Utils;
using Tidus.TidusCode.Character;

namespace Tidus.TidusCode.Potions;

[Pool(typeof(TidusPotionPool))]
public abstract class TidusPotion : CustomPotionModel;