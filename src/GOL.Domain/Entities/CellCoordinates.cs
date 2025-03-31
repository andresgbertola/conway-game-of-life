using System.Text.Json.Serialization;

namespace GOL.Domain.Entities
{
    /// <summary>
    /// Represent the coordinates.
    /// </summary>
    public readonly struct CellCoordinates
    {
        /// <summary>
        /// Row.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Column.
        /// </summary>
        public int Col { get; }

        [JsonConstructor]
        public CellCoordinates(int row, int col)
        {
            Row = row;
            Col = col;
        }
    }
}
