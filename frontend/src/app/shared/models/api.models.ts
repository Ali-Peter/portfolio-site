/** Typed mirror of Portfolio.Application DTOs. Field names match the API JSON exactly. */

export type ProjectStatus = 'Planned' | 'InProgress' | 'Completed' | 'Archived';

export type SkillCategory = 'Backend' | 'Frontend' | 'DataAndInfra' | 'AiMl' | 'Tools';

export type SkillProficiency = 'Beginner' | 'Intermediate' | 'Proficient' | 'Advanced' | 'Expert';

export interface ProjectDto {
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
  technologies: string[];
}

export interface BlogPostDto {
  slug: string;
  title: string;
  excerpt: string;
  bodyMarkdown: string;
  coverImageUrl: string | null;
  publishedAt: string | null;
  readingMinutes: number;
  viewCount: number;
  tags: string[];
}

export interface SkillDto {
  name: string;
  category: SkillCategory;
  proficiency: SkillProficiency;
  sortOrder: number;
}

export interface ExperienceDto {
  company: string;
  title: string;
  description: string;
  technologies: string[];
  startedOn: string | null;
  endedOn: string | null;
  isCurrent: boolean;
  sortOrder: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CreateContactMessageRequest {
  name: string;
  email: string;
  message: string;
  /** Honeypot — must stay empty for humans; bots fill it and are silently accepted. */
  website?: string | null;
}

export interface ContactMessageAcceptedDto {
  id: string;
  receivedAtUtc: string;
}

/**
 * Typed mirror of the RAG assistant surface (Portfolio.Application.Dto).
 * Field names match the API JSON exactly so the SSE frames deserialize
 * against these records.
 */
export interface ChatSourceDto {
  type: string;
  title: string;
  slug: string;
}

export interface CreateChatRequest {
  message: string;
}

export interface ChatStreamEvent {
  type: 'sources' | 'delta' | 'done';
  text?: string;
  sources?: ChatSourceDto[];
}
