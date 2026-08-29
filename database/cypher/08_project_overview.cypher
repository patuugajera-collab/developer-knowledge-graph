// Question: summarise a project's neighbourhood in a single pass.
// Shows developers, technologies, repositories, tasks and direct dependencies in one query.

MATCH (p:Project {id: $projectId})
OPTIONAL MATCH (d:Developer)-[r:WORKS_ON]->(p)
OPTIONAL MATCH (p)-[u:USES]->(t:Technology)
OPTIONAL MATCH (repo:Repository)-[:BELONGS_TO]->(p)
OPTIONAL MATCH (p)-[:HAS_TASK]->(task:Task)
OPTIONAL MATCH (p)-[dep:DEPENDS_ON]->(dependency:Project)
RETURN p.name AS project,
       collect(DISTINCT {type: 'developer', name: d.name, role: r.role}) AS developers,
       collect(DISTINCT {type: 'technology', name: t.name, purpose: u.purpose}) AS technologies,
       collect(DISTINCT {type: 'repository', name: repo.name}) AS repositories,
       collect(DISTINCT {type: 'task', title: task.title, status: task.status}) AS tasks,
       collect(DISTINCT {type: 'dependency', name: dependency.name}) AS dependencies;