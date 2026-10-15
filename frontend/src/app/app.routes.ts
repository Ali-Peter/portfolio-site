import { Routes } from '@angular/router';
import { siteRoutes } from './routes/site.routes';
import { adminRoutes } from './routes/admin.routes';

export const routes: Routes = [
  // Admin first: it carries its own shell + guard, and the public layout's
  // wildcard would otherwise swallow /admin/*.
  ...adminRoutes,
  // Public site: the layout (header/footer/AI FAB/toasts live at app root) wraps
  // every page via the router outlet.
  {
    path: '',
    loadComponent: () =>
      import('./layout/site-layout/site-layout.component').then(
        (m) => m.SiteLayoutComponent,
      ),
    children: siteRoutes,
  },
  { path: '**', redirectTo: '' },
];

