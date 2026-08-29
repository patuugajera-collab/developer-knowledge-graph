// Question: what is the full dependency graph of a project?
// Variable-length traversal follows DEPENDS_ON edges transitively (depth 1..5).
// In a relational schema this is a recursive CTE that becomes painful at any depth.

MATCH (p:Project {id: $projectId})
MATCH path = (p)-[:DEPENDS_ON*1..5]->(dep:Project)
WITH dep, min(length(path)) AS depth
RETURN dep.name AS dependency,
       dep.status AS status,
       depth
ORDER BY depth, dep.name;

// Question: which technologies are used by a project's dependency graph
// but NOT directly by the project itself? (relationally awkward multi-hop query)

MATCH (p:Project {id: $projectId})
MATCH path = (p)-[:DEPENDS_ON*1..5]->(dep:Project)-[:USES]->(t:Technology)
WHERE NOT (p)-[:USES]->(t)
WITH dep, t, min(length(path)) AS depth
RETURN DISTINCT t.name AS technology,
       t.category AS category,
       dep.name AS usedByDependency,
       depth
ORDER BY t.name, depth;