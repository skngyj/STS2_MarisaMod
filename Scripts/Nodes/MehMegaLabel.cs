using MegaCrit.Sts2.addons.mega_text;

namespace marisamod.Scripts.Nodes;

public partial class MehMegaLabel : MegaLabel
{
    public override void _Ready()
    {
        MinFontSize = 32;
        MaxFontSize = 36;
        base._Ready();
    }
}
