export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface HealthResponse {
  status: string;
  database: string;
  message: string | null;
}

export interface ErrorResponse {
  message: string;
  detail?: string | null;
  traceId: string;
}

// ---- Developers ----
export interface DeveloperSummary {
  id: string;
  name: string;
  email: string;
  role: string;
  organizationName: string | null;
}

export interface DeveloperDetail {
  id: string;
  name: string;
  email: string;
  role: string;
  organizationName: string | null;
  projectCount: number;
  skillCount: number;
  repositoryCount: number;
}

export interface DeveloperProject {
  projectId: string;
  projectName: string;
  projectStatus: string;
  role: string;
  since: string;
}

export interface DeveloperSkill {
  technologyId: string;
  technologyName: string;
  category: string;
  proficiency: string;
  since: string;
}

export interface DeveloperRepository {
  repositoryId: string;
  repositoryName: string;
  url: string;
  contributionCount: number;
  since: string;
}

// ---- Projects ----
export interface ProjectSummary {
  id: string;
  name: string;
  description: string;
  status: string;
}

export interface ProjectDetail {
  id: string;
  name: string;
  description: string;
  status: string;
  developerCount: number;
  technologyCount: number;
  repositoryCount: number;
  taskCount: number;
}

export interface ProjectDeveloper {
  developerId: string;
  name: string;
  role: string;
  since: string;
  organizationName: string | null;
}

export interface ProjectTechnology {
  technologyId: string;
  technologyName: string;
  category: string;
  purpose: string | null;
}

export interface ProjectDependency {
  projectId: string;
  projectName: string;
  projectStatus: string;
  depth: number;
}

export interface ProjectTask {
  taskId: string;
  title: string;
  status: string;
  priority: number | null;
}

export interface ProjectRepository {
  repositoryId: string;
  repositoryName: string;
  url: string;
}

export interface RecommendedDeveloper {
  developerId: string;
  name: string;
  role: string;
  matchedSkills: number;
  totalRequired: number;
  coverage: number;
  organizationName: string | null;
}

export interface IndirectTechnology {
  technologyId: string;
  technologyName: string;
  category: string;
  dependencyProjectId: string;
  dependencyProjectName: string;
  depth: number;
}

export interface ProjectContributor {
  developerId: string;
  developerName: string;
  role: string;
  repositoryName: string;
  contributionCount: number;
  since: string;
}

// ---- Technologies ----
export interface TechnologySummary {
  id: string;
  name: string;
  category: string;
}

export interface TechnologyDetail {
  id: string;
  name: string;
  category: string;
  projectCount: number;
  developerCount: number;
}

export interface TechnologyDeveloper {
  developerId: string;
  name: string;
  role: string;
  proficiency: string;
  since: string;
}

export interface TechnologyProject {
  projectId: string;
  projectName: string;
  projectStatus: string;
  purpose: string | null;
}

export interface CentralTechnology {
  id: string;
  name: string;
  category: string;
  projectUsage: number;
  skillCount: number;
  centrality: number;
}

// ---- Organizations ----
export interface OrganizationSummary {
  id: string;
  name: string;
  developerCount: number;
  projectCount: number;
}

// ---- Dashboard ----
export interface StatusCount {
  status: string;
  count: number;
}

export interface RelationshipTypeCount {
  type: string;
  count: number;
}

export interface DashboardStats {
  developers: number;
  projects: number;
  technologies: number;
  repositories: number;
  tasks: number;
  organizations: number;
  relationships: number;
  averageConnectionsPerDeveloper: number;
  projectStatus: StatusCount[];
  relationshipTypes: RelationshipTypeCount[];
  topTechnologies: CentralTechnology[];
}

// ---- Search ----
export interface SearchItem {
  id: string;
  name: string;
  subtitle: string;
  type: string;
}

export interface SearchGroup {
  category: string;
  results: SearchItem[];
}

export interface SearchResponse {
  groups: SearchGroup[];
  total: number;
}

// ---- Graph ----
export interface GraphNode {
  id: string;
  label: string;
  type: string;
  properties: Record<string, unknown>;
}

export interface GraphEdge {
  id: string;
  source: string;
  target: string;
  type: string;
  properties: Record<string, unknown>;
}

export interface GraphResponse {
  nodes: GraphNode[];
  edges: GraphEdge[];
  rootId: string;
  maxDepth: number;
}

export interface PathStep {
  nodeType: string;
  nodeId: string;
  nodeName: string;
  relationship: string | null;
}

export interface ShortestPath {
  developerId: string;
  developerName: string;
  projectId: string;
  projectName: string;
  steps: PathStep[];
  length: number;
}