import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageHeaderComponent } from '@shared/ui/page-header/page-header.component';
import { AiAssistantComponent } from './chat-widget.component';

/** The RAG chat widget: streaming SSE answer from the portfolio index. */
@Component({
  selector: 'app-ai-assistant-page',
  standalone: true,
  imports: [PageHeaderComponent, RouterLink, AiAssistantComponent],
  template: `
    <app-page-header
      kicker="Assistant"
      title="AI Assistant"
      subtitle="Ask about my work — retrieval-augmented answers over this very portfolio."
    />
    <app-ai-assistant />
    <p class="mt-6">
      <a routerLink="/contact" class="text-sm text-amber-400 transition hover:text-amber-300">
        Meanwhile, the fastest way to reach me is the contact form →
      </a>
    </p>
  `,
})
export class AiAssistantPageComponent {}
