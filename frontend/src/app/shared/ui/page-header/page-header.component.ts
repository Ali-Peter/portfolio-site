import { Component, input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  standalone: true,
  template: `
    <header class="mb-10">
      @if (kicker()) {
        <p class="kicker mb-3">{{ kicker() }}</p>
      }
      <h1 class="text-4xl font-bold tracking-tight text-white text-balance sm:text-5xl">{{ title() }}</h1>
      <div class="mt-4 h-px w-24 bg-linear-to-r from-amber-400 to-transparent"></div>
      @if (subtitle()) {
        <p class="mt-4 max-w-2xl text-lg leading-relaxed text-neutral-400">{{ subtitle() }}</p>
      }
    </header>
  `,
})
export class PageHeaderComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
  /** Small uppercase eyebrow above the title (e.g. section name). */
  readonly kicker = input<string>();
}
