import { Routes } from '@angular/router';

/** Public site routes. Every page is lazy-loaded; the static shell (app.html) stays resident. */
export const siteRoutes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('../features/home/home-page.component').then((m) => m.HomePageComponent),
    title: 'Ali Ogochukwu Peter — Software & AI Engineer',
  },
  {
    path: 'projects',
    loadComponent: () =>
      import('../features/projects/projects-page.component').then((m) => m.ProjectsPageComponent),
    title: 'Projects — Ali Ogochukwu Peter',
  },
  {
    path: 'projects/:slug',
    loadComponent: () =>
      import('../features/projects/project-detail-page.component').then(
        (m) => m.ProjectDetailPageComponent,
      ),
    title: 'Project — Ali Ogochukwu Peter',
  },
  {
    path: 'skills',
    loadComponent: () =>
      import('../features/skills/skills-page.component').then((m) => m.SkillsPageComponent),
    title: 'Skills — Ali Ogochukwu Peter',
  },
  {
    path: 'experience',
    loadComponent: () =>
      import('../features/experience/experience-page.component').then(
        (m) => m.ExperiencePageComponent,
      ),
    title: 'Experience — Ali Ogochukwu Peter',
  },
  {
    path: 'blog',
    loadComponent: () =>
      import('../features/blog/blog-page.component').then((m) => m.BlogPageComponent),
    title: 'Blog — Ali Ogochukwu Peter',
  },
  {
    path: 'blog/:slug',
    loadComponent: () =>
      import('../features/blog/blog-detail-page.component').then((m) => m.BlogDetailPageComponent),
    title: 'Article — Ali Ogochukwu Peter',
  },
  {
    path: 'contact',
    loadComponent: () =>
      import('../features/contact/contact-page.component').then((m) => m.ContactPageComponent),
    title: 'Contact — Ali Ogochukwu Peter',
  },
  {
    path: 'ai-assistant',
    loadComponent: () =>
      import('../features/ai-assistant/ai-assistant-page.component').then(
        (m) => m.AiAssistantPageComponent,
      ),
    title: 'AI Assistant — Ali Ogochukwu Peter',
  },
  {
    path: 'ai',
    redirectTo: '/ai-assistant',
  },
  {
    path: 'ai-assistant/chat',
    loadComponent: () =>
      import('../features/ai-assistant/chat-widget.component').then(
        (m) => m.AiAssistantComponent,
      ),
    title: 'Ask AI — Ali Ogochukwu Peter',
  },
  { path: '**', redirectTo: '' },
];
