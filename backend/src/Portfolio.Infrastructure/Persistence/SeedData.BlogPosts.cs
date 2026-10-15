using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence;

public static partial class SeedData
{
    // The original portfolio has no local blog content (articles live on dev.to),
    // so the blog is seeded with posts grounded in the real projects from the old
    // site: the portfolio rebuild itself and the Outbound AI backend. Replace or
    // extend with real posts.
    private static BlogPost[] CreateBlogPosts() =>
    [
        new()
        {
            Slug = "hello-world",
            Title = "Welcome to My Engineering Blog",
            Excerpt = "Why I rebuilt this portfolio on a clean, testable architecture — and what I plan to write about next.",
            BodyMarkdown = """
                # Hello world

                Welcome! This blog is part of my portfolio rebuild: an **ASP.NET Core** +
                **Angular** + **PostgreSQL** system designed with clean architecture,
                automated tests, and an AI-powered RAG assistant.

                ## Why rebuild

                My previous portfolio did the job visually, but the code told a
                different story: no types, no tests, no database, and a backend that
                could not survive a bad day. This version is built the way I would
                build any production system.

                ## What's next

                - Deep dives into the architecture of this very site
                - PostgreSQL patterns I use in production
                - Practical RAG: chunking, embeddings, and evaluation

                Stay tuned.
                """,
            IsPublished = true,
            PublishedAt = DateTime.UtcNow,
            ReadingMinutes = 3,
            Tags = ["meta", "architecture"],
        },
        new()
        {
            Slug = "scaling-an-ai-calling-backend",
            Title = "What Scaling an AI Calling Backend Taught Me",
            Excerpt = "Lessons from building the Outbound AI backend: concurrent voice calls, speech-to-text/TTS pipelines, and Celery doing the heavy lifting.",
            BodyMarkdown = """
                # What scaling an AI calling backend taught me

                On **Outbound AI** I built the backend for an AI-powered outbound
                calling product: FastAPI, PostgreSQL, Celery, Deepgram for
                transcription and ElevenLabs for voice synthesis. Here are the
                lessons that stuck.

                ## 1. Voice calls are a queue problem

                A call is a long-lived, stateful job. You cannot run them in request
                handlers. Celery workers own the call lifecycle; the HTTP API stays
                thin. Once calls became tasks, retries, concurrency limits and
                observability all fell into place.

                ## 2. Speech providers are I/O, not CPU

                Deepgram and ElevenLabs latency dominates everything. The rule that
                emerged: never block a worker on a single provider call when you can
                overlap I/O — pipeline the audio in segments and keep connections
                warm.

                ## 3. PostgreSQL still has to be boring

                Every call session, transcript and outcome lands in Postgres. Clear
                schemas, indexes on the queries you actually run, and migrations you
                trust beat any "scaling" trick. Boring data layers keep AI products
                debuggable.

                ## Takeaway

                Most of the "AI" work was distributed-systems work: queues, retries,
                idempotency, and honest logging. The models were the easy part.
                """,
            IsPublished = true,
            PublishedAt = DateTime.UtcNow.AddDays(-7),
            ReadingMinutes = 4,
            Tags = ["ai", "fastapi", "celery"],
        },
    ];
}
