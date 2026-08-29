import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AppComponent } from './app.component';
import { ApiService } from './services/api.service';
import { HealthResponse } from './models/api-models';

describe('AppComponent', () => {
  const api = jasmine.createSpyObj<ApiService>('ApiService', ['getHealth', 'search']);

  beforeEach(async () => {
    api.getHealth.and.returnValue(of({ status: 'healthy', database: 'connected', message: null } as HealthResponse));
    api.search.and.returnValue(of({ groups: [], total: 0 }));

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter([]), { provide: ApiService, useValue: api }],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render the KnowledgeGraph brand', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.brand')?.textContent?.replace(/\s/g, '')).toEqual('KnowledgeGraph');
  });

  it('should mark the database healthy when the health check succeeds', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    expect(fixture.componentInstance.databaseHealthy).toBeTrue();
  });

  it('should show the db-banner when the database is unreachable', () => {
    api.getHealth.and.returnValue(throwError(() => new Error('down')));
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(fixture.componentInstance.databaseHealthy).toBeFalse();
    expect(compiled.querySelector('.db-banner')).not.toBeNull();
  });
});