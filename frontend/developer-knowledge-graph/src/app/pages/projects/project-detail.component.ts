import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { Subject, forkJoin, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import {
  IndirectTechnology,
  ProjectContributor,
  ProjectDependency,
  ProjectDetail,
  ProjectDeveloper,
  ProjectRepository,
  ProjectTask,
  ProjectTechnology,
  RecommendedDeveloper,
} from '../../models/api-models';

@Component({
  selector: 'app-project-detail',
  imports: [MatCardModule, MatIconModule, MatTabsModule, MatButtonModule, MatDividerModule, MatListModule, MatChipsModule, RouterLink],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.scss',
})
export class ProjectDetailComponent implements OnInit, OnDestroy {
  project: ProjectDetail | null = null;
  developers: ProjectDeveloper[] = [];
  technologies: ProjectTechnology[] = [];
  repositories: ProjectRepository[] = [];
  tasks: ProjectTask[] = [];
  dependencies: ProjectDependency[] = [];
  contributors: ProjectContributor[] = [];
  recommended: RecommendedDeveloper[] = [];
  indirectTechnologies: IndirectTechnology[] = [];

  loading = true;
  error = false;
  private id = '';

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.id = this.route.snapshot.paramMap.get('id') ?? '';
    forkJoin({
      project: this.api.getProject(this.id),
      developers: this.api.getProjectDevelopers(this.id),
      technologies: this.api.getProjectTechnologies(this.id),
      repositories: this.api.getProjectRepositories(this.id),
      tasks: this.api.getProjectTasks(this.id),
      dependencies: this.api.getProjectDependencies(this.id),
      contributors: this.api.getProjectContributors(this.id),
      recommended: this.api.getRecommendedDevelopers(this.id),
      indirect: this.api.getIndirectTechnologies(this.id),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.project = data.project;
          this.developers = data.developers;
          this.technologies = data.technologies;
          this.repositories = data.repositories;
          this.tasks = data.tasks;
          this.dependencies = data.dependencies;
          this.contributors = data.contributors;
          this.recommended = data.recommended;
          this.indirectTechnologies = data.indirect;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.error = true;
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  viewDeveloper(id: string): void {
    void this.router.navigate(['developers', id]);
  }

  viewTechnology(id: string): void {
    void this.router.navigate(['technologies', id]);
  }

  viewProject(id: string): void {
    void this.router.navigate(['projects', id]);
  }

  coveragePct(coverage: number): string {
    return `${Math.round(coverage * 100)}%`;
  }
}