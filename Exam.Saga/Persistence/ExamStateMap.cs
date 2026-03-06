using Exam.Saga.StateMachines;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Saga.Persistence
{
    public class ExamStateMap:SagaClassMap<ExamState>
    {
        protected override void Configure(EntityTypeBuilder<ExamState> entity, ModelBuilder model)
        {
            entity.ToTable("ExamStates");

            entity.HasKey(x => x.CorrelationId);

            entity.Property(x => x.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

            entity.Property(x => x.StudentId).IsRequired();

            entity.Property(x => x.ExpirationTokenId);

            entity.HasIndex(x => x.StudentId);
        }
    }
}
