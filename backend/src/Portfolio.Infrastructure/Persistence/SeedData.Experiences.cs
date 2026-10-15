using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence;

public static partial class SeedData
{
    // Dates are null on purpose: the original portfolio does not state employment
    // dates anywhere, and inventing them would put false claims in the database.
    private static Experience[] CreateExperiences() =>
    [
        new()
        {
            Company = "Nigerian Stockbrokers Ltd (NSL)",
            Title = "Software Engineer",
            Description = "Software engineer on GlobalSuite ERP, the enterprise platform that runs the firm's day-to-day operations. Own 15+ modules (Accounting, Payments, Customer Management, KYC Dashboard, Risk, HR): extending functionality, optimizing SQL Server stored procedures for performance, and implementing secure authentication and role-based access control for 100+ users.",
            Technologies = ["C#", ".NET Framework", "SQL Server", "Stored Procedures"],
            IsCurrent = true,
            SortOrder = 1,
        },
        new()
        {
            Company = "Outbound AI",
            Title = "Backend / AI Engineer",
            Description = "Built the backend for an AI-powered call automation product: speech-to-text and text-to-speech integrations (Deepgram, ElevenLabs), NLP pipelines for customer interactions, and Celery-based task processing for high-volume concurrent outbound calling on FastAPI + PostgreSQL.",
            Technologies = ["FastAPI", "PostgreSQL", "Celery", "Deepgram", "ElevenLabs"],
            IsCurrent = false,
            SortOrder = 2,
        },
        new()
        {
            Company = "HNG Internship",
            Title = "Intern",
            Description = "Completed the HNG Internship, a intensive multi-stage software development program covering programming fundamentals, teamwork and delivery on real-world tasks.",
            Technologies = [],
            IsCurrent = false,
            SortOrder = 3,
        },
    ];
}
