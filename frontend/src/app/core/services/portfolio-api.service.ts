import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  BlogPostDto,
  ChatStreamEvent,
  ContactMessageAcceptedDto,
  CreateContactMessageRequest,
  ExperienceDto,
  PagedResult,
  ProjectDto,
  SkillDto,
} from '@shared/models/api.models';

/**
 * Typed client for the public Portfolio.Api v1 surface.
 * Base URL comes from the file-replaced environment — never from `environment.dev`.
 */
@Injectable({ providedIn: 'root' })
export class PortfolioApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/v1`;

  getProjects(page = 1, pageSize = 20, tag?: string): Observable<PagedResult<ProjectDto>> {
    return this.http.get<PagedResult<ProjectDto>>(`${this.baseUrl}/projects`, {
      params: {
        page: String(page),
        pageSize: String(pageSize),
        ...(tag ? { tag } : {}),
      },
    });
  }

  getProjectBySlug(slug: string): Observable<ProjectDto> {
    return this.http.get<ProjectDto>(
      `${this.baseUrl}/projects/${encodeURIComponent(slug)}`,
    );
  }

  getSkills(): Observable<SkillDto[]> {
    return this.http.get<SkillDto[]>(`${this.baseUrl}/skills`);
  }

  getExperience(): Observable<ExperienceDto[]> {
    return this.http.get<ExperienceDto[]>(`${this.baseUrl}/experience`);
  }

  getPosts(page = 1, pageSize = 20, search?: string): Observable<PagedResult<BlogPostDto>> {
    return this.http.get<PagedResult<BlogPostDto>>(`${this.baseUrl}/blog`, {
      params: {
        page: String(page),
        pageSize: String(pageSize),
        ...(search ? { search } : {}),
      },
    });
  }

  getPostBySlug(slug: string): Observable<BlogPostDto> {
    return this.http.get<BlogPostDto>(`${this.baseUrl}/blog/${encodeURIComponent(slug)}`);
  }

  submitContact(request: CreateContactMessageRequest): Observable<ContactMessageAcceptedDto> {
    return this.http.post<ContactMessageAcceptedDto>(`${this.baseUrl}/contact`, request);
  }

  /**
   * Streams the RAG assistant answer as Server-Sent Events. Each frame is
   * passed to the subscriber as a parsed {@link ChatStreamEvent}:
   * <code>sources</code> (retrieved grounding chunks, sent first),
   * <code>delta</code> (answer text increment), <code>done</code> (terminal).
   *
   * Grounding behavior (per the prompt guardrail): the answer may only use
   * the retrieved portfolio context; unrelated questions are refused; when the
   * index is empty the assistant says so explicitly.
   */
  askAiAssistant(message: string): Observable<ChatStreamEvent> {
    const body = JSON.stringify({ message });
    return this.http.post(`${this.baseUrl}/ai/assistant`, body, {
      headers: { 'Content-Type': 'application/json' },
      responseType: 'text',
    }).pipe(
      // `HttpClient.post` with `responseType: 'text'` returns the raw event
      // stream as a single string. Split on blank lines (the SSE framing)
      // into individual frames, then return the last (terminal) event.
      map((raw) => {
        const frames: ChatStreamEvent[] = [];
        const lines = raw.split('\n');
        for (let i = 0; i < lines.length; i += 2) {
          const line = lines[i]?.trimStart();
          if (line === 'data:') {
            continue;
          }
          if (line?.startsWith('data: ')) {
            try {
              frames.push(JSON.parse(line.substring(6)));
            } catch {
              // Non-JSON frame (e.g. a comment) — ignore.
            }
          }
        }
        return frames[frames.length - 1];
      }),
    );
  }
}
