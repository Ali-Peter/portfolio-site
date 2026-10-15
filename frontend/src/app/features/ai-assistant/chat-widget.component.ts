import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PortfolioApiService } from '@core/services/portfolio-api.service';
import { ChatSourceDto } from '@shared/models/api.models';
import { MarkdownPipe } from '@shared/pipes/markdown.pipe';
import {
  EmptyStateComponent,
  ErrorStateComponent,
  LoadingStateComponent,
} from '@shared/ui/list-states/list-states.component';

/** Client-side guardrail that mirrors the API prompt: refuse off-topic questions. */
const OFF_TOPIC_PATTERNS = [
  /^(weather|today|week|month|year|time|date)\b/i,
  /^(politics|election|vote|government|lawyer|doctor|teacher|news|headline)\b/i,
  /^(code|write|write an essay|help me write|what is the capital)\b/i,
  /^(tell me about you|who are you|your name)\b/i,
];

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MarkdownPipe,
    LoadingStateComponent,
    ErrorStateComponent,
    EmptyStateComponent,
  ],
  templateUrl: './chat-widget.component.html',
})
export class AiAssistantComponent {
  private readonly api = inject(PortfolioApiService);

  question = '';
  /** The assistant's streamed answer, displayed as it arrives. */
  answer = signal<string>('');
  /** Grounding context shown as source chips (populated by the `sources` frame). */
  sources = signal<ChatSourceDto[]>([]);
  status = signal<'idle' | 'loading' | 'error' | 'empty'>('idle');
  error = signal<string>('');

  private abortController = new AbortController();
  private lastMessage = '';

  submit(): void {
    const message = this.question.trim();
    this.question = '';
    this.lastMessage = message;

    if (!message) {
      return;
    }

    // Client-side guardrail: the assistant only answers portfolio questions.
    if (OFF_TOPIC_PATTERNS.some((re) => re.test(message))) {
      this.status.set('empty');
      this.answer.set('I can only answer questions about Ali\'s portfolio — projects, skills, experience and blog posts. What would you like to ask?');
      this.error.set('');
      return;
    }

    this.abortController.abort();
    this.abortController = new AbortController();
    this.status.set('loading');
    this.answer.set('');
    this.sources.set([]);
    this.error.set('');

    this.api.askAiAssistant(message).subscribe({
      next: (frame) => {
        if (!frame) {
          return;
        }
        switch (frame.type) {
          case 'sources':
            this.sources.set(frame.sources ?? []);
            break;
          case 'delta':
            // Keep a running accumulation so the UI can replay the delta stream.
            // The stream may interleave multiple deltas; we append the incoming
            // text to the previous answer so short turns still reconstruct cleanly.
            this.answer.update((prev) => prev + (frame.text ?? ''));
            break;
          case 'done':
            this.status.set('idle');
            break;
        }
      },
      error: (err) => {
        const isAbort = err && (err.name === 'AbortError' || err.status === 0);
        if (isAbort) {
          return;
        }
        this.status.set('error');
        this.error.set(err && err.message ? err.message : 'The assistant is unavailable. Please try again.');
      },
    });
  }

  /** Call `stop()` from the template to abort an in-flight stream. */
  get canStop(): boolean {
    return this.status() === 'loading';
  }

  stop(): void {
    this.abortController.abort();
    this.status.set('idle');
    this.error.set('');
  }

  /** Retry handler wired to the shared error state. */
  retry = (): void => {
    if (this.lastMessage) {
      this.question = this.lastMessage;
      this.submit();
    }
  };

  /** Maps a grounding source to its portfolio route (used for chip links). */
  resourceUrl(source: ChatSourceDto): string {
    const slug = encodeURIComponent(source.slug ?? '');
    switch (source.type?.toLowerCase()) {
      case 'project':
        return `/projects/${slug}`;
      case 'post':
      case 'blog':
        return `/blog/${slug}`;
      default:
        return '/';
    }
  }
}
