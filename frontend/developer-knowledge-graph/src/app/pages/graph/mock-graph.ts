import { GraphResponse, GraphNode, GraphEdge } from '../../models/api-models';

const mockNodes: GraphNode[] = [
  { id: 'd1', label: 'Alice Chen', type: 'Developer', properties: { role: 'Backend Engineer' } },
  { id: 'd2', label: 'Marcus Johnson', type: 'Developer', properties: { role: 'Frontend Engineer' } },
  { id: 'd3', label: 'Priya Sharma', type: 'Developer', properties: { role: 'Full-Stack Engineer' } },
  { id: 'd6', label: 'Omar Farouk', type: 'Developer', properties: { role: 'ML Engineer' } },
  { id: 'p1', label: 'Atlas ERP', type: 'Project', properties: { status: 'active' } },
  { id: 'p2', label: 'Nova Analytics', type: 'Project', properties: { status: 'active' } },
  { id: 'p3', label: 'Pulse Monitor', type: 'Project', properties: { status: 'active' } },
  { id: 't01', label: '.NET / C#', type: 'Technology', properties: { category: 'Backend' } },
  { id: 't05', label: 'TypeScript', type: 'Technology', properties: { category: 'Frontend' } },
  { id: 't03', label: 'Python', type: 'Technology', properties: { category: 'Backend' } },
  { id: 't06', label: 'React', type: 'Technology', properties: { category: 'Frontend' } },
  { id: 't17', label: 'TensorFlow', type: 'Technology', properties: { category: 'ML' } },
  { id: 'o1', label: 'Acme Corp', type: 'Organization', properties: {} },
  { id: 'o2', label: 'Nimbus Labs', type: 'Organization', properties: {} },
  { id: 'r01', label: 'atlas-api', type: 'Repository', properties: {} },
  { id: 'r04', label: 'nova-query', type: 'Repository', properties: {} },
];

const mockEdges: GraphEdge[] = [
  { id: 'e1', source: 'd1', target: 'o1', type: 'WORKS_FOR', properties: { since: '2020' } },
  { id: 'e2', source: 'd2', target: 'o1', type: 'WORKS_FOR', properties: { since: '2020' } },
  { id: 'e3', source: 'd3', target: 'o2', type: 'WORKS_FOR', properties: { since: '2020' } },
  { id: 'e4', source: 'd6', target: 'o2', type: 'WORKS_FOR', properties: { since: '2020' } },
  { id: 'e5', source: 'd1', target: 'p1', type: 'WORKS_ON', properties: { role: 'Lead' } },
  { id: 'e6', source: 'd2', target: 'p2', type: 'WORKS_ON', properties: { role: 'Lead' } },
  { id: 'e7', source: 'd3', target: 'p1', type: 'WORKS_ON', properties: { role: 'Contributor' } },
  { id: 'e8', source: 'd6', target: 'p3', type: 'WORKS_ON', properties: { role: 'Lead' } },
  { id: 'e9', source: 'o1', target: 'p1', type: 'OWNS', properties: {} },
  { id: 'e10', source: 'o1', target: 'p2', type: 'OWNS', properties: {} },
  { id: 'e11', source: 'o2', target: 'p3', type: 'OWNS', properties: {} },
  { id: 'e12', source: 'p1', target: 't01', type: 'USES', properties: { purpose: 'build' } },
  { id: 'e13', source: 'p1', target: 't05', type: 'USES', properties: { purpose: 'frontend' } },
  { id: 'e14', source: 'p2', target: 't03', type: 'USES', properties: { purpose: 'processing' } },
  { id: 'e15', source: 'p2', target: 't06', type: 'USES', properties: { purpose: 'frontend' } },
  { id: 'e16', source: 'p3', target: 't17', type: 'USES', properties: { purpose: 'prediction' } },
  { id: 'e17', source: 'd1', target: 't01', type: 'HAS_SKILL', properties: { proficiency: 'Expert' } },
  { id: 'e18', source: 'd2', target: 't05', type: 'HAS_SKILL', properties: { proficiency: 'Expert' } },
  { id: 'e19', source: 'd3', target: 't05', type: 'HAS_SKILL', properties: { proficiency: 'Advanced' } },
  { id: 'e20', source: 'd6', target: 't17', type: 'HAS_SKILL', properties: { proficiency: 'Expert' } },
  { id: 'e21', source: 'd1', target: 'r01', type: 'CONTRIBUTED_TO', properties: { count: 120 } },
  { id: 'e22', source: 'd2', target: 'r04', type: 'CONTRIBUTED_TO', properties: { count: 80 } },
  { id: 'e23', source: 'p2', target: 'p1', type: 'DEPENDS_ON', properties: {} },
];

export const MOCK_GRAPH: GraphResponse = {
  nodes: mockNodes,
  edges: mockEdges,
  rootId: 'd1',
  maxDepth: 3,
};