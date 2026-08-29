// Question: which developers are connected to a project through MULTIPLE relationship paths?
// Demonstrates how one node type connects to another via distinct typed edges:
//   WORKS_ON (direct) | CONTRIBUTED_TO -> BELONGS_TO (2 hops)

MATCH (p:Project {id: $projectId})

// Path A: developers working directly on the project
MATCH (p)<-[:WORKS_ON]-(worker:Developer)
WITH p, collect(DISTINCT worker) AS workers

// Path B: developers who contributed code to the project's repositories (multi-hop)
MATCH (contributor:Developer)-[:CONTRIBUTED_TO]->(:Repository)-[:BELONGS_TO]->(p)
WITH workers, collect(DISTINCT contributor) AS contributors

// Path C: developers who had a task on another project that depends on this one
MATCH (p)<-[:DEPENDS_ON]-(dependent:Project)
MATCH (dependent)-[:HAS_TASK]->(task:Task)
RETURN count(DISTINCT task) AS dependentTasks
LIMIT 0;

// The useful part of this query is exposed through the API as:
// GET /api/projects/{id}/contributors  (2-hop traversal)
// GET /api/projects/{id}/recommended-developers (multi-hop skill matching)