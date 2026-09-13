using Verse;

namespace jl08lib.Translation
{
    /// <summary>
    /// Translates using the general verse translation service
    /// </summary>
    /// <remarks>
    /// You should probably always use this unless you're unit testing
    /// </remarks>
    public class VerseStringTranslator : IStringTranslator
    {
        public string Translate(string toTranslate)
        {
            return toTranslate?.Translate();
        }
    }
}