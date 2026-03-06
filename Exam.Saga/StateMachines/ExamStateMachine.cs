using MassTransit;
using OnlineExam.Shared.Contracts;
using OnlineExam.Shared.Contracts.Commands;
using OnlineExam.Shared.Contracts.Events;

namespace Exam.Saga.StateMachines
{
    public class ExamStateMachine : MassTransitStateMachine<ExamState>
    {

        // States
        public State Preparing { get; private set; }
        public State InProgress { get; private set; }
        public State Finished { get; private set; }

        // Events
        public Event<StartExam> StartExam { get; private set; }
        public Event<QuestionPrepared> QuestionPrepared { get; private set; }
        public Event<SubmitAnswers> SubmitAnswers { get; private set; }
        public Event<ExamFinishedEvent> ExamFinished { get; private set; }


        // Schedule
        public Schedule<ExamState, ExamTimeoutExpired> ExamTimeout { get; private set; }


        public ExamStateMachine()
        {
            // 1. We say in which property the status will be stored
            InstanceState(x => x.CurrentState);

            // 2. CorrelateId bind ExamId
            Event(() => StartExam, x => x.CorrelateById(m => m.Message.ExamId));
            Event(() => QuestionPrepared, x => x.CorrelateById(m => m.Message.ExamId));
            Event(() => SubmitAnswers, x => x.CorrelateById(m => m.Message.ExamId));
            Event(() => ExamFinished, x => x.CorrelateById(m => m.Message.ExamId));

            // 3. Timer Configuration
            Schedule(() => ExamTimeout, x => x.ExpirationTokenId, s =>
            {
                s.Delay = TimeSpan.FromSeconds(60);                       //    60 seconds wait
                s.Received = x => x.CorrelateById(m => m.Message.ExamId); //    Find me by ID when I get back.
            });

            // 4. Process Start
            Initially(
                When(StartExam) // When the "Start Exam" message comes from the API
                .Then(context =>
                {
                    // We transfer the data in the message to the database (Saga State)
                    context.Saga.StudentId = context.Message.StudentId;
                    context.Saga.StartTime = DateTime.UtcNow;
                })
                .TransitionTo(Preparing)
                .Publish(context => new PrepareQuestions(context.Saga.CorrelationId)) // Give the service that prepares the questions an order
                );

            // We are in the preparation
            During(Preparing
                , When(QuestionPrepared)
                .TransitionTo(InProgress)
                .Schedule(ExamTimeout, context => new ExamTimeoutExpired(context.Saga.CorrelationId))); // Now we set the 60 minute timer


            // Exam InProgress
            During(InProgress,

                // Scenario A: The student submitted the answers on time
                When(SubmitAnswers)
                .Unschedule(ExamTimeout)
                .Then(context => Console.WriteLine($"Tələbə {context.Saga.StudentId} cavabları təqdim etdi."))
                .TransitionTo(Finished)
                .Finalize(),

                // Scenario B: 60 minutes are up and the timer "rang" (Received)
                When(ExamTimeout.Received)
                .Then(context => Console.WriteLine($"VAXT BİTDİ! Imtahan: {context.Saga.CorrelationId} Telebe: {context.Saga.StudentId}."))
                .Publish(context => new ExamFinishedEvent(context.Saga.CorrelationId, context.Saga.StudentId, "TimeOut"))
                .TransitionTo(Finished)
                .Finalize());

            // SetCompletedWhenFinalized();
        }


    }
}
