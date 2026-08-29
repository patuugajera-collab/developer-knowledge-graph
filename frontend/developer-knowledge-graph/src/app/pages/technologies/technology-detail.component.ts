import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { Subject, forkJoin, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { TechnologyDetail, TechnologyDeveloper, TechnologyProject } from '../../models/api-models';

@Component({
  selector: 'app-technology-detail',
  imports: [MatCardModule, MatIconModule, MatButtonModule, MatListModule, RouterLink],
  templateUrl: './technology-detail.component.html',
  styleUrl: './technology-detail.component.scss',
})
export class TechnologyDetailComponent implements OnInit, OnDestroy {
  technology: TechnologyDetail | null = null;
  projects: TechnologyProject[] = [];
  developers: TechnologyDeveloper[] = [];
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
      technology: this.api.getTechnology(this.id),
      projects: this.api.getTechnologyProjects(this.id),
      developers: this.api.getTechnologyDevelopers(this.id),
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ technology, projects, developers }) => {
          this.technology = technology;
          this.projects = projects;
          this.developers = developers;
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

  viewDeveloper(id: string): void {
    void this.router.navigate(['developers', id]);
  }
}