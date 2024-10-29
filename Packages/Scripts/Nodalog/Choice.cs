#nullable enable

namespace Nodalog
{
    public class ChoiceData
    {
        public string Text;
        public bool Selectable;

        public ChoiceData(string text, bool selectable)
        {
            Text = text;
            Selectable = selectable;
        }
    }
}