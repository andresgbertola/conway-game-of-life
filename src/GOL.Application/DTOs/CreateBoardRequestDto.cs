using GOL.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace GOL.Application.DTOs
{
    /// <summary>
    /// Create Board Request class.
    /// </summary>
    public class CreateBoardRequestDto
    {
        /// <summary>
        /// List of live cells. Supports negative and positive numbers.
        /// </summary>
        [Required]
        public List<CellCoordinates> LiveCells { get; set; }
    }
}
