/**
 * Contact form markup, kept separate so the component file stays small.
 * The `website` field is a honeypot: off-screen, never autofilled.
 */
export const CONTACT_FORM_TEMPLATE = `
  <app-page-header
    kicker="Contact"
    title="Let's talk"
    subtitle="Direct message — I usually reply within a day."
  />

  <div class="grid gap-10 lg:grid-cols-[1.4fr_1fr]">
    <div>
      @if (sent()) {
        <div class="card border-emerald-800 bg-emerald-950/40 px-6 py-10 text-center">
          <p class="text-lg font-semibold text-emerald-100">Message sent — thank you.</p>
          <p class="mt-2 text-sm text-emerald-300/80">
            I'll get back to you at the address you provided.
          </p>
        </div>
      } @else {
        <form [formGroup]="form" (ngSubmit)="submit()" class="card space-y-5 p-6 sm:p-8" novalidate>
          <div>
            <label class="mb-1.5 block text-sm font-medium text-neutral-300" for="contact-name">
              Name
            </label>
            <input
              id="contact-name"
              type="text"
              formControlName="name"
              autocomplete="name"
              class="input"
              placeholder="Your name"
            />
            @if (showError('name')) {
              <p class="field-error">{{ fieldError('name') }}</p>
            }
          </div>
          <div>
            <label class="mb-1.5 block text-sm font-medium text-neutral-300" for="contact-email">
              Email
            </label>
            <input
              id="contact-email"
              type="email"
              formControlName="email"
              autocomplete="email"
              class="input"
              placeholder="you@example.com"
            />
            @if (showError('email')) {
              <p class="field-error">{{ fieldError('email') }}</p>
            }
          </div>
          <div>
            <label class="mb-1.5 block text-sm font-medium text-neutral-300" for="contact-message">
              Message
            </label>
            <textarea
              id="contact-message"
              formControlName="message"
              rows="6"
              class="input resize-y"
              placeholder="What would you like to talk about?"
            ></textarea>
            @if (showError('message')) {
              <p class="field-error">{{ fieldError('message') }}</p>
            }
          </div>
          <div class="absolute -left-[9999px] top-auto h-px w-px overflow-hidden" aria-hidden="true">
            <label for="contact-website">Website</label>
            <input id="contact-website" type="text" formControlName="website" tabindex="-1" autocomplete="off" />
          </div>
          <button type="submit" [disabled]="sending()" class="btn btn-primary w-full py-3 sm:w-auto">
            {{ sending() ? 'Sending…' : 'Send message' }}
          </button>
        </form>
      }
    </div>

    <aside class="space-y-4">
      <div class="card p-6">
        <p class="kicker mb-3">Reach me directly</p>
        <a
          href="mailto:aliogochukwu06@gmail.com"
          class="text-sm text-neutral-300 transition hover:text-amber-300"
        >
          aliogochukwu06@gmail.com
        </a>
        <ul class="mt-4 space-y-2.5 text-sm">
          <li>
            <a
              href="https://github.com/Ali-Peter"
              target="_blank"
              rel="noopener noreferrer"
              class="text-neutral-400 transition hover:text-amber-300"
            >
              GitHub ↗
            </a>
          </li>
          <li>
            <a
              href="https://dev.to/ali-peter"
              target="_blank"
              rel="noopener noreferrer"
              class="text-neutral-400 transition hover:text-amber-300"
            >
              Dev.to ↗
            </a>
          </li>
          <li>
            <a
              href="https://x.com/ali_ogochu2581"
              target="_blank"
              rel="noopener noreferrer"
              class="text-neutral-400 transition hover:text-amber-300"
            >
              X / Twitter ↗
            </a>
          </li>
        </ul>
      </div>
      <div class="card p-6">
        <p class="kicker mb-3">Good to know</p>
        <ul class="space-y-3 text-sm leading-relaxed text-neutral-400">
          <li class="flex gap-2.5">
            <span class="mt-[7px] h-1.5 w-1.5 shrink-0 rounded-full bg-amber-400/80" aria-hidden="true"></span>
            <span>Open to freelance and full-time roles.</span>
          </li>
          <li class="flex gap-2.5">
            <span class="mt-[7px] h-1.5 w-1.5 shrink-0 rounded-full bg-amber-400/80" aria-hidden="true"></span>
            <span>Backend, full-stack and AI engineering work.</span>
          </li>
          <li class="flex gap-2.5">
            <span class="mt-[7px] h-1.5 w-1.5 shrink-0 rounded-full bg-amber-400/80" aria-hidden="true"></span>
            <span>Usually replies within 24 hours.</span>
          </li>
        </ul>
      </div>
    </aside>
  </div>
`;
