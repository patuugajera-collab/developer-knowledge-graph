import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { Observable, Subject, map, takeUntil } from 'rxjs';
import { forceCenter, forceCollide, forceLink, forceManyBody, forceSimulation, SimulationNodeDatum } from 'd3-force';
import { ApiService } from '../../services/api.service';
import { GraphNode, GraphEdge, GraphResponse, ShortestPath } from '../../models/api-models';
import { MOCK_GRAPH } from './mock-graph';

interface LayoutNode extends SimulationNodeDatum {
  id: string;
  label: string;
  type: string;
  r: number;
  color: string;
}

interface LayoutLink {
  id: string;
  type: string;
  source: string;
  target: string;
}

interface EntityOption {
  id: string;
  name: string;
}

@Component({
  selector: 'app-graph-explorer',
  imports: [
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSliderModule,
  ],
  templateUrl: './graph-explorer.component.html',
  styleUrl: './graph-explorer.component.scss',
})
export class GraphExplorerComponent implements OnInit {
  @ViewChild('graphCanvas', { static: false }) canvas?: ElementRef<SVGSVGElement>;

  readonly nodeColors: Record<string, string> = {
    Developer: '#5c6bc0',
    Project: '#00897b',
    Technology: '#9c27b0',
    Organization: '#8d6e63',
    Repository: '#f9a825',
    Task: '#e53935',
  };

  entityTypes = ['Developer', 'Project', 'Technology'];
  entityType = 'Developer';
  options: EntityOption[] = [];
  selectedId = '';
  maxDepth = 3;

  loading = false;
  error = false;
  offlineMode = false;
  graph: GraphResponse | null = null;
  nodes: LayoutNode[] = [];
  links: LayoutLink[] = [];
  hoveredId: string | null = null;
  neighborIds: Set<string> = new Set();

  developerOptions: EntityOption[] = [];
  projectOptions: EntityOption[] = [];
  pathDeveloperId = '';
  pathProjectId = '';
  pathLoading = false;
  path: ShortestPath | null = null;

  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly api: ApiService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
  ) {}

  ngOnInit(): void {
    void this.loadOptions();

    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const entityId = params['id'];
      if (entityId) {
        this.loadGraphFor(params['entityType'] ?? 'Developer', entityId);
      }
    });

    // Populate the shortest-path pickers.
    this.api.getDevelopers('', 1, 100).subscribe({
      next: (result) => (this.developerOptions = result.items.map((d) => ({ id: d.id, name: d.name }))),
      error: () => undefined,
    });
    this.api.getProjects('', undefined, 1, 100).subscribe({
      next: (result) => (this.projectOptions = result.items.map((p) => ({ id: p.id, name: p.name }))),
      error: () => undefined,
    });
  }

  onEntityTypeChange(type: string): void {
    this.entityType = type;
    this.selectedId = '';
    void this.loadOptions();
  }

  loadOptions(): void {
    this.loading = true;
    this.entityOptions().subscribe({
      next: (items) => {
        this.options = items;
        this.selectedId = this.selectedId || this.options[0]?.id || '';
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.enterOfflineMode();
      },
    });
  }

  private entityOptions(): Observable<EntityOption[]> {
    switch (this.entityType) {
      case 'Developer':
        return this.api.getDevelopers('', 1, 100).pipe(map((result) => result.items.map((item) => ({ id: item.id, name: item.name }))));
      case 'Project':
        return this.api.getProjects('', undefined, 1, 100).pipe(map((result) => result.items.map((item) => ({ id: item.id, name: item.name }))));
      default:
        return this.api.getTechnologies('', undefined, 1, 100).pipe(map((result) => result.items.map((item) => ({ id: item.id, name: item.name }))));
    }
  }

  private enterOfflineMode(): void {
    this.offlineMode = true;
    this.error = false;
    this.options = MOCK_GRAPH.nodes
      .filter((node) => node.type === this.entityType)
      .map((node) => ({ id: node.id, name: node.label }));
    this.selectedId = this.selectedId || this.options[0]?.id || '';
    this.loadMockGraph();
  }

  private loadMockGraph(): void {
    const rootId = this.selectedId && MOCK_GRAPH.nodes.some((n) => n.id === this.selectedId)
      ? this.selectedId
      : MOCK_GRAPH.rootId;
    this.graph = { ...MOCK_GRAPH, rootId };
    this.layout(this.graph);
    this.loading = false;
  }

  private loadGraphFor(type: string, id: string): void {
    this.entityType = type;
    this.selectedId = id;
    this.loadGraph();
  }

  loadGraph(): void {
    if (!this.selectedId) {
      return;
    }
    this.loading = true;
    this.error = false;
    this.graph = null;
    this.links = [];
    this.nodes = [];

    this.api.getGraph(this.entityType, this.selectedId, this.maxDepth).subscribe({
      next: (graph) => {
        this.graph = graph;
        this.layout(graph);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.enterOfflineMode();
      },
    });
  }

  onDepthChange(value: number): void {
    this.maxDepth = value;
    if (this.selectedId) {
      this.loadGraph();
    }
  }

  nodeColor(type: string): string {
    return this.nodeColors[type] ?? '#9e9e9e';
  }

  get nodeTypeKeys(): string[] {
    return Object.keys(this.nodeColors);
  }

  nodeX(id: string): number {
    return this.nodes.find((n) => n.id === id)?.x ?? 0;
  }

  nodeY(id: string): number {
    return this.nodes.find((n) => n.id === id)?.y ?? 0;
  }

  hoveredName(id: string): string {
    return this.nodes.find((n) => n.id === id)?.label ?? id;
  }

  iconForStep(step: { nodeType: string }): string {
    switch (step.nodeType) {
      case 'Developer':
        return 'engineering';
      case 'Project':
        return 'rocket_launch';
      case 'Technology':
        return 'memory';
      case 'Organization':
        return 'groups';
      case 'Repository':
        return 'code';
      case 'Task':
        return 'checklist';
      default:
        return 'circle';
    }
  }

  private layout(graph: GraphResponse): void {
    const nodes: LayoutNode[] = graph.nodes.map((node: GraphNode) => ({
      id: node.id,
      label: node.label,
      type: node.type,
      r: node.id === graph.rootId ? 22 : 14,
      color: this.nodeColor(node.type),
    }));

    const links: LayoutLink[] = graph.edges.map((edge: GraphEdge) => ({
      id: edge.id,
      type: edge.type,
      source: edge.source,
      target: edge.target,
    }));

    // Terminate early so the sim never keeps ticking.
    const simulation = forceSimulation<LayoutNode>(nodes)
      .force(
        'charge',
        forceManyBody<LayoutNode>().strength(-220),
      )
      .force(
        'link',
        forceLink<LayoutNode, LayoutLink>(links)
          .id((d) => d.id)
          .distance(70)
          .strength(0.6),
      )
      .force('center', forceCenter(400, 320))
      .force('collide', forceCollide<LayoutNode>(20).iterations(2));

    // Let the layout settle synchronously.
    for (let i = 0; i < 250; i++) {
      simulation.tick();
    }
    simulation.stop();

    this.nodes = nodes;
    this.links = links;
  }

  onHover(id: string | null): void {
    this.hoveredId = id;
    if (id) {
      const set = new Set<string>();
      for (const link of this.links) {
        if (link.source === id) set.add(link.target);
        if (link.target === id) set.add(link.source);
      }
      this.neighborIds = set;
    } else {
      this.neighborIds = new Set();
    }
  }

  isDimmed(nodeId: string): boolean {
    return !!this.hoveredId && nodeId !== this.hoveredId && !this.neighborIds.has(nodeId);
  }

  openNode(node: LayoutNode): void {
    const route = this.routeFor(node.type);
    if (route) {
      void this.router.navigate([route, node.id]);
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

  findPath(): void {
    if (!this.pathDeveloperId || !this.pathProjectId) {
      return;
    }
    this.pathLoading = true;
    this.path = null;
    this.api.getShortestPath(this.pathDeveloperId, this.pathProjectId).subscribe({
      next: (path) => {
        this.path = path;
        this.pathLoading = false;
      },
      error: () => {
        this.pathLoading = false;
      },
    });
  }

  openPathNode(step: { nodeType: string; nodeId: string }): void {
    const route = this.routeFor(step.nodeType);
    if (route) {
      void this.router.navigate([route, step.nodeId]);
    }
  }
}