import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '@shared/models/api.models';
import {
  AdminBlogPostDto,
  AdminContactMessageDto,
  AdminExperienceDto,
  AdminInboxPage,
  AdminProjectDto,
  AdminSkillDto,
  ReindexResult,
  UpsertBlogPostRequest,
  UpsertExperienceRequest,
  UpsertProjectRequest,
  UpsertSkillRequest,
} from '@shared/models/admin.models';

/**
 * Typed client for the Portfolio.Api v1 admin surface. The bearer token is
 * attached by authInterceptor — never set headers here. The generic <T>
 * request shape follows a pattern the public service has always used.
 */
@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/api/v1/admin`;

  // --- Projects ------------------------------------------------------------

  getProjects(): Observable<AdminProjectDto[]> {
    return this.http.get<AdminProjectDto[]>(`${this.base}/projects`);
  }

  getProject(id: string): Observable<AdminProjectDto> {
    return this.http.get<AdminProjectDto>(`${this.base}/projects/${id}`);
  }

  createProject(request: UpsertProjectRequest): Observable<AdminProjectDto> {
    return this.http.post<AdminProjectDto>(`${this.base}/projects`, request);
  }

  updateProject(id: string, request: UpsertProjectRequest): Observable<AdminProjectDto> {
    return this.http.put<AdminProjectDto>(`${this.base}/projects/${id}`, request);
  }

  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/projects/${id}`);
  }

  // --- Blog ----------------------------------------------------------------

  getPosts(): Observable<AdminBlogPostDto[]> {
    return this.http.get<AdminBlogPostDto[]>(`${this.base}/blog`);
  }

  getPost(id: string): Observable<AdminBlogPostDto> {
    return this.http.get<AdminBlogPostDto>(`${this.base}/blog/${id}`);
  }

  createPost(request: UpsertBlogPostRequest): Observable<AdminBlogPostDto> {
    return this.http.post<AdminBlogPostDto>(`${this.base}/blog`, request);
  }

  updatePost(id: string, request: UpsertBlogPostRequest): Observable<AdminBlogPostDto> {
    return this.http.put<AdminBlogPostDto>(`${this.base}/blog/${id}`, request);
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/blog/${id}`);
  }

  // --- Skills --------------------------------------------------------------

  getSkills(): Observable<AdminSkillDto[]> {
    return this.http.get<AdminSkillDto[]>(`${this.base}/skills`);
  }

  createSkill(request: UpsertSkillRequest): Observable<AdminSkillDto> {
    return this.http.post<AdminSkillDto>(`${this.base}/skills`, request);
  }

  updateSkill(id: string, request: UpsertSkillRequest): Observable<AdminSkillDto> {
    return this.http.put<AdminSkillDto>(`${this.base}/skills/${id}`, request);
  }

  deleteSkill(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/skills/${id}`);
  }

  // --- Experience ----------------------------------------------------------

  getExperiences(): Observable<AdminExperienceDto[]> {
    return this.http.get<AdminExperienceDto[]>(`${this.base}/experience`);
  }

  createExperience(request: UpsertExperienceRequest): Observable<AdminExperienceDto> {
    return this.http.post<AdminExperienceDto>(`${this.base}/experience`, request);
  }

  updateExperience(
    id: string,
    request: UpsertExperienceRequest,
  ): Observable<AdminExperienceDto> {
    return this.http.put<AdminExperienceDto>(`${this.base}/experience/${id}`, request);
  }

  deleteExperience(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/experience/${id}`);
  }

  // --- Inbox ---------------------------------------------------------------

  getInbox(page = 1, pageSize = 20, handled?: boolean): Observable<AdminInboxPage> {
    let params = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    if (handled !== undefined) {
      params = params.set('handled', String(handled));
    }
    return this.http.get<PagedResult<AdminContactMessageDto>>(`${this.base}/inbox`, {
      params,
    });
  }

  markHandled(id: string, handled: boolean): Observable<void> {
    return this.http.patch<void>(`${this.base}/inbox/${id}/handled`, { handled });
  }

  deleteMessage(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/inbox/${id}`);
  }

  // --- Maintenance ---------------------------------------------------------

  reindex(): Observable<ReindexResult> {
    return this.http.post<ReindexResult>(`${this.base}/reindex`, {});
  }
}
