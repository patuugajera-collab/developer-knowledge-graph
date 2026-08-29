// Question: which developers have the skills required by a project?
// Multi-hop (mandatory) traversal:
//   Developer -[:HAS_SKILL]-> Technology <-[:REQUIRES_SKILL]- Task <-[:HAS_TASK]- Project
// Also reports coverage = matched skills / total required skills.

MATCH (p:Project {id: $projectId})-[:HAS_TASK]->(task:Task)-[:REQUIRES_SKILL]->(requiredTech:Technology)
WITH p, count(DISTINCT requiredTech) AS totalRequired
MATCH (p)-[:HAS_TASK]->()<-[:REQUIRES_SKILL]-(requiredSkill:Technology)
WITH p, totalRequired, requiredSkill
MATCH (d:Developer)-[:HAS_SKILL]->(requiredSkill)
WITH d, totalRequired, count(DISTINCT requiredSkill) AS matchedSkills
OPTIONAL MATCH (d)-[:WORKS_FOR]->(org:Organization)
RETURN d.name AS developer,
       d.role AS role,
       org.name AS organization,
       matchedSkills,
       totalRequired,
       toFloat(matchedSkills) / totalRequired AS coverage
ORDER BY coverage DESC, matchedSkills DESC;