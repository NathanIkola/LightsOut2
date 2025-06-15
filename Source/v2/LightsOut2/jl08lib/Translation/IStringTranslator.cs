namespace jl08lib.Translation
{
    /// <summary>
    /// An interface for string translation services
    /// </summary>
    public interface IStringTranslator
    {
        /// <summary>
        /// Retrieves the translated string for the given input
        /// </summary>
        /// <param name="toTranslate">The string to translate</param>
        /// <returns>The translated string</returns>
        string Translate(string toTranslate);
    }
}