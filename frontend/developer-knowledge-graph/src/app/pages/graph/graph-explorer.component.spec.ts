import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { GraphExplorerComponent } from './graph-explorer.component';
import { ApiService } from '../../services/api.service';
import { MOCK_GRAPH } from './mock-graph';

const emptyPage = { items: [], page: 1, pageSize: 100, totalCount: 0, totalPages: 0 } as any;
const devPage = {
  items: [{ id: 'd1', name: 'Alice Chen', email: 'alice.chen@example.com', role: 'Backend Engineer', organizationName: 'Acme Corp' }],
  page: 1, pageSize: 100, totalCount: 1, totalPages: 1,
} as any;

describe('GraphExplorerComponent', () => {
  const api = jasmine.createSpyObj<ApiService>('ApiService', [
    'getDevelopers',
    'getProjects',
    'getTechnologies',
    'getGraph',
    'getShortestPath',
  ]);

  function setup(): GraphExplorerComponent {
    const fixture = TestBed.createComponent(GraphExplorerComponent);
    fixture.detectChanges();
    return fixture.componentInstance;
  }

  beforeEach(async () => {
    api.getDevelopers.and.returnValue(of(devPage));
    api.getProjects.and.returnValue(of(emptyPage));
    api.getTechnologies.and.returnValue(of(emptyPage));
    api.getGraph.and.returnValue(of(MOCK_GRAPH));
    api.getShortestPath.and.returnValue(of({
      developerId: 'd1', developerName: 'Alice Chen', projectId: 'p1', projectName: 'Atlas ERP',
      steps: [], length: 0,
    } as any));

    await TestBed.configureTestingModule({
      imports: [GraphExplorerComponent],
      providers: [provideRouter([]), { provide: ApiService, useValue: api }],
    }).compileComponents();
  });

  it('should create the component', () => {
    expect(setup()).toBeTruthy();
  });

  it('should render the demo graph when the API is unreachable', () => {
    api.getDevelopers.and.returnValue(throwError(() => new Error('down')));
    api.getGraph.and.returnValue(throwError(() => new Error('down')));

    const component = setup();
    expect(component.offlineMode).toBeTrue();
    expect(component.graph).not.toBeNull();
    expect(component.nodes.length).toBeGreaterThan(0);
    expect(component.links.length).toBeGreaterThan(0);
  });

  it('should not enter offline mode when the API responds', () => {
    const component = setup();
    expect(component.offlineMode).toBeFalse();
    expect(component.options.length).toBeGreaterThan(0);
  });
});
