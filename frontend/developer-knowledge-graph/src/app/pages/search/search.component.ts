import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { Subject, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { SearchGroup } from '../../models/api-models';

@Component({
  selector: 'app-search',
  imports: [CommonModule, MatCardModule, MatIconModule, MatListModule, MatDividerModule],
  templateUrl: './search.component.html',
  styleUrl: './search.component.scss',
})
export class SearchComponent implements OnInit, OnDestroy {
  query = '';
  groups: SearchGroup[] = [];
  total = 0;
  loading = true;
  error = false;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      this.query = params['q'] ?? '';
      this.run();
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  run(): void {
    if (!this.query.trim()) {
      this.loading = false;
      this.groups = [];
      return;
    }
    this.loading = true;
    this.error = false;
    this.api.search(this.query.trim()).subscribe({
      next: (result) => {
        this.groups = result.groups;
        this.total = result.total;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = true;
      },
    });
  }

  go(item: { type: string; id: string }): void {
    const route = this.routeFor(item.type);
    if (route) {
      void this.router.navigate([route, item.id]);
    }
  }

  routeFor(type: string): string {
    switch (type) {
      case 'Developer':
        return 'developers';
      case 'Project':
        return 'projects';
      case 'Technology':
        return 'technologies';
      default:
        return '';
    }
  }

  iconFor(type: string): string {
    switch (type) {
      case 'Developer':
        return 'engineering';
      case 'Project':
        return 'rocket_launch';
      case 'Technology':
        return 'memory';
      case 'Organization':
        return 'groups';
      default:
        return 'search';
    }
  }
}