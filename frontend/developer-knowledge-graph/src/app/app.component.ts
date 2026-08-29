import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent,
} from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, of, switchMap, takeUntil } from 'rxjs';
import { ApiService } from './services/api.service';
import { SearchGroup } from './models/api-models';

@Component({
  selector: 'app-root',
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit, OnDestroy {
  databaseHealthy: boolean | null = null;
  databaseMessage: string | null = null;
  searching = false;
  searchGroups: SearchGroup[] = [];

  private readonly searchTerms = new Subject<string>();
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly api: ApiService,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    this.refreshHealth();

    this.searchTerms
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((term) => {
          const trimmed = term.trim();
          if (!trimmed) {
            this.searching = false;
            this.searchGroups = [];
            return of(null);
          }
          this.searching = true;
          return this.api.search(trimmed);
        }),
        takeUntil(this.destroy$),
      )
      .subscribe({
        next: (response) => {
          this.searching = false;
          this.searchGroups = response?.groups ?? [];
        },
        error: () => {
          this.searching = false;
        },
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSearchInput(event: Event): void {
    this.searchTerms.next((event.target as HTMLInputElement).value);
  }

  onSearchSubmit(event: Event): void {
    event.preventDefault();
    const input = document.querySelector<HTMLInputElement>('.app-search-input');
    const value = input?.value?.trim();
    if (value) {
      void this.router.navigate(['/search'], { queryParams: { q: value } });
    }
  }

  onOptionSelected(event: MatAutocompleteSelectedEvent): void {
    const item = event.option.value as { type: string; id: string } | null;
    if (item) {
      this.goToResult(item.type, item.id);
    }
  }

  goToResult(type: string, id: string): void {
    const route = this.routeFor(type);
    void this.router.navigate([route, id]);
  }

  refreshHealth(): void {
    this.api.getHealth().subscribe({
      next: (health) => {
        this.databaseHealthy = health.status === 'healthy';
        this.databaseMessage = health.message;
      },
      error: () => {
        this.databaseHealthy = false;
        this.databaseMessage = 'Unable to connect to the database. Please try again.';
      },
    });
  }

  routeFor(type: string): string {
    switch (type) {
      case 'Developer':
        return 'developers';
      case 'Project':
        return 'projects';
      case 'Technology':
        return 'technologies';
      case 'Repository':
        return 'repositories';
      default:
        return '';
    }
  }

  displayFn(): string {
    return '';
  }
}