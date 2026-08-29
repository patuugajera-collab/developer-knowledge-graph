import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CentralTechnology,
  DashboardStats,
  DeveloperDetail,
  DeveloperProject,
  DeveloperRepository,
  DeveloperSkill,
  DeveloperSummary,
  GraphResponse,
  HealthResponse,
  IndirectTechnology,
  OrganizationSummary,
  PaginatedResponse,
  ProjectContributor,
  ProjectDependency,
  ProjectDeveloper,
  ProjectDetail,
  ProjectRepository,
  ProjectSummary,
  ProjectTask,
  ProjectTechnology,
  RecommendedDeveloper,
  SearchResponse,
  ShortestPath,
  TechnologyDetail,
  TechnologyDeveloper,
  TechnologyProject,
  TechnologySummary,
} from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = environment.apiUrl;

  constructor(private readonly http: HttpClient) {}

  // ---- Health & dashboard ----
  getHealth(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>(`${this.base}/health`);
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.base}/dashboard/stats`);
  }

  // ---- Search ----
  search(q: string, limit?: number): Observable<SearchResponse> {
    let params = new HttpParams().set('q', q);
    if (limit != null) {
      params = params.set('limit', limit);
    }
    return this.http.get<SearchResponse>(`${this.base}/search`, { params });
  }

  // ---- Developers ----
  getDevelopers(search?: string, page = 1, pageSize = 20): Observable<PaginatedResponse<DeveloperSummary>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<PaginatedResponse<DeveloperSummary>>(`${this.base}/developers`, { params });
  }

  getDeveloper(id: string): Observable<DeveloperDetail> {
    return this.http.get<DeveloperDetail>(`${this.base}/developers/${id}`);
  }

  getDeveloperProjects(id: string): Observable<DeveloperProject[]> {
    return this.http.get<DeveloperProject[]>(`${this.base}/developers/${id}/projects`);
  }

  getDeveloperSkills(id: string): Observable<DeveloperSkill[]> {
    return this.http.get<DeveloperSkill[]>(`${this.base}/developers/${id}/skills`);
  }

  getDeveloperRepositories(id: string): Observable<DeveloperRepository[]> {
    return this.http.get<DeveloperRepository[]>(`${this.base}/developers/${id}/repositories`);
  }

  // ---- Projects ----
  getProjects(search?: string, status?: string, page = 1, pageSize = 20): Observable<PaginatedResponse<ProjectSummary>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) {
      params = params.set('search', search);
    }
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<PaginatedResponse<ProjectSummary>>(`${this.base}/projects`, { params });
  }

  getProject(id: string): Observable<ProjectDetail> {
    return this.http.get<ProjectDetail>(`${this.base}/projects/${id}`);
  }

  getProjectDependencies(id: string, maxDepth = 5): Observable<ProjectDependency[]> {
    return this.http.get<ProjectDependency[]>(`${this.base}/projects/${id}/dependencies`, {
      params: new HttpParams().set('maxDepth', maxDepth),
    });
  }

  getProjectTechnologies(id: string): Observable<ProjectTechnology[]> {
    return this.http.get<ProjectTechnology[]>(`${this.base}/projects/${id}/technologies`);
  }

  getProjectDevelopers(id: string): Observable<ProjectDeveloper[]> {
    return this.http.get<ProjectDeveloper[]>(`${this.base}/projects/${id}/developers`);
  }

  getProjectRepositories(id: string): Observable<ProjectRepository[]> {
    return this.http.get<ProjectRepository[]>(`${this.base}/projects/${id}/repositories`);
  }

  getProjectTasks(id: string): Observable<ProjectTask[]> {
    return this.http.get<ProjectTask[]>(`${this.base}/projects/${id}/tasks`);
  }

  getProjectContributors(id: string): Observable<ProjectContributor[]> {
    return this.http.get<ProjectContributor[]>(`${this.base}/projects/${id}/contributors`);
  }

  getRecommendedDevelopers(id: string): Observable<RecommendedDeveloper[]> {
    return this.http.get<RecommendedDeveloper[]>(`${this.base}/projects/${id}/recommended-developers`);
  }

  getIndirectTechnologies(id: string, maxDepth = 5): Observable<IndirectTechnology[]> {
    return this.http.get<IndirectTechnology[]>(`${this.base}/projects/${id}/indirect-technologies`, {
      params: new HttpParams().set('maxDepth', maxDepth),
    });
  }

  // ---- Technologies ----
  getTechnologies(search?: string, category?: string, page = 1, pageSize = 20): Observable<PaginatedResponse<TechnologySummary>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) {
      params = params.set('search', search);
    }
    if (category) {
      params = params.set('category', category);
    }
    return this.http.get<PaginatedResponse<TechnologySummary>>(`${this.base}/technologies`, { params });
  }

  getTechnology(id: string): Observable<TechnologyDetail> {
    return this.http.get<TechnologyDetail>(`${this.base}/technologies/${id}`);
  }

  getTechnologyDevelopers(id: string): Observable<TechnologyDeveloper[]> {
    return this.http.get<TechnologyDeveloper[]>(`${this.base}/technologies/${id}/developers`);
  }

  getTechnologyProjects(id: string): Observable<TechnologyProject[]> {
    return this.http.get<TechnologyProject[]>(`${this.base}/technologies/${id}/projects`);
  }

  getTechnologyCategories(): Observable<string[]> {
    return this.http.get<string[]>(`${this.base}/technologies/categories`);
  }

  getCentralTechnologies(limit = 8): Observable<CentralTechnology[]> {
    return this.http.get<CentralTechnology[]>(`${this.base}/graph/central-technologies`, {
      params: new HttpParams().set('limit', limit),
    });
  }

  // ---- Organizations ----
  getOrganizations(): Observable<OrganizationSummary[]> {
    return this.http.get<OrganizationSummary[]>(`${this.base}/organizations`);
  }

  getOrganizationDevelopers(id: string): Observable<DeveloperSummary[]> {
    return this.http.get<DeveloperSummary[]>(`${this.base}/organizations/${id}/developers`);
  }

  // ---- Graph ----
  getGraph(entityType: string, id: string, maxDepth = 3): Observable<GraphResponse> {
    const params = new HttpParams().set('entityType', entityType).set('id', id).set('maxDepth', maxDepth);
    return this.http.get<GraphResponse>(`${this.base}/graph/explore`, { params });
  }

  getShortestPath(developerId: string, projectId: string): Observable<ShortestPath> {
    const params = new HttpParams().set('developerId', developerId).set('projectId', projectId);
    return this.http.get<ShortestPath>(`${this.base}/graph/shortest-path`, { params });
  }
}