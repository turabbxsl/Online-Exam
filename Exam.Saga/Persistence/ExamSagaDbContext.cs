using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace Exam.Saga.Persistence
{
    public class ExamSagaDbContext : SagaDbContext
    {
        public ExamSagaDbContext(DbContextOptions<ExamSagaDbContext> options) : base(options)
        {
        }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get
            {
                yield return new ExamStateMap();
            }
        }
    }
}
