using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Persistence;

public static partial class SeedData
{
    // Projects are the real portfolio pieces from the original site, reordered
    // so professional/enterprise work leads. Cover images point at files under
    // frontend/public/assets/projects/; entries without a real screenshot keep
    // a null cover so the UI can render a gradient placeholder instead of a
    // broken image.
    private static Project[] CreateProjects() =>
    [
        new()
        {
            Slug = "globalsuite-erp",
            Title = "GlobalSuite ERP",
            Summary = "Enterprise ERP platform for Nigerian Stockbrokers Ltd: 15+ modules, role-based access and reporting used daily by 100+ staff.",
            Description = "GlobalSuite is the enterprise ERP that powers Nigerian Stockbrokers Ltd (NSL). I maintain and evolve its 15+ modules — Accounting, Payments, Customer Management, KYC Dashboard, Risk and HR — working across the full stack of a long-lived C# / .NET system.\n\nRecent work includes optimizing SQL Server stored procedures for performance under real production load, extending modules for new business requirements, and implementing secure authentication with role-based access control for 100+ users. It is legacy-heavy work done with production rigor: careful schema changes, backwards-compatible rollouts and a strong focus on data integrity.",
            Status = ProjectStatus.InProgress,
            Featured = true,
            SortOrder = 1,
            CoverImageUrl = "/assets/projects/globalsuite-erp.png",
            Technologies = ["C#", ".NET Framework", "WinForms", "SQL Server", "Stored Procedures"],
        },
        new()
        {
            Slug = "outbound-ai-backend",
            Title = "Outbound AI Backend",
            Summary = "AI-powered call automation backend: speech-to-text, voice synthesis and scalable concurrent outbound calling.",
            Description = "An AI-powered call automation solution designed to help businesses manage outbound calls efficiently. This system leverages speech-to-text and text-to-speech technologies to handle customer interactions, automate follow-ups, and provide real-time insights. It integrates with multiple AI providers for transcription, natural language processing, and voice synthesis, ensuring high-quality, human-like conversations. The backend is built with FastAPI and optimized for scalability, handling concurrent calls seamlessly with Celery task processing and PostgreSQL for data storage.",
            DemoUrl = "https://outbound.im/",
            CoverImageUrl = "/assets/projects/outbound-ai-backend.png",
            Status = ProjectStatus.Completed,
            Featured = true,
            SortOrder = 2,
            Technologies = ["FastAPI", "PostgreSQL", "Celery", "Deepgram", "ElevenLabs"],
        },
        new()
        {
            Slug = "devatlas-academy",
            Title = "DevAtlas Academy",
            Summary = "Full-stack online learning platform with JWT authentication, admin dashboard, REST API and Azure CI/CD.",
            Description = "A full-stack online learning platform built for educators and students. Features secure user authentication (JWT + BCrypt), admin dashboard for course management, responsive frontend with modern UI, and a scalable REST API backend. Deployed on Microsoft Azure with CI/CD via GitHub Actions. Includes Swagger documentation, MongoDB Atlas integration, and production-grade environment configuration.",
            DemoUrl = "https://ashy-forest-0480f531e.3.azurestaticapps.net",
            CoverImageUrl = "/assets/projects/devatlas-academy.png",
            Status = ProjectStatus.Completed,
            Featured = true,
            SortOrder = 3,
            Technologies = ["React", "Vite", "Tailwind CSS", "Node.js", ".NET 8", "MongoDB Atlas", "Azure", "JWT Auth", "GitHub Actions"],
        },
        new()
        {
            Slug = "flower-imperials",
            Title = "Flower Imperials",
            Summary = "E-commerce storefront for a flower shop with product listings, cart, checkout and secure Flutterwave payments.",
            Description = "A fully functional e-commerce website for a flower shop, featuring product listings, a shopping cart, checkout functionality, and secure online payments via Flutterwave. The platform provides a seamless user experience with a modern UI, responsive design, and real-time API interactions.",
            DemoUrl = "https://lagfloral.netlify.app/",
            CoverImageUrl = "/assets/projects/flower-imperials.jpg",
            Status = ProjectStatus.Completed,
            Featured = true,
            SortOrder = 4,
            Technologies = ["React", "Node.js", "Tailwind CSS", "Flutterwave"],
        },
        new()
        {
            Slug = "wechora-foods",
            Title = "Wechora Foods",
            Summary = "E-commerce landing page for a food and nutrition company.",
            Description = "An e-commerce website landing page for a food and nutrition company called Wechora Foods. Built with React and Tailwind CSS, styled after the brand's identity and responsive across devices.",
            DemoUrl = "https://www.wechorafoods.com/",
            Status = ProjectStatus.Completed,
            Featured = false,
            SortOrder = 5,
            Technologies = ["React", "Node.js", "Tailwind CSS"],
        },
        new()
        {
            Slug = "finimage-inventory",
            Title = "FinImage Inventory",
            Summary = "Supermarket inventory management system.",
            Description = "A supermarket inventory management system for tracking products and stock levels day to day.",
            Status = ProjectStatus.Completed,
            Featured = false,
            SortOrder = 6,
            Technologies = [],
        },
    ];
}
