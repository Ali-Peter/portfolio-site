import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AdminAuthService } from '@core/services/admin-auth.service';

interface AdminNavLink {
  path: string;
  label: string;
}

/** Admin shell: top bar + section sidebar + routed page outlet. */
@Component({
  selector: 'app-admin-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './admin-layout.component.html',
})
export class AdminLayoutComponent {
  protected readonly auth = inject(AdminAuthService);

  protected readonly navLinks: AdminNavLink[] = [
    { path: '/admin/dashboard', label: 'Dashboard' },
    { path: '/admin/projects', label: 'Projects' },
    { path: '/admin/blog', label: 'Blog' },
    { path: '/admin/skills', label: 'Skills' },
    { path: '/admin/experience', label: 'Experience' },
    { path: '/admin/inbox', label: 'Inbox' },
  ];

  protected logout(): void {
    this.auth.logout();
  }
}
