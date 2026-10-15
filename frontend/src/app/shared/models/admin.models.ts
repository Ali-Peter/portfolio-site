import { PagedResult, SkillCategory, SkillProficiency, ProjectStatus } from './api.models';

/** Typed mirror of the Portfolio.Api admin surface (Portfolio.Application.Dto). */

// --- Auth ------------------------------------------------------------------

export interface AdminLoginRequest {
  email: string;
  password: string;
}

export interface AdminLoginResponse {
  token: string;
  expiresAtUtc: string;
}

// --- Projects --------------------------------------------------------------

export interface AdminProjectDto {
  id: string;
  slug: string;
  title: string;
  summary: string;
  description: string;
  repoUrl: string | null;
  demoUrl: string | null;
  coverImageUrl: string | null;
  status: ProjectStatus;
  featured: boolean;
  startedOn: string | null;
  completedOn: string | null;
  sortOrder: number;
  technologies: string[];
}

export interface UpsertProjectRequest {
  slug: string;
  title: string;
  summary: string;
  description: string;
  repoUrl: string | null;
  demoUrl: string | null;
  coverImageUrl: string | null;
  status: ProjectStatus;
  featured: boolean;
  startedOn: string | null;
  completedOn: string | null;
  sortOrder: number;
  technologies: string[] | null;
}

// --- Blog ------------------------------------------------------------------

export interface AdminBlogPostDto {
  id: string;
  slug: string;
  title: string;
  excerpt: string;
  bodyMarkdown: string;
  coverImageUrl: string | null;
  publishedAt: string | null;
  readingMinutes: number;
  viewCount: number;
  isPublished: boolean;
  tags: string[];
}

export interface UpsertBlogPostRequest {
  slug: string;
  title: string;
  excerpt: string;
  bodyMarkdown: string;
  coverImageUrl: string | null;
  publishedAt: string | null;
  readingMinutes: number;
  isPublished: boolean;
  tags: string[] | null;
}

// --- Skills ----------------------------------------------------------------

export interface AdminSkillDto {
  id: string;
  name: string;
  category: SkillCategory;
  proficiency: SkillProficiency;
  sortOrder: number;
}

export interface UpsertSkillRequest {
  name: string;
  category: SkillCategory;
  proficiency: SkillProficiency;
  sortOrder: number;
}

// --- Experience ------------------------------------------------------------

export interface AdminExperienceDto {
  id: string;
  company: string;
  title: string;
  description: string;
  technologies: string[];
  startedOn: string | null;
  endedOn: string | null;
  isCurrent: boolean;
  sortOrder: number;
}

export interface UpsertExperienceRequest {
  company: string;
  title: string;
  description: string;
  technologies: string[] | null;
  startedOn: string | null;
  endedOn: string | null;
  isCurrent: boolean;
  sortOrder: number;
}

// --- Contact inbox ---------------------------------------------------------

export interface AdminContactMessageDto {
  id: string;
  name: string;
  email: string;
  message: string;
  isHandled: boolean;
  createdAtUtc: string;
}

export type AdminInboxPage = PagedResult<AdminContactMessageDto>;

// --- Reindex ---------------------------------------------------------------

export interface ReindexResult {
  chunks: number;
}
