namespace MsSqlToolBelt.DataObjects
{
    /// <summary>
    /// Represents a id / text item
    /// </summary>
    internal sealed class TextValueItem
    {
        /// <summary>
        /// Gets or sets the id of the entry
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets or sets the text of the item
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="TextValueItem"/>
        /// </summary>
        /// <param name="id">The id of the item</param>
        /// <param name="text">The text of the item</param>
        public TextValueItem(int id, string text)
        {
            Id = id;
            Text = text;
        }

        /// <summary>
        /// Returns the text of the item
        /// </summary>
        /// <returns>The text</returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
