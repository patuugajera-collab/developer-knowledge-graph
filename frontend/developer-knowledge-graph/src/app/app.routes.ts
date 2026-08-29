import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    title: 'Dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then((c) => c.DashboardComponent),
  },
  {
    path: 'developers',
    title: 'Developers',
    loadComponent: () => import('./pages/developers/developers.component').then((c) => c.DevelopersComponent),
  },
  {
    path: 'developers/:id',
    loadComponent: () => import('./pages/developers/developer-detail.component').then((c) => c.DeveloperDetailComponent),
  },
  {
    path: 'projects',
    title: 'Projects',
    loadComponent: () => import('./pages/projects/projects.component').then((c) => c.ProjectsComponent),
  },
  {
    path: 'projects/:id',
    loadComponent: () => import('./pages/projects/project-detail.component').then((c) => c.ProjectDetailComponent),
  },
  {
    path: 'technologies',
    title: 'Technologies',
    loadComponent: () => import('./pages/technologies/technologies.component').then((c) => c.TechnologiesComponent),
  },
  {
    path: 'technologies/:id',
    loadComponent: () => import('./pages/technologies/technology-detail.component').then((c) => c.TechnologyDetailComponent),
  },
  {
    path: 'graph',
    title: 'Graph Explorer',
    loadComponent: () => import('./pages/graph/graph-explorer.component').then((c) => c.GraphExplorerComponent),
  },
  {
    path: 'search',
    title: 'Search',
    loadComponent: () => import('./pages/search/search.component').then((c) => c.SearchComponent),
  },
  { path: 'repositories', redirectTo: 'projects', pathMatch: 'full' },
  { path: 'repositories/:id', redirectTo: 'projects', pathMatch: 'full' },
  { path: '**', redirectTo: 'dashboard' },
];