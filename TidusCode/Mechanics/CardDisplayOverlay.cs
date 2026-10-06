using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Tidus.TidusCode.Mechanics;

public static class CardDisplayOverlay
{
    private const string NodeName = "BlitzCard_UI";
    private const string ScenePath = "res://Tidus/scenes/card_display.tscn";

    public static void Ensure(NCard card)
    {
        var model = card.Model;
        var body = card.Body;

        if (model == null || body == null)
            return;

        var node = body.GetNodeOrNull<Control>(NodeName);

        if (node == null)
        {
            var scene = GD.Load<PackedScene>(ScenePath);

            if (scene == null)
                return;

            node = scene.Instantiate<Control>();
            node.Name = NodeName;
            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            body.AddChild(node);
        }

        node.Visible = model is IBlitz;
        node.Position = new Vector2(95f, -225f);
    }
}