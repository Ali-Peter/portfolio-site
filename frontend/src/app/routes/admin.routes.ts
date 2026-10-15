import { Routes } from '@angular/router';
import { adminGuard } from '@core/guards/admin.guard';

/**
 * Admin area routes. The login page sits outside the guarded layout so it
 * stays reachable; everything else renders inside AdminLayoutComponent
 * (sidebar shell + <router-outlet/>), lazily loaded per page.
 */
export const adminRoutes: Routes = [
  {
    path: 'admin/login',
    loadComponent: () =>
      import('../features/admin/login/login-page.component').then(
        (m) => m.AdminLoginPageComponent,
      ),
    title: 'Admin login — Ali Ogochukwu Peter',
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('../features/admin/admin-layout.component').then(
        (m) => m.AdminLayoutComponent,
      ),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('../features/admin/dashboard/dashboard-page.component').then(
            (m) => m.AdminDashboardPageComponent,
          ),
        title: 'Admin dashboard — Ali Ogochukwu Peter',
      },
      {
        path: 'projects',
        loadComponent: () =>
          import('../features/admin/projects/projects-page.component').then(
            (m) => m.AdminProjectsPageComponent,
          ),
        title: 'Manage projects — Admin',
      },
      {
        path: 'blog',
        loadComponent: () =>
          import('../features/admin/blog/blog-page.component').then(
            (m) => m.AdminBlogPageComponent,
          ),
        title: 'Manage blog — Admin',
      },
      {
        path: 'skills',
        loadComponent: () =>
          import('../features/admin/skills/skills-page.component').then(
            (m) => m.AdminSkillsPageComponent,
          ),
        title: 'Manage skills — Admin',
      },
      {
        path: 'experience',
        loadComponent: () =>
          import('../features/admin/experience/experience-page.component').then(
            (m) => m.AdminExperiencePageComponent,
          ),
        title: 'Manage experience — Admin',
      },
      {
        path: 'inbox',
        loadComponent: () =>
          import('../features/admin/inbox/inbox-page.component').then(
            (m) => m.AdminInboxPageComponent,
          ),
        title: 'Contact inbox — Admin',
      },
      { path: '**', redirectTo: 'dashboard' },
    ],
  },
];
