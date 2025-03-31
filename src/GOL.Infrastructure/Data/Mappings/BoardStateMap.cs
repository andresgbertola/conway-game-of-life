using GOL.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GOL.Infrastructure.Data.Mappings
{
    public class BoardStateMap
    {
        public BoardStateMap(EntityTypeBuilder<BoardState> entity)
        {
            // Define the primary key on Id as non-clustered.
            entity.HasKey(b => b.Id)
                  .IsClustered(false);

            // Required properties.
            entity.Property(b => b.BoardId).IsRequired();
            entity.Property(b => b.State).IsRequired();
            entity.Property(b => b.StateHash).IsRequired();
            entity.Property(b => b.Iteration).IsRequired();
            entity.Property(b => b.Status)
                  .HasConversion<int>()
                  .IsRequired();

            // Ignore the computed property.
            entity.Ignore(b => b.LiveCells);

            // Create a composite clustered index on BoardId and Iteration.
            entity.HasIndex(b => new { b.BoardId, b.Iteration })
                  .IsClustered(true)
                  .IsUnique(true);

            // Create a composite non-clustered index for the most common query.
            entity.HasIndex(b => new { b.BoardId, b.StateHash, b.Iteration })
                  .IsClustered(false);
        }
    }
}
