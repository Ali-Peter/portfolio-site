import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

interface NavLink {
  path: string;
  label: string;
}

interface SocialLink {
  href: string;
  label: string;
}

@Component({
  selector: 'app-site-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './site-layout.component.html',
})
export class SiteLayoutComponent {
  protected readonly navLinks: NavLink[] = [
    { path: '/', label: 'Home' },
    { path: '/projects', label: 'Projects' },
    { path: '/skills', label: 'Skills' },
    { path: '/experience', label: 'Experience' },
    { path: '/blog', label: 'Blog' },
    { path: '/contact', label: 'Contact' },
  ];

  protected readonly socialLinks: SocialLink[] = [
    { href: 'https://github.com/Ali-Peter', label: 'GitHub' },
    { href: 'https://dev.to/ali-peter', label: 'Dev.to' },
    { href: 'https://x.com/ali_ogochu2581', label: 'X / Twitter' },
    { href: 'mailto:aliogochukwu06@gmail.com', label: 'Email' },
  ];

  protected readonly year = new Date().getFullYear();

  protected readonly showAiAssistant = signal(false);

  protected trackByPath(_index: number, link: NavLink): string {
    return link.path;
  }
}

