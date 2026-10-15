import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  { path: '', renderMode: RenderMode.Prerender },
  { path: 'projects', renderMode: RenderMode.Prerender },
  { path: 'blog', renderMode: RenderMode.Prerender },
  { path: 'contact', renderMode: RenderMode.Prerender },
  { path: 'ai-assistant', renderMode: RenderMode.Prerender },
  // Slug detail pages and unknown URLs render on demand: their data comes
  // from the API at request time (Phase 4), so prerendering cannot enumerate them.
  { path: '**', renderMode: RenderMode.Server },
];

