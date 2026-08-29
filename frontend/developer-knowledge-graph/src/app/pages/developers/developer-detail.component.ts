import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatListModule } from '@angular/material/list';
import { Subject, forkJoin, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import {
  DeveloperDetail,
  DeveloperProject,
  DeveloperRepository,
  DeveloperSkill,
} from '../../models/api-models';

@Component({
  selector: 'app-developer-detail',
  imports: [MatCardModule, MatIconModule, MatTabsModule, MatButtonModule, MatChipsModule, MatListModule, RouterLink],
  templateUrl: './developer-detail.component.html',
  styleUrl: './developer-detail.component.scss',
})
export class DeveloperDetailComponent implements OnInit, OnDestroy {
  developer: DeveloperDetail | null = null;
  skills: DeveloperSkill[] = [];
  repositories: DeveloperRepository[] = [];
  projects: DeveloperProject[] = [];
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
      developer: this.api.getDeveloper(this.id),
      projects: this.api.getDeveloperProjects(this.id),
      skills: this.api.getDeveloperSkills(this.id),
      repositories: this.api.getDeveloperRepositories(this.id),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ developer, projects, skills, repositories }) => {
          this.developer = developer;
          this.projects = projects;
          this.skills = skills;
          this.repositories = repositories;
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

  viewProject(id: string): void {
    void this.router.navigate(['projects', id]);
  }

  viewTechnology(id: string): void {
    void this.router.navigate(['technologies', id]);
  }
}