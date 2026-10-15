import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ServerSafePage } from '@shared/server-safe-page';
import { PageHeaderComponent } from '@shared/ui/page-header/page-header.component';
import {
  ErrorStateComponent,
  LoadingStateComponent,
} from '@shared/ui/list-states/list-states.component';
import { BlogPostDto } from '@shared/models/api.models';

/** Single article. Unknown slugs render the not-found panel (API 404). */
@Component({
  selector: 'app-blog-detail-page',
  standalone: true,
  imports: [PageHeaderComponent, RouterLink, LoadingStateComponent, ErrorStateComponent],
  template: `
    @if (loading()) {
      <app-loading-state />
    } @else if (post(); as p) {
      <a routerLink="/blog" class="text-sm text-neutral-500 transition hover:text-amber-300">
        ← All articles
      </a>

      <header class="mt-6 max-w-3xl">
        @if (p.tags.length > 0) {
          <div class="mb-4 flex flex-wrap gap-1.5">
            @for (tag of p.tags; track tag) {
              <span class="chip chip-accent">{{ tag }}</span>
            }
          </div>
        }
        <h1 class="text-4xl font-bold tracking-tight text-white sm:text-5xl">{{ p.title }}</h1>
        <div class="mt-4 h-px w-16 bg-gradient-to-r from-amber-400 to-transparent"></div>
        <p class="mt-4 text-sm text-neutral-500">{{ meta(p) }}</p>
      </header>

      <article class="article-body mt-12 max-w-3xl" [innerHTML]="renderedBody()"></article>

      <div class="mt-10 border-t border-neutral-800 pt-6">
        <a routerLink="/blog" class="text-sm text-amber-400 transition hover:text-amber-300">
          ← All articles
        </a>
      </div>
    } @else if (notFound()) {
      <app-page-header title="Article not found" subtitle="No published post uses that address." />
      <a routerLink="/blog" class="btn btn-outline">← Back to blog</a>
    } @else {
      <app-error-state />
    }
  `,
})
export class BlogDetailPageComponent extends ServerSafePage implements OnInit {
  private readonly api = inject(PortfolioApiService);
  private readonly route = inject(ActivatedRoute);

  protected readonly post = signal<BlogPostDto | null>(null);
  protected readonly loading = signal(true);
  protected readonly notFound = signal(false);
  protected readonly loadError = signal(false);

  /** Markdown body rendered to HTML (Angular sanitizes innerHTML bindings). */
  protected readonly renderedBody = computed(() => {
    const post = this.post();
    return post ? renderMarkdown(post.bodyMarkdown) : '';
  });

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug') ?? '';
    if (!this.isBrowser) {
      this.loading.set(false);
      return;
    }
    this.api.getPostBySlug(slug).subscribe({
      next: (post) => {
        this.post.set(post);
        this.loading.set(false);
      },
      error: (error: unknown) => {
        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.notFound.set(true);
        } else {
          this.loadError.set(true);
        }
        this.loading.set(false);
      },
    });
  }

  protected meta(post: BlogPostDto): string {
    const date = post.publishedAt ? new Date(post.publishedAt).toLocaleDateString() : 'Draft';
    return `${date} · ${post.readingMinutes} min read`;
  }
}

/**
 * Minimal markdown renderer for the seeded post subset: `#`/`##`/`###`
 * headings, paragraphs, `-` lists, `**bold**` and `` `code` `` inline styles.
 * Output passes through Angular's built-in HTML sanitizer via [innerHTML].
 */
function renderMarkdown(markdown: string): string {
  const escapeHtml = (text: string): string =>
    text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

  const inline = (text: string): string =>
    escapeHtml(text)
      .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
      .replace(/`([^`]+)`/g, '<code>$1</code>');

  const lines = markdown.replace(/\r\n/g, '\n').split('\n');
  const out: string[] = [];
  let paragraph: string[] = [];
  let inList = false;

  const flushParagraph = (): void => {
    if (paragraph.length > 0) {
      out.push(`<p>${inline(paragraph.join(' '))}</p>`);
      paragraph = [];
    }
  };
  const closeList = (): void => {
    if (inList) {
      out.push('</ul>');
      inList = false;
    }
  };

  for (const raw of lines) {
    const line = raw.trim();
    if (!line) {
      flushParagraph();
      closeList();
      continue;
    }
    const heading = /^(#{1,3})\s+(.*)$/.exec(line);
    if (heading) {
      flushParagraph();
      closeList();
      const level = heading[1].length + 1; // demote: h1 → h2 (page owns h1)
      out.push(`<h${level}>${inline(heading[2])}</h${level}>`);
      continue;
    }
    if (line.startsWith('- ')) {
      flushParagraph();
      if (!inList) {
        out.push('<ul>');
        inList = true;
      }
      out.push(`<li>${inline(line.slice(2))}</li>`);
      continue;
    }
    paragraph.push(line);
  }
  flushParagraph();
  closeList();
  return out.join('\n');
}
