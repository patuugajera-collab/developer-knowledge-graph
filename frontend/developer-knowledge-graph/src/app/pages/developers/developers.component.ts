import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { FormsModule } from '@angular/forms';
import { Subject, catchError, distinctUntilChanged, debounceTime, of, switchMap, takeUntil } from 'rxjs';
import { ApiService } from '../../services/api.service';
import { DeveloperSummary } from '../../models/api-models';

@Component({
  selector: 'app-developers',
  imports: [
    MatCardModule,
    MatTableModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    FormsModule,
  ],
  templateUrl: './developers.component.html',
  styleUrl: './developers.component.scss',
})
export class DevelopersComponent implements OnInit, OnDestroy {
  developers: DeveloperSummary[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  loading = true;
  error = false;
  search = '';

  readonly displayedColumns = ['name', 'role', 'organization'];

  private readonly searchTerms = new Subject<string>();
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
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

  onPageChange(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadPage().subscribe();
  }

  open(id: string): void {
    void this.router.navigate(['developers', id]);
  }

  initials(name: string): string {
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0] ?? '')
      .join('')
      .toUpperCase();
  }

  private loadPage() {
    this.loading = true;
    this.error = false;
    return this.api.getDevelopers(this.search || undefined, this.page, this.pageSize).pipe(
      switchMap((result) => {
        this.developers = result.items;
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