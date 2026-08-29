import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatChipsModule } from '@angular/material/chips';
import { Subject, catchError, debounceTime, distinctUntilChanged, of, switchMap, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { TechnologySummary } from '../../models/api-models';

@Component({
  selector: 'app-technologies',
  imports: [
    MatCardModule,
    MatTableModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatPaginatorModule,
    MatChipsModule,
  ],
  templateUrl: './technologies.component.html',
  styleUrl: './technologies.component.scss',
})
export class TechnologiesComponent implements OnInit, OnDestroy {
  technologies: TechnologySummary[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  loading = true;
  error = false;
  search = '';
  category = '';
  categories: string[] = [];

  readonly displayedColumns = ['name', 'category'];

  private readonly searchTerms = new Subject<string>();
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.api.getTechnologyCategories().subscribe({
      next: (categories) => (this.categories = categories),
      error: () => undefined,
    });

    this.searchTerms
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap(() => this.loadPage()),
        takeUntil(this.destroy$),
      )
      .subscribe();

    this.loadPage().subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchChange(value: string): void {
    this.search = value;
    this.page = 1;
    this.searchTerms.next(value);
  }

  onCategoryChange(category: string): void {
    this.category = category;
    this.page = 1;
    this.loadPage().subscribe();
  }

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadPage().subscribe();
  }

  open(id: string): void {
    void this.router.navigate(['technologies', id]);
  }

  initials(name: string): string {
    return name.slice(0, 2).toUpperCase();
  }

  private loadPage() {
    this.loading = true;
    this.error = false;
    return this.api.getTechnologies(this.search || undefined, this.category || undefined, this.page, this.pageSize).pipe(
      switchMap((result) => {
        this.technologies = result.items;
        this.totalCount = result.totalCount;
        this.pageSize = result.pageSize;
        this.loading = false;
        return [result];
      }),
      catchError(() => {
        this.loading = false;
        this.error = true;
        return of(null);
      }),
    );
  }
}