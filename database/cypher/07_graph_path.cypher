// Question: what is the shortest / most relevant path between a developer and a project?
// shortestPath() runs a bidirectional BFS across any combination of relationship types.

MATCH (d:Developer {id: $developerId}), (p:Project {id: $projectId})
MATCH path = shortestPath((d)-[*0..6]-(p))
RETURN [node IN nodes(path) | labels(node)[0] + ':' + node.name] AS path,
       length(path) AS hops;

// The API returns a structured step list:
// GET /api/graph/shortest-path?developerId={id}&projectId={id}